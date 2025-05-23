using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class XRMovementController : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform turnTransform;

    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float turnSpeed = 90;

    [SerializeField] private Vector2 moveInput;
    [SerializeField] private Vector2 turnInput;

    private Rigidbody rb;



    private void OnEnable() => UpdateScheduler.Register(OnUpdate);
    private void OnDisable() => UpdateScheduler.Unregister(OnUpdate);


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>().normalized;
    }
    public void OnTurn(InputAction.CallbackContext ctx)
    {
        turnInput = ctx.ReadValue<Vector2>().normalized;
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnUpdate()
    {
        Move();
        Turn();
    }

    private void Move()
    {

#if UNITY_EDITOR

        if (overrideMoveControls)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 overrideMoveInput = new Vector3(moveX, 0f, moveZ).normalized;

            //set velocity to moveinput and XR head transforms forward direction
            Vector3 overrideMoveDir = headTransform.TransformDirection(new Vector3(overrideMoveInput.x, 0, overrideMoveInput.y).normalized);
            Vector3 overrideVel = overrideMoveDir * moveSpeed;

            rb.velocity = new Vector3(overrideVel.x, rb.velocity.y, overrideVel.z);

            return;
        }
#endif


        //set velocity to moveinput and XR head transforms forward direction
        Vector3 moveDir = headTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        Vector3 vel = new Vector3(moveDir.x, 0, moveDir.z).normalized * moveSpeed;

        rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
    }

    private void Turn()
    {
#if UNITY_EDITOR

        if (overrideMouseControls)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);

            yRotation += mouseX;

            transform.localRotation = Quaternion.Euler(0, yRotation, 0f);
            turnTransform.localRotation = Quaternion.Euler(xRotation, 0, 0f);

            return;
        }
#endif

        if (turnInput.x != 0)
        {
            turnTransform.Rotate(Vector3.up, turnInput.x * turnSpeed * Time.deltaTime);
        }
    }



#if UNITY_EDITOR

    [SerializeField] private bool overrideMoveControls = false;

    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private bool overrideMouseControls = false;

    private float xRotation, yRotation;

#endif
}
