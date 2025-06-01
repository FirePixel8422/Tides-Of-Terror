using TMPro;
using Unity.Mathematics;
using UnityEngine;


public class PhysicsPickupable : Interactable
{
    private Transform headTransform;
    private InteractionController leftHandController;
    private InteractionController rightHandController;

    private Vector3 leftHandOffset;
    private Vector3 rightHandOffset;

    [SerializeField] private float movePower = 5;
    [SerializeField] private float velocityDecayPower = 0.1f;

    [SerializeField] private bool keepObjectUpRight = true;
    [SerializeField] private float rotPower = 1;


    [Header("How hard can you throw this object")]
    [SerializeField] private float throwVelocityMultiplier = 1;

    [Header("Does this object recieve angular verlocity when thrown?")]
    [SerializeField] private bool throwUseAngularVelocity = true;

    [Header("Max velocity on each axis (direction is kept)")]
    [SerializeField] private float3 throwVelocityClamp = new Vector3(10, 10, 10);

    [Header("Release object with 0 velocity of released with less then minRequiredVelocity")]
    [SerializeField] private float throwMinRequiredVelocityXYZ = 0.065f;

    private Rigidbody rb;


    protected override void Start()
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

    public override void Throw(HandType handType, float3 throwVelocity, float3 moveVelocity, float3 angularVelocity)
    {
        Drop(handType);

        float3 targetVelocity = throwVelocity * throwVelocityMultiplier + moveVelocity;

        //only if velocity is MORE then minRequiredVelocityXYZ set rigidBody velocity to targetVelocity
        if (targetVelocity.AbsoluteSum() > throwMinRequiredVelocityXYZ)
        {
            if (throwUseAngularVelocity)
            {
                rb.angularVelocity = angularVelocity;
            }

            // Calculate the radius vector from the center of mass to the point
            float3 radius = transform.position - rb.worldCenterOfMass;

            // Calculate the linear velocity caused by angular velocity
            float3 tangentialVelocity = Vector3.Cross(angularVelocity, radius);

            rb.velocity = VectorLogic.ClampDirection(targetVelocity + tangentialVelocity, throwVelocityClamp);
        }
    }


    private void OnFixedUpdate()
    {
        Vector3 closestTargetPos = Vector3.zero;

        if (leftHandController != null)
        {
            closestTargetPos = leftHandController.transform.position - leftHandOffset;
        }
        if (rightHandController != null)
        {
            Vector3 newTargetPos = rightHandController.transform.position - rightHandOffset;

            if (closestTargetPos.sqrMagnitude > newTargetPos.sqrMagnitude)
            {
                closestTargetPos = newTargetPos;
            }
        }

        Vector3 moveDir = (closestTargetPos - rb.position);

        rb.velocity = new Vector3(moveDir.x * movePower, rb.velocity.y + moveDir.y, moveDir.z * movePower);


        if (keepObjectUpRight)
        {
            Quaternion currentRotation = rb.rotation;
            Vector3 currentEuler = currentRotation.eulerAngles;

            // Desired rotation: keep current Y, zero X and Z
            Quaternion targetRotation = Quaternion.Euler(0f, currentEuler.y, 0f);

            // Calculate difference
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);

            // Convert to axis-angle representation
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            // Normalize angle to avoid over-torqueing for tiny differences
            if (angleInDegrees > 180f) angleInDegrees -= 360f;

            // Calculate torque based on angle difference and power
            Vector3 torque = rotationAxis * angleInDegrees * rotPower;

            // Apply torque (in world space)
            rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
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
