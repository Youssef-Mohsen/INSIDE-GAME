using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    
    public float moveSpeed;
    public float runMultiplier;
    
    private Animator _animator;
    private CharacterController _characterController;
    
    private InputSystem_Actions2 _inputSystem;
    
    // Player input values
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _currentRunMovement;
    private bool _isMovementPressed;
    private bool _isRunPressed;
    
    // Jumping variables
    public float _maxJumpHeight = 5f;
    public float _maxJumpTime = 1.0f;
    private bool _isJumpPressed;
    private float _initialJumpVelocity;
    private bool _isJumping;
    
    // gravity
    float gravity = 9.8f;
    float groundedGravity = -0.5f;
    
    private readonly float _rotationFactorPerFrame = 10.0f;
    
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
        
        _inputSystem.PlayerControls.Move.started += OnMovementInput;
        _inputSystem.PlayerControls.Move.canceled += OnMovementInput;
        _inputSystem.PlayerControls.Move.performed += OnMovementInput;

        _inputSystem.PlayerControls.Run.started += OnRun;
        _inputSystem.PlayerControls.Run.canceled += OnRun;

        _inputSystem.PlayerControls.Jump.started += OnJump;
        _inputSystem.PlayerControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }
    
    private void Update()
    {
        HandleRotation();
        HandleAnimation();
        HandleMovement();
        HandleGravity();
        HandleJump();
    }

    private void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2* _maxJumpHeight) / timeToApex; 
    }
    
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = -_currentMovementInput.x * moveSpeed;
        _currentMovement.z = -_currentMovementInput.y * moveSpeed;
        _currentRunMovement.x = -_currentMovementInput.x * moveSpeed * runMultiplier;
        _currentRunMovement.z = -_currentMovementInput.y * moveSpeed * runMultiplier;
        
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed =  context.ReadValueAsButton();
    }

    private void HandleMovement()
    {
        if (_isRunPressed)
        {
            _characterController.Move(_currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(_currentMovement * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (!_isJumping && _isJumpPressed && _characterController.isGrounded)
        {
            _animator.SetBool("isJumping", true);
            _isJumping = true;
            _currentMovement.y = _initialJumpVelocity * 0.5f;
            _currentRunMovement.y = _initialJumpVelocity * 0.5f;
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
            _currentRunMovement.y = groundedGravity;
        } else if (isFalling) {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
    }
    
}