using System.Collections;
using UnityEngine;



public class Cannon : Interactable
{
    [SerializeField] private Rigidbody cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPoint;

    [SerializeField] private Animator cannonAnim;
    [SerializeField] private Rigidbody cartRigidBody;

    [SerializeField] private AudioSource fuseSource;
    [SerializeField] private AudioSource barrelSource;

    [SerializeField] private float fuseIgniteTime = 0.5f;
    [SerializeField] private float cannonShootDelay = 0.1f;
    [SerializeField] private float cannonBallSpeed = 10f;

    [SerializeField] private float backThrustPower = 10f;
    [SerializeField] private float backThrustUpwardsPower = 1f;
    [SerializeField] private float backThrustAngularPower = 1f;

    [SerializeField] private float cooldownTime = 5;

    private bool onCooldown = false;



    public override void Pickup(InteractionController handInteractor)
    {
        if (onCooldown) return;

        StartCoroutine(ShootCannonBall());
        onCooldown = true;
    }


    private IEnumerator ShootCannonBall()
    {
        fuseSource.Play();

        yield return new WaitForSeconds(fuseIgniteTime);

        barrelSource.Play();

        yield return new WaitForSeconds(cannonShootDelay);

        cartRigidBody.velocity = -transform.forward * backThrustPower + transform.up * backThrustUpwardsPower;
        cartRigidBody.angularVelocity = transform.up * backThrustAngularPower;
        cannonAnim.SetTrigger("Shoot");

        Rigidbody cannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPoint.position, cannonBallSpawnPoint.rotation);
        cannonBall.velocity = cannonBallSpawnPoint.forward * cannonBallSpeed;

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);

        onCooldown = false;
    }



#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        base.OnDrawGizmosSelected();
        
        if(cannonBallSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cannonBallSpawnPoint.position, 0.25f);

            Gizmos.DrawLine(cannonBallSpawnPoint.position, cannonBallSpawnPoint.position + cannonBallSpawnPoint.forward * 2);
        }
    }


    [ContextMenu("Test Shoot")]
    private void SHOOT()
    {
        if (onCooldown) return;

        StartCoroutine(ShootCannonBall());
        onCooldown = true;
    }

#endif
}
