using UnityEngine;
using System.Collections;

public class StartPanel : MonoBehaviour
{
    [SerializeField] private GameObject firstTimePanel; // Reference to the UI panel

    void Start()
    {
        // Start a coroutine to deactivate the panel after 2 seconds
        StartCoroutine(DeactivatePanelAfterTime(2f));
    }

    // Coroutine to deactivate the panel after a delay
    private IEnumerator DeactivatePanelAfterTime(float delay)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(delay);

        // Deactivate the panel
        if (firstTimePanel != null)
        {
            firstTimePanel.SetActive(false);
            Debug.Log("Panel deactivated after 2 seconds.");
        }
    }
}
