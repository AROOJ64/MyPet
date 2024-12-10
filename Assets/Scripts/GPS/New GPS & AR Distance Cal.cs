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

    private float gpsLatitude;
    private float gpsLongitude;

    #region Step Detection Variables

    private int totalSteps = 0;

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
    private float totalDistance = 0f;
    private float stepDistance = 0f;

    private Vector3 lastARPosition;
    private bool isFirstARPosition = true;

    private float lastDistanceReached = 0f;
    private int lastStepsReached = 0;
    private readonly float distanceEventCooldown = 1f; // Cooldown period in seconds
    private float lastDistanceEventTime = 0f;
    private float lastStepsEventTime = 0f;

    #region Properties

    internal float TotalDistance => totalDistance;
    internal int TotalSteps => totalSteps;

    #endregion Properties

    public static GPSAndStepCounter Instance { get; private set; }

    [Header("Events")]
    public float distanceThreshold = 1000f; // Example threshold in meters

    public UnityEvent OnDistanceReached;

    public int stepsThreshold = 2000; // Example threshold in steps
    public UnityEvent OnStepsReached;
    
    //Daily Reset
    private const string LastResetDateKey = "LastResetDate";
    private string cachedResetDate;

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
            return;
        }

        Input.location.Start();

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

        // Save the initial AR position
        lastARPosition = arCamera.position;

        // Initialize previousFilteredValue with the initial acceleration magnitude
        previousFilteredValue = Input.acceleration.magnitude;

        // Pre-fill the accelerationHistory with the initial value
        for (int i = 0; i < historySize; i++)
            accelerationHistory.Enqueue(previousFilteredValue);

        LoadData();
        SaveData(); // Ensure data is saved after loading initial values

        cachedResetDate = PlayerPrefs.GetString(LastResetDateKey, string.Empty);
        CheckDailyReset();
    }

    private void Update()
    {
        // Update GPS data
        UpdateGPSData();

        // Update AR distance
        UpdateARDistance();

        // Update total distance based on weights
        totalDistance = (gpsWeight * gpsTotalDistance) + (arWeight * arTotalDistance);

        // Detect steps using accelerometer
        DetectSteps();

        // Reset totalSteps after initialization
        if (!hasInitialized && accelerationHistory.Count >= historySize)
        {
            totalSteps = 0;
            hasInitialized = true;
        }

        // Calculate distance based on steps
        stepDistance = totalSteps * averageStepLength;

        // Check thresholds and invoke events
        if (totalDistance - lastDistanceReached >= distanceThreshold && Time.time - lastDistanceEventTime >= distanceEventCooldown)
        {
            lastDistanceReached += distanceThreshold;
            lastDistanceEventTime = Time.time;
            OnDistanceReached?.Invoke();
            SaveData();
        }

        if (totalSteps - lastStepsReached >= stepsThreshold && Time.time - lastStepsEventTime >= distanceEventCooldown)
        {
            lastStepsReached += stepsThreshold;
            lastStepsEventTime = Time.time;
            OnStepsReached?.Invoke();
            SaveData();
        }

        // Update the UI with the calculated values
        totalDistanceText.text = $"{totalDistance:F2} meters"; // Format total distance to 2 decimal places
        totalStepsText.text = $"{totalSteps}"; // Display total steps as an integer

        // Debugging output
        Debug.Log($"Total Distance Walked: {totalDistance:F2} meters");
        Debug.Log($"Total Steps Taken: {totalSteps}");
        Debug.Log($"Distance Calculated from Steps: {stepDistance:F2} meters");
        SaveData();
    }


    private void UpdateGPSData()
    {
        if (Input.location.status == LocationServiceStatus.Running)
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
                    Debug.Log("GPS data not accurate enough. Skipping this update.");
                }
            }

            // Update GPS coordinates
            gpsLatitude = latitude;
            gpsLongitude = longitude;
        }
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
            totalSteps++;
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
        Debug.Log($"Mean: {mean:F3}, StdDev: {standardDeviation:F3}, DynamicThreshold: {dynamicThreshold:F3}, CurrentAccel: {currentAcceleration:F3}");

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
        PlayerPrefs.SetFloat("TotalDistance", totalDistance);
        PlayerPrefs.SetInt("TotalSteps", totalSteps);
        PlayerPrefs.SetFloat("StepDistance", stepDistance);
        PlayerPrefs.Save();
        Debug.Log("Data Saved");
        Debug.Log("Data Saved - Total Distance: " + totalDistance + ", Total Steps: " + totalSteps + ", Step Distance: " + stepDistance);
    }

    private void LoadData()
    {
        totalDistance = PlayerPrefs.GetFloat("TotalDistance", 0f);
        totalSteps = PlayerPrefs.GetInt("TotalSteps", 0);
        stepDistance = PlayerPrefs.GetFloat("StepDistance", 0f);

        Debug.Log($"Loaded Data - Total Distance: {totalDistance}, Total Steps: {totalSteps}, Step Distance: {stepDistance}");
    }
    private void OnApplicationQuit()
    {
        SaveData(); // Save data when the application is closed
        Debug.Log("Application is quitting");
    }
    private void CheckDailyReset()
    {
        //string lastResetDate = PlayerPrefs.GetString(LastResetDateKey, string.Empty);
        //string currentDate = DateTime.Now.ToString("yyyy-MM-dd"); // Format date as "2024-12-10"

        //if (lastResetDate != currentDate)
        //{
        //    ResetData();
        //    PlayerPrefs.SetString(LastResetDateKey, currentDate); // Update the reset date
        //    PlayerPrefs.Save();
        //}
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (cachedResetDate != currentDate)
        {
            ResetData();
            cachedResetDate = currentDate;
            PlayerPrefs.SetString(LastResetDateKey, currentDate);
            PlayerPrefs.Save();
        }
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteKey("TotalDistance");
        PlayerPrefs.DeleteKey("TotalSteps");
        PlayerPrefs.DeleteKey("StepDistance");
        PlayerPrefs.Save();

        totalDistance = 0f;
        totalSteps = 0;
        stepDistance = 0f;

        Debug.Log("Data Reset for a new day");
    }

}