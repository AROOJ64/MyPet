using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacement : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> animalPrefabs;

    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject ballPrefab;

    private GameObject spawnedAnimal;
    private Animator animalAnimator;
    private bool isObjectPlaced = false;
    private int selectedAnimalIndex = 0;

    // Camera movement tracking
    private Vector3 lastCameraPosition;

    private float movementThreshold = 0.05f; // Minimum movement distance to trigger walking animation
    private bool isMoving = false;
    private float movementSmoothingFactor = 0.1f; // How much smoothing to apply

    private void Start()
    {
        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager is not assigned in the inspector.");
        }
        if (animalPrefabs == null || animalPrefabs.Count == 0)
        {
            Debug.LogError("Animal prefabs list is empty or not assigned.");
        }

        lastCameraPosition = Camera.main.transform.position; // Initialize the camera position
    }

    private void Update()
    {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            Vector2 touchPosition;

            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
            }
            else
            {
                touchPosition = Input.mousePosition;
            }

            PerformRaycast(touchPosition);
        }

        // Track camera movement with smoothing
        Vector3 currentCameraPosition = Camera.main.transform.position;
        float distanceMoved = Vector3.Distance(currentCameraPosition, lastCameraPosition);

        if (distanceMoved > movementThreshold)
        {
            isMoving = true; // Camera has moved significantly
            lastCameraPosition = Vector3.Lerp(lastCameraPosition, currentCameraPosition, movementSmoothingFactor);

            // Trigger walking animation if the camera has moved
            PlayWalkingAnimation();
            MoveAnimal();
        }
        else
        {
            // Smoothly transition to idle state
            if (isMoving)
            {
                StartCoroutine(SmoothlyTransitionToIdle());
            }
        }
    }

    private void PerformRaycast(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new();

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            //Debug.Log($"Hit detected at: {hitPose.position}");

            if (!isObjectPlaced && animalPrefabs[selectedAnimalIndex] != null)
            {
                ARInstantiate(hitPose.position, hitPose.rotation);
            }
            else
            {
                //Debug.Log("Object already placed or prefab is null.");
            }
        }
        else
        {
            //Debug.Log("No plane detected at touch position.");
        }
    }

    private void ARInstantiate(Vector3 position, Quaternion rotation)
    {
        spawnedAnimal = Instantiate(animalPrefabs[selectedAnimalIndex], position, rotation);
        //Debug.Log("Animal instantiated at: " + position);

        animalAnimator = spawnedAnimal.GetComponent<Animator>();
        if (animalAnimator == null)
        {
            //Debug.LogWarning("Animator not found on the spawned animal.");
        }

        isObjectPlaced = true;
    }

    public void SelectAnimal(int index)
    {
        if (index >= 0 && index < animalPrefabs.Count)
        {
            selectedAnimalIndex = index;
            //Debug.Log("Animal selected: " + animalPrefabs[index].name);
        }
        else
        {
            //Debug.LogWarning("Selected animal index is out of range.");
        }
    }

    public void PlaceFood()
    {
        if (spawnedAnimal != null && foodPrefab != null)
        {
            GameObject food = Instantiate(foodPrefab, spawnedAnimal.transform.position + Vector3.forward, Quaternion.identity);
            //Debug.Log("Food placed in front of the animal.");

            // Destroy the food after 2 seconds
            Destroy(food, 2f);

            PlayRunningAnimation();
        }
        else
        {
            //Debug.LogWarning("Cannot place food; either the animal or foodPrefab is missing.");
        }
    }

    public void ThrowBall()
    {
        if (spawnedAnimal != null && ballPrefab != null)
        {
            GameObject ball = Instantiate(ballPrefab, spawnedAnimal.transform.position + Vector3.forward * 2, Quaternion.identity);
            //Debug.Log("Ball thrown in front of the animal.");

            // Destroy the ball after 2 seconds
            Destroy(ball, 2f);

            PlayRunningAnimation();
        }
        else
        {
            //Debug.LogWarning("Cannot throw ball; either the animal or ballPrefab is missing.");
        }
    }

    private void PlayWalkingAnimation()
    {
        if (animalAnimator != null && isMoving)
        {
            animalAnimator.SetBool("isWalking", true);
            //Debug.Log("Walking animation triggered.");
        }
        else
        {
            //Debug.LogWarning("No Animator found to play the walking animation.");
        }
    }

    private IEnumerator SmoothlyTransitionToIdle()
    {
        isMoving = false;
        float idleTransitionTime = 0.5f; // Adjust this value for smoother transitions
        float elapsedTime = 0f;

        while (elapsedTime < idleTransitionTime)
        {
            // Gradually reduce the walking animation speed
            if (animalAnimator != null)
                animalAnimator.speed = Mathf.Lerp(1f, 0f, elapsedTime / idleTransitionTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finally, stop walking and trigger the idle animation
        if (animalAnimator != null)
        {
            animalAnimator.SetBool("isWalking", false);
            animalAnimator.speed = 1f; // Reset animation speed
            animalAnimator.SetTrigger("isIdle");

            //Debug.Log("Walking animation stopped and transitioned to idle.");
        }
    }

    private void MoveAnimal()
    {
        if (spawnedAnimal != null && isMoving)
        {
            // Move the animal along with the camera movement or any desired movement logic.
            spawnedAnimal.transform.position += Camera.main.transform.forward * Time.deltaTime;
        }
    }

    private void PlayRunningAnimation()
    {
        if (animalAnimator != null)
        {
            animalAnimator.SetTrigger("isRunning");
            //Debug.Log("Running animation triggered.");

            // Reset trigger after the animation is expected to finish
            StartCoroutine(ResetRunningAnimation());
        }
        else
        {
            //Debug.LogWarning("No Animator found to play the running animation.");
        }
    }

    private IEnumerator ResetRunningAnimation()
    {
        // Wait for the animation to finish (adjust the time based on your animation length)
        yield return new WaitForSeconds(1.0f); // Replace 1.0f with the actual animation length
        if (animalAnimator != null)
        {
            animalAnimator.ResetTrigger("isRunning");
            //Debug.Log("Running animation reset.");
        }
    }
}