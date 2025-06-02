using UnityEngine;


public class MonsterCore : MonoBehaviour
{
    [SerializeField] private float health;

    private MonsterStateMachine stateMachine;


    private void Start()
    {
        stateMachine = GetComponent<MonsterStateMachine>();
    }

    public void Hit(float damage)
    {
        health -= damage;

        if (health >= 0)
        {
            stateMachine.Die();
        }
        else
        {
            stateMachine.GetHurt();
        }

        Destroy(gameObject, 0.5f);
    }
}
