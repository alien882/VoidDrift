using System.Collections;
using UnityEngine;

/// <summary>
/// Screen shake al morir. Se añade al GameObject de la Main Camera.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        if (e.finalScore == 0 && e.survivalTime == 0f) return;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
