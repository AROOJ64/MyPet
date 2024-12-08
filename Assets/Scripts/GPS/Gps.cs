using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;

public class GPS : MonoBehaviour
{
    public TextMeshProUGUI gpsOut;        // Displays GPS coordinates
    public TextMeshProUGUI distanceOut;    // Displays total distance
    public bool isUpdating;

    private float lastLatitude;
    private float lastLongitude;
    private float totalDistance = 0f;
    private const float MovementThreshold = 5f; // Minimum movement in meters to register
    private Vector3 lastPosition; // Store last known position for smoother movement

    private void Start()
    {
        // Request location permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }

        // Initialize last position
        lastPosition = Vector3.zero;
    }

    private void Update()
    {
        if (!isUpdating)
        {
            StartCoroutine(GetLocation());
            isUpdating = true;
        }
    }

    IEnumerator GetLocation()
    {
        // Check if location service is enabled by the user
        if (!Input.location.isEnabledByUser)
        {
            gpsOut.text = "Location service is not enabled by the user.";
            yield return new WaitForSeconds(10);
            yield break;
        }

        // Start the location service
        Input.location.Start();

        // Wait for the location service to initialize
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in time
        if (maxWait < 1)
        {
            gpsOut.text = "Location service timed out.";
            yield break;
        }

        // If the service fails
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            gpsOut.text = "Unable to determine device location.";
            yield break;
        }

        // Successfully retrieved location data
        LocationInfo currentLocation = Input.location.lastData;

        // Check horizontal accuracy
        if (currentLocation.horizontalAccuracy > 10f) // Ignore data with > 10 meters of inaccuracy
        {
            gpsOut.text = "GPS signal is inaccurate.";
            isUpdating = false;
            Input.location.Stop();
            yield break;
        }

        float currentLatitude = currentLocation.latitude;
        float currentLongitude = currentLocation.longitude;

        // Calculate distance using the Haversine formula
        float distance = CalculateDistance(lastLatitude, currentLatitude, lastLongitude, currentLongitude);

        // Only add to total distance if movement exceeds the threshold
        if (distance > MovementThreshold)
        {
            totalDistance += distance;
            distanceOut.text = $"Total Distance: {totalDistance:F2} meters";
        }

        // Update last known position and latitude/longitude
        lastLatitude = currentLatitude;
        lastLongitude = currentLongitude;

        gpsOut.text = $"Location: {currentLatitude:F6}, {currentLongitude:F6}";

        // Stop the location service
        Input.location.Stop();
        isUpdating = false;
    }

    // Helper function to convert GPS coordinates to Unity world coordinates
    Vector3 GPStoUnityWorld(float latitude, float longitude)
    {
        // Adjust this based on your Unity world setup
        float worldScale = 100f; // Scale factor for world coordinates
        float worldOffset = 1000f; // Offset for world coordinates

        float x = longitude * worldScale + worldOffset;
        float z = latitude * worldScale + worldOffset;

        return new Vector3(x, 0, z);
    }

    // Method to calculate the distance using Haversine formula
    private float CalculateDistance(float lat_1, float lat_2, float long_1, float long_2)
    {
        const int R = 6371; // Earth's radius in kilometers
        var lat_rad_1 = Mathf.Deg2Rad * lat_1;
        var lat_rad_2 = Mathf.Deg2Rad * lat_2;
        var d_lat_rad = Mathf.Deg2Rad * (lat_2 - lat_1);
        var d_long_rad = Mathf.Deg2Rad * (long_2 - long_1);

        // Haversine formula
        var a = Mathf.Pow(Mathf.Sin(d_lat_rad / 2), 2) + Mathf.Cos(lat_rad_1) * Mathf.Cos(lat_rad_2) * Mathf.Pow(Mathf.Sin(d_long_rad / 2), 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        // Convert to meters (multiply by 1000)
        var total_dist = R * c * 1000;

        return total_dist;
    }
}
