using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class StackVisual : MonoBehaviour
{
    [Header("Settings")]
    public int stackIndex;
    public float bowlHeight = 5f;

    [Header("Prefabs and Materials")]
    public GameObject bowlPrefab;
    public Material redMaterial;
    public Material blueMaterial;

    [Header("Selection Indicator")]
    public GameObject selectionIndicator;

    public List<GameObject> bowlObjects = new List<GameObject>();

    public TextMeshPro stackLabel;

    void Start()
    {
        stackLabel.text = "Stack " + (stackIndex + 1);
    }
    void Update()
    {
        stackLabel.transform.LookAt(Camera.main.transform);
    }

    public void RenderStack(GameStack stack)
    {
        foreach (GameObject bowl in bowlObjects)
            Destroy(bowl);
        bowlObjects.Clear();

        for (int i = 0; i < stack.plates.Count; i++)
        {
            Vector3 localPos = new Vector3(0, i * bowlHeight, 0);
            GameObject bowlObj = Instantiate(bowlPrefab, transform);
            bowlObj.transform.localPosition = localPos;
            bowlObj.transform.localRotation = Quaternion.identity;

            Bowl bowl = bowlObj.GetComponent<Bowl>();
            if (bowl != null)
                bowl.SetPlate(stack.plates[i]);

            bowlObjects.Add(bowlObj);
        }
    }

    public Color highlightColor = Color.yellow;
    private Color originalTopColor;
    private Renderer topBowlRenderer;
    private Renderer currentRenderer;
    private Color originalColor;

    public void SetSelected(bool selected)
    {
        if (bowlObjects.Count == 0) return;

        // Reset previous highlight
        if (currentRenderer != null)
        {
            currentRenderer.material.color = originalColor;
            currentRenderer = null;
        }

        if (selected)
        {
            GameObject topBowl = bowlObjects[bowlObjects.Count - 1];
            Renderer renderer = topBowl.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                currentRenderer = renderer;
                originalColor = renderer.material.color;

                // Brighten instead of replacing color
                renderer.material.color = originalColor * 4f;
            }
        }
    }

    // Animate top bowl moving to another stack
    public IEnumerator AnimateMoveTo(StackVisual targetStack,
      int targetHeight, float duration = 0.5f)
    {
        if (bowlObjects.Count == 0) yield break;

        GameObject originalBowl = bowlObjects[bowlObjects.Count - 1];
        originalBowl.SetActive(false);

        GameObject movingBowl = Instantiate(
            originalBowl,
            originalBowl.transform.position,
            originalBowl.transform.rotation
        );
        movingBowl.SetActive(true);
        movingBowl.transform.localScale = originalBowl.transform.lossyScale;

        Vector3 startPos = movingBowl.transform.position;
        Vector3 endPos = GetWorldPositionForSlot(targetStack,
            targetHeight - 1);

        // Find highest point across ALL stacks to ensure bowl
        // clears everything in its path
        float highestPoint = Mathf.Max(startPos.y, endPos.y);

        // Check all stacks for obstacles
        GameController gameController =
            FindObjectOfType<GameController>();
        if (gameController != null)
        {
            foreach (StackVisual sv in gameController.stackVisuals)
            {
                if (sv.bowlObjects.Count > 0)
                {
                    // Get world position of top bowl of each stack
                    GameObject topBowl =
                        sv.bowlObjects[sv.bowlObjects.Count - 1];
                    float topHeight = topBowl.transform.position.y;
                    if (topHeight > highestPoint)
                        highestPoint = topHeight;
                }
            }
        }

        // Arc must go above the highest point plus a clearance buffer
        float clearance = 0.2f;
        float arcPeak = highestPoint + clearance;

        Vector3 midPoint = (startPos + endPos) / 2f;
        Vector3 controlPoint = new Vector3(
            midPoint.x,
            arcPeak,
            midPoint.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            float oneMinusT = 1f - t;
            movingBowl.transform.position =
                (oneMinusT * oneMinusT * startPos) +
                (2f * oneMinusT * t * controlPoint) +
                (t * t * endPos);

            yield return null;
        }

        _pendingMovingBowl = movingBowl;
    }

    private Vector3 GetWorldPositionForSlot(StackVisual targetStack, int slotIndex)
    {
        // If the target stack already has bowls, use them as a reference
        // for the real world-space step size — no scale assumptions needed
        if (targetStack.bowlObjects.Count >= 2)
        {
            // Measure the actual world-space gap between two real bowls
            Vector3 bottom = targetStack.bowlObjects[0].transform.position;
            Vector3 second = targetStack.bowlObjects[1].transform.position;
            Vector3 step = second - bottom;
            return bottom + step * slotIndex;
        }
        else if (targetStack.bowlObjects.Count == 1)
        {
            // One bowl exists — land just above it using this stack's own
            // bowl as a reference for step size instead
            Vector3 referenceStep = GetOwnWorldStep();
            return targetStack.bowlObjects[0].transform.position
                + referenceStep * slotIndex;
        }
        else
        {
            // Target stack is empty — use this stack's step size
            // and land at the base of the target stack
            Vector3 referenceStep = GetOwnWorldStep();
            return targetStack.transform.position + referenceStep * slotIndex;
        }
    }

    private Vector3 GetOwnWorldStep()
    {
        // Get real world-space bowl spacing from this stack
        if (bowlObjects.Count >= 2)
        {
            return bowlObjects[1].transform.position -
                   bowlObjects[0].transform.position;
        }
        // Fallback: use bowlHeight scaled by this transform
        return new Vector3(0, bowlHeight * transform.lossyScale.y, 0);
    }

    [HideInInspector] public GameObject _pendingMovingBowl;

    public int GetBowlCount()
    {
        return bowlObjects.Count;
    }
}