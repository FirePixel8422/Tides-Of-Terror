//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;

//public class HealthHandler : MonoBehaviour
//{
//    private Volume globalVolume;

//    [SerializeField] private AnimationCurve vignetteCurve;

//    [SerializeField] private MinMaxFloat vignetteStrength;

//    [SerializeField] private int bounceCount;
//    [SerializeField] private float bounceTime;
//    [SerializeField] private float vignetteMultiplier;
//    [SerializeField] private float bounceCooldown;

//    public float damagePercent;
//    public float volumeMaxPercent;


//    private void Start()
//    {
//        globalVolume = GetComponent<Volume>();
//    }

//    private IEnumerator VignetteLoop()
//    {
//        while (true)
//        {
//            float elapsed;
//            while (true)
//            {
//                yield return null;

//                globalVolume.weight = vignetteCurve.Evaluate( * volumeMaxPercent;
//            }

//            yield return new WaitForSeconds(bounceCooldown);
//        }
//    }
//}
