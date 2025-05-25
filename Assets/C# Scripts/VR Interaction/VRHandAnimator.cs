using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Animator))]
public class VRHandAnimator : MonoBehaviour
{
    private Animator anim;

    [SerializeField] private float valueUpdateSpeed;
    [SerializeField] private float controllerButtonPressPercent;
    [SerializeField] private float cButtonPressPercent;

    private Vector3 localPos;
    private Quaternion localRot;


    private void Start()
    {
        anim = GetComponent<Animator>();

        localPos = transform.localPosition;
        localRot = transform.localRotation;
    }

    public void OnBigTriggerStateChange(InputAction.CallbackContext ctx)
    {
        controllerButtonPressPercent = ctx.ReadValue<float>();
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        cButtonPressPercent = Mathf.MoveTowards(cButtonPressPercent, controllerButtonPressPercent, valueUpdateSpeed * Time.deltaTime);
        anim.SetFloat("GrabStrength", cButtonPressPercent);
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
}
