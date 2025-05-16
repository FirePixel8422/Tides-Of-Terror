﻿using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


public static class EzRandom
{
    private static Unity.Mathematics.Random random;


    // Initialize the random instance
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Generate a seed for the random using the current time
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
    }


    public static void ReSeed()
    {
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
    }

    public static void ReSeed(uint seed)
    {
        random = new Unity.Mathematics.Random(seed);
    }


    public static int Range(int min, int max)
    {
        return random.NextInt(min, max);
    }
    public static int Range(MinMaxInt value)
    {
        return random.NextInt(value.min, value.max);
    }


    public static uint Range(uint min, uint max)
    {
        return random.NextUInt(min, max);
    }


    public static float Range(float min, float max)
    {
        return random.NextFloat(min, max);
    }

    public static float Range(MinMaxFloat value)
    {
        return random.NextFloat(value.min, value.max);
    }


    public static Vector3 Range(Vector3 min, Vector3 max)
    {
        Vector3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }

    public static float3 Range(float3 min, float3 max)
    {
        float3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }

    public static int3 Range(int3 min, int3 max)
    {
        int3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }

    public static Color RandomColor(bool randomizeAlpha = false)
    {
        Color color;
        color.r = random.NextFloat();
        color.g = random.NextFloat();
        color.b = random.NextFloat();
        color.a = randomizeAlpha ? random.NextFloat() : 1;


        return color;
    }
}