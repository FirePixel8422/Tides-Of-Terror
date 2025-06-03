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

    [SerializeField] protected float sinkSpeed = 4f;
    [SerializeField] protected float sinkY = -10;

    [SerializeField] protected float riseDelay = 4;
    [SerializeField] protected float riseSpeed = 7.5f;
    [SerializeField] protected float riseY = 0;



    protected virtual void Start()
    {
        stateMachine = GetComponent<MonsterStateMachine>();

        transform.parent.transform.SetParent(BoatEngine.Instance.transform, false, false);

        StartCoroutine(AttackLoop());
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
        //select first side
        SwapSide();
        //and animate it
        StartCoroutine(SwapSideAnimation());

        while (true)
        {
            stateMachine.Attack(attackData[cAttackId].attackId);

            yield return new WaitForSeconds(attackData[cAttackId].attackTime);

            stateMachine.Idle();

            yield return new WaitForSeconds(attackInterval);

            //if attack has a follow up, do that
            if (attackData[cAttackId].followUpAttackDataId != -1)
            {
                cAttackId = attackData[cAttackId].followUpAttackDataId;
            }
            else
            {
                //otheriwse assign new attack and chance to swap direction
                if (EzRandom.Chance(swapSideChance))
                {
                    SwapSide();

                    //await finish of animation
                    yield return SwapSideAnimation();
                }

                SwapAttack();
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

            float newR = EzRandom.Range(0f, 100f);

            if (newR > r)
            {
                r = newR;
                newAttackPosition = (AttackPosition)iPowerOf2;
            }
        }

        attackPosition = newAttackPosition;
    }
    private IEnumerator SwapSideAnimation()
    {
        while (transform.position.y > sinkY)
        {
            yield return null;

            transform.position -= sinkSpeed * Time.deltaTime * Vector3.up;
        }

        transform.position = new Vector3(transform.position.x, sinkY, transform.position.z);

        yield return new WaitForSeconds(riseDelay);

        float rotY = 0;
        switch (attackPosition)
        {
            case AttackPosition.left:
                rotY = leftRot;
                break;

            case AttackPosition.right:
                rotY = rightRot;
                break;

            case AttackPosition.front:
                rotY = frontRot;
                break;
        }

        transform.parent.localRotation = Quaternion.Euler(0, rotY, 0);

        while (transform.position.y < riseY)
        {
            yield return null;

            transform.position += riseSpeed * Time.deltaTime * Vector3.up;
        }

        transform.position = new Vector3(transform.position.x, riseY, transform.position.z);
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
            if (attackData[i].attackPosition.HasFlag(attackPosition) == false) continue;

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
