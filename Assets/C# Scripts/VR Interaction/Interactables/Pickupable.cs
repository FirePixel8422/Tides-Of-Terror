using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
[BurstCompile]
public class Pickupable : Interactable
{
    [Header("Pickup Settings")]
    [SerializeField] protected Vector3 pickupPosOffset;

    [SerializeField] protected Vector3 pickUpRotOffset;


    [Header("How hard can you throw this object")]
    [SerializeField] private float throwVelocityMultiplier = 1;

    [Header("Does this object recieve angular verlocity when thrown?")]
    [SerializeField] private bool useAngularVelocity = true;

    [Header("Max velocity on each axis (direction is kept)")]
    [SerializeField] private float3 velocityClamp = new Vector3(10, 10, 10);

    [Header("Release object with 0 velocity of released with less then minRequiredVelocity")]
    [SerializeField] private float minRequiredVelocityXYZ = 0.065f;


    private Rigidbody rb;
    private Collider[] colliders;



    [BurstCompile]
    private void Awake()
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


    [BurstCompile]
    public override void Pickup(InteractionController handInteractor)
    {
        bool isRightHand = handInteractor.hand.isRightHand;

        base.Pickup(handInteractor);

        transform.SetParent(handInteractor.heldItemHolder, false, false);

        print("pickup: " + gameObject.name);

        TogglePhysics(false);


        if (pickupPosOffset != Vector3.zero)
        {
            transform.localPosition += pickupPosOffset;
        }
        if (pickUpRotOffset != Vector3.zero)
        {
            transform.localRotation *= Quaternion.Euler(pickUpRotOffset);
        }

        if (isRightHand)
        {
            transform.localPosition = new Vector3(-transform.localPosition.x, -transform.localPosition.y, -transform.localPosition.z);
            transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, -transform.localEulerAngles.y - 180, -transform.localEulerAngles.z);
        }

        rb.isKinematic = true;
    }


    [BurstCompile]
    public override void Throw(HandType handType, float3 velocity, float3 angularVelocity)
    {
        base.Throw(handType, velocity, angularVelocity);

        TogglePhysics(true);

        transform.parent = null;


        rb.isKinematic = false;


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


    [BurstCompile]
    public override void Drop(HandType handType)
    {
        base.Drop(handType);

        TogglePhysics(true);

        transform.parent = null;

        rb.isKinematic = false;
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
