using UnityEngine;
using UnityEngine.VFX;



public class Hydra : MonsterCore
{
    public bool alive = true;

    private VisualEffect breathVVF;

    [SerializeField] private float firstDeathAttackMultiplier = 0.8f;
    [SerializeField] private float secondDeathAttackMultiplier = 0.25f;



    protected override void Start()
    {
        base.Start();

        breathVVF = GetComponentInChildren<VisualEffect>();
    }


    protected override void Death()
    {
        alive = false;
        stateMachine.Die();

        StopAllCoroutines();
        Destroy(transform.parent.gameObject, deathDelay);

        int headsAlive = 0;

        //check for alive hydras
        if (transform.TryFindObjectsOfType(out Hydra[] hydras))
        {
            for (int i = 0; i < hydras.Length; i++)
            {
                if (hydras[i].alive == true)
                {
                    headsAlive += 1;
                }
            }
        }

        if (headsAlive == 0)
        {
            //only if all hydras are dead, end encounter and kill parent GameObject
            Destroy(transform.parent.parent.gameObject);

            ZoneLoader.Instance.EndEncounter();
        }
        //enrage alive heads
        else
        {
            if (headsAlive == 2)
            {
                for (int i = 0; i < hydras.Length; i++)
                {
                    if (hydras[i].alive == true)
                    {
                        hydras[i].FirstDeath();
                    }
                }
            }
            if (headsAlive == 1)
            {
                for (int i = 0; i < hydras.Length; i++)
                {
                    if (hydras[i].alive == true)
                    {
                        hydras[i].SecondDeath();
                    }
                }
            }
        }
    }

    protected override void Attack()
    {
        base.Attack();

        breathVVF.Play();
    }



    public void FirstDeath()
    {
        attackInterval *= firstDeathAttackMultiplier;

        riseSpeed /= firstDeathAttackMultiplier;
        riseDelay *= firstDeathAttackMultiplier;
        sinkSpeed /= firstDeathAttackMultiplier;
    }

    public void SecondDeath()
    {
        attackInterval *= secondDeathAttackMultiplier;

        riseSpeed /= secondDeathAttackMultiplier;
        riseDelay *= secondDeathAttackMultiplier;
        sinkSpeed /= secondDeathAttackMultiplier;

        swapSideChance = 80;
    }
}
