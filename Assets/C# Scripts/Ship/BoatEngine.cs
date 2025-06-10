using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;



public class BoatEngine : MonoBehaviour
{
    public static BoatEngine Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }



    [SerializeField] private float enginePower = 1;
    [SerializeField] private float forwardSwaySpeed = 2;
    [SerializeField] private float forwardSwayInterval = 2;
    [SerializeField] private float turnSwaySpeed = 12;

    [SerializeField] private float health = 100;

    public float swayAngle;
    public bool locked;

    public Transform chestPoint;



    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        float forwardSway = math.sin(Time.time * forwardSwayInterval) * forwardSwaySpeed;
        float turnSway = -swayAngle * turnSwaySpeed;

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, turnSway + forwardSway);

        if (locked) return;

        transform.position += enginePower * Time.deltaTime * (Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward);
    }


    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            GameRestartHandler.Instance.Lose();
        }
    }
}
