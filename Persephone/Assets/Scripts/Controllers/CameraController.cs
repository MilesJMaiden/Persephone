using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    public float panSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 20f;
    public float maxZoom = 60f;

    private Camera mainCamera;
    private InputActions inputActions;
    private Vector2 panDirection;

    private void Awake()
    {
        inputActions = new InputActions();

        // Bind movement and zoom actions
        inputActions.Camera.Move.performed += ctx => panDirection = ctx.ReadValue<Vector2>();
        inputActions.Camera.Move.canceled += ctx => panDirection = Vector2.zero;

        inputActions.Camera.ZoomIn.performed += ctx => HandleCameraZoom(-1); // Zoom in
        inputActions.Camera.ZoomOut.performed += ctx => HandleCameraZoom(1);  // Zoom out
    }

    private void Start()
    {
        mainCamera = Camera.main;

        // Enable camera input actions
        inputActions.Camera.Enable();
    }

    private void Update()
    {
        HandleCameraMovement();
    }

    private void OnDisable()
    {
        inputActions.Camera.Disable();
    }

    private void HandleCameraMovement()
    {
        if (panDirection != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(panDirection.x, panDirection.y, 0);
            mainCamera.transform.position += moveDirection * panSpeed * Time.deltaTime;
        }
    }

    private void HandleCameraZoom(float zoomDirection)
    {
        mainCamera.fieldOfView = Mathf.Clamp(
            mainCamera.fieldOfView + zoomDirection * zoomSpeed,
            minZoom,
            maxZoom
        );
    }

    public void ResetCameraPosition()
    {
        Debug.Log("Camera position reset.");
        // Logic for resetting camera position can be implemented here.
    }

    private void OnDestroy()
    {
        inputActions.Camera.Disable();
    }
}
