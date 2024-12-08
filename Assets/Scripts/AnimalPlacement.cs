//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;

//public class AnimalPlacement : MonoBehaviour
//{
//    [SerializeField] private GameObject animalPrefab;
//    private ARPlaneManager planeManager;

//    private void Start()
//    {
//        planeManager = FindObjectOfType<ARPlaneManager>();
//    }

//    public void InstantiateTheAnimal()
//    {
//        Debug.Log("Button clicked!");

//        // Ensure there's at least one detected plane
//        if (planeManager.trackables.count > 0)
//        {
//            // Get the first detected plane
//            ARPlane plane = planeManager.trackables[0]; // Change to use the correct method

//            // You can also iterate through planes if you want to choose a specific one
//            foreach (var detectedPlane in planeManager.trackables)
//            {
//                // Choose the first detected plane's center for simplicity
//                Vector3 animalPosition = detectedPlane.center;

//                GameObject animal = Instantiate(animalPrefab, animalPosition, Quaternion.identity);
//                Debug.Log("Animal instantiated on the ground at: " + animalPosition);
//                return; // Exit after instantiating the first animal
//            }
//        }
//        else
//        {
//            Debug.LogWarning("No ground plane detected. Please move the camera to find a surface.");
//        }
//    }
//}
