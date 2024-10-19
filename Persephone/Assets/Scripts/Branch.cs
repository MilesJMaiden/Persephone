using UnityEngine;
using System.Collections.Generic;

public class Branch
{
    public GameObject LineRendererObject { get; private set; }
    public Branch Parent { get; private set; }  // Parent branch
    private List<Branch> children;  // To store child branches

    public Branch(GameObject lineRendererObject, Branch parent)
    {
        LineRendererObject = lineRendererObject;
        children = new List<Branch>();

        // Set the parent if the parent exists and hasn't been destroyed
        if (parent != null && parent.LineRendererObject != null)
        {
            SetParent(parent);
        }
    }

    // Set the parent of this branch
    public void SetParent(Branch parent)
    {
        if (parent != null && parent.LineRendererObject != null && LineRendererObject != null)
        {
            Parent = parent;
            parent.AddChild(this);
            LineRendererObject.transform.SetParent(parent.LineRendererObject.transform);  // Set as a child in the Unity hierarchy
            Debug.Log($"Branch {LineRendererObject.name} set as child of {parent.LineRendererObject.name}");
        }
        else
        {
            Debug.LogWarning("Attempted to set a parent for a branch, but the parent or child object was destroyed.");
        }
    }

    // Add a child branch to this branch
    public void AddChild(Branch child)
    {
        if (child != null && child.LineRendererObject != null)
        {
            children.Add(child);
            Debug.Log($"Branch {child.LineRendererObject.name} added to children of {LineRendererObject.name}");
        }
        else
        {
            Debug.LogWarning("Attempted to add a child to a branch, but the child object was destroyed.");
        }
    }

    // Retrieve children of this branch
    public List<Branch> GetChildren()
    {
        return children;
    }

    // Clear all children
    public void ClearChildren()
    {
        children.Clear();
    }

    // Recursively traverse the branches and print the structure
    public void Traverse()
    {
        Debug.Log($"Traversing branch: {LineRendererObject.name}");
        foreach (var child in children)
        {
            child.Traverse();  // Recursive call to traverse each child
        }
    }
}
