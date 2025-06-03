using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class Pickupable : Interactable
{
    [SerializeField] private InteractionController connectedHandController;

    [Header("Pickup Settings")]
    [SerializeField] protected Vector3 pickupPosOffset;

    [SerializeField] protected Vector3 pickUpRotOffset;


    [Header("How hard can you throw this object")]
    [SerializeField] private float throwVelocityMultiplier = 1;

    [Header("Does this object recieve angular velocity when thrown?")]
    [SerializeField] private bool throwUseAngularVelocity = true;

    [Header("Max velocity on each axis (direction is kept)")]
    [SerializeField] private float3 throwVelocityClamp = new Vector3(10, 10, 10);

    [Header("Release object with 0 velocity of released with less then minRequiredVelocity")]
    [SerializeField] private float throwMinRequiredVelocityXYZ = 0.065f;


    private Rigidbody rb;
    private Collider[] colliders;



    protected override void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        List<Collider> coll = GetComponents<Collider>().ToList();
        for (int i = 0; i < coll.Count; i++)
        {
            if (coll[i].isTrigger)
            {
                coll.RemoveAt(i);
            }
        }

        colliders = coll.ToArray();
    }


    public override void Pickup(InteractionController handInteractor)
    {
        base.Pickup(handInteractor);

        //let the current connected hand drop this item if other tries to pick it up
        if (connectedHandController != null)
        {
            connectedHandController.ForceDrop();
        }

        connectedHandController = handInteractor;


        transform.SetParent(handInteractor.HeldItemHolder, false, false);

        TogglePhysics(false);


        if (pickupPosOffset != Vector3.zero)
        {
            transform.localPosition += pickupPosOffset;
        }
        if (pickUpRotOffset != Vector3.zero)
        {
            transform.localRotation *= Quaternion.Euler(pickUpRotOffset);
        }

        if (handInteractor.hand.IsRightHand)
        {
            transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - 180, transform.localEulerAngles.z);
        }

        rb.isKinematic = true;
    }


    public override void Drop(HandType handType)
    {
        base.Drop(handType);

        connectedHandController = null;
        transform.parent = null;

        TogglePhysics(true);
        rb.isKinematic = false;
    }

    public override void Throw(HandType handType, float3 throwVelocity, float3 moveVelocity, float3 angularVelocity)
    {
        Drop(handType);

        float3 targetVelocity = throwVelocity * throwVelocityMultiplier + moveVelocity;

        //only if velocity is MORE then minRequiredVelocityXYZ set rigidBody velocity to targetVelocity
        if (math.abs(targetVelocity.x) + math.abs(targetVelocity.y) + math.abs(targetVelocity.z) > throwMinRequiredVelocityXYZ)
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


    public void TogglePhysics(bool state, bool keepColliders = false)
    {
        if (keepColliders == false)
        {
            foreach (Collider coll in colliders)
            {
                coll.enabled = state;
            }
        }

        rb.constraints = state ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (heldByPlayer)
        {
            connectedHandController.ForceDrop();
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
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.3f); // Visualize center of mass
        }
    }
#endif
}
