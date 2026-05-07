using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonOrbitCamera : MonoBehaviour
{

    // ==========================================================================================
    // Variables
    // ==========================================================================================
    
    [Header("Target Settings")]
    [Tooltip("Drag your Player object here")]
    public Transform target;
    [Tooltip("How far the camera stays from the player.")]
    public float distance = 5f;
    [Tooltip("Offsets the camera's focus point so it looks at the player's upper body/head.")]
    public float lookAtHeightOffset = 1.5f;
    [Header("Mouse Controls")]
    public float mouseSensitivity = 0.2f;
    [Tooltip("Limits how far up or down you can look. (Min, Max)")]
    public Vector2 pitchMinMax = new Vector2(-40f, 85f);
    public bool invertY = false;

    // Current rotation values
    private float pitch;
    private float yaw;

    // ==========================================================================================
    // Unity Methods
    // ==========================================================================================

    private void Start()
    {
        // Lock the cursor to the center of the screen and hide it for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonOrbitCamera: No target assigned!");
            return;
        }

        HandleMouseInput();
        UpdateCameraPositionAndRotation();
    }

    // ==========================================================================================
    // Camera Behaviors
    // ==========================================================================================

    private void HandleMouseInput()
    {
        // Read mouse movement delta using the New Input System
        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        // Adjust yaw (left/right) and pitch (up/down) based on input and sensitivity
        yaw += mouseDelta.x * mouseSensitivity;
        
        if (invertY)
        {
            pitch += mouseDelta.y * mouseSensitivity;
        }
        else
        {
            pitch -= mouseDelta.y * mouseSensitivity;
        }

        // Clamp the pitch so the camera doesn't flip upside down
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
    }

    private void UpdateCameraPositionAndRotation()
    {
        // 1. Calculate the exact point we are looking at
        Vector3 targetLookPosition = target.position + (Vector3.up * lookAtHeightOffset);

        // 2. Rotate the camera based on our pitch and yaw
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // 3. Move the camera backward away from the target based on our rotation and distance
        transform.position = targetLookPosition - transform.forward * distance;

    }
}