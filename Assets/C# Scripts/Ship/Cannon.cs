using System.Collections;
using UnityEngine;



public class Cannon : Interactable
{
    [SerializeField] private Rigidbody cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPoint;

    [SerializeField] private Animator cannonAnim;
    [SerializeField] private Rigidbody cartRigidBody;

    [SerializeField] private ParticleSystem explosionEffect;

    [SerializeField] private AudioSource fuseSource;
    [SerializeField] private AudioSource barrelSource;

    [SerializeField] private float fuseIgniteTime = 0.5f;
    [SerializeField] private float cannonShootDelay = 0.1f;
    [SerializeField] private float cannonBallSpeed = 10f;

    [SerializeField] private float backThrustPower = 10f;
    [SerializeField] private float backThrustUpwardsPower = 1f;
    [SerializeField] private float backThrustAngularPower = 1f;

    [SerializeField] private float cooldownTime = 5;

    private Material fuseShader;

    private bool onCooldown;

    private static int IsPrimedBoolId = Shader.PropertyToID("_IsPrimed");
    private static int TransparencyFloatId = Shader.PropertyToID("_Transparency");


    protected override void Start()
    {
        base.Start();

        if (TryGetComponent(out Renderer renderer))
        {
            fuseShader = renderer.material;
        }

        onCooldown = false;
    }


    public override void Pickup(InteractionController handInteractor)
    {
        if (onCooldown) return;

        StartCoroutine(ShootCannonBall());
        onCooldown = true;
    }


    private IEnumerator ShootCannonBall()
    {
        if (fuseShader != null)
        {
            //prime fuse
            fuseShader.SetInt(IsPrimedBoolId, 1);
            //play sound for fuse
            fuseSource.Play();
        }

        float elapsed = 0;
        while (elapsed <= fuseIgniteTime)
        {
            yield return null;

            elapsed += Time.deltaTime;

            if (fuseShader != null)
            {
                //reset fuse
                fuseShader.SetFloat(TransparencyFloatId, 1 - elapsed / fuseIgniteTime);
            }
        }

        if (fuseShader != null)
        {
            //disable fuse fire
            fuseShader?.SetInt(IsPrimedBoolId, 0);
            //play barrel shoot sound
            barrelSource?.Play();
        }

        yield return new WaitForSeconds(cannonShootDelay);

        if (fuseShader != null)
        {
            //reset fuse
            fuseShader?.SetFloat(TransparencyFloatId, 1);
        }

        //set velocity
        if (cartRigidBody != null)
        {
            cartRigidBody.velocity = -transform.forward * backThrustPower + transform.up * backThrustUpwardsPower;
            cartRigidBody.angularVelocity = transform.up * backThrustAngularPower;
        }

        //call shoot animation
        cannonAnim.SetTrigger("Shoot");

        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        Rigidbody projectile = Instantiate(cannonBallPrefab, cannonBallSpawnPoint.position, cannonBallSpawnPoint.rotation);
        projectile.velocity = cannonBallSpawnPoint.forward * cannonBallSpeed;

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

        if (cannonBallSpawnPoint != null)
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