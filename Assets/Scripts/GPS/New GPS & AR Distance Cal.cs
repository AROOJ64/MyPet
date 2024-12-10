using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class GPSAndStepCounter : MonoBehaviour
{
    [Header("AR Elements")]
    [SerializeField] private XROrigin arSessionOrigin;

    [SerializeField] private Transform arCamera;

    [SerializeField] private TextMeshProUGUI totalDistanceText;
    [SerializeField] private TextMeshProUGUI totalStepsText;

    // Weights for combining GPS and AR distances
    [SerializeField, Range(0f, 1f)]
    private float gpsWeight = 0.7f; // Increased GPS weight

    [SerializeField, Range(0f, 1f)]
    private float arWeight = 0.3f;  // Decreased AR weight

    // Average step length in meters (adjust as needed)
    [SerializeField] private float averageStepLength = 0.76f; // Average adult step length

    [Header("Events")]
    public float distanceThreshold = 1000f; // Example threshold in meters

    public UnityEvent OnDistanceReached;

    public int stepsThreshold = 2000; // Example threshold in steps
    public UnityEvent OnStepsReached;

    private float gpsLatitude;
    private float gpsLongitude;

    #region Step Detection Variables

    private int _totalSteps = 0;

    // Step detection variables
    private readonly Queue<float> accelerationHistory = new();

    private readonly int historySize = 50;  // Increased history size
    private float previousFilteredValue = 0f;

    // Peak detection parameters
    private readonly float lowPassFilterFactor = 0.1f; // Decreased from 0.3f

    private readonly float peakThreshold = 0.2f; // Increased from 0.1f
    private readonly float stepCooldown = 0.5f; // Increased from 0.2f
    private float lastStepTime = 0f;

    #endregion Step Detection Variables

    private bool hasInitialized = false;

    private float gpsTotalDistance = 0f;
    private float arTotalDistance = 0f;
    private float _totalDistance = 0f;
    private float _stepDistance = 0f;
    private float loadedTotalDistance = 0f;

    private Vector3 lastARPosition;
    private bool isFirstARPosition = true;

    private float lastDistanceReached = 0f;
    private int lastStepsReached = 0;
    private readonly float distanceEventCooldown = 1f; // Cooldown period in seconds
    private float lastDistanceEventTime = 0f;
    private float lastStepsEventTime = 0f;

    private bool _isDataLoaded = false;

    public static GPSAndStepCounter Instance { get; private set; }

    #region Properties

    internal float TotalDistance
    {
        get => _totalDistance;
        set
        {
            _totalDistance = value;
            SaveData();
        }
    }

    internal int TotalSteps
    {
        get => _totalSteps;
        set
        {
            _totalSteps = value;
            SaveData();
        }
    }

    #endregion Properties

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Persist across scenes if needed
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadData();
    }

    private void Start()
    {
        // Request location permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }

        // Initialize GPS
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled.");
        }

        // Validate AR setup
        if (arSessionOrigin == null || arCamera == null)
        {
            Debug.LogError("AR Session Origin or AR Camera not set!");
        }

        // Initialize accelerometer
        if (!SystemInfo.supportsAccelerometer)
        {
            Debug.LogError("Accelerometer not supported on this device!");
        }

        Input.location.Start();

        // Save the initial AR position
        lastARPosition = arCamera.position;

        // Initialize previousFilteredValue with the initial acceleration magnitude
        previousFilteredValue = Input.acceleration.magnitude;

        // Pre-fill the accelerationHistory with the initial value
        for (int i = 0; i < historySize; i++)
            accelerationHistory.Enqueue(previousFilteredValue);
    }

    private void Update()
    {
        // Check if GPS tracking is available
        bool isGPSTrackingAvailable = Input.location.status == LocationServiceStatus.Running && Permission.HasUserAuthorizedPermission(Permission.FineLocation);

        if (isGPSTrackingAvailable)
        {
            // Update GPS data
            UpdateGPSData();

            // Update AR distance
            UpdateARDistance();

            // Update total distance based on weights
            TotalDistance = (gpsWeight * gpsTotalDistance) + (arWeight * arTotalDistance);
        }
        else
        {
            // Use step distance if GPS tracking is unavailable
            //TotalDistance = _stepDistance;

            //Debug.Log("GPS tracking not available. Using step distance instead.");
        }

        // Detect steps using accelerometer
        DetectSteps();

        // Reset totalSteps after initialization
        //if (!hasInitialized && accelerationHistory.Count >= historySize)
        //{
        //    hasInitialized = true;
        //}

        // Calculate distance based on steps
        _stepDistance = TotalSteps * averageStepLength;

        // Check thresholds and invoke events
        if (TotalDistance - lastDistanceReached >= distanceThreshold && Time.time - lastDistanceEventTime >= distanceEventCooldown)
        {
            lastDistanceReached += distanceThreshold;
            lastDistanceEventTime = Time.time;
            OnDistanceReached?.Invoke();
        }

        if (TotalSteps - lastStepsReached >= stepsThreshold && Time.time - lastStepsEventTime >= distanceEventCooldown)
        {
            lastStepsReached += stepsThreshold;
            lastStepsEventTime = Time.time;
            OnStepsReached?.Invoke();
        }

        // Update the UI with the calculated values
        totalDistanceText.text = $"{TotalDistance:F2} meters"; // Format total distance to 2 decimal places
        totalStepsText.text = $"{TotalSteps}"; // Display total steps as an integer
    }

    private void OnApplicationQuit()
    {
        SaveData(); // Save data when the application is closed
    }

    private void UpdateGPSData()
    {
        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;
        float horizontalAccuracy = Input.location.lastData.horizontalAccuracy;

        if (gpsLatitude != 0 && gpsLongitude != 0)
        {
            // Calculate GPS distance using the Haversine formula
            float gpsDistance = CalculateGPSDistance(gpsLatitude, gpsLongitude, latitude, longitude);

            // Check GPS accuracy before adding distance
            if (horizontalAccuracy <= 10f) // Acceptable accuracy threshold (in meters)
            {
                // Apply a threshold to ignore minor GPS noise
                if (gpsDistance > 0.5f)
                    gpsTotalDistance += gpsDistance;
            }
            else
            {
                //Debug.Log("GPS data not accurate enough. Skipping this update.");
            }
        }

        // Update GPS coordinates
        gpsLatitude = latitude;
        gpsLongitude = longitude;
    }

    private void UpdateARDistance()
    {
        Vector3 currentARPosition = arCamera.position;
        if (!isFirstARPosition)
        {
            float arDistance = Vector3.Distance(lastARPosition, currentARPosition);

            // Increase the threshold to ignore minor positional noise
            if (arDistance > 0.2f) // Increased threshold in meters
                arTotalDistance += arDistance;
        }
        else
        {
            isFirstARPosition = false;
        }

        lastARPosition = currentARPosition;
    }

    private float CalculateGPSDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371e3f; // Earth's radius in meters
        float latRad1 = Mathf.Deg2Rad * lat1;
        float latRad2 = Mathf.Deg2Rad * lat2;
        float deltaLat = Mathf.Deg2Rad * (lat2 - lat1);
        float deltaLon = Mathf.Deg2Rad * (lon2 - lon1);

        float a = Mathf.Sin(deltaLat / 2f) * Mathf.Sin(deltaLat / 2f) +
                  Mathf.Cos(latRad1) * Mathf.Cos(latRad2) *
                  Mathf.Sin(deltaLon / 2f) * Mathf.Sin(deltaLon / 2f);
        float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));

        return R * c; // Distance in meters
    }

    private void DetectSteps()
    {
        if (IsDeviceStationary())
        {
            // Device is stationary; skip step detection
            return;
        }

        // Get current acceleration magnitude
        float currentAcceleration = Input.acceleration.magnitude;

        // Apply a low-pass filter
        float filteredAcceleration = lowPassFilterFactor * currentAcceleration + (1 - lowPassFilterFactor) * previousFilteredValue;
        previousFilteredValue = filteredAcceleration;

        // Store the filtered value in the history
        accelerationHistory.Enqueue(filteredAcceleration);
        if (accelerationHistory.Count > historySize)
            accelerationHistory.Dequeue();

        // Only perform peak detection if accelerationHistory is full
        if (accelerationHistory.Count < historySize)
        {
            // Not enough data yet; skip step detection
            return;
        }

        // Perform peak detection
        if (IsStepDetected())
            TotalSteps++;
    }

    private bool IsStepDetected()
    {
        float[] data = accelerationHistory.ToArray();
        int lastIndex = data.Length - 1;

        if (lastIndex < 1)
            return false;

        float currentTime = Time.time;

        // Calculate mean and standard deviation
        float sum = 0f;
        foreach (float value in data)
            sum += value;

        float mean = sum / data.Length;

        float sumOfSquares = 0f;
        foreach (float value in data)
            sumOfSquares += (value - mean) * (value - mean);

        float standardDeviation = Mathf.Sqrt(sumOfSquares / data.Length);

        // Set minimum standard deviation
        if (standardDeviation < 0.1f)
            standardDeviation = 0.1f;

        float dynamicThreshold = mean + standardDeviation * 0.5f;

        // Ensure the dynamic threshold is not lower than the minimum peak threshold
        if (dynamicThreshold < peakThreshold)
        {
            dynamicThreshold = peakThreshold;
        }

        float prevAcceleration = data[lastIndex - 1];
        float currentAcceleration = data[lastIndex];

        // Debugging output
        //Debug.Log($"Mean: {mean:F3}, StdDev: {standardDeviation:F3}, DynamicThreshold: {dynamicThreshold:F3}, CurrentAccel: {currentAcceleration:F3}");

        // Detect a peak
        if ((currentAcceleration > dynamicThreshold) &&
            (currentAcceleration > prevAcceleration) &&
            (currentTime - lastStepTime > stepCooldown))
        {
            lastStepTime = currentTime;
            return true;
        }

        return false;
    }

    private bool IsDeviceStationary() => Input.acceleration.magnitude < 0.05f; // Adjust threshold as needed

    private void SaveData()
    {
        if (!_isDataLoaded)
            return;

        PlayerPrefs.SetFloat("gpsTotalDistance", gpsTotalDistance);
        PlayerPrefs.SetFloat("arTotalDistance", arTotalDistance);

        PlayerPrefs.SetInt("TotalSteps", TotalSteps);
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());

        PlayerPrefs.Save();

        Debug.Log("Data Saved - Total Distance: " + TotalDistance + ", Total Steps: " + TotalSteps);
    }

    private void LoadData()
    {
        gpsTotalDistance = PlayerPrefs.GetFloat("gpsTotalDistance");
        arTotalDistance = PlayerPrefs.GetFloat("arTotalDistance");

        TotalSteps = PlayerPrefs.GetInt("TotalSteps");

        string lastSaveTimeStr = PlayerPrefs.GetString("LastSaveTime", string.Empty);
        if (!string.IsNullOrEmpty(lastSaveTimeStr))
        {
            DateTime lastSaveTime = DateTime.Parse(lastSaveTimeStr);
            if ((DateTime.Now - lastSaveTime).TotalHours >= 24)
            {
                ResetData();
            }
        }

        _isDataLoaded = true;

        Debug.Log("Data Loaded - Total Distance: " + TotalDistance + ", Total Steps: " + TotalSteps);
    }

    private void ResetData()
    {
        // Clear saved data
        PlayerPrefs.DeleteKey("TotalDistance");
        PlayerPrefs.DeleteKey("TotalSteps");
        PlayerPrefs.DeleteKey("LastSaveTime");
        PlayerPrefs.Save();

        // Reset in-memory values
        TotalDistance = 0f;
        TotalSteps = 0;
        _stepDistance = 0f;

        // Update the UI
        totalDistanceText.text = $"{TotalDistance:F2} meters";
        totalStepsText.text = $"{TotalSteps}";

        Debug.Log("Data Reset - Total Distance: " + TotalDistance + ", Total Steps: " + TotalSteps);
    }
}