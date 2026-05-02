using UnityEngine;

public class boyMovement : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    [Header("References")]
    [SerializeField] Transform playerObj;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask ground;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float jumpForce = 6f; // base jump height
    [SerializeField] float runningJumpBonus = 20f; // extra height when running
    [SerializeField] float rotationSpeed = 10f;

    [Header("Punch Settings")]
    [SerializeField] float doubleClickTime = 0.3f;
    [SerializeField] float holdPunchTime = 0.5f;

    Vector3 moveDirection;

    bool isWalking;
    bool isRunning;
    bool isCrouching;

    bool isPunching1;
    bool isPunching2;
    bool isPunching3;

    float lastClickTime;
    float mouseHoldTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.useGravity = true;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // ================= INPUT =================
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // dead zone safety
        if (Mathf.Abs(h) < 0.1f) h = 0f;
        if (Mathf.Abs(v) < 0.1f) v = 0f;
        moveDirection = new Vector3(h, 0f, v).normalized;

        bool grounded = IsGrounded();

        // ================= STATES =================
        isCrouching = Input.GetKey(KeyCode.LeftControl) && grounded;

        isRunning = Input.GetKey(KeyCode.LeftShift) &&
                    moveDirection.magnitude > 0 &&
                    grounded &&
                    !isCrouching;

        isWalking = moveDirection.magnitude > 0 &&
                    grounded &&
                    !isRunning &&
                    !isCrouching;

        // ================= JUMP =================
        if (Input.GetKeyDown(KeyCode.Space) && grounded && !isCrouching)
        {
            float finalJump = jumpForce;

            // ⭐ Running jump boost
            if (isRunning)
            {
                finalJump += runningJumpBonus;
            }

            // reset vertical speed first (important for consistency)
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                finalJump,
                rb.linearVelocity.z
            );

            // animation choice
            if (isRunning)
            {
                animator.SetTrigger("RunningJump");
            }
            else
            {
                animator.SetTrigger("Jump");
            }
        }

        // ================= PUNCH =================
        isPunching1 = false;
        isPunching2 = false;
        isPunching3 = false;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime <= doubleClickTime)
                isPunching2 = true;
            else
                isPunching1 = true;

            lastClickTime = Time.time;
            mouseHoldTimer = Time.time;
        }

        if (Input.GetMouseButton(0))
        {
            if (Time.time - mouseHoldTimer >= holdPunchTime)
            {
                isPunching3 = true;
                isPunching1 = false;
                isPunching2 = false;
            }
        }

        // ================= SPEED =================
        float currentSpeed = walkSpeed;

        if (isRunning)
            currentSpeed = runSpeed;
        else if (isCrouching)
            currentSpeed = crouchSpeed;

        if (isPunching1 || isPunching2 || isPunching3)
            currentSpeed = 0f;

        // ================= MOVEMENT =================
        rb.linearVelocity = new Vector3(
            moveDirection.x * currentSpeed,
            rb.linearVelocity.y,
            moveDirection.z * currentSpeed
        );

        // ================= ROTATION =================
        if (moveDirection != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(
                playerObj.forward,
                moveDirection,
                rotationSpeed * Time.deltaTime
            );
        }

        // ================= ANIMATOR =================
        animator.SetFloat("moveX", h);
        animator.SetFloat("moveZ", v);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);

        animator.SetBool("isPunching1", isPunching1);
        animator.SetBool("isPunching2", isPunching2);
        animator.SetBool("isPunching3", isPunching3);
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.2f, ground);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }
}