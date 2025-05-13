using Unity.Mathematics;
using UnityEngine;



public class WheelInteractable : Interactable
{
    [Header("What axises can this interactable turn?")]
    [SerializeField] private SingleTurnConstraints turnOnAxis;


    [SerializeField] private bool snapHandPosToWheel;
    [SerializeField] private Vector3[] handPinPoints;
    [SerializeField] private float pinPointDist;

    [SerializeField] private float interactionRange;


    [SerializeField] private float rotSpeed;
    [SerializeField] private float maxRotSpeed;
    [SerializeField] private float rotDecayPercent;

    [SerializeField] private float waveResetSteerIntervalMin, waveResetSteerIntervalMax;
    [SerializeField] private float waveResetSteerPowerMin, waveResetSteerPowerMax;
    [SerializeField] private float cWaveSteerPower;

    private float cWaveSteerResetTime, cWaveSteerResetInterval;

    [SerializeField] private float steerAngleClamp;


    private Vector3 rotDirection;
    [SerializeField] private float steerAngle;



    protected override void Start()
    {
        UpdateScheduler.Register(OnUpdate);

        rotDirection = Vector3.zero;

        if (turnOnAxis.HasFlag(SingleTurnConstraints.X))
        {
            rotDirection = Vector3.right;
        }
        else if (turnOnAxis.HasFlag(SingleTurnConstraints.Y))
        {
            rotDirection = Vector3.up;
        }
        else if (turnOnAxis.HasFlag(SingleTurnConstraints.Z))
        {
            rotDirection = Vector3.forward;
        }


        cWaveSteerResetInterval = EzRandom.Range(waveResetSteerIntervalMin, waveResetSteerIntervalMax);
        cWaveSteerPower = EzRandom.Range(waveResetSteerPowerMin, waveResetSteerPowerMax);
    }

    private void OnEnable() => UpdateScheduler.Register(OnUpdate);
    private void OnDisable() => UpdateScheduler.Unregister(OnUpdate);


    public override void Pickup(InteractionController handInteractor)
    {
        Hand leftHand = Hand.Left;
        Hand rightHand = Hand.Right;

        leftHand.interactionController.ForceDrop();
        rightHand.interactionController.ForceDrop();

        leftHand.vrHandAnimator.ResetHandTransform();
        rightHand.vrHandAnimator.ResetHandTransform();

        connectedHand = handInteractor;
        heldByPlayer = true;


        //call pickup for non interacting hand
        if (handInteractor.hand.isLeftHand)
        {
            SetupUnconnectedHand(leftHand, transform.position);
        }
        else
        {
            SetupUnconnectedHand(rightHand, transform.position);
        }
    }

    private void SetupUnconnectedHand(Hand hand, Vector3 wheelPos)
    {
        Vector3 handTransformPos = hand.vrHandAnimator.transform.position;

        float handDistanceToTransform = Vector3.Distance(transform.position, handTransformPos);

        if (handDistanceToTransform > interactionRange)
        {
            connectedHand.hand.vrHandAnimator.ResetHandTransform();

            connectedHand.isHoldingObject = false;
            connectedHand = null;
            heldByPlayer = false;
        }
        else
        {
            Vector3 handPos = connectedHand.hand.vrHandAnimator.transform.position;

            if (turnOnAxis.HasFlag(SingleTurnConstraints.X))
            {
                handPos.x = wheelPos.x;
            }
            else if (turnOnAxis.HasFlag(SingleTurnConstraints.Y))
            {
                handPos.y = wheelPos.y;
            }
            else if (turnOnAxis.HasFlag(SingleTurnConstraints.Z))
            {
                handPos.z = wheelPos.z;
            }

            connectedHand.hand.vrHandAnimator.UpdateHandTransform(handPos);
        }

        hand.interactionController.enabled = false;
    }




    public override void Drop()
    {
        Hand.Left.vrHandAnimator.ResetHandTransform();
        Hand.Right.vrHandAnimator.ResetHandTransform();

        base.Drop();
    }

    public override void Throw(float3 velocity, float3 angularVelocity)
    {
        Hand.Left.vrHandAnimator.ResetHandTransform();
        Hand.Right.vrHandAnimator.ResetHandTransform();

        base.Throw(velocity, angularVelocity);
    }


    private void OnUpdate()
    {
        if (heldByPlayer)
        {
            if (snapHandPosToWheel)
            {
                Vector3 transformPos = transform.position;
                Vector3 handTransformPos = connectedHand.transform.position;

                SnapHandToTransform(transformPos, handTransformPos);
            }
        }
        else
        {
            cWaveSteerResetTime += Time.deltaTime;

            if (cWaveSteerResetTime >= cWaveSteerResetInterval)
            {
                cWaveSteerResetInterval = EzRandom.Range(waveResetSteerIntervalMin, waveResetSteerIntervalMax);

                cWaveSteerPower = EzRandom.Range(waveResetSteerPowerMin, waveResetSteerPowerMax);

                cWaveSteerResetTime = 0;
            }

            WaveUpdateSteeringWheel();

            DecayRotSpeed();
        }

        UpdateRotation();
    }


    private void SnapHandToTransform(Vector3 wheelPos, Vector3 handTransformPos)
    {
        float handDistanceToTransform = Vector3.Distance(transform.position, handTransformPos);

        if (handDistanceToTransform > interactionRange)
        {
            Hand.Left.vrHandAnimator.ResetHandTransform();
            Hand.Right.vrHandAnimator.ResetHandTransform();

            connectedHand.isHoldingObject = false;
            connectedHand = null;
            heldByPlayer = false;
        }
        else
        {
            Vector3 handPos = connectedHand.hand.vrHandAnimator.transform.position;

            if (turnOnAxis.HasFlag(SingleTurnConstraints.X))
            {
                handPos.x = wheelPos.x;
            }
            else if (turnOnAxis.HasFlag(SingleTurnConstraints.Y))
            {
                handPos.y = wheelPos.y;
            }
            else if (turnOnAxis.HasFlag(SingleTurnConstraints.Z))
            {
                handPos.z = wheelPos.z;
            }

            connectedHand.hand.vrHandAnimator.UpdateHandTransform(handPos);
        }
    }

    private void WaveUpdateSteeringWheel()
    {
        steerAngle += cWaveSteerPower * Time.deltaTime;
    }

    private void DecayRotSpeed()
    {
        rotSpeed = Mathf.MoveTowards(rotSpeed, 0, maxRotSpeed * rotDecayPercent * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        steerAngle = Mathf.Clamp(steerAngle + rotSpeed * Time.deltaTime, -steerAngleClamp, steerAngleClamp);

        transform.rotation = Quaternion.Euler(rotDirection * steerAngle);
    }




    private void OnDestroy()
    {
        UpdateScheduler.Unregister(OnUpdate);
    }


    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < handPinPoints.Length; i++)
        {
            Gizmos.DrawLine(transform.position, transform.position + handPinPoints[i].normalized * pinPointDist);
            Gizmos.DrawWireSphere(transform.position + handPinPoints[i].normalized * pinPointDist, 0.05f);
        }
    }
}
