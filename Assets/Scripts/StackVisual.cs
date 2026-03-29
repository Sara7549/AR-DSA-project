using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private List<GameObject> bowlObjects = new List<GameObject>();

    public void RenderStack(GameStack stack)
    {
        // Clear existing bowl objects
        foreach (GameObject bowl in bowlObjects)
            Destroy(bowl);
        bowlObjects.Clear();

        // Spawn bowls for each plate in stack
        for (int i = 0; i < stack.plates.Count; i++)
        {
            Vector3 localPos = new Vector3(0, i * bowlHeight, 0);
            GameObject bowlObj = Instantiate(bowlPrefab,
                transform);
            bowlObj.transform.localPosition = localPos;
            bowlObj.transform.localRotation = Quaternion.identity;

            // Set plate data
            Bowl bowl = bowlObj.GetComponent<Bowl>();
            if (bowl != null)
            {
                bowl.redMaterial = redMaterial;
                bowl.blueMaterial = blueMaterial;
                bowl.SetPlate(stack.plates[i]);
            }

            bowlObjects.Add(bowlObj);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }

    // Animate top bowl moving to another stack
    public IEnumerator AnimateMoveTo(StackVisual targetStack,
        float duration = 0.5f)
    {
        if (bowlObjects.Count == 0) yield break;

        GameObject topBowl = bowlObjects[bowlObjects.Count - 1];
        Vector3 startPos = topBowl.transform.position;

        // Arc up then across then down
        Vector3 midPos = new Vector3(
            (startPos.x + targetStack.transform.position.x) / 2f,
            startPos.y + 0.2f,
            (startPos.z + targetStack.transform.position.z) / 2f);

        Vector3 endPos = targetStack.transform.position +
            new Vector3(0,
                targetStack.GetBowlCount() * bowlHeight, 0);

        // Move up to mid
        float elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            topBowl.transform.position =
                Vector3.Lerp(startPos, midPos, t);
            yield return null;
        }

        // Move down to target
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            topBowl.transform.position =
                Vector3.Lerp(midPos, endPos, t);
            yield return null;
        }
    }

    public int GetBowlCount()
    {
        return bowlObjects.Count;
    }
}