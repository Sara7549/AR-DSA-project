using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinkedListGameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject[] carriagePrefabs;
    public static LinkedListGameManager Instance;

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
    private Node lastConnectedNode = null;
    

    // Store previous state for undo
    private Node undoFromNode = null;
    private Node undoPreviousNext = null;
    private Node undoPreviousTempTarget = null;
    private bool lastConnectionWasFromTemp = false;
    private Dictionary<Node, Vector3> slideTargets = new Dictionary<Node, Vector3>();

    // The locomotive — fixed, never moves, never counted in order
    public Node locomotiveNode { get; private set; }

    // All carriages (excludes locomotive)
    private List<Node> allCarriages = new List<Node>();
    private Node mostRecentlyMovedNode = null;
    private bool mostRecentMoveWasToTemp = false;

    private enum HintPhase { Removal, Arrangement }
    private HintPhase currentHintPhase = HintPhase.Removal;

    // Fixed world position of locomotive

    private void Start()
    {
        if (LinkedListStatisticsTracker.Instance != null)
            LinkedListStatisticsTracker.Instance.StartTracking();
    }


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
        lastConnectedNode = null;
        lastConnectionWasFromTemp = false;
        mostRecentlyMovedNode = null;
        mostRecentMoveWasToTemp = false;
        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);

        currentHintPhase = HintPhase.Removal;
        ResetHints();

        // Always spawn nodeCount carriages
        // Target is either all of them or one less
        // 70% chance of 3 carriages, 30% chance of 4
        targetLength = Random.value < 1f ? nodeCount - 1 : nodeCount;

        // Target is a subset of nodeCount values
        // e.g. if nodeCount=4 and targetLength=3, 
        // pick 3 values from [1,2,3,4]
        targetOrder = GenerateSubsetOrder(nodeCount, targetLength);

        // Start always has ALL nodeCount carriages
        startOrder = GenerateRandomOrder(nodeCount);

        SpawnLocomotive();

        TempPointer tp = FindObjectOfType<TempPointer>();
        if (tp != null)
        {
            tp.transform.SetParent(transform, true);
            tp.PlaceAtStart(locomotiveNode.transform.position);
        }

        SpawnCarriages(startOrder);

        if (uiManager != null)
            ShowTargetOrder();
        

        //if (uiManager != null &&  uiManager.hintButton != null)
        //    uiManager.hintButton.onClick.AddListener(OnHintButtonPressed);

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

   

    private System.Collections.IEnumerator SetupAfterSpawn()
    {
        yield return null;        // frame 1 — nodes Awake() done
        ConnectInitialChain();    // set next references
        yield return null;        // frame 2 — positions settled
        yield return null;        // frame 3 — extra safety

    }

    private void SpawnLocomotive()
    {
        GameObject obj = Instantiate(headPrefab, transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(0, -90, 0);

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
            int colorIndex = (order[i] - 1) % carriagePrefabs.Length;
            GameObject prefabToUse = carriagePrefabs[colorIndex];

            GameObject obj = Instantiate(prefabToUse, transform);
            obj.transform.localPosition = new Vector3((i + 1) * nodeSpacing, 0f, 0f);
            obj.transform.localRotation = Quaternion.Euler(0, -90, 0);

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
                if (LinkedListStatisticsTracker.Instance != null)
                    LinkedListStatisticsTracker.Instance
                        .RecordGarbageCollection();
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
            StartCoroutine(SlideAfterDelay(0.45f, logicalOrder, tempNode, visited));
        }
        else
        {
            // No fade — slide immediately
            SlideNodes(logicalOrder, tempNode, visited);
            CheckWin();
        }
    }

    //Extract sliding into its own method so both paths can use it
    //private void SlideNodes(List<Node> logicalOrder,
    //Node tempNode, HashSet<Node> visited)
    //{
    //    int lineIndex = 1;
    //    for (int i = 0; i < logicalOrder.Count; i++)
    //    {
    //        Node n = logicalOrder[i];
    //        if (n == null) continue;
    //        if (n.isGarbageCollected) continue;
    //        if (n == tempNode) continue;
    //        if (n == lastConnectedNode && lastConnectionWasFromTemp) continue;

    //        Vector3 localTarget = new Vector3(lineIndex * nodeSpacing, 0, 0);
    //        Vector3 worldTarget = transform.TransformPoint(localTarget);
    //        n.SlideToPosition(worldTarget, slideSpeed);
    //        lineIndex++;
    //    }

    //    if (tempNode != null)
    //    {
    //        Vector3 localTempPos = new Vector3(0f, 0f, -nodeSpacing);
    //        Vector3 worldTempPos = transform.TransformPoint(localTempPos);
    //        tempNode.SlideToPosition(worldTempPos, slideSpeed);

    //        Node after = tempNode.next;
    //        int chainIndex = 1;
    //        HashSet<Node> afterVisited = new HashSet<Node>();

    //        while (after != null && !afterVisited.Contains(after))
    //        {
    //            afterVisited.Add(after);

    //            // This node was pulled into the main line by the most recent move
    //            // (e.g. head->blue, so blue and everything after it belongs to main)
    //            // Stop sliding to the side — the main line loop already placed it
    //            if (after == lastConnectedNode && !lastConnectionWasFromTemp)
    //                break; // <-- break instead of continue; stops the whole trailing chain


    //            Vector3 localChain = new Vector3(
    //                chainIndex * nodeSpacing, 0f, -nodeSpacing);
    //            Vector3 worldChain = transform.TransformPoint(localChain);
    //            after.SlideToPosition(worldChain, slideSpeed);
    //            chainIndex++;
    //            after = after.next;
    //        }
    //    }
    //}

    private void SlideNodes(List<Node> logicalOrder, Node tempNode, HashSet<Node> visited)
    {
        HashSet<Node> alreadyPlaced = new HashSet<Node>();

        if (mostRecentMoveWasToTemp)
        {
            PlaceTempChain(tempNode, alreadyPlaced);
            PlaceMainChain(alreadyPlaced);
        }
        else
        {
            PlaceMainChain(alreadyPlaced);
            PlaceTempChain(tempNode, alreadyPlaced);
        }
    }

    private void PlaceMainChain(HashSet<Node> alreadyPlaced)
    {
        int slot = 1;
        Node cur = locomotiveNode.next;
        HashSet<Node> visited = new HashSet<Node>();

        while (cur != null && !visited.Contains(cur))
        {
            visited.Add(cur);
            if (!cur.isGarbageCollected && !alreadyPlaced.Contains(cur))
            {
                Vector3 local = new Vector3(slot * nodeSpacing, 0f, 0f);
                cur.SlideToPosition(transform.TransformPoint(local), slideSpeed);
                alreadyPlaced.Add(cur);
                slot++;
            }
            cur = cur.next;
        }
    }

    private void PlaceTempChain(Node tempNode, HashSet<Node> alreadyPlaced)
    {
        if (tempNode == null || tempNode.isGarbageCollected) return;

        // Place temp node itself
        Vector3 localTemp = new Vector3(0f, 0f, -nodeSpacing);
        tempNode.SlideToPosition(transform.TransformPoint(localTemp), slideSpeed);
        alreadyPlaced.Add(tempNode);

        // Place everything after it
        int slot = 1;
        Node after = tempNode.next;
        HashSet<Node> visited = new HashSet<Node>();

        while (after != null && !visited.Contains(after))
        {
            visited.Add(after);
            if (!after.isGarbageCollected && !alreadyPlaced.Contains(after))
            {
                Vector3 local = new Vector3(slot * nodeSpacing, 0f, -nodeSpacing);
                after.SlideToPosition(transform.TransformPoint(local), slideSpeed);
                alreadyPlaced.Add(after);
                slot++;
            }
            after = after.next;
        }
    }

    private IEnumerator SlideAfterDelay(float delay,
    List<Node> logicalOrder, Node tempNode, HashSet<Node> visited)
    {
        yield return new WaitForSeconds(delay);
        SlideNodes(logicalOrder, tempNode, visited);
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

        if (TempPointer.Instance != null)
            TempPointer.Instance.PointAt(null);
        StartCoroutine(SlideToWinPosition());

        // Reset layout flags so main chain gets priority
        mostRecentMoveWasToTemp = false;
        lastConnectedNode = null;
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
    public void RecordMove(Node fromNode, Node previousNext, Node previousTempTarget = null, bool fromTemp = false)
    {
        ResetHints();
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

        // In RecordMove, after move is recorded
        if (FindNodeToRemove() == null)
            currentHintPhase = HintPhase.Arrangement;

        if (LinkedListStatisticsTracker.Instance != null)
            LinkedListStatisticsTracker.Instance.RecordConnection();
        undoFromNode = fromNode;
        undoPreviousNext = previousNext;
        undoPreviousTempTarget = previousTempTarget;


        // This is the key: track which node moved AND where it went
        if (fromNode != null)
        {
            lastConnectedNode = fromNode.next;   // the node that was just connected
            mostRecentlyMovedNode = fromNode.next;
            mostRecentMoveWasToTemp = fromTemp;  // true = node went to temp chain
        }


        // Auto minimize instructions after first move
        if (moveCount == 1 && uiManager != null)
            uiManager.MinimizeInstructions();


        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);

        if (fromNode != null)
        {
            mostRecentlyMovedNode = fromNode.next;
            mostRecentMoveWasToTemp = fromTemp;
        }

    }

    public void UndoLastMove()
    {
        // Allow undo if either a connection or temp move was recorded
        if (!hasMoveToUndo) return;

        if (LinkedListStatisticsTracker.Instance != null)
            LinkedListStatisticsTracker.Instance.RecordUndo();

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
        lastConnectedNode = null;
        lastConnectionWasFromTemp = false;
        mostRecentlyMovedNode = null;
        mostRecentMoveWasToTemp = false;
        if (FindNodeToRemove() != null)
            currentHintPhase = HintPhase.Removal;
        else
            currentHintPhase = HintPhase.Arrangement;
        ResetHints();

        StartCoroutine(UpdateReachabilityDelayed(0.25f));
        hasMoveToUndo = false;

        if (uiManager != null)
            uiManager.UpdateMoveCount(moveCount);
    }
    public void SetTempPriority(bool val)
    {
        mostRecentMoveWasToTemp = val;
    }
    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator SlideToWinPosition()
    {
        yield return new WaitForSeconds(0.1f); // let PointAt(null) settle

        HashSet<Node> placed = new HashSet<Node>();
        PlaceMainChain(placed);
    }

    private int hintLevel = 0;

    public void OnHintButtonPressed()
    {
        hintLevel++;
        if (hintLevel > 3) hintLevel = 3;
        if (LinkedListStatisticsTracker.Instance != null)
            LinkedListStatisticsTracker.Instance.RecordHint(hintLevel);
        ShowHint(hintLevel);
        if (uiManager != null) uiManager.UpdateHintButton(hintLevel);
    }

    public void ResetHints()
    {
        hintLevel = 0;
        if (uiManager != null)
        {
            uiManager.HideHint();
            uiManager.ResetHintButton();
        }
    }

    private void ShowHint(int level)
    {
        if (uiManager == null) return;
        if (NeededNodesAreGone())
        {
            ShowUndoHint(level);
            return;
        }

        if (currentHintPhase == HintPhase.Removal)
            ShowRemovalHint(level);
        else
            ShowArrangementHint(level);
    }

    private void ShowRemovalHint(int level)
    {
        Node nodeToRemove = FindNodeToRemove();
        if (nodeToRemove == null)
        {
            currentHintPhase = HintPhase.Arrangement;
            ShowArrangementHint(level);
            return;
        }

        Node previous = FindPreviousNode(nodeToRemove);
        string removeName = GetColorName(nodeToRemove.value);
        string previousName = previous == locomotiveNode ?
            "head" : GetColorName(previous.value);

        bool isLastNode = nodeToRemove.next == null ||
            !System.Array.Exists(targetOrder, v => v == nodeToRemove.next?.value);

        // nextName is the node after the one being removed
        string nextName = (!isLastNode && nodeToRemove.next != null) ?
            GetColorName(nodeToRemove.next.value) : "";

        // Safe node to point back to when removing last node
        string safeNodeName = GetColorName(targetOrder[0]);
        string safePreviousName = previous == locomotiveNode ?
            "head" : GetColorName(previous.value);

        switch (level)
        {
            case 1:
                uiManager.ShowHint(
                    $"Remove the {removeName} carriage");
                break;
            case 2:
                uiManager.ShowHint(isLastNode
                    ? $"Set {safePreviousName}.next = {safeNodeName} to detach {removeName}"
                    : $"Set {previousName}.next = {nextName} to skip over {removeName}");
                break;
            case 3:
                uiManager.ShowHint(isLastNode
                    ? $"Drag {safePreviousName}'s arrow to the {safeNodeName} carriage to remove {removeName}"
                    : $"Drag {previousName}'s arrow to the {nextName} carriage to skip {removeName}");
                break;
        }
    }

    private Node FindNodeByValue(int value)
    {
        foreach (Node n in allCarriages)
            if (n.value == value) return n;
        return null;
    }

    private string GetColorName(int value)
    {
        int colorIndex = (value - 1) % carriagePrefabs.Length;
        switch (colorIndex)
        {
            case 0: return "blue";
            case 1: return "green";
            case 2: return "red";
            case 3: return "yellow";
            default: return "unknown";
        }
    }

    private bool NeededNodesAreGone()
    {
        foreach (int value in targetOrder)
        {
            Node n = FindNodeByValue(value);
            if (n == null || n.isGarbageCollected) return true;
        }
        return false;
    }

    private bool IsReachableFromHead(Node target)
    {
        Node cur = locomotiveNode?.next;
        HashSet<Node> visited = new HashSet<Node>();
        while (cur != null && !visited.Contains(cur))
        {
            if (cur == target) return true;
            visited.Add(cur);
            cur = cur.next;
        }
        return false;
    }

    private Node FindNodeToRemove()
    {
        // The node whose value doesn't appear in targetOrder
        foreach (Node n in allCarriages)
        {
            if (n.isGarbageCollected) continue;
            bool inTarget = false;
            foreach (int v in targetOrder)
                if (n.value == v) { inTarget = true; break; }
            if (!inTarget) return n;
        }
        return null;
    }

    private Node FindPreviousNode(Node target)
    {
        Node cur = locomotiveNode;
        while (cur?.next != null)
        {
            if (cur.next == target) return cur;
            cur = cur.next;
        }
        return null;
    }

    private void ShowUndoHint(int level)
    {
        switch (level)
        {
            case 1:
                uiManager.ShowHint("A needed carriage was lost!");
                break;
            case 2:
                uiManager.ShowHint("You need to undo to recover it");
                break;
            case 3:
                uiManager.ShowHint("Press the Undo button to get the carriage back");
                break;
        }
    }

    private void ShowArrangementHint(int level)
    {
        if (uiManager == null) return;

        if (NeededNodesAreGone())
        {
            switch (level)
            {
                case 1:
                    uiManager.ShowHint("Some carriages have been lost!");
                    break;
                case 2:
                    uiManager.ShowHint("You need to undo to recover the lost carriages");
                    break;
                case 3:
                    uiManager.ShowHint("Press the Undo button to get back the lost carriages");
                    break;
            }
            return;
        }

        // Find first wrong position
        Node current = locomotiveNode?.next;
        int firstWrongIndex = -1;
        for (int i = 0; i < targetOrder.Length; i++)
        {
            if (current == null || current.value != targetOrder[i])
            {
                firstWrongIndex = i;
                break;
            }
            current = current.next;
        }

        if (firstWrongIndex == -1) return;

        string targetColor = GetColorName(targetOrder[firstWrongIndex]);
        string sourceColor = firstWrongIndex == 0 ? "head" :
            GetColorName(targetOrder[firstWrongIndex - 1]);

        // Check if making this move would cause garbage collection
        // i.e. does head.next exist and is it not the target?
        Node sourceNode = firstWrongIndex == 0 ? locomotiveNode :
            FindNodeByValue(targetOrder[firstWrongIndex - 1]);
        //Node targetNode = FindNodeByValue(targetOrder[firstWrongIndex]);
        //Node tempNode = TempPointer.Instance?.pointingAt;

        //// Simulate the move and check if anything gets dropped
        //bool sourceNextLost = sourceNode?.next != null &&
        //    sourceNode.next != targetNode &&
        //    !WouldBeReachableAfterMove(sourceNode.next, sourceNode, targetNode);

        //bool targetNextLost = targetNode?.next != null &&
        //    !WouldBeReachableAfterMove(targetNode.next, sourceNode, targetNode);



        ////// Also check if temp itself is pointing at something that would be orphaned
        ////bool tempUnsaved = tempNode != null &&
        ////    !IsReachableFromHead(tempNode) &&
        ////    sourceNode?.next != tempNode;

        //bool wouldCauseGarbage = sourceNextLost || targetNextLost;

        // Expand the garbage check to also look at target's current next
        Node targetNode = FindNodeByValue(targetOrder[firstWrongIndex]);
        Node tempNode = TempPointer.Instance?.pointingAt;

        // If temp is null and source already has a next that isn't the target,
        // it WILL be lost — no need to simulate
        bool sourceNextLost = sourceNode?.next != null &&
            sourceNode.next != targetNode &&
            (tempNode == null || !WouldBeReachableAfterMove(sourceNode.next, sourceNode, targetNode));

        bool targetNextLost = targetNode?.next != null &&
            !WouldBeReachableAfterMove(targetNode.next, sourceNode, targetNode);

        bool wouldCauseGarbage = sourceNextLost || targetNextLost;

        Node nodeToSave = sourceNextLost ? sourceNode.next :
                          targetNextLost ? targetNode.next : null;

        if (wouldCauseGarbage && nodeToSave != null)
        {
            string saveColor = GetColorName(nodeToSave.value);

            if (tempNode == null)
            {
                // Beginning of game / temp is free — tell them to use temp first
                switch (level)
                {
                    case 1:
                        uiManager.ShowHint(
                            $"Careful! The {saveColor} carriage will be lost. Save it first.");
                        break;
                    case 2:
                        uiManager.ShowHint(
                            $"Set temp = {saveColor} to save it, then move {sourceColor}.next = {targetColor}");
                        break;
                    case 3:
                        uiManager.ShowHint(
                            $"First drag temp to the {saveColor} carriage, then drag {sourceColor}'s arrow to {targetColor}");
                        break;
                }
            }
            else
            {
                // Mid-game — temp is occupied, tell them to point temp's current 
                // node at the node that would be lost
                string tempColor = GetColorName(tempNode.value);
                switch (level)
                {
                    case 1:
                        uiManager.ShowHint(
                            $"Careful! The {saveColor} carriage will be lost.");
                        break;
                    case 2:
                        uiManager.ShowHint(
                            $"Set {tempColor}.next = {saveColor} to save it, then move {sourceColor}.next = {targetColor}");
                        break;
                    case 3:
                        uiManager.ShowHint(
                            $"First drag {tempColor}'s arrow to {saveColor}, then drag {sourceColor}'s arrow to {targetColor}");
                        break;
                }
            }
        }
        else
        {
            // Safe to make the move directly
            switch (level)
            {
                case 1:
                    uiManager.ShowHint(
                        $"Position {firstWrongIndex + 1} should be the {targetColor} carriage");
                    break;
                case 2:
                    uiManager.ShowHint(
                        $"Try setting {sourceColor}.next = {targetColor}");
                    break;
                case 3:
                    uiManager.ShowHint(
                        $"Drag {sourceColor}'s arrow to the {targetColor} carriage");
                    break;
            }
        }
    }
    private bool IsReachableFromHeadExcluding(Node target, Node excludeSource)
    {
        Node cur = locomotiveNode?.next;
        HashSet<Node> visited = new HashSet<Node>();
        while (cur != null && !visited.Contains(cur))
        {
            if (cur == excludeSource)
            {
                cur = cur.next;
                continue; // skip the source node's contribution
            }
            if (cur == target) return true;
            visited.Add(cur);
            cur = cur.next;
        }
        return false;
    }

    private bool WouldBeReachableAfterMove(Node target, Node sourceNode, Node newTarget)
    {
        // Simulate: sourceNode.next = newTarget, then check if target is reachable
        HashSet<Node> visited = new HashSet<Node>();
        Node cur = locomotiveNode?.next;

        while (cur != null && !visited.Contains(cur))
        {
            visited.Add(cur);
            if (cur == target) return true;

            // Simulate the redirect
            if (cur == sourceNode)
                cur = newTarget;
            else
                cur = cur.next;
        }

        // Also reachable if saved in temp
        Node tempNode = TempPointer.Instance?.pointingAt;
        if (tempNode != null)
        {
            cur = tempNode;
            while (cur != null && !visited.Contains(cur))
            {
                visited.Add(cur);
                if (cur == target) return true;
                cur = cur.next;
            }
        }

        return false;
    }
}