using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Playermovement : NetworkBehaviour
{

    [Header("Movement")]
    private float MoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpforce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;


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



    [Header("Slope Hnadling")]
    public float maxSlopeAngle;
    private RaycastHit slopHit;
    private bool exitingSlope;
    Rigidbody rb;
    public MovementState state;

    private PlayerCam playerCamScript;
    public MoveCamera moveCameraScript;

    [SerializeField] Camera fpsCam;

    private Transform cameraPos;

    private GameObject ui;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }


    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

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
        Debug.Log(grounded);
        wasGrounded = grounded;
        MyInput(); // adding hide ui to this
        MovePlayer();
        SpeedControl();
        StateHandler();
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
            rb.linearDamping = 0;

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.SetActive(!ui.activeInHierarchy);
        }
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

    private void SpeedControl()
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


    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpforce, ForceMode.Impulse);
    }


    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }


    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }


    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopHit.normal).normalized;
    }


    private void StateHandler()
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

}
