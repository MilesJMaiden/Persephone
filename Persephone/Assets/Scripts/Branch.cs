using UnityEngine;
using System.Collections.Generic;

public class Branch
{
    public GameObject LineRendererObject { get; private set; }
    public Branch Parent { get; private set; }
    private List<Branch> children;

    public Branch(GameObject lineRendererObject, Branch parent)
    {
        LineRendererObject = lineRendererObject;
        children = new List<Branch>();

        if (parent != null && parent.LineRendererObject != null)
        {
            SetParent(parent);
        }
    }

    public void SetParent(Branch parent)
    {
        if (parent != null && parent.LineRendererObject != null && LineRendererObject != null)
        {
            Parent = parent;
            parent.AddChild(this);
            LineRendererObject.transform.SetParent(parent.LineRendererObject.transform);
            Debug.Log($"Branch {LineRendererObject.name} set as child of {parent.LineRendererObject.name}");
        }
        else
        {
            Debug.LogWarning("Attempted to set a parent for a branch, but the parent or child object was destroyed.");
        }
    }

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

    public List<Branch> GetChildren()
    {
        return children;
    }

    public void ClearChildren()
    {
        children.Clear();
    }

    public void Traverse()
    {
        Debug.Log($"Traversing branch: {LineRendererObject.name}");
        foreach (var child in children)
        {
            child.Traverse();
        }
    }
}
