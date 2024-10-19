using UnityEngine;
using UnityEngine.EventSystems; // Required for event handling
using ProceduralGraphics.LSystems.Generation; // Include the necessary namespace

public class NodeBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color originalColor;
    private Renderer renderer;
    private LSystemGenerator generator; // Reference to the generator
    private Branch branch; // Reference to the branch this node belongs to

    public void Initialize(Branch parentBranch) // Method to set the branch reference
    {
        branch = parentBranch;
    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;
        generator = FindObjectOfType<LSystemGenerator>(); // Find the generator
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse Entered Node");
        renderer.material.color = Color.red; // Change color on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse Exited Node");
        renderer.material.color = originalColor; // Revert color
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Node Clicked");
        // Destroy this node and any branches stemming from its parent branch
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

        // Optionally: Remove child branches from the parent branch's list
        branchToDestroy.ClearChildren(); // Ensure child references are cleaned up
    }
}