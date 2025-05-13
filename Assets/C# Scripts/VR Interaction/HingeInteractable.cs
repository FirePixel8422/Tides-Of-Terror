using System;
using Unity.Mathematics;
using UnityEngine;


public class HingeInteractable : Interactable
{
    [Header("Highest parent of this interactable")]
    [SerializeField] private Transform objectRoot;

    [Header("What axises can this interactable turn?")]
    [SerializeField] private MultiTurnConstraints turnOnAxis;


    [SerializeField] private bool snapPlayerHandToTransform;
    [SerializeField] private Transform snapTransform;
    [SerializeField] private Quaternion handRotOffset;

    [SerializeField] private float interactionRange;


    //public Transform DEBUG_HAND;

    [SerializeField] private HandType forHand;
    [SerializeField] private Vector3 rotOffset;
    [SerializeField] private Vector3 rotClampMin, rotClampMax;



    protected override void Start()
    {
        UpdateScheduler.Register(OnUpdate);
    }


    public override void Pickup(InteractionController handInteractor)
    {
        if (heldByPlayer)
        {
            connectedHand.hand.vrHandAnimator.ResetHandTransform();
        }

        base.Pickup(handInteractor);
    }

    public override void Drop()
    {
        connectedHand.hand.vrHandAnimator.ResetHandTransform();

        base.Drop();
    }

    public override void Throw(float3 velocity, float3 angularVelocity)
    {
        connectedHand.hand.vrHandAnimator.ResetHandTransform();

        base.Throw(velocity, angularVelocity);
    }




    private void OnUpdate()
    {
        if(heldByPlayer == false) return;

        Vector3 transformPos = transform.position;
        Vector3 handTransformPos = connectedHand.transform.position;

        if (snapPlayerHandToTransform)
        {
            SnapHandToTransform(handTransformPos);
        }


        RotateTransform(transformPos, handTransformPos);
    }


    private void SnapHandToTransform(Vector3 handTransformPos)
    {
        float handDistanceToTransform = Vector3.Distance(snapTransform.position, handTransformPos);

        if (handDistanceToTransform > interactionRange)
        {
            connectedHand.hand.vrHandAnimator.ResetHandTransform();

            connectedHand.isHoldingObject = false;
            connectedHand = null;
            heldByPlayer = false;
        }
        else
        {
            bool flip = false;

            if (forHand != HandType.None && forHand == HandType.Left != connectedHand.hand.isLeftHand)
            {
                flip = true;
            }

            connectedHand.hand.vrHandAnimator.UpdateHandTransform(snapTransform.position, snapTransform.rotation, flip);
        }
    }


    protected virtual void RotateTransform(Vector3 transformPos, Vector3 handTransformPos)
    {
        Vector3 dir = (handTransformPos - transformPos).normalized;

        if (turnOnAxis.HasFlag(MultiTurnConstraints.X))
        {
            dir.x = 0;
        }
        if (turnOnAxis.HasFlag(MultiTurnConstraints.Y))
        {
            dir.y = 0;
        }
        if (turnOnAxis.HasFlag(MultiTurnConstraints.Z))
        {
            dir.z = 0;
        }


        Debug.DrawLine(transformPos, transformPos + dir);

        // Calculate quaternion for the local direction
        Vector3 eulerRotation = (Quaternion.LookRotation(dir) * Quaternion.Euler(rotOffset - objectRoot.eulerAngles)).eulerAngles;

        eulerRotation.x = math.clamp(NormalizeAngle(eulerRotation.x), rotClampMin.x, rotClampMax.x);
        eulerRotation.y = math.clamp(NormalizeAngle(eulerRotation.y), rotClampMin.y, rotClampMax.y);
        eulerRotation.z = math.clamp(NormalizeAngle(eulerRotation.z), rotClampMin.z, rotClampMax.z);

        transform.localEulerAngles = eulerRotation;
    }


    // Normalize angle to [-180, 180] range
    private float NormalizeAngle(float angle)
    {
        return (angle + 180f) % 360f - 180f;
    }




    protected override void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
    }
}
