using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class XR_Placement : MonoBehaviour
{
    [SerializeField] private List<GameObject> animalPrefabs; // List of animal prefabs
    private int selectedAnimalIndex = 0; // Index of the currently selected animal

    private Animator animalAnimator;
    private GameObject spawnedAnimal;
    private ARRaycastManager raycastManager;

    private bool isObjectPlaced = false;
    private Vector3 lastCameraPosition;
    private Vector3 targetPosition;
    private float timeSinceLastMove = 0f;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        lastCameraPosition = Camera.main.transform.position;
    }

    void ARRaycasting(Vector2 pos)
    {
        List<ARRaycastHit> hits = new();

        if (raycastManager.Raycast(pos, hits, TrackableType.PlaneEstimated))
        {
            Pose pose = hits[0].pose;
            ARInstantiation(pose.position, pose.rotation);
        }
    }

    void ARInstantiation(Vector3 pos, Quaternion rot)
    {
        if (!isObjectPlaced)
        {
            // Instantiate the selected animal prefab based on the chosen index
            spawnedAnimal = Instantiate(animalPrefabs[selectedAnimalIndex], pos, rot);
            isObjectPlaced = true;

            animalAnimator = spawnedAnimal.GetComponent<Animator>();
            targetPosition = spawnedAnimal.transform.position;
        }
    }

    void MoveAnimalWithCamera()
    {
        if (isObjectPlaced && spawnedAnimal != null)
        {
            Vector3 currentCameraPosition = Camera.main.transform.position;
            Vector3 deltaPosition = currentCameraPosition - lastCameraPosition;

            float movementThreshold = 0.005f;

            if (deltaPosition.magnitude > movementThreshold)
            {
                targetPosition += new Vector3(0, 0, deltaPosition.z);

                if (animalAnimator != null && !animalAnimator.GetBool("isWalking"))
                {
                    animalAnimator.SetBool("isWalking", true);
                }

                timeSinceLastMove = 0f;
            }
            else
            {
                timeSinceLastMove += Time.deltaTime;
                if (timeSinceLastMove > 0.2f && animalAnimator != null && animalAnimator.GetBool("isWalking"))
                {
                    animalAnimator.SetBool("isWalking", false);
                }
            }

            spawnedAnimal.transform.position = Vector3.Lerp(spawnedAnimal.transform.position, targetPosition, Time.deltaTime * 3f);

            lastCameraPosition = currentCameraPosition;
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            ARRaycasting(touchPosition);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            ARRaycasting(mousePos2D);
        }

        MoveAnimalWithCamera();
    }

    // Method to set the selected animal based on the button clicked
    public void SetSelectedAnimal(int index)
    {
        if (isObjectPlaced) return; // Prevent changing animal once placed

        selectedAnimalIndex = index;
    }
}
