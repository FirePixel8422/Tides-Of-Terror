using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class BoatEngine : MonoBehaviour
{
    [SerializeField] private float enginePower;
    [SerializeField] private float swaySpeed;


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        transform.position += enginePower * Time.deltaTime * Vector3.forward;

        float sway = math.sin(Time.time * 2f) * swaySpeed;
        sway += enginePower;

        transform.Rotate(Vector3.forward, sway);
    }
}
