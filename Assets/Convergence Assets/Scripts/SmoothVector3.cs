using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Simple container to simplify smooths
public class SmoothVector3
{
    public Vector3 velocity;

    private Vector3 value;
    private Vector3 target;

    public SmoothVector3(Vector3 value)
    {
        this.value = value;
        this.target = value;
        this.velocity = Vector3.zero;
    }

    public Vector3 Update(float smoothTime, float delta, float maxVel = float.PositiveInfinity)
    {
        if (delta == 0)
        {
            Debug.LogError("Delta can't be 0 in SmoothDamp");
            delta = Time.unscaledDeltaTime;
        }

        this.value = Vector3.SmoothDamp(this.value, target, ref velocity, smoothTime, maxVel, delta);

        return this.value;
    }

    public Vector3 Target { get { return target; } set { this.target = value; } }

    public Vector3 Value
    {
        get { return value; }
        set
        { 
            this.value = value;
            velocity = Vector3.zero; 
        }
    }
}