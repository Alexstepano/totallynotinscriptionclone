using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Settings")]
    public float defaultDuration = 0.2f;
    public float defaultMagnitude = 0.15f;

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void Shake(float magnitude) => Shake(defaultDuration, magnitude);
    public void Shake() => Shake(defaultDuration, defaultMagnitude);

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            magnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            yield return null;
        }
        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
