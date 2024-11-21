using UnityEngine;
using System.Collections.Generic;

public class WindManager : MonoBehaviour
{
    [Header("Wind Settings")]
    [Range(0f, 2f)] public float WindStrength = 0.5f;
    [Range(0.1f, 2f)] public float WindFrequency = 1f;
    [Range(0f, 1f)] public float Gustiness = 0.3f;
    public Vector3 WindDirection = Vector3.right; // Default wind direction

    private List<Branch> branches = new List<Branch>();
    private bool isWindEnabled = false; // Track wind state

    private void Update()
    {
        if (isWindEnabled)
        {
            ApplyWindToBranches();
        }

        // Optional: Debug branch connections in editor view
        foreach (var branch in branches)
        {
            DebugDrawBranchConnections(branch);
        }
    }

    public void RegisterBranch(Branch branch)
    {
        if (!branches.Contains(branch))
        {
            branches.Add(branch);
        }
    }

    private void ApplyWindToBranches()
    {
        float time = Time.time;

        foreach (var branch in branches)
        {
            if (branch.Parent == null && branch.LineRendererObject != null) // Start from root branches
            {
                Vector3 rootPosition = branch.LineRendererObject.transform.position;
                ApplyWindRecursively(branch, time, Quaternion.identity);
            }
        }
    }

    private void ApplyWindRecursively(Branch branch, float time, Quaternion accumulatedRotation)
    {
        if (branch.LineRendererObject == null) return;

        // Compute wind rotation for this branch
        Quaternion windRotation = CalculateWindRotation(time);
        Quaternion newRotation = accumulatedRotation * windRotation;

        // Apply local rotation to the branch's transform
        branch.LineRendererObject.transform.localRotation = newRotation;

        // Ensure LineRenderer respects the local transformation
        if (branch.LineRendererObject.TryGetComponent(out LineRenderer lineRenderer))
        {
            lineRenderer.useWorldSpace = false;

            // Update start and end positions dynamically
            Vector3 startPosition = branch.LineRendererObject.transform.position;
            Vector3 localEndPosition = newRotation * Vector3.up * branch.Length;
            Vector3 endPosition = startPosition + localEndPosition;

            // Update the LineRenderer's positions
            //lineRenderer.positionCount = 2;
            //lineRenderer.SetPosition(0, branch.LineRendererObject.transform.InverseTransformPoint(startPosition));
            //lineRenderer.SetPosition(1, branch.LineRendererObject.transform.InverseTransformPoint(endPosition));
        }

        // Recursively apply wind to child branches
        foreach (var childBranch in branch.GetChildren())
        {
            ApplyWindRecursively(childBranch, time, newRotation);
        }
    }

    private Quaternion CalculateWindRotation(float time)
    {
        float gust = Mathf.PerlinNoise(time * WindFrequency, 0f) * Gustiness;
        float swayAmountX = Mathf.Sin(time * WindFrequency) * WindStrength + gust;
        float swayAmountY = Mathf.Cos(time * WindFrequency * 0.5f) * WindStrength * 0.7f;
        Vector3 sway = new Vector3(swayAmountX, swayAmountY, 0f); // Adjust axes for desired effect

        return Quaternion.Euler(sway);
    }

    public void ToggleWind(bool enableWind)
    {
        isWindEnabled = enableWind;
        Debug.Log($"Wind is now {(isWindEnabled ? "enabled" : "disabled")}");
    }

    public void SetWindDirection(Vector3 newDirection)
    {
        WindDirection = newDirection.normalized; // Ensure wind direction is normalized
        Debug.Log($"Wind direction set to {WindDirection}");
    }

    private void DebugDrawBranchConnections(Branch branch)
    {
        if (branch.LineRendererObject == null) return;

        if (branch.Parent != null && branch.Parent.LineRendererObject != null)
        {
            Debug.DrawLine(
                branch.Parent.LineRendererObject.transform.position,
                branch.LineRendererObject.transform.position,
                Color.green
            );
        }

        // Recursively draw connections for child branches
        foreach (var childBranch in branch.GetChildren())
        {
            DebugDrawBranchConnections(childBranch);
        }
    }
}
