using Unity.Mathematics;
using UnityEngine;


public class CannonTrail : PhysicsPickupable
{
    [Header("Trail")]
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Transform startPoint;

    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int positionCount;

    [SerializeField] private float distanceMultiplier = 1f;
    [SerializeField] private float heightMultiplier = 1f;



    protected override void Start()
    {
        base.Start();

        lineRenderer.enabled = false;

        CalculateTrail();
    }


    public override void Pickup(InteractionController handInteractor)
    {
        base.Pickup(handInteractor);

        lineRenderer.enabled = true;
    }

    public override void Drop(HandType handType)
    {
        base.Drop(handType);

        lineRenderer.enabled = false;
    }
    public override void Throw(HandType handType, float3 throwVelocity, float3 moveVelocity, float3 angularVelocity)
    {
        base.Throw(handType, throwVelocity, moveVelocity, angularVelocity);

        lineRenderer.enabled = false;
    }


    [ContextMenu("Calculate Trail")]
    private void CalculateTrail()
    {
        if (lineRenderer == null || positionCount <= 0)
        {
            return;
        }

        Vector3[] positions = new Vector3[positionCount];

        for (int i = 0; i < positionCount; i++)
        {
            float percent = (float)i / positionCount;

            positions[i] = Quaternion.Inverse(transform.rotation) * (startPoint.position + (distanceMultiplier * percent * startPoint.forward + curve.Evaluate(percent) * heightMultiplier * Vector3.up) - transform.position);
        }

        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPositions(positions);
    }
}
