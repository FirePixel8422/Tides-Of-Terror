using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Animator))]
public class VRHandAnimator : MonoBehaviour
{
    private Animator anim;

    [SerializeField] private float controllerButtonPressPercent;
    [SerializeField] private float _cButtonPressPercent;
    public float valueUpdateSpeed;

    private Vector3 localPos;
    [HideInInspector]
    public Quaternion localRot;


    private void Start()
    {
        anim = GetComponent<Animator>();

        localPos = transform.localPosition;
        localRot = transform.localRotation;

        UpdateScheduler.Register(OnUpdate);
    }

    public void OnBigTriggerStateChange(InputAction.CallbackContext ctx)
    {
        controllerButtonPressPercent = ctx.ReadValue<float>();
    }



    private void OnUpdate()
    {
        _cButtonPressPercent = Mathf.MoveTowards(_cButtonPressPercent, controllerButtonPressPercent, valueUpdateSpeed * Time.deltaTime);
        anim.SetFloat("GrabStrength", _cButtonPressPercent);
    }




    public void UpdateHandTransform(Vector3 pos, Quaternion rot, bool flipHand = false)
    {
        Quaternion targetRot = rot;
        if (flipHand)
        {
            targetRot *= Quaternion.Euler(0, 0, 180);
        }

        transform.SetPositionAndRotation(pos, targetRot);
    }
    public void UpdateHandTransform(Vector3 pos)
    {
        transform.position = pos;
    }


    public void ResetHandTransform()
    {
        transform.SetLocalPositionAndRotation(localPos, localRot);
    }

    private void OnDestroy()
    {
        UpdateScheduler.Unregister(OnUpdate);
    }
}
