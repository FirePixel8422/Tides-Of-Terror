using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking System/Recipe", order = 2)]
public class RecipeSO : ScriptableObject
{
    public Recipe recipe;
}



[System.Serializable]
public struct Recipe : IEquatable<Recipe>
{
    public Recipe(FoodType[] _requiredFood, Applience _requiredApplience)
    {
        requiredFood = new FoodSO[_requiredFood.Length];

        for (int i = 0; i < _requiredFood.Length; i++)
        {
            requiredFood[i] = ScriptableObject.CreateInstance<FoodSO>();
            requiredFood[i].foodType = _requiredFood[i];
        }


        requiredApplience = ScriptableObject.CreateInstance<ApplienceSO>();

        requiredApplience.applience = _requiredApplience;
        requiredApplience.applienceName = _requiredApplience.ToString();

        foodIndex = -1;
    }


    public FoodSO[] requiredFood;

    public ApplienceSO requiredApplience;

    public int foodIndex;


    public bool Equals(Recipe other)
    {
        if (other.requiredFood.Length != requiredFood.Length || other.requiredApplience.applience != requiredApplience.applience)
        {
            return false;
        }

        for (int i = 0; i < requiredFood.Length; i++)
        {
            for (int i2 = 0; i2 < other.requiredFood.Length; i2++)
            {
                if (requiredFood[i].foodType == other.requiredFood[i2].foodType)
                {
                    //Debug.Log(requiredFood[i].name.ToString() + " Found");
                    break;
                }
                //if the last food checked isnt the required food, return false
                else if(i2 == other.requiredFood.Length - 1)
                {
                    //Debug.Log(requiredFood[i].name.ToString() + " Exited");
                    return false;
                }
            }
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is Recipe other)
        {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < requiredFood.Length; i++)
        {
            hash += requiredFood[i].GetHashCode() * 13;
        }
        return hash;
    }


    public static bool operator ==(Recipe lhs, Recipe rhs)
    {
        return lhs.Equals(rhs);
    }


    public static bool operator !=(Recipe lhs, Recipe rhs)
    {
        return !lhs.Equals(rhs);
    }
}