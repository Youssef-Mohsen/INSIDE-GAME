using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // ==========================================================================================
    // Variables
    // ==========================================================================================
    
    [Header("Movement Speeds")]
    public float moveSpeed;
    public float runMultiplier;
    public float crouchMultiplier;
    
    [Header("Slide Settings")]
    public float slideSpeed = 10f;
    public float slideDuration = 0.8f;
    
    private Animator _animator;
    private CharacterController _characterController;
    private InputSystem_Actions2 _inputSystem;
    private Transform _mainCamera; // ADDED: Reference to the camera
    
    // Player input values
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private bool _isMovementPressed;
    private bool _isRunPressed;
    private bool _isCrouchPressed;
    
    // Sliding variables
    private bool _isSliding;
    private Vector3 _slideDirection;
    
    // Jumping variables
    [Header("Jump Settings")]
    public float _maxJumpHeight = 5f;
    public float _maxJumpTime = 1.0f;
    private bool _isJumpPressed;
    private float _initialJumpVelocity;
    private bool _isJumping;
    
    // Combat variables
    [Header("Combat Settings")]
    public int numberOfPunches = 3;
    private bool _isDead = false;
    
    // gravity
    float gravity = 9.8f;
    float groundedGravity = -0.5f;
    
    private readonly float _rotationFactorPerFrame = 20.0f;
    
    // ==========================================================================================
    // Event Functions
    // ==========================================================================================
    
    private void OnEnable()
    {
        _inputSystem.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.PlayerControls.Disable();
    }
    
    private void Awake()
    {
        _inputSystem = new InputSystem_Actions2();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        // ADDED: Cache the main camera's transform
        if (Camera.main != null)
        {
            _mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No camera tagged 'MainCamera' found in the scene!");
        }
        
        _inputSystem.PlayerControls.Move.started += OnMovementInput;
        _inputSystem.PlayerControls.Move.canceled += OnMovementInput;
        _inputSystem.PlayerControls.Move.performed += OnMovementInput;

        _inputSystem.PlayerControls.Run.started += OnRun;
        _inputSystem.PlayerControls.Run.canceled += OnRun;

        _inputSystem.PlayerControls.Crouch.started += OnCrouch;
        _inputSystem.PlayerControls.Crouch.canceled += OnCrouch;

        _inputSystem.PlayerControls.Jump.started += OnJump;
        _inputSystem.PlayerControls.Jump.canceled += OnJump;
        
        _inputSystem.PlayerControls.Punch.started += OnPunch;

        SetupJumpVariables();
    }
    private void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2* _maxJumpHeight) / timeToApex; 
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the object we hit has the "Enemy" tag
        if (hit.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Die");
            Die();
        }
    }
   
    private void Update()
    {
        
            CalculateCameraRelativeMovement(); // ADDED: Calculate the direction first
            HandleRotation();
            HandleAnimation();
            HandleMovement();
            HandleGravity();
            HandleJump();
        
    }
    
    // ==========================================================================================
    // Input Callback Functions
    // ==========================================================================================
    
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_isMovementPressed && _isRunPressed && _characterController.isGrounded && !_isSliding)
            {
                StartCoroutine(SlideRoutine());
            }
            else
            {
                _isCrouchPressed = true;
            }
        }
        else if (context.canceled)
        {
            _isCrouchPressed = false;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed =  context.ReadValueAsButton();
    }
    
    private void OnPunch(InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded && !_isSliding)
        {
            PerformRandomPunch();
        }
    }
    
    // ==========================================================================================
    // Triggers / Coroutines
    // ==========================================================================================
    
    private void PerformRandomPunch()
    {
        int randomPunch = UnityEngine.Random.Range(1, numberOfPunches);
        _animator.SetInteger("punchType", randomPunch);
        _animator.SetTrigger("doPunch");
    }
    
    private IEnumerator SlideRoutine()
    {
        _isSliding = true;
        _animator.SetBool("isSliding", true);
        
        _slideDirection = transform.forward;

        yield return new WaitForSeconds(slideDuration);

        _isSliding = false;
        _animator.SetBool("isSliding", false);
    }

    private IEnumerator DeathRoutine()
    {
        _inputSystem.PlayerControls.Disable();
        yield return new WaitForSeconds(1.0f);
        _animator.SetTrigger("Dead");
    }

    public void Die()
    {
        if (!_isDead)
        {
            _isDead = true;
            StartCoroutine(DeathRoutine());
            StartCoroutine(LoadGameOver());
        }
    }
    IEnumerator LoadGameOver()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(5); // change 4 to your GameOver scene index
    }

    // ==========================================================================================
    // Per-frame Functions
    // ==========================================================================================

    private void CalculateCameraRelativeMovement()
    {
        if (_mainCamera == null) return;

        // Get camera forward and right vectors
        Vector3 cameraForward = _mainCamera.forward;
        Vector3 cameraRight = _mainCamera.right;

        // Flatten them on the Y axis so the player doesn't fly up/down based on camera pitch
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * _currentMovementInput.y + cameraRight * _currentMovementInput.x).normalized;

        // Set the X and Z movement based on speed
        _currentMovement.x = moveDirection.x * moveSpeed;
        _currentMovement.z = moveDirection.z * moveSpeed;
    }
    
    private void HandleMovement()
    {
        Vector3 appliedMovement = _currentMovement;
        
        if (_isCrouchPressed)
        {
            appliedMovement.x *= crouchMultiplier;
            appliedMovement.z *= crouchMultiplier;
        }
        else if (_isRunPressed)
        {
            appliedMovement.x *= runMultiplier;
            appliedMovement.z *= runMultiplier;
        }

        _characterController.Move(appliedMovement * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (!_isJumping && _isJumpPressed && _characterController.isGrounded)
        {
            _animator.SetBool("isJumping", true);
            _isJumping = true;
            _currentMovement.y = _initialJumpVelocity * 0.5f;
        }
        else if (!_isJumpPressed && _isJumping && _characterController.isGrounded)
        {
            _isJumping = false;
        }
    }
    
    private void HandleAnimation()
    {
        bool isWalking = _animator.GetBool("isWalking");
        bool isRunning =  _animator.GetBool("isRunning");
        bool isCrouching = _animator.GetBool("isCrouching");
        if (_isMovementPressed && !isWalking)
        {
            _animator.SetBool("isWalking", true);
        } 
        else if (!_isMovementPressed && isWalking)
        {
            _animator.SetBool("isWalking", false);
        }
        
        if ((_isMovementPressed && _isRunPressed) && !isRunning)
        {
            _animator.SetBool("isRunning", true);
        }
        else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
        {
            _animator.SetBool("isRunning", false);
        }
        
        if ((_isMovementPressed && _isCrouchPressed) && !isCrouching)
        {
            _animator.SetBool("isCrouching", true);
        }
        else if ((!_isMovementPressed || !_isCrouchPressed) && isCrouching)
        {
            _animator.SetBool("isCrouching", false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;
        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = _currentMovement.z;
        
        Quaternion currentRotation = transform.rotation;
        if (_isMovementPressed)
        {
            // Now positionToLookAt will automatically point away from the camera when pressing 'W'
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        bool isFalling = _currentMovement.y <= 0.0f || !_isJumpPressed;
        float fallMultiplier = 2.0f;
        if (_characterController.isGrounded) {
            _animator.SetBool("isJumping", false);
            _currentMovement.y = groundedGravity;
        } else if (isFalling) {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            _currentMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            _currentMovement.y = nextYVelocity;
        }
    }
}