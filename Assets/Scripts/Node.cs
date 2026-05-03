using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public int value;
    public Node next;
    public bool isReachable;

    private LineRenderer shaft;
    private LineRenderer arrowLeft;
    private LineRenderer arrowRight;

    private Vector3 targetPosition;
    private bool isSliding = false;
    private float slideSpeed = 5f;

    private void Awake()
    {
        shaft = CreateLine("Shaft", Color.yellow, 0.008f);
        arrowLeft = CreateLine("ArrowLeft", Color.yellow, 0.008f);
        arrowRight = CreateLine("ArrowRight", Color.yellow, 0.008f);
        SetArrowsEnabled(false);
    }

    private LineRenderer CreateLine(string objName, Color color, float width)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(transform);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        // Use Universal Render Pipeline if available
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        lr.material = new Material(shader);
        lr.material.color = color;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        return lr;
    }

    private void SetArrowsEnabled(bool enabled)
    {
        if (shaft != null) shaft.enabled = enabled;
        if (arrowLeft != null) arrowLeft.enabled = enabled;
        if (arrowRight != null) arrowRight.enabled = enabled;
    }

    public void SetNext(Node newNext)
    {
        next = newNext;
        UpdatePointerVisual();
    }

    public void UpdatePointerVisual()
    {
        if (next == null)
        {
            SetArrowsEnabled(false);
            return;
        }

        // Only update POSITIONS, don't change visibility
        Vector3 from = transform.position + Vector3.up * 0.12f;
        Vector3 to = next.transform.position + Vector3.up * 0.12f;

        shaft.SetPosition(0, from);
        shaft.SetPosition(1, to);

        Vector3 dir = (to - from).normalized;
        float headLength = 0.04f;
        float headAngle = 25f;

        Vector3 right = Quaternion.Euler(0, headAngle, 0)
            * (-dir) * headLength;
        Vector3 left = Quaternion.Euler(0, -headAngle, 0)
            * (-dir) * headLength;

        arrowRight.SetPosition(0, to);
        arrowRight.SetPosition(1, to + right);
        arrowLeft.SetPosition(0, to);
        arrowLeft.SetPosition(1, to + left);
    }

    public void SetReachable(bool reachable)
    {
        isReachable = reachable;
        Color arrowColor = reachable ? Color.yellow : Color.red;
        if (shaft != null)
        {
            shaft.startColor = arrowColor;
            shaft.endColor = arrowColor;
        }
        if (arrowLeft != null)
        {
            arrowLeft.startColor = arrowColor;
            arrowLeft.endColor = arrowColor;
        }
        if (arrowRight != null)
        {
            arrowRight.startColor = arrowColor;
            arrowRight.endColor = arrowColor;
        }
    }

    public void SlideToPosition(Vector3 target, float speed)
    {
        targetPosition = target;
        slideSpeed = speed;
        isSliding = true;
    }

    private bool isSelected = false;

    public void SetArrowVisible(bool visible)
    {
        isSelected = visible;
        SetArrowsEnabled(visible);
    }

    private void Update()
    {
        if (next == null)
        {
            // Nothing to point at, hide arrow
            shaft.enabled = false;
            arrowLeft.enabled = false;
            arrowRight.enabled = false;
        }
        else if (isSelected && next != null)
        {
            // Force update positions every frame
            shaft.enabled = true;
            arrowLeft.enabled = true;
            arrowRight.enabled = true;

            Vector3 from = transform.position + Vector3.up * 0.12f;
            Vector3 to = next.transform.position + Vector3.up * 0.12f;

            shaft.SetPosition(0, from);
            shaft.SetPosition(1, to);

            Vector3 dir = (to - from).normalized;
            Vector3 right = Quaternion.Euler(0, 25f, 0) * (-dir) * 0.04f;
            Vector3 left = Quaternion.Euler(0, -25f, 0) * (-dir) * 0.04f;

            arrowRight.SetPosition(0, to);
            arrowRight.SetPosition(1, to + right);
            arrowLeft.SetPosition(0, to);
            arrowLeft.SetPosition(1, to + left);
        }

        // Sliding code
        if (!isSliding) return;
        Vector3 localTarget = transform.parent != null
            ? transform.parent.InverseTransformPoint(targetPosition)
            : targetPosition;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, localTarget,
            Time.deltaTime * slideSpeed);
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.localPosition = localTarget;
            isSliding = false;
        }
    }
    public string GetColorName()
    {
        switch ((value - 1) % 4)
        {
            case 0: return "Blue";
            case 1: return "Green";
            case 2: return "Red";
            case 3: return "Yellow";
            default: return "Unknown";
        }
    }

    public bool isGarbageCollected = false;

    public void FadeToGarbage(float duration = 0.8f)
    {
        isGarbageCollected = true;
        StartCoroutine(FadeOutCoroutine(duration));
    }

    public void RestoreFromGarbage()
    {
        isGarbageCollected = false;
        StopAllCoroutines(); // stop any fade that might still be running
        gameObject.SetActive(true);

        // Force all materials back to fully opaque
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                Color c = m.color;
                c.a = 1f;
                m.color = c;

                // Restore opaque rendering mode
                m.SetFloat("_Mode", 0);
                m.SetInt("_SrcBlend",
                    (int)UnityEngine.Rendering.BlendMode.One);
                m.SetInt("_DstBlend",
                    (int)UnityEngine.Rendering.BlendMode.Zero);
                m.SetInt("_ZWrite", 1);
                m.DisableKeyword("_ALPHATEST_ON");
                m.DisableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = -1;
            }
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetFloat("_Mode", 2);
                m.SetInt("_SrcBlend",
                    (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend",
                    (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
                materials.Add(m);
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            foreach (Material m in materials)
            {
                Color c = m.color;
                c.a = alpha;
                m.color = c;
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator FadeInCoroutine(float duration)
    {
        gameObject.SetActive(true);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetFloat("_Mode", 2);
                m.SetInt("_SrcBlend",
                    (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend",
                    (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
                materials.Add(m);
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            foreach (Material m in materials)
            {
                Color c = m.color;
                c.a = alpha;
                m.color = c;
            }
            yield return null;
        }
    }
}