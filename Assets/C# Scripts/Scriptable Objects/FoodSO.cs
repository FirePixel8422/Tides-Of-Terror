using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "New Food", menuName = "Cooking System/Food", order = 1)]
public class FoodSO : ScriptableObject
{
    public string foodName;



    private void OnValidate()
    {
        if (string.IsNullOrEmpty(foodName) == false)
        {
            foodType.foodName = foodName;
        }
    }

    [HideInInspector]
    public FoodType foodType;
}



[System.Serializable]
public struct FoodType : IEquatable<FoodType>
{
    public string foodName;


    public bool Equals(FoodType other)
    {
        return foodName == other.foodName;
    }


    public override bool Equals(object obj)
    {
        if (obj is FoodType other)
        {
            return Equals(other);
        }
        return false;
    }


    public override int GetHashCode()
    {
        return foodName.GetHashCode();
    }


    public static bool operator ==(FoodType lhs, FoodType rhs)
    {
        return lhs.Equals(rhs);
    }


    public static bool operator !=(FoodType lhs, FoodType rhs)
    {
        return !lhs.Equals(rhs);
    }
}