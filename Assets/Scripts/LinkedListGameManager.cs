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

    private bool hasWon = false;
    public int[] targetOrder { get; private set; }
    public int[] startOrder { get; private set; }

    [Header("Moves")]
    public int moveCount = 0;
    private int targetLength;
    private bool hasMoveToUndo = false;

    // Store previous state for undo
    private Node undoFromNode = null;
    private Node undoPreviousNext = null;
    private Node undoPreviousTempTarget = null;

    // The locomotive — fixed, never moves, never counted in order
    public Node locomotiveNode { get; private set; }

    // All carriages (excludes locomotive)
    private List<Node> allCarriages = new List<Node>();

    // Fixed world position of locomotive
    private Vector3 locomotiveWorldPos;

    public void SetupInitialList()
    {
        hasMoveToUndo = false;
        if (uiManager != null && uiManager.winPanel != null)
        {
            uiManager.winPanel.SetActive(false);
            uiManager.SetStateDrag();
           // uiManager.ShowInstructions();
        }

        foreach (Node n in allCarriages)
            if (n != null) Destroy(n.gameObject);

        allCarriages.Clear();

        if (locomotiveNode != null)
            Destroy(locomotiveNode.gameObject);

        hasWon = false;
        moveCount = 0;
        undoFromNode = null;
        undoPreviousNext = null;
        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);

        // Always spawn nodeCount carriages
        // Target is either all of them or one less
        // 70% chance of 3 carriages, 30% chance of 4
        targetLength = Random.value < 0.5f ? nodeCount - 1 : nodeCount;

        // Target is a subset of nodeCount values
        // e.g. if nodeCount=4 and targetLength=3, 
        // pick 3 values from [1,2,3,4]
        targetOrder = GenerateSubsetOrder(nodeCount, targetLength);

        // Start always has ALL nodeCount carriages
        startOrder = GenerateRandomOrder(nodeCount);

        SpawnLocomotive();

        TempPointer tp = FindObjectOfType<TempPointer>();
        if (tp != null)
            tp.PlaceAtStart(locomotiveWorldPos);

        SpawnCarriages(startOrder);

        if (uiManager != null)
            ShowTargetOrder();

        StartCoroutine(SetupAfterSpawn());
    }

    // Pick targetLength values randomly from a pool of poolSize
    // e.g. pool=[1,2,3,4], pick 3 -> [2,4,1]
    private int[] GenerateSubsetOrder(int poolSize, int targetLength)
    {
        List<int> pool = new List<int>();
        for (int i = 1; i <= poolSize; i++)
            pool.Add(i);

        Shuffle(pool);

        // Take only targetLength values from the shuffled pool
        int[] result = new int[targetLength];
        for (int i = 0; i < targetLength; i++)
            result[i] = pool[i];

        return result;
    }

    private void Update()
    {
        if (locomotiveNode != null)
            locomotiveNode.transform.position = locomotiveWorldPos;
    }

    private System.Collections.IEnumerator SetupAfterSpawn()
    {
        yield return null;        // frame 1 — nodes Awake() done
        ConnectInitialChain();    // set next references
        yield return null;        // frame 2 — positions settled
        yield return null;        // frame 3 — extra safety

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

        Node tempNode = TempPointer.Instance?.pointingAt;

        // Protect temp node AND everything reachable from it
        if (tempNode != null)
        {
            Node t = tempNode;
            HashSet<Node> tempVisited = new HashSet<Node>();
            while (t != null && !tempVisited.Contains(t))
            {
                tempVisited.Add(t);
                t.SetReachable(true);
                t = t.next;
            }
        }

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

        // Check for newly garbage collected nodes
        bool anyFaded = false;
        foreach (Node n in allCarriages)
        {
            if (!n.isReachable && !n.isGarbageCollected)
            {
                n.FadeToGarbage(0.4f);
                anyFaded = true;
                if (uiManager != null)
                    uiManager.ShowFeedback(
                        n.GetColorName() + " carriage lost!", Color.red);
            }
        }

        if (anyFaded)
        {
            // Wait for fade to finish then slide
            StartCoroutine(SlideAfterDelay(0.45f, logicalOrder, tempNode));
        }
        else
        {
            // No fade — slide immediately
            SlideNodes(logicalOrder, tempNode);
            CheckWin();
        }
    }

    // Extract sliding into its own method so both paths can use it
    private void SlideNodes(List<Node> logicalOrder, Node tempNode)
    {
        int lineIndex = 1;
        for (int i = 0; i < logicalOrder.Count; i++)
        {
            if (logicalOrder[i] == tempNode) continue;
            if (logicalOrder[i] == null) continue;
            if (logicalOrder[i].isGarbageCollected) continue;

            Vector3 localTarget = new Vector3(lineIndex * nodeSpacing, 0, 0);
            Vector3 worldTarget = transform.TransformPoint(localTarget);
            logicalOrder[i].SlideToPosition(worldTarget, slideSpeed);
            lineIndex++;
        }

        if (tempNode != null)
        {
            Vector3 localTempPos = new Vector3(0f, 0f, -nodeSpacing);
            Vector3 worldTempPos = transform.TransformPoint(localTempPos);
            tempNode.SlideToPosition(worldTempPos, slideSpeed);

            Node after = tempNode.next;
            int chainIndex = 1;
            HashSet<Node> afterVisited = new HashSet<Node>();
            while (after != null && !afterVisited.Contains(after))
            {
                afterVisited.Add(after);
                Vector3 localChain = new Vector3(
                    chainIndex * nodeSpacing, 0f, -nodeSpacing);
                Vector3 worldChain = transform.TransformPoint(localChain);
                after.SlideToPosition(worldChain, slideSpeed);
                chainIndex++;
                after = after.next;
            }
        }
    }

    private IEnumerator SlideAfterDelay(float delay,
        List<Node> logicalOrder, Node tempNode)
    {
        yield return new WaitForSeconds(delay);
        SlideNodes(logicalOrder, tempNode);
        CheckWin();
    }

    private IEnumerator UpdateReachabilityDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateReachability();
    }

    public bool CheckWin()
    {
        if (hasWon) return true;
        if (targetOrder == null) return false;

        Node current = locomotiveNode?.next;

        for (int i = 0; i < targetOrder.Length; i++)
        {
            if (current == null) return false;
            if (current.value != targetOrder[i]) return false;
            current = current.next;
        }

        // Removed the "if (current != null) return false" check
        // Player wins as long as the first N nodes are correct
        // regardless of dangling references after

        hasWon = true;
        if (uiManager != null)
            uiManager.SetStateComplete(moveCount);
        

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
            string hex = GetColorHex(v);
            target += $"<color={hex}>O -> ";
        }
        target = target.TrimEnd(' ', '-', '>').TrimEnd();

        // Add hint if a carriage needs to be removed
        if (targetLength < nodeCount)
            target += "\n<color=#FF6600> Remove one carriage!</color>";

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



    // GenerateRandomOrder already handles this
    private int[] GenerateRandomOrder(int count)
    {
        List<int> values = new List<int>();
        for (int i = 1; i <= count; i++)
            values.Add(i);
        Shuffle(values);
        return values.ToArray();
    }

    //private int[] GenerateDifferentOrder(int[] existing, int count)
    //{
    //    int[] candidate;
    //    int attempts = 0;

    //    do
    //    {
    //        candidate = GenerateRandomOrder(count);
    //        attempts++;
    //        if (attempts > 100) break;
    //    }
    //    while (ArraysEqual(candidate, existing));

    //    return candidate;
    //}

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
    public void RecordMove(Node fromNode, Node previousNext, Node previousTempTarget = null)
    {
        hasMoveToUndo = true;
        // Permanently destroy any garbage collected nodes
        // since we're making a new move and undo won't save them
        List<Node> toDestroy = new List<Node>();
        foreach (Node n in allCarriages)
        {
            if (n.isGarbageCollected)
                toDestroy.Add(n);
        }
        foreach (Node n in toDestroy)
        {
            allCarriages.Remove(n);
            Destroy(n.gameObject);
        }

        moveCount++;
        undoFromNode = fromNode;
        undoPreviousNext = previousNext;
        undoPreviousTempTarget = previousTempTarget;


        // Auto minimize instructions after first move
        if (moveCount == 1 && uiManager != null)
            uiManager.MinimizeInstructions();


        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);
    }

    public void UndoLastMove()
    {
        // Allow undo if either a connection or temp move was recorded
        if (!hasMoveToUndo) return;

      

        moveCount++;

        // Only restore connection if there was one
        if (undoFromNode != null)
            undoFromNode.SetNext(undoPreviousNext);

        // Restore temp pointer
        if (TempPointer.Instance != null)
            TempPointer.Instance.PointAt(undoPreviousTempTarget);

        foreach (Node n in allCarriages)
        {
            if (n.isGarbageCollected)
            {
                n.isGarbageCollected = false;
                n.RestoreFromGarbage();
            }
        }

        undoFromNode = null;
        undoPreviousNext = null;
        undoPreviousTempTarget = null;

        StartCoroutine(UpdateReachabilityDelayed(0.25f));
        hasMoveToUndo = false;

        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);
    }

}