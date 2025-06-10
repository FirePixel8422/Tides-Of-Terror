

[System.Serializable]
public struct AttackData
{
    public float weight;
    public AttackPosition attackPosition;

    public float attackTime;

    public int attackId;
    public int followUpAttackDataId;

    public int continuedAttackId;
}