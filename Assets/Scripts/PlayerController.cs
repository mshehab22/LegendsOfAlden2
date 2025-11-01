// PlayerController: “Central loop: reads inputs, moves CC, checks grounded, feeds Animator. States request high-level switches; controller keeps   physics and animation in sync.”
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // -------------- Inspector-tuned values ----------------
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;             // Camera used to make movement camera-relative
    [SerializeField] private bool shouldFaceMoveDirection = false; // Optional: face the way we move

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;     // Max speed while walking
    [SerializeField] private float runSpeed = 5f;        // Max speed while running
    [SerializeField] private float acceleration = 12f;   // How quickly we approach the target speed
    [SerializeField] private float rotationSpeed = 720f; // Slerp factor for facing target direction (units/sec)

    [Header("Crouch")]
    [SerializeField] private bool disableMovementWhileCrouched = true; // If true, crouch holds position

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;       // Jump apex height (used to compute initial velocity)
    [SerializeField] private float gravity = -18f;          // Constant gravity (negative!)
    [SerializeField] private float groundSticky = -2f;      // Small downward push to keep CC grounded

    [Header("Ground Check (optional extra)")]
    [SerializeField] private Transform groundCheck;             // Empty transform near the feet
    [SerializeField] private float groundCheckRadius = 0.25f;   // Radius for the ground overlap
    [SerializeField] private LayerMask groundLayers = ~0;       // What counts as ground

    [Header("Locks")]
    [SerializeField] private float attackMoveLockMultiplier = 0f; // 0 = lock movement during attack, 1 = no lock


    // -------- Exposed read-only properties for States --------
    public Animator Animator => animator;              // Give states safe access to the Animator
    public float Gravity => gravity;                   // So states can read gravity without hard-coding
    public float JumpHeight => jumpHeight;
    public bool IsGrounded { get; private set; }       // True after ground check each frame
    public float VerticalVelocity => verticalVelocity; // Y velocity (read-only for states)

    
    public int UpperBodyLayerIndex => 1; // Optional: if you use layers in the Animator (e.g., UpperBody = layer 1)


    // -------- Input bag (everything the states may need) --------
    // Decouples Input.GetKey calls from the states (single place to read input)
    public struct PlayerInputBag
    {
        public float MoveMagnitude;      // 0..1 length of input vector
        public bool RunningHeld;
        public bool CrouchHeld;
        public bool CrouchPressed;       // pressed this frame
        public bool JumpPressed;         // pressed this frame
        public bool AttackPressed;       // pressed this frame
    }
    public PlayerInputBag Inputs;


    // -------- State flags (things states can set to influence base logic) --------
    public struct PlayerStateFlags { public bool AttackLock; } // e.g., block rotation/motion while attacking
    public PlayerStateFlags StateFlags;

    // -------- State machine and concrete states --------
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerState_Locomotion LocomotionState { get; private set; }
    public PlayerState_Attack AttackState { get; private set; }
    public PlayerState_Crouch CrouchState { get; private set; }
    public PlayerState_Jump JumpState { get; private set; }

    // -------- Private runtime fields --------
    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity;         // Accumulated Y velocity (gravity / jump)
    private float currentSpeed;             // Smoothed XZ speed
    private Vector3 moveDirectionWorld;     // Current XZ direction in world space

    private static readonly Collider[] groundHitsBuffer = new Collider[8];

    private void Awake()
    {
        // Cache component refs
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Create state machine + concrete states
        stateMachine = new PlayerStateMachine();
        LocomotionState = new PlayerState_Locomotion(this, stateMachine);
        AttackState = new PlayerState_Attack(this, stateMachine);
        CrouchState = new PlayerState_Crouch(this, stateMachine);
        JumpState = new PlayerState_Jump(this, stateMachine);

        // Enter initial state
        stateMachine.Initialize(LocomotionState);
    }

    private void Update()
    {
        // 1) Gather raw inputs once per frame and fill the bag.
        Vector2 raw = new Vector2( Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical") );

        Vector3 inputVec = Vector3.ClampMagnitude(new Vector3(raw.x, 0f, raw.y), 1f);
        Inputs.MoveMagnitude = inputVec.magnitude;
        Inputs.RunningHeld = Input.GetKey(KeyCode.LeftShift);
        Inputs.CrouchHeld = Input.GetKey(KeyCode.LeftControl);
        Inputs.CrouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        Inputs.JumpPressed = Input.GetKeyDown(KeyCode.Space);
        Inputs.AttackPressed = Input.GetMouseButtonDown(0);

        // 2) Let the current state react to inputs or change state.
        stateMachine.Current.HandleInput();
        stateMachine.Current.LogicUpdate();


        // 3) Resolve camera-relative movement vector (XZ only).
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * inputVec.z + cameraRight * inputVec.x;

        // 4) Compute target speed (walk/run, optionally blocked by crouch).
        float targetSpeed = 0f;
        if (Inputs.MoveMagnitude > 0.01f)
        {
            targetSpeed = (Inputs.CrouchHeld && disableMovementWhileCrouched) ? 0f : (Inputs.RunningHeld ? runSpeed : walkSpeed);
        }

        if (StateFlags.AttackLock) { targetSpeed *= attackMoveLockMultiplier; } // freeze or damp movement while the state has us locked

        // 5) Smooth speed and build world-space motion vector.
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        moveDirectionWorld = moveDirection * currentSpeed;

        // 6) Optional facing: rotate towards movement direction if allowed.
        if (shouldFaceMoveDirection && !StateFlags.AttackLock && moveDirectionWorld.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirectionWorld, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 7) Vertical motion: integrate gravity first.
        verticalVelocity += gravity * Time.deltaTime;

        // 8) Apply motion once. (CharacterController expects motion *per frame*, not per second.)
        Vector3 motion = moveDirectionWorld;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        // 9) After moving, evaluate grounded using overlap sphere (robust) + CC flags (fallback).
        bool isGrounded = false;
        if (groundCheck != null)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(groundCheck.position, groundCheckRadius,
                                                         groundHitsBuffer, groundLayers,
                                                         QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                var c = groundHitsBuffer[i];
                if (c == null) continue;
                if (c.transform.root == transform.root) continue; // ignore self
                isGrounded = true; 
                break;
            }
        }

        if (!isGrounded) { isGrounded = controller.isGrounded || (controller.collisionFlags & CollisionFlags.Below) != 0; }
        IsGrounded = isGrounded;

        // 10) If grounded, stick to ground and allow states to start jumps next frame.
        if (IsGrounded && verticalVelocity < 0f) verticalVelocity = groundSticky;


        // 11) Animator parameters (simple, state-friendly booleans)
        bool isRunning = Inputs.MoveMagnitude > 0.05f && Inputs.RunningHeld && !Inputs.CrouchHeld && !StateFlags.AttackLock;
        bool isWalking = Inputs.MoveMagnitude > 0.05f && !Inputs.RunningHeld && !Inputs.CrouchHeld && !StateFlags.AttackLock;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", Inputs.CrouchHeld);
        animator.SetBool("isGrounded", isGrounded);

        // Fire crouch entry trigger on press (used by base layer transitions).
        if (Inputs.CrouchPressed) { animator.SetTrigger("crouchStart"); }
    }

    // States call this to instantly change the vertical velocity (e.g., when jump starts).
    public void SetVerticalVelocity(float v) => verticalVelocity = v;

    // Small gizmo to visualize the ground check radius.
    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
