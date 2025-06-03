using UnityEngine;


public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out MonsterCore monster))
        {
            monster.Hit(damage);
        }
    }
}
