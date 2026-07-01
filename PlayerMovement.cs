using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D pb;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction dashAction;
    Vector2 moveValue;

    // General settings variables
    public float speed = 6.5f;
    public float jumpForce = 6.5f;
    private float acceleration = 0.15f;
    private float deceleration = 0.1f;
    public float dashForce = 10f;

    // Jump variables
    public Transform groundCheck;
    public LayerMask groundLayer;
    private float checkRadius = 0.05f;
    private bool isGrounded;
    private bool doJump;

    // Jump Gravity variables
    public float fallMultiplier = 3.7f;
    public float stopJumpGravity = 2.5f;

    // Coyote timer and jump buffering variables
    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    // Dash variables
    private bool canDash = true;
    private bool doDash;
    public float dashCooldown = 1.5f;
    private float dashCooldownTimer;

    private void Start()
    {        
        pb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dashAction = InputSystem.actions.FindAction("Dash");
    }

    void FixedUpdate()
    {
        var targetSpeed = moveValue.x * speed;
        var acc = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        acc = speed / acc;
        pb.linearVelocityX = Mathf.MoveTowards(pb.linearVelocityX, targetSpeed, acc * Time.fixedDeltaTime);

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            doJump = true;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        if (doJump)
            pb.linearVelocityY = jumpForce;

        if (doDash && canDash)
        {
            pb.linearVelocityX += moveValue.x * dashForce;
            canDash = false;
        }

        if (pb.linearVelocityY < 0)
            pb.linearVelocityY += Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (pb.linearVelocityY > 0 && !jumpAction.IsPressed())
            pb.linearVelocityY += Physics2D.gravity.y * (stopJumpGravity - 1) * Time.fixedDeltaTime;

        doJump = false;
        doDash = false;
    }

    private void Update()
    {
        moveValue = moveAction.ReadValue<Vector2>();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            doJump = true;

        if (dashAction.WasPressedThisFrame())
            doDash = true;

        // Se sei nel ground il timer non parte, mentre appena inizi a cadere parte il conto alla rovescia.
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
        // Il conto alla rovescia parte soltanto quando non hai premuto il jump in quel frame.
        // In combinazione con il coyote time riesci a permettere di effettuare questo saltino 
        // soltanto mentre sei in aria e non hai premuto il jump in quel frame.
        jumpBufferCounter = jumpAction.WasPressedThisFrame() ? jumpBufferTime : jumpBufferCounter - Time.deltaTime;

        // Dash handling
        dashCooldownTimer = canDash ? dashCooldown : dashCooldownTimer - Time.deltaTime;
        canDash = dashCooldownTimer <= 0f ? true : canDash;
    }
}
