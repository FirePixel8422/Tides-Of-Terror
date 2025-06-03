using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterCore : MonoBehaviour
{
    [SerializeField] private float health;

    private MonsterStateMachine stateMachine;

    [SerializeField] protected float leftRot, rightRot, frontRot;
    [SerializeField] protected AttackPosition attackPosition;

    [SerializeField] protected AttackData[] attackData;
    [SerializeField] protected int cAttackId;

    [SerializeField] protected float attackInterval = 5;
    [SerializeField] protected float swapSideChance = 33;



    protected virtual void Start()
    {
        stateMachine = GetComponent<MonsterStateMachine>();

        transform.parent.transform.SetParent(BoatEngine.Instance.transform, false, false);

        StartCoroutine(AttackLoop());

        SwapSide();
    }

    public virtual void Hit(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            stateMachine.Die();

            StopAllCoroutines();
            Destroy(gameObject, 0.5f);
        }
        else
        {
            stateMachine.GetHurt();
        }
    }


    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackData[cAttackId].attackTime);

            stateMachine.Idle();

            yield return new WaitForSeconds(attackInterval);

            if (EzRandom.Chance(swapSideChance))
            {
                SwapSide();
            }
        }
    }

    [ContextMenu("Swap Side")]
    protected virtual void SwapSide()
    {
        AttackPosition newAttackPosition = attackPosition;
        float r = -1;

        for (int i = 0; i < 3; i++)
        {
            int iPowerOf2 = MathLogic.ConvertToPowerOf2(i + 1);

            if ((int)attackPosition == iPowerOf2) continue;

            float newR = EzRandom.Range(0f, 100f);

            if (newR > r)
            {
                r = newR;
                newAttackPosition = (AttackPosition)iPowerOf2;
            }
        }

        attackPosition = newAttackPosition;
    }

    /// <summary>
    /// Swap to new attack compatible with the current side of
    /// </summary>
    protected virtual void SwapAttack()
    {
        List<AttackData> compatibleAttackDataList = new List<AttackData>();
        float totalWeight = 0;

        for (int i = 0; i < attackData.Length; i++)
        {
            if (attackData[i].attackPosition.HasFlag(attackPosition)) continue;

            compatibleAttackDataList.Add(attackData[i]);
            totalWeight += attackData[i].weight;
        }


        float r = EzRandom.Range(0, totalWeight);

        for (int i = 0; i < compatibleAttackDataList.Count; i++)
        {
            float cAttackWeight = compatibleAttackDataList[i].weight;
            if (cAttackWeight > r)
            {
                cAttackId = compatibleAttackDataList[i].attackId;
                break;
            }
            r -= cAttackWeight;
        }
    }
}
