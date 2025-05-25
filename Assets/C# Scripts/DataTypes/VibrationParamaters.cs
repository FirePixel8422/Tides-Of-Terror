using UnityEngine;


[System.Serializable]
public struct VibrationParamaters
{
    public VibrationParamaters(float _amplitude, float _duration, int _pulseCount = 1, float _pulseInterval = 0)
    {
        amplitude = _amplitude;
        duration = _duration;

        pulseCount = _pulseCount;
        pulseInterval = _pulseInterval;
    }

    [Header("Vibration Strength"), Range(0f, 1f)]
    public float amplitude;

    [Header("Duration off a vibration")]
    public float duration;

    [Header("Amount of times the controller vibrates"), Range(1, 16)]
    public int pulseCount;

    [Header("Delay between vibrations (after it finished)")]
    public float pulseInterval;
}