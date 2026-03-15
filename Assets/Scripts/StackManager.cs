using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class StackManager : MonoBehaviour
{
    public GameObject nodePrefab;
    private Transform stackParent;
    private Stack<GameObject> stack = new Stack<GameObject>();
    float nodeHeight = 30f;
    private int nextValue = 1;
    private Color[] nodeColors = new Color[]
  {
        new Color(0.2f, 0.6f, 1f),    // blue
        new Color(0.2f, 0.9f, 0.4f),  // green
        new Color(1f, 0.8f, 0.2f),    // yellow
        new Color(1f, 0.4f, 0.4f),    // red
        new Color(0.8f, 0.4f, 1f),    // purple
        new Color(1f, 0.6f, 0.2f),    // orange
  };
    IEnumerator MoveNode(GameObject node, Vector3 start, Vector3 target)
    {
        float time = 0f;

        while (time < 1f)
        {
            node.transform.localPosition = Vector3.Lerp(start, target, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }

        node.transform.localPosition = target;
    }
    IEnumerator PopAnimation(GameObject node)
    {
        Vector3 start = node.transform.localPosition;
        Vector3 end = start + new Vector3(0, 100f, 0);

        float time = 0f;

        while (time < 1f)
        {
            node.transform.localPosition = Vector3.Lerp(start, end, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }

        Destroy(node);
    }

    private void Awake()
    {
        stackParent = transform.Find("StackParent");

        if (stackParent == null)
            Debug.LogError("StackParent not found! Check hierarchy name matches exactly.");
        else
            Debug.Log("StackParent found at: " + stackParent.position);
    }

    public void Push()
    {
        if (stackParent == null) return;

        Vector3 localPos = new Vector3(0, stack.Count * nodeHeight, 0);
        GameObject node = Instantiate(nodePrefab, stackParent);
        Vector3 startPos = localPos + new Vector3(0, 100f, 0);
        node.transform.localPosition = startPos;

        StartCoroutine(MoveNode(node, startPos, localPos)); 
        node.transform.localRotation = Quaternion.identity;

        // Assign color based on stack count
        Color color = nodeColors[stack.Count % nodeColors.Length];
        Renderer renderer = node.GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = color;

        // Set label
        TextMeshPro label = node.GetComponentInChildren<TextMeshPro>();
        if (label != null)
            label.text = nextValue.ToString();
        else
            Debug.LogError("Label not found on node prefab");

        nextValue++;
        stack.Push(node);
    }

    public void Pop()
    {
        if (stack.Count == 0) return;
        GameObject node = stack.Pop();
        StartCoroutine(PopAnimation(node));
        nextValue--;
    }
}