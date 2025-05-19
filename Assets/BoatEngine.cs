using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatEngine : MonoBehaviour
{
    [SerializeField] private float enginePower;


    private void OnEnable() => UpdateScheduler.Register(OnUpdate);
    private void OnDisable() => UpdateScheduler.Unregister(OnUpdate);


    private void OnUpdate()
    {
        transform.position += transform.forward * enginePower * Time.deltaTime;
    }
}
