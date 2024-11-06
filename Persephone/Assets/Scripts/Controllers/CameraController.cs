using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    public float panSpeed = 10f;         // Speed of panning when using arrow keys or similar
    public float zoomSpeed = 5f;         // Speed of zoom when using scroll wheel
    public float minZoom = 20f;          // Minimum field of view for zoom (closer)
    public float maxZoom = 60f;          // Maximum field of view for zoom (further away)

    private Camera mainCamera;
    private InputActions inputActions;
    private Vector3 initialCameraPosition;

    private void Awake()
    {
        inputActions = new InputActions();

        // Bind movement and zoom actions
        inputActions.Camera.Move.performed += ctx => HandleCameraMovement(ctx.ReadValue<Vector2>());
        inputActions.Camera.Move.canceled += ctx => HandleCameraMovement(Vector2.zero);

        // Bind zoom actions based on zoom direction
        inputActions.Camera.ZoomIn.performed += ctx => HandleCameraZoom(-1); // Zoom in (reduce FOV)
        inputActions.Camera.ZoomOut.performed += ctx => HandleCameraZoom(1);  // Zoom out (increase FOV)
    }

    private void Start()
    {
        mainCamera = Camera.main;
        initialCameraPosition = mainCamera.transform.position;

        // Enable camera input actions
        inputActions.Camera.Enable();
    }

    private void OnDisable()
    {
        inputActions.Camera.Disable();
    }

    private void HandleCameraMovement(Vector2 movementInput)
    {
        // Pan camera based on input direction
        Vector3 moveDirection = new Vector3(movementInput.x, movementInput.y, 0);
        mainCamera.transform.position += moveDirection * panSpeed * Time.deltaTime;
    }

    private void HandleCameraZoom(float zoomDirection)
    {
        // Adjust Field of View directly for zoom effect
        mainCamera.fieldOfView = Mathf.Clamp(
            mainCamera.fieldOfView + zoomDirection * zoomSpeed,
            minZoom,
            maxZoom
        );
    }

    public void ResetCameraPosition()
    {
        // Resets camera position to initial setup
        mainCamera.transform.position = initialCameraPosition;
        Debug.Log("Camera position reset.");
    }

    private void OnDestroy()
    {
        inputActions.Camera.Disable();
    }
}
