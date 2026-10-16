
using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public int colorIndex;

    private Vector3 originalPosition;
    private Collider tileCollider;
    private Renderer tileRenderer;
    private Coroutine hideCoroutine;
    private bool isHidden = false;

    void Awake()
    {
        originalPosition = transform.position;
        tileCollider = GetComponent<Collider>();
        tileRenderer = GetComponent<Renderer>();
    }

    public void SetColor(int index, Color color)
    {
        colorIndex = index;

        if (tileRenderer != null)
        {
            tileRenderer.material.color = color;
        }
    }

    public void Hide()
    {
        if (isHidden)
            return;

        isHidden = true;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (tileCollider != null)
            tileCollider.enabled = false;

        hideCoroutine = StartCoroutine(HideAnimation());
    }

    IEnumerator HideAnimation()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = originalPosition + Vector3.down * 5f;

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            yield return null;
        }

        transform.position = targetPosition;
hideCoroutine = null;
gameObject.SetActive(false);
    }

    public void Show()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        gameObject.SetActive(true);
        transform.position = originalPosition;
        isHidden = false;

        if (tileCollider != null)
            tileCollider.enabled = true;
    }
}

