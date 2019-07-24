using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Simple container to simplify smooths
public class SmoothQuaternion
{
	private Quaternion value;
	private Vector3 velocity;
	private Quaternion target;

	public SmoothQuaternion (Quaternion value)
	{
		this.value = value;
		this.target = value;
		this.velocity = Vector3.zero;
	}

	public Quaternion Update(float smoothTime, float delta, float maxVel = float.PositiveInfinity)
	{
		this.value = MathUtils.SmoothDamp(this.value, target, ref velocity, smoothTime, delta, maxVel);
		return this.value;
	}

	public Quaternion Target { get { return target; } set { this.target = value; } }
	public Quaternion Value { get { return value; } set { this.value = value; velocity = Vector3.zero; } }
}