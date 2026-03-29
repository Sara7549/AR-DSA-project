using UnityEngine;

public class ReticleAnimator : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float rotationSpeed = 45f;
    public float baseScale = 0.3f;
    public float pulseAmount = 0.05f;

    private void Update()
    {
        // Pulse around a fixed base scale
        float scale = baseScale +
            Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(scale, 0.001f, scale);

        // Rotate slowly
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}