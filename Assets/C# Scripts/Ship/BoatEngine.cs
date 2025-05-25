using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoatEngine : MonoBehaviour
{
    [SerializeField] private float enginePower;


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        transform.position += enginePower * Time.deltaTime * transform.forward;
    }
}
