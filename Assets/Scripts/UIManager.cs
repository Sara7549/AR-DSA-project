using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    [Header("AR UI")]
    public TextMeshProUGUI instructionText;
    public GameObject reticle;

    private bool isPlaced = false;

    private void Start()
    {
        if (instructionText != null)
            instructionText.text = "Move your phone slowly to detect a surface";
    }

    private void Update()
    {
        if (!isPlaced)
        {
            if (reticle != null && reticle.activeSelf)
                instructionText.text = "Tap to place your stacks here";
            else if (ARSession.state == ARSessionState.SessionTracking)
                instructionText.text = "Move your phone slowly to detect a surface";
            else
                instructionText.text = "";
        }
    }

    public void OnStackPlaced(GameObject stackGroup)
    {
        isPlaced = true;

        // Hide reticle
        if (reticle != null)
            reticle.SetActive(false);

        // Show placed message then hide
        instructionText.text = "Stacks placed! Tap a stack to select it";
        Invoke("HideInstruction", 3f);

        // Play placement animation
        StartCoroutine(PlacementAnimation(stackGroup));
    }

    private void HideInstruction()
    {
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);
    }

    private IEnumerator PlacementAnimation(GameObject stackGroup)
    {
        if (stackGroup == null) yield break;

        Vector3 targetScale = stackGroup.transform.localScale;

        if (targetScale == Vector3.zero)
            targetScale = new Vector3(1f, 1f, 1f);

        stackGroup.transform.localScale = Vector3.zero;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            stackGroup.transform.localScale =
                Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        stackGroup.transform.localScale = targetScale;
    }
}