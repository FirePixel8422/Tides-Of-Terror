using System.Collections;
using UnityEngine;



public class Cannon : Interactable
{
    [SerializeField] private Rigidbody cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPoint;

    [SerializeField] private Animator cannonAnim;

    [SerializeField] private float fuseIgniteTime = 0.5f;
    [SerializeField] private float cannonShootDelay;

    [SerializeField] private float cannonBallSpeed = 10f;

    public override void Pickup(InteractionController handInteractor)
    {
        StartCoroutine(ShootCannonBall());
    }


    private IEnumerator ShootCannonBall()
    {
        yield return new WaitForSeconds(fuseIgniteTime);

        yield return new WaitForSeconds(cannonShootDelay);

        cannonAnim.SetTrigger("Shoot");

        Rigidbody cannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPoint.position, cannonBallSpawnPoint.rotation);

        cannonBall.velocity = cannonBallSpawnPoint.forward * cannonBallSpeed;
    }
}
