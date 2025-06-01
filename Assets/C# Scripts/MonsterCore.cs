using UnityEngine;


public class MonsterCore : MonoBehaviour
{
    [SerializeField] private float health;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Projectile proj))
        {
            health -= proj.damage;

            Destroy(proj.gameObject);

            if (health <= 0)
            {
                 Destroy(gameObject);
            }
        }
    }
}
