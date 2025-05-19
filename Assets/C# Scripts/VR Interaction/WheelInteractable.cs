using Unity.Mathematics;
using UnityEngine;

public class WheelInteractable : Interactable
{
    [SerializeField] private Transform boatTransform;

    [Header("What axis can this interactable turn?")]
    [SerializeField] private SingleTurnConstraints turnOnAxis;

    [Header("How much degrees per second does boat rotate at full steer (L/R)")]
    [SerializeField] private float boatSteerSpeed = 1;

    [Header("Pinpoints where the hands clip onto when the wheel is held and rotation offset")]
    [SerializeField] private Vector3[] handPinPoints;
    [SerializeField] private float pinPointDist = 1;

    [SerializeField] private Quaternion handRotOffset = Quaternion.identity;
    [SerializeField] private Vector3 handPosOffset = Vector3.zero;

    [SerializeField] private float pinPointMaxRange = 0.5f;

    [SerializeField] private float steerAngle;
    [SerializeField] private float steerAngleClamp = 360;

    [Header("Max rotational power of the wheel and rot power decay speed")]
    [SerializeField] private float rotSpeed;
    [SerializeField] private float wheelSpinPower = 5;
    [SerializeField] private float maxRotSpeed = 200;
    [SerializeField] private float rotDecayPercent = 0.2f;

    [Header("1 divided by how much time for wheel to fully spin back to center")]
    [SerializeField] private float centeringStrength = 0.1f;

    [Header("How often and how hard should tiny waves affect the wheel")]
    [SerializeField] private MinMaxFloat waveResetSteerInterval = new MinMaxFloat(1, 3);
    [SerializeField] private MinMaxFloat waveResetSteerPower = new MinMaxFloat(-3, 3);

    [Header("How often and how hard should a random hard wave spin the wheel")]
    [SerializeField] private float heavyWaveChance = 10;
    [SerializeField] private float heavyWaveInterval = 15;
    [SerializeField] private MinMaxFloat heavyWavePower = new MinMaxFloat(50, 125);

    private float cWaveSteerPower;
    private float cWaveSteerResetTime;
    private float cWaveSteerResetInterval;

    private float cHeavyWaveTime;

    private Vector3 rotDirection;

    private Hand leftHand;
    private Hand rightHand;

    private int leftHandPinPointIndex = -1;
    private int rightHandPinPointIndex = -1;

    private Vector3 leftHandPrevLocalPos;
    private Vector3 rightHandPrevLocalPos;



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


        cWaveSteerResetInterval = EzRandom.Range(waveResetSteerInterval);
        cWaveSteerPower = EzRandom.Range(waveResetSteerPower);
    }


    private void OnEnable() => UpdateScheduler.Register(OnUpdate);
    private void OnDisable() => UpdateScheduler.Unregister(OnUpdate);


    public override void Pickup(InteractionController handInteractor)
    {
        //call pickup for non interacting hand
        if (handInteractor.hand.isLeftHand)
        {
            if (SnapHandToClosestWheelPinPoint(handInteractor.hand))
            {
                leftHand = handInteractor.hand;
            }
            else
            {
                handInteractor.ForceDrop();
                return;
            }
        }
        else
        {
            if (SnapHandToClosestWheelPinPoint(handInteractor.hand))
            {
                rightHand = handInteractor.hand;
            }
            else
            {
                handInteractor.ForceDrop();
                return;
            }
        }

        heldByPlayer = true;
    }

    public override void Drop(HandType handType)
    {
        if (handType == HandType.Left)
        {
            leftHand.vrHandAnimator.ResetHandTransform();
            leftHand = null;
            leftHandPinPointIndex = -1;

            if (rightHand == null)
            {
                heldByPlayer = false;
            }
        }
        else
        {
            rightHand.vrHandAnimator.ResetHandTransform();
            rightHand = null;
            rightHandPinPointIndex = -1;

            if (leftHand == null)
            {
                heldByPlayer = false;
            }
        }
    }

    public override void Throw(HandType handType, float3 velocity, float3 angularVelocity)
    {
        if (handType == HandType.Left)
        {
            leftHand.vrHandAnimator.ResetHandTransform();
            leftHand = null;

            if (rightHand == null)
            {
                heldByPlayer = false;
            }
        }
        else
        {
            rightHand.vrHandAnimator.ResetHandTransform();
            rightHand = null;

            if (leftHand == null)
            {
                heldByPlayer = false;
            }
        }
    }


    private void OnUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (heldByPlayer)
        {
            Quaternion boatRot = boatTransform.rotation;
            Vector3 boatPos = boatTransform.position;
            boatTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            Vector3 wheelpos = transform.position;

            float addedRotSpeed = 0;

            if (leftHandPinPointIndex != -1)
            {
                if (UpdateHandTransform(leftHand, wheelpos))
                {
                    Vector3 newHandLocalPos = leftHand.interactionController.transform.position;
                    addedRotSpeed += ApplyHandRotation(wheelpos, newHandLocalPos, leftHandPrevLocalPos);
                    leftHandPrevLocalPos = newHandLocalPos;
                }
            }

            if (rightHandPinPointIndex != -1)
            {
                if (UpdateHandTransform(rightHand, wheelpos))
                {
                    Vector3 newHandLocalPos = rightHand.interactionController.transform.position;
                    addedRotSpeed += ApplyHandRotation(wheelpos, newHandLocalPos, rightHandPrevLocalPos);
                    rightHandPrevLocalPos = newHandLocalPos;
                }
            }

            //if both hands are on the wheel, they both contriubute half to the rotation
            if (rightHandPinPointIndex != -1 && leftHandPinPointIndex != -1)
            {
                addedRotSpeed *= 0.5f;
            }

            rotSpeed += addedRotSpeed;

            float prevStearAngle = steerAngle;

            steerAngle = math.clamp(steerAngle + addedRotSpeed, -steerAngleClamp, steerAngleClamp);

            //add new steer change to rotSpeed
            rotSpeed = math.clamp(rotSpeed + (steerAngle - prevStearAngle) * wheelSpinPower / Time.deltaTime, -maxRotSpeed, maxRotSpeed);

            transform.localRotation = Quaternion.Euler(rotDirection * steerAngle);

            boatTransform.SetPositionAndRotation(boatPos, boatRot);
        }
        else
        {
            cWaveSteerResetTime += deltaTime;
            //if cWaveSteerResetTime is greater than the interval, reset the interval and set a new interval and wave power
            if (cWaveSteerResetTime >= cWaveSteerResetInterval)
            {
                cWaveSteerResetInterval = EzRandom.Range(waveResetSteerInterval);

                cWaveSteerPower = EzRandom.Range(waveResetSteerPower);

                cWaveSteerResetTime = 0;
            }

            cHeavyWaveTime += deltaTime;
            if (cHeavyWaveTime >= heavyWaveInterval)
            {
                //if random chance happends > apply heavy wave to rotational power of the wheel
                if (EzRandom.Chance(heavyWaveChance))
                {
                    bool rotDirectionRight = EzRandom.CoinFlip();

                    float rotationalPower = EzRandom.Range(heavyWavePower);

                    rotSpeed += rotDirectionRight ? rotationalPower : -rotationalPower;
                }

                cHeavyWaveTime = 0;
            }

            WaveUpdateSteeringWheel();
        
            UpdateRotation();
        }

        DecayRotSpeed();

        float boatRotY = steerAngle == 0 ? 0 : steerAngle / steerAngleClamp;

        boatTransform.Rotate(Vector3.up, boatRotY * boatSteerSpeed * deltaTime);
    }


    private bool SnapHandToClosestWheelPinPoint(Hand hand)
    {
        Quaternion boatRot = boatTransform.rotation;
        Vector3 boatPos = boatTransform.position;
        boatTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        Vector3 handPos = hand.interactionController.transform.position;

        float closestDist = float.MaxValue;
        float dist;
        int targetIndex = 0;

        for (int i = 0; i < handPinPoints.Length; i++)
        {
            dist = Vector3.Distance(CalculateWheelPinPoint(i), handPos);

            if (dist < closestDist && dist < pinPointMaxRange)
            {
                closestDist = dist;
                targetIndex = i;
            }
        }

        //if no pinPoint is in range, return false
        if (targetIndex == -1) return false;


        //otherwise save closest pinPoint index
        if (hand.isLeftHand)
        {
            leftHandPinPointIndex = targetIndex;
            leftHandPrevLocalPos = handPos;

            hand.vrHandAnimator.UpdateHandTransform(CalculateWheelPinPoint(targetIndex));
        }
        else
        {
            rightHandPinPointIndex = targetIndex;
            rightHandPrevLocalPos = handPos;

            hand.vrHandAnimator.UpdateHandTransform(CalculateWheelPinPoint(targetIndex));
        }

        boatTransform.SetPositionAndRotation(boatPos, boatRot);

        return true;
    }

    private bool UpdateHandTransform(Hand hand, Vector3 wheelpos)
    {
        bool isLeftHand = hand.isLeftHand;

        Vector3 targetPos = CalculateWheelPinPoint(isLeftHand ? leftHandPinPointIndex : rightHandPinPointIndex) + handPosOffset;

        if (Vector3.Distance(hand.interactionController.transform.position, targetPos) > pinPointMaxRange)
        {
            hand.interactionController.ForceDrop();

            return false;
        }
        else
        {
            Vector3 toWheelCenter = (wheelpos - targetPos).normalized;
            Quaternion handRotation = Quaternion.LookRotation(toWheelCenter, transform.up) * handRotOffset;

            if (isLeftHand == false)
            {
                handRotation *= Quaternion.Euler(180, 180, 0);
            }

            hand.vrHandAnimator.UpdateHandTransform(targetPos, handRotation);

            return true;
        }
    }

    private float ApplyHandRotation(Vector3 wheelPos, Vector3 currentPos, Vector3 previousPos)
    {
        Vector3 toHandCurrent = currentPos - wheelPos;
        Vector3 toHandPrevious = previousPos - wheelPos;

        Vector3 axisVector = GetAxisVector(turnOnAxis);
        toHandCurrent = Vector3.ProjectOnPlane(toHandCurrent, axisVector);
        toHandPrevious = Vector3.ProjectOnPlane(toHandPrevious, axisVector);

        // Calculate the angle between the two hand positions around the wheel's center
        float angleDelta = Vector3.Angle(toHandPrevious, toHandCurrent);

        // Calculate the direction of the rotation (clockwise or counterclockwise)
        Vector3 crossProduct = Vector3.Cross(toHandPrevious, toHandCurrent);
        float sign = (Vector3.Dot(crossProduct, axisVector) > 0) ? 1 : -1;

        // Return the signed angle delta for rotation
        return sign * angleDelta;
    }

    private Vector3 GetAxisVector(SingleTurnConstraints axis)
    {
        switch (axis)
        {
            case SingleTurnConstraints.X: return transform.right;
            case SingleTurnConstraints.Y: return transform.up;
            case SingleTurnConstraints.Z: return transform.forward;
            default: return Vector3.zero;
        }
    }



    private Vector3 CalculateWheelPinPoint(int targetIndex)
    {
        return transform.TransformPoint(handPinPoints[targetIndex].normalized * pinPointDist);
    }

    private void WaveUpdateSteeringWheel()
    {
        steerAngle += cWaveSteerPower * Time.deltaTime;

        steerAngle = Mathf.Lerp(steerAngle, 0f, centeringStrength * Time.deltaTime);
    }

    private void DecayRotSpeed()
    {
        rotSpeed = Mathf.MoveTowards(rotSpeed, 0, maxRotSpeed * rotDecayPercent * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        steerAngle = Mathf.Clamp(steerAngle + rotSpeed * Time.deltaTime, -steerAngleClamp, steerAngleClamp);

        transform.localRotation = Quaternion.Euler(rotDirection * steerAngle);
    }


#if UNITY_EDITOR

    [Space(12)]

    [SerializeField] private InteractionController DEBUG_handL;
    [SerializeField] private InteractionController DEBUG_handR;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        for (int i = 0; i < handPinPoints.Length; i++)
        {
            Gizmos.DrawLine(transform.position, CalculateWheelPinPoint(i));

            Gizmos.DrawWireSphere(CalculateWheelPinPoint(i), pinPointMaxRange * 0.5f);
        }

        Gizmos.color = Color.green;
        Vector3 pos;

        if (DEBUG_handL != null && DrawHandPinPointGizmo(DEBUG_handL.transform.position, out pos))
        {
            Gizmos.DrawWireSphere(pos, pinPointMaxRange * 0.5f);
        }

        if (DEBUG_handR != null && DrawHandPinPointGizmo(DEBUG_handR.transform.position, out pos))
        {
            Gizmos.DrawWireSphere(pos, pinPointMaxRange * 0.5f);
        }
    }

    private bool DrawHandPinPointGizmo(Vector3 handPos, out Vector3 gizmoSpherePos)
    {
        float closestDist = float.MaxValue;
        float dist;
        int selecedPinPoint = -1;

        for (int i = 0; i < handPinPoints.Length; i++)
        {
            dist = Vector3.Distance(CalculateWheelPinPoint(i), handPos);

            if (dist < closestDist && dist < pinPointMaxRange)
            {
                closestDist = dist;
                selecedPinPoint = i;
            }
        }

        if (selecedPinPoint != -1)
        {
            gizmoSpherePos = CalculateWheelPinPoint(selecedPinPoint);
            return true;
        }

        gizmoSpherePos = Vector3.zero;
        return false;
    }

    [ContextMenu("Grab")]
    private void GrabWheel()
    {
        DEBUG_handL.DEBUG_ForcePickup(this);
        DEBUG_handR.DEBUG_ForcePickup(this);
    }

#endif
}