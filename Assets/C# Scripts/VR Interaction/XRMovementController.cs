using UnityEngine;
using UnityEngine.InputSystem;



public class XRMovementController : MonoBehaviour
{
    public static XRMovementController Instance { get; private set; }

    [SerializeField] private Transform headTransform;

    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float turnSpeed = 90;

    [SerializeField] private Vector2 moveInput;
    [SerializeField] private Vector2 turnInput;

    private Rigidbody rb;
    public static Collider PlayerCollider;



    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>().normalized;
    }
    public void OnTurn(InputAction.CallbackContext ctx)
    {
        turnInput = ctx.ReadValue<Vector2>().normalized;
    }


    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        PlayerCollider = GetComponent<Collider>();
    }


    private void OnUpdate()
    {
        Move();
    }
    private void LateUpdate()
    {
        Turn();
    }

    private void Move()
    {
        //set velocity to moveinput and XR head transforms forward direction
        Vector3 moveDir = headTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        Vector3 vel = new Vector3(moveDir.x, 0, moveDir.z).normalized * moveSpeed;

        rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
    }

    private void Turn()
    {
        if (turnInput.x != 0)
        {
            UnityEngine.InputSystem.XR.TrackedPoseDriver.rotYOffset += turnInput.x * turnSpeed * Time.deltaTime;
        }
    }
}
