using UnityEngine;

public class StackTapDetector : MonoBehaviour
{
    public int stackIndex;
    private GameController gameController;
    private Camera arCamera;
    private bool isActive = false;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        arCamera = Camera.main;
    }

    private void OnEnable()
    {
        InputHandler.OnTap += OnTap;
    }

    private void OnDisable()
    {
        InputHandler.OnTap -= OnTap;
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void OnTap(Vector2 touchPosition)
    {
        if (!isActive) return;

        // Convert stack world position to screen position
        Vector3 screenPos = arCamera.WorldToScreenPoint(
            transform.position);

        // Check if tap is within distance of stack on screen
        float distance = Vector2.Distance(
            touchPosition,
            new Vector2(screenPos.x, screenPos.y));

        // If tap is within 200 pixels of stack center
        if (distance < 200f && screenPos.z > 0)
        {
            if (gameController != null)
                gameController.OnStackTapped(stackIndex);
        }
    }
}