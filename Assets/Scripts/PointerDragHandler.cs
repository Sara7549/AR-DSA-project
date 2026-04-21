using UnityEngine;

public class PointerDragHandler : MonoBehaviour
{
    private Node node;
    private Camera cam;

    private bool dragging = false;

    void Start()
    {
        node = GetComponent<Node>();
        cam = Camera.main;
    }

    void Update()
    {
        if (dragging)
        {
            DragPointer();
        }
    }

    void OnMouseDown()
    {
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
        TryConnect();
    }

    void DragPointer()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            node.SetNext(null); // temporarily disconnect
            node.UpdatePointerVisual();
        }
    }

    void TryConnect()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Node target = hit.collider.GetComponent<Node>();

            if (target != null && target != node)
            {
                node.SetNext(target);

                FindObjectOfType<LinkedListGameManager>().UpdateReachability();
            }
        }
    }
}