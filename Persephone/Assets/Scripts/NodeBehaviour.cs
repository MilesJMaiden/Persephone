using UnityEngine;
using UnityEngine.EventSystems;
using ProceduralGraphics.LSystems.Generation;
using System.Collections;

public class NodeBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color originalColor;
    private Color highlightColor = Color.red;
    private Renderer renderer;
    private LSystemGenerator generator;
    private Branch branch;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleLerpDuration = 0.2f; // Duration for scaling effect
    private float colorLerpDuration = 0.3f; // Duration for color fade effect

    private Coroutine colorCoroutine;
    private Coroutine scaleCoroutine;

    public void Initialize(Branch parentBranch)
    {
        branch = parentBranch;
    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;
        originalScale = transform.localScale;
        targetScale = originalScale * 1.2f; // Increase scale by 20% when highlighted
        generator = FindObjectOfType<LSystemGenerator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse Entered Node");
        // Start color fade to highlight color
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(LerpColor(originalColor, highlightColor, colorLerpDuration));

        // Start scaling up
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(LerpScale(originalScale, targetScale, scaleLerpDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse Exited Node");
        // Start color fade back to original color
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(LerpColor(renderer.material.color, originalColor, colorLerpDuration));

        // Start scaling back to original size
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(LerpScale(transform.localScale, originalScale, scaleLerpDuration));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Node Clicked");
        if (branch != null)
        {
            DestroyBranch(branch);
        }
    }

    private void DestroyBranch(Branch branchToDestroy)
    {
        // Destroy the node itself
        Destroy(gameObject);

        // Destroy all child branches
        foreach (var child in branchToDestroy.GetChildren())
        {
            if (child != null && child.LineRendererObject != null)
            {
                Destroy(child.LineRendererObject);
            }
        }

        branchToDestroy.ClearChildren();
    }

    private IEnumerator LerpColor(Color startColor, Color endColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            renderer.material.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        renderer.material.color = endColor;
    }

    private IEnumerator LerpScale(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endScale;
    }
}
