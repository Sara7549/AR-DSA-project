using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinkedListGameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject[] carriagePrefabs;

    [Header("Layout")]
    public float nodeSpacing = 0.3f;
    public float slideSpeed = 5f;
    public int nodeCount = 4;

    [Header("UI")]
    public LinkedListUIManager uiManager;


    public int[] targetOrder { get; private set; }
    public int[] startOrder { get; private set; }

    // The locomotive — fixed, never moves, never counted in order
    public Node locomotiveNode { get; private set; }

    // All carriages (excludes locomotive)
    private List<Node> allCarriages = new List<Node>();

    // Fixed world position of locomotive
    private Vector3 locomotiveWorldPos;

    public void SetupInitialList()
    {
        foreach (Node n in allCarriages)
            if (n != null) Destroy(n.gameObject);

        allCarriages.Clear();

        if (locomotiveNode != null)
            Destroy(locomotiveNode.gameObject);

        targetOrder = GenerateRandomOrder(nodeCount);
        startOrder = GenerateDifferentOrder(targetOrder);

        SpawnLocomotive();
        // Place temp pointer above locomotive at start
        TempPointer tp = FindObjectOfType<TempPointer>();
        if (tp != null)
            tp.PlaceAtStart(locomotiveWorldPos);
        SpawnCarriages(startOrder);

        if (uiManager != null)
            ShowTargetOrder();

        StartCoroutine(SetupAfterSpawn());
    }

    private System.Collections.IEnumerator SetupAfterSpawn()
    {
        yield return null;        // frame 1 — nodes Awake() done
        ConnectInitialChain();    // set next references
        yield return null;        // frame 2 — positions settled
        yield return null;        // frame 3 — extra safety

        // Now update arrows after everything is in place
        locomotiveNode?.UpdatePointerVisual();
        foreach (Node node in allCarriages)
            node.UpdatePointerVisual();
    }

    private void SpawnLocomotive()
    {
        locomotiveWorldPos = transform.TransformPoint(Vector3.zero);

        GameObject obj = Instantiate(headPrefab,
     locomotiveWorldPos,
     transform.rotation * Quaternion.Euler(0, -90, 0), transform);

        obj.name = "Locomotive";

        locomotiveNode = obj.GetComponent<Node>();
        if (locomotiveNode == null)
            locomotiveNode = obj.GetComponentInChildren<Node>();

        locomotiveNode.value = 0;

        EnsureCollider(obj);
    }

    private void SpawnCarriages(int[] order)
    {
        for (int i = 0; i < order.Length; i++)
        {
            Vector3 localPos = new Vector3((i + 1) * nodeSpacing, 0f, 0f);

            Vector3 worldPos = transform.TransformPoint(localPos);

            int colorIndex = (order[i] - 1) % carriagePrefabs.Length;
            GameObject prefabToUse = carriagePrefabs[colorIndex];

            GameObject obj = Instantiate(prefabToUse,
      worldPos, transform.rotation * Quaternion.Euler(0, -90, 0), transform);

            Node node = obj.GetComponent<Node>();
            if (node == null)
                node = obj.GetComponentInChildren<Node>();

            if (node == null)
            {
                Debug.LogError("No Node on " + prefabToUse.name);
                continue;
            }

            node.value = order[i];
            obj.name = "Carriage_" + order[i];

            EnsureCollider(obj);
            allCarriages.Add(node);
        }
    }

    private void ConnectInitialChain()
    {
        if (locomotiveNode != null && allCarriages.Count > 0)
            locomotiveNode.SetNext(allCarriages[0]);

        for (int i = 0; i < allCarriages.Count - 1; i++)
            allCarriages[i].SetNext(allCarriages[i + 1]);

        if (allCarriages.Count > 0)
            allCarriages[allCarriages.Count - 1].SetNext(null);
    }

    public void UpdateReachability()
    {
        foreach (Node n in allCarriages) n.SetReachable(false);

        // Protect temp node from garbage collection
        Node tempNode = TempPointer.Instance?.pointingAt;
        if (tempNode != null)
            tempNode.SetReachable(true);

        HashSet<Node> visited = new HashSet<Node>();
        List<Node> logicalOrder = new List<Node>();
        Node current = locomotiveNode.next;

        while (current != null && !visited.Contains(current))
        {
            visited.Add(current);
            current.SetReachable(true);
            logicalOrder.Add(current);
            current = current.next;
        }

        // Slide reachable into line
        for (int i = 0; i < logicalOrder.Count; i++)
        {
            Vector3 localTarget = new Vector3((i + 1) * nodeSpacing, 0, 0);
            Vector3 worldTarget = transform.TransformPoint(localTarget);
            logicalOrder[i].SlideToPosition(worldTarget, slideSpeed);
        }

        // Garbage — but skip temp node
        int garbageIndex = 0;
        foreach (Node n in allCarriages)
        {
            if (!n.isReachable)
            {
                Vector3 localGarbage = new Vector3(
                    0f, 0f, (garbageIndex + 1) * nodeSpacing * 2f);
                Vector3 worldGarbage = transform.TransformPoint(localGarbage);
                n.SlideToPosition(worldGarbage, slideSpeed);
                garbageIndex++;
            }
            else if (n == tempNode)
            {
                // Keep temp node in a fixed visible spot
                Vector3 localTempPos = new Vector3(0f, 0f, -nodeSpacing * 2f);
                Vector3 worldTempPos = transform.TransformPoint(localTempPos);
                n.SlideToPosition(worldTempPos, slideSpeed);
            }
        }

        foreach (Node n in allCarriages) n.UpdatePointerVisual();
        locomotiveNode.UpdatePointerVisual();

        CheckWin();
    }

    public bool CheckWin()
    {
        if (targetOrder == null) return false;

        Node current = locomotiveNode?.next;

        for (int i = 0; i < targetOrder.Length; i++)
        {
            if (current == null) return false;
            if (current.value != targetOrder[i]) return false;

            current = current.next;
        }

        if (current != null) return false;

        if (uiManager != null)
            uiManager.SetStateComplete();

        return true;
    }

    public List<Node> GetAllCarriages() => allCarriages;
    public Node GetLocomotive() => locomotiveNode;

    private void ShowTargetOrder()
    {
        if (uiManager == null) return;

        string target = "Target: ";
        foreach (int v in targetOrder)
        {
            // Each number maps to a carriage color
            // Use rich text colored squares
            string hex = GetColorHex(v);
            target += $"<color={hex}> O  -> ";
        }
        target = target.TrimEnd(' ', '-', '>');
        uiManager.ShowTargetOrder(target);
    }

    private string GetColorHex(int value)
    {
        // Match these to your actual carriagePrefabs colors
        int colorIndex = (value - 1) % carriagePrefabs.Length;
        switch (colorIndex)
        {
            case 0: return "#0000FF"; // blue
            case 1: return "#00FF00"; // green
            case 2: return "#FF0000"; // red
            case 3: return "#FFFF00"; // yellow
            default: return "#FFFFFF"; // white
        }
    }

    

    private int[] GenerateRandomOrder(int count)
    {
        List<int> values = new List<int>();

        for (int i = 1; i <= count; i++)
            values.Add(i);

        Shuffle(values);

        return values.ToArray();
    }

    private int[] GenerateDifferentOrder(int[] existing)
    {
        int[] candidate;
        int attempts = 0;

        do
        {
            candidate = GenerateRandomOrder(existing.Length);
            attempts++;

            if (attempts > 100) break;
        }
        while (ArraysEqual(candidate, existing));

        return candidate;
    }

    private void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponentInChildren<Collider>() != null)
            return;

        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;

        foreach (Renderer r in rends)
            b.Encapsulate(r.bounds);

        BoxCollider col = obj.AddComponent<BoxCollider>();

        col.center = obj.transform.InverseTransformPoint(b.center);

        col.size = new Vector3(
            Mathf.Abs(obj.transform.InverseTransformVector(new Vector3(b.size.x, 0, 0)).x),
            Mathf.Abs(obj.transform.InverseTransformVector(new Vector3(0, b.size.y, 0)).y),
            Mathf.Abs(obj.transform.InverseTransformVector(new Vector3(0, 0, b.size.z)).z)
        );
    }

    private bool ArraysEqual(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;

        return true;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}