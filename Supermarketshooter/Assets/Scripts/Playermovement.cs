using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Playermovement : NetworkBehaviour
{

    [Header("Movement")]
    public float MoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float butterGroundDrag;
    private float baseGroundDrag;
    public float jumpforce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    [HideInInspector]
    public bool isButtered = false;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;


    private float maxFallSpeed = 0f;
    private bool wasGrounded = true;

    [Header("Double Jump")]
    public int maxJumps = 1;            // How many times we can jump before touching the ground
    private int jumpCount = 0;

    [Header("Slope Hnadling")]
    public float maxSlopeAngle;
    private RaycastHit slopHit;
    private bool exitingSlope;
    Rigidbody rb;
    public MovementState state;

    public PlayerCam playerCamScript;
    public MoveCamera moveCameraScript;

    [SerializeField] Camera fpsCam;

    public Transform cameraPos;

    public GameObject ui;

    public Gun_Base gun;

    [Header("Falling")]
    public float fallMultiplier = 5f;  // Tweak as needed


    [Header("Coyote Time")]
    public float coyoteTime = 0.2f;          // Duration to still allow jumping after stepping off
    private float coyoteTimeCounter;

    public bool canMove = true;

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }


    public void Start()
    {
        // set up seed generation
        FindFirstObjectByType<SeedGenManager>().PlayerJoinOrHost();
        // get bullets
        MultiplayerHandler.Instance.SpawnBulletsStart();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        baseGroundDrag = groundDrag;
        // checks if this player is the local one anything that would be set for
        // only this player should be set in this statement(camera, orientation, etc)
        if (IsLocalPlayer)
        {
            ui = GameObject.Find("Canvas"); // to hide ui
            fpsCam.gameObject.SetActive(true);
            cameraPos = transform.Find("CameraPos");
            if (cameraPos == null)
            {
                Debug.LogWarning("not found");
                return;
            }
            if (playerCamScript == null)
            {
                playerCamScript = Object.FindFirstObjectByType<PlayerCam>();
                playerCamScript.enabled = true;
                if (playerCamScript == null)
                {
                    Debug.LogWarning("no script");
                    return;
                }
            }
            playerCamScript.orientation = orientation;
            if (moveCameraScript == null)
            {
                moveCameraScript = Object.FindFirstObjectByType<MoveCamera>();
                if (moveCameraScript == null)
                {
                    Debug.LogWarning("no script");
                    return;
                }
            }
            moveCameraScript.cameraPosition = cameraPos;
            gun.fpsCam = fpsCam;
        }
        else
        {
            fpsCam.gameObject.SetActive(false);
        }
    }

    public void OnEnable()
    {
        if (IsLocalPlayer)
        {
            fpsCam.gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        // only allow the active player to control this object
        if (!IsOwner) return;


        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        if (grounded && !wasGrounded)
        {
 
            jumpCount = 0;
            coyoteTimeCounter = coyoteTime;
        }
        else if (!grounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }


        wasGrounded = grounded;




        MyInput(); // adding hide ui to this
        if (canMove)
        {
            MovePlayer();
        }
        SpeedControl();
        StateHandler();
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
            rb.linearDamping = 0;

    }

    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");



        if (Input.GetKeyDown(jumpKey) && jumpCount < maxJumps && readyToJump)
        {
            Debug.Log("Jump pressed. jumpCount=" + jumpCount +
  " | coyoteTimeCounter=" + coyoteTimeCounter +
  " | readyToJump=" + readyToJump);
            if (jumpCount == 0)
            {

                if (grounded || coyoteTimeCounter > 0f)
                {
                    DoTheJump();
                }
            }

            else
            {

                DoTheJump();
            }
        }



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.SetActive(!ui.activeInHierarchy);
        }
    }
    void DoTheJump()
    {
        coyoteTimeCounter = 0f; 
        readyToJump = false;
        Jump();
        jumpCount++;
        Invoke(nameof(ResetJump), jumpCooldown);
    }
    public void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * MoveSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        if (grounded)
            rb.AddForce(moveDirection.normalized * MoveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * MoveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();

    }

    public void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > MoveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * MoveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > MoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * MoveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }


    public void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpforce, ForceMode.Impulse);
    }


    public void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }


    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }


    public Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopHit.normal).normalized;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;


        if (!grounded && !OnSlope() && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
        }
    }
    public void StateHandler()
    {
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            MoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            MoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    public void GotButtered()
    {
        if (!isButtered)
        {
            StartCoroutine(ButterTimer());
        }
       
    }

    public IEnumerator ButterTimer()
    {
        groundDrag = butterGroundDrag;
        yield return new WaitForSeconds(3);
        groundDrag = baseGroundDrag;
    }

}
