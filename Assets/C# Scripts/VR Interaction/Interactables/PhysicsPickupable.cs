using Unity.Mathematics;
using UnityEngine;


public class PhysicsPickupable : Interactable
{
    private Transform headTransform;
    private InteractionController leftHandController;
    private InteractionController rightHandController;

    private Quaternion playerInvRotationOffset;
    private Vector3 leftHandOffset;
    private Vector3 rightHandOffset;

    public float A, B;

    [Header("How hard can you throw this object")]
    [SerializeField] private float throwVelocityMultiplier = 1;

    [Header("Does this object recieve angular verlocity when thrown?")]
    [SerializeField] private bool useAngularVelocity = true;

    [Header("Max velocity on each axis (direction is kept)")]
    [SerializeField] private float3 velocityClamp = new Vector3(10, 10, 10);

    [Header("Release object with 0 velocity of released with less then minRequiredVelocity")]
    [SerializeField] private float minRequiredVelocityXYZ = 0.065f;


    private Rigidbody rb;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }


    public override void Pickup(InteractionController handInteractor)
    {
        if (handInteractor.hand.IsLeftHand)
        {
            leftHandController = handInteractor;
            leftHandOffset = handInteractor.transform.position - transform.position;
        }
        else
        {
            rightHandController = handInteractor;
            rightHandOffset = handInteractor.transform.position - transform.position;
        }

        if (heldByPlayer == false)
        {
            headTransform = handInteractor.hand.transform.parent;
            playerInvRotationOffset = Quaternion.Inverse(headTransform.rotation);
        }

        heldByPlayer = true;
        OnInteract?.Invoke();

        UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    }


    public override void Drop(HandType handType)
    {
        base.Drop(handType);

        if (handType == HandType.Left)
        {
            leftHandController = null;
        }
        else
        {
            rightHandController = null;
        }

        UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);
    }

    public override void Throw(HandType handType, float3 velocity, float3 angularVelocity)
    {
        Drop(handType);

        float3 targetVelocity = velocity * throwVelocityMultiplier;

        //only if velocity is MORE then minRequiredVelocityXYZ set rigidBody velocity to targetVelocity
        if (math.abs(targetVelocity.x) + math.abs(targetVelocity.y) + math.abs(targetVelocity.z) > minRequiredVelocityXYZ)
        {
            if (useAngularVelocity)
            {
                rb.angularVelocity = angularVelocity;
            }

            // Calculate the radius vector from the center of mass to the point
            float3 radius = transform.position - rb.worldCenterOfMass;

            // Calculate the linear velocity caused by angular velocity
            float3 tangentialVelocity = Vector3.Cross(angularVelocity, radius);

            rb.velocity = VectorLogic.ClampDirection(targetVelocity + tangentialVelocity, velocityClamp);
        }
    }


    private void OnFixedUpdate()
    {
        Vector3 targetPos = Vector3.zero;
        int activeHandCount = 0;

        if (leftHandController != null)
        {
            targetPos += leftHandController.transform.position - leftHandOffset;
            activeHandCount++;
        }
        if (rightHandController != null)
        {
            targetPos += rightHandController.transform.position - rightHandOffset;
            activeHandCount++;
        }

        targetPos /= activeHandCount;

        Vector3 toTarget = targetPos - rb.position;

        // Optional: limit max force or dampen
        float distance = toTarget.magnitude;
        Vector3 force = toTarget.normalized * A * Time.fixedDeltaTime;

        // Optionally scale with distance
        force *= distance;

        rb.AddForce(force, ForceMode.Acceleration);

        // Optional: apply torque to rotate toward your head transform if needed
        Quaternion targetRotation = headTransform.rotation * playerInvRotationOffset;
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        Vector3 torque = axis * angle * B * Time.fixedDeltaTime;
        rb.AddTorque(torque, ForceMode.Acceleration);
    }



#if UNITY_EDITOR
    public bool debugRBCenterOfMass;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (debugRBCenterOfMass && TryGetComponent(out rb))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.03f); // Visualize center of mass
        }
    }
#endif
}
