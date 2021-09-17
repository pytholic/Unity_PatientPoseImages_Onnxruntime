using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class BlinkingUi : MonoBehaviour
{
    [Tooltip("How many times to blink in a second")]
    public float BlinkingFrequency = 2.0f;

    void Start()
    {
        target = GetComponent<CanvasGroup>();
        if (target != null) StartCoroutine(BeginBlink());
    }

    void OnEnable()
    {
        if (target != null) StartCoroutine(BeginBlink());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private CanvasGroup target;

    private IEnumerator BeginBlink()
    {
        // TODO: start & continue from original source alpha

        var fadeOut = true;
        var half = BlinkingFrequency * 0.5f;
        while (true)
        {
            float elapse = 0.0f;
            while (elapse < half)
            {
                var dt = Time.deltaTime;
                elapse += dt;
                var alpha = fadeOut ? Mathf.Lerp(1, 0, elapse / half) : Mathf.Lerp(0, 1, elapse / half);
                target.alpha = alpha;
                yield return null;
            }
            fadeOut = !fadeOut;
        }
    }
}
