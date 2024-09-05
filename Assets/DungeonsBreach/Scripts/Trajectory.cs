using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


/// <summary>
/// Prajectile formular for 2d coordinate system
/// x = v0_x*t
/// y = v0_y*t - 0.5*g*t*t
/// </summary>


public class Trajectory2D
{
    public float2 v0;
    public float g;


    public Trajectory2D(float2 v0, float g)
    {
        this.v0 = v0;
        this.g = g;
    }

    public Trajectory2D(float2 p0, float g, float t)
    {
        //x = v0_x * t
        v0.x = p0.x / t;

        //y = v0_y*t - 0.5*g*t*t
        v0.y = p0.y / t + 0.5f * g * t;

        this.g = g;
    }


    public float2[] OutputSequenceWithTime(float[] input_t)
    {
        var output = new float2[input_t.Length];
        for (int i = 0; i < input_t.Length; i++)
        {
            float t = input_t[i];
            output[i] = new float2(v0.x * t, v0.y * t - 0.5f * g * t * t);
        }
        return output;
    }

    public float2[] OutputSequenceWithX(float[] x)
    {
        float2[] output = new float2[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            float t = x[i] / v0.x;
            output[i] = new float2(x[i], v0.y * t - 0.5f * g * t * t);
        }
        return output;
    }

    public float2 OutputPointWithTime(float t)
    {
        return new float2(v0.x * t, v0.y * t - 0.5f * g * t * t);
    }

}
