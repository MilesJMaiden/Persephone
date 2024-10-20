using UnityEngine;
using UnityEngine.EventSystems;
using ProceduralGraphics.LSystems.Generation;

public class NodeBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color originalColor;
    private Renderer renderer;
    private LSystemGenerator generator;
    private Branch branch;

    public void Initialize(Branch parentBranch)
    {
        branch = parentBranch;
    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;
        generator = FindObjectOfType<LSystemGenerator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse Entered Node");
        renderer.material.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse Exited Node");
        renderer.material.color = originalColor;
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
}