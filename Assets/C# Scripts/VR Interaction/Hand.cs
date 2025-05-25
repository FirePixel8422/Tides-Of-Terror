using System.Collections;
using Unity.Burst;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class Hand : MonoBehaviour
{
    public HandType handType;

    public static Hand Left;
    public static Hand Right;

    private void Awake()
    {
        if (handType == HandType.Left)
        {
            Left = this;
        }
        else
        {
            Right = this;
        }
    }


    public bool IsLeftHand => handType == HandType.Left;
    public bool IsRightHand => handType == HandType.Right;


    [HideInInspector] public InteractionController interactionController;
    [HideInInspector] public VRHandAnimator vrHandAnimator;

    private HapticImpulsePlayer hapticImpulsePlayer;


    private void Start()
    {
        interactionController = GetComponent<InteractionController>();
        hapticImpulsePlayer = GetComponent<HapticImpulsePlayer>();
        vrHandAnimator = GetComponentInChildren<VRHandAnimator>();
    }




    [BurstCompile]
    public void SendVibration(float amplitude, float duration)
    {
        hapticImpulsePlayer?.SendHapticImpulse(amplitude, duration);
    }
    
    [BurstCompile]
    public void SendVibration(VibrationParamaters vibrationParams)
    {
        if (vibrationParams.pulseCount > 1)
        {
            StartCoroutine(PulseVibration(vibrationParams.amplitude, vibrationParams.duration, vibrationParams.pulseCount, vibrationParams.pulseInterval));

            return;
        }

        hapticImpulsePlayer?.SendHapticImpulse(vibrationParams.amplitude, vibrationParams.duration);
    }


    private IEnumerator PulseVibration(float amplitude, float duration, int pulseCount, float pulseInterval)
    {
        WaitForSeconds waitPulseInterval = new WaitForSeconds(duration + pulseInterval);

        for (int i = 0; i < pulseCount; i++)
        {
            hapticImpulsePlayer?.SendHapticImpulse(amplitude, duration);

            yield return waitPulseInterval;
        }
    }
}