using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class DragDebugger : MonoBehaviour
{
    public Camera arCamera;
    private string log = "Waiting for tap...";
    private GUIStyle style;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
    }

    private void OnFingerDown(Finger finger)
    {
        if (arCamera == null) arCamera = Camera.main;
        log = "Finger down at: " + finger.screenPosition + "\n";

        Ray ray = arCamera.ScreenPointToRay(finger.screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        log += "Hits: " + hits.Length + "\n";
        foreach (RaycastHit hit in hits)
        {
            bool hasVV =
                hit.collider.GetComponentInParent<VehicleVisual>()
                != null;
            log += "  -> " + hit.collider.gameObject.name +
                   " VV=" + hasVV +
                   " trigger=" + hit.collider.isTrigger + "\n";
        }

        Collider[] all = FindObjectsOfType<Collider>();
        log += "Total colliders: " + all.Length + "\n";
    }

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box);
            style.fontSize = 28;
            style.normal.textColor = Color.white;
            style.wordWrap = true;
            style.alignment = TextAnchor.UpperLeft;
        }
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height * 0.55f),
            GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10,
            Screen.width - 20, Screen.height * 0.55f - 20),
            log, style);
    }
}