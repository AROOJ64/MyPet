//using System.Collections;
//using UnityEngine;
//using UnityEngine.Android;

//public class GPSDistanceTracker : MonoBehaviour
//{
//    private float totalDistance = 0f;
//    private Vector2 previousPosition;
//    private bool isPermissionGranted = false;

//    IEnumerator Start()
//    {
//        // Check and request location permissions for Android
//        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
//        {
//            Permission.RequestUserPermission(Permission.FineLocation);

//            // Wait for the user to grant or deny the permission
//            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
//        }

//        isPermissionGranted = Permission.HasUserAuthorizedPermission(Permission.FineLocation);

//        if (!isPermissionGranted)
//        {
//            Debug.LogError("Location permission not granted.");
//            yield break;
//        }

//        // Check if location services are enabled
//        if (!Input.location.isEnabledByUser)
//        {
//            Debug.LogError("Location services are not enabled by the user. Prompting them to enable it.");

//            // Show a dialog prompting the user to enable location services
//            ShowEnableLocationDialog();

//            // Wait until location services are enabled
//            yield return new WaitUntil(() => Input.location.isEnabledByUser);
//        }

//        // Start location services
//        Input.location.Start();

//        // Wait for initialization
//        int maxWait = 20;
//        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
//        {
//            yield return new WaitForSeconds(1);
//            maxWait--;
//        }

//        // Check if location service failed
//        if (Input.location.status == LocationServiceStatus.Failed)
//        {
//            Debug.LogError("Unable to determine device location.");
//            yield break;
//        }

//        // Initialize the first position
//        previousPosition = new Vector2(
//            Input.location.lastData.latitude,
//            Input.location.lastData.longitude
//        );

//        // Start tracking
//        StartCoroutine(TrackDistance());
//    }

//    IEnumerator TrackDistance()
//    {
//        while (Input.location.status == LocationServiceStatus.Running)
//        {
//            // Get current position
//            Vector2 currentPosition = new Vector2(
//                Input.location.lastData.latitude,
//                Input.location.lastData.longitude
//            );

//            // Calculate distance and update total
//            float distance = GPSDistanceCalculator.CalculateDistance(
//                previousPosition.x, previousPosition.y,
//                currentPosition.x, currentPosition.y
//            );

//            if (distance > 0.5f) // Ignore minor inaccuracies
//            {
//                totalDistance += distance;
//                Debug.Log($"Moved Distance: {distance} meters. Total: {totalDistance} meters.");
//            }

//            // Update the previous position
//            previousPosition = currentPosition;

//            // Wait before next update
//            yield return new WaitForSeconds(1); // Adjust frequency as needed
//        }
//    }

//    private void ShowEnableLocationDialog()
//    {
//        // Show a message or UI to guide the user
//        Debug.LogWarning("Please enable location services in your device settings.");
//    }

//    private void OnDisable()
//    {
//        // Stop location services to save battery
//        if (Input.location.status == LocationServiceStatus.Running)
//        {
//            Input.location.Stop();
//        }
//    }

//    private void SaveDistance()
//    {
//        PlayerPrefs.SetFloat("TotalDistance", totalDistance);
//        PlayerPrefs.Save();
//        Debug.Log("Distance saved.");
//    }

//    private void LoadDistance()
//    {
//        if (PlayerPrefs.HasKey("TotalDistance"))
//        {
//            totalDistance = PlayerPrefs.GetFloat("TotalDistance");
//            Debug.Log($"Loaded Distance: {totalDistance} meters.");
//        }
//    }
//}