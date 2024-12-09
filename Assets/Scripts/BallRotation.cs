using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BallRotation : MonoBehaviour
{
    private float rotationSpeed = 100f; // Speed of rotation
    private void Start()
    {
        StartRotating();
    }
    public void StartRotating()
    {
        Debug.Log("BallRotation: Starting rotation..."); // Debug log to verify that the method is being called
        StartCoroutine(RotateBall());
    }

    private IEnumerator RotateBall()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime); // Rotate the ball
            yield return null;
        }
    }
}
