using UnityEngine;
using System.Collections;

public class MathUtils 
{
	/// <summary>
	/// Samples the hemisphere. Takes focus as 1 - spread
	/// </summary>
	/// <param name="focus">focus = 1 - spread</param>
	public static Vector3 SampleHemisphere(float focus)
	{
		float u = Random.value;
		float v = Mathf.Clamp01(focus + Random.value * (1f - focus));

		float theta = 2f * Mathf.PI * u;
		float phi = Mathf.Acos(v);

		float cosPhi = Mathf.Cos(phi);
		float tmp = Mathf.Sqrt(1f - cosPhi * cosPhi);

		return new Vector3(tmp * Mathf.Cos(theta), cosPhi, tmp * Mathf.Sin(theta));
	}
    
    public static Quaternion SmoothDamp(Quaternion fromQuat, Quaternion toQuat, ref Vector3 velocity, float smoothTime, float deltaTime, float maxVel = float.PositiveInfinity)
    {
        if (deltaTime <= 0.001f)
            return fromQuat;

        Vector3 fromQuatEuler = fromQuat.eulerAngles;
        Vector3 targetQuatEuler = toQuat.eulerAngles;

        Quaternion smoothedRotation = Quaternion.Euler(
            new Vector3(
                Mathf.SmoothDampAngle(fromQuatEuler.x, targetQuatEuler.x, ref velocity.x, smoothTime, maxVel, deltaTime),
                Mathf.SmoothDampAngle(fromQuatEuler.y, targetQuatEuler.y, ref velocity.y, smoothTime, maxVel, deltaTime),
                Mathf.SmoothDampAngle(fromQuatEuler.z, targetQuatEuler.z, ref velocity.z, smoothTime, maxVel, deltaTime))
            );

        return smoothedRotation;
    }

    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector2 SmoothDamp(Vector2 fromVector, Vector2 toVector, ref Vector2 velocity, float smoothTime, float deltaTime, float maxVel = float.PositiveInfinity)
    {
        if (deltaTime <= 0.001f)
            return fromVector;

        Vector3 velocity3 = velocity;
        Vector3 smoothedPosition = Vector3.SmoothDamp(fromVector, toVector, ref velocity3, smoothTime, maxVel, deltaTime);
        velocity = velocity3;

        return smoothedPosition;
    }

    public static Vector4 SmoothDamp(Vector4 fromVector, Vector4 toVector, ref Vector4 velocity, float smoothTime, float deltaTime, float maxVel = float.PositiveInfinity)
    {
        if (deltaTime <= 0.001f)
            return fromVector;

       return new Vector4(
            Mathf.SmoothDampAngle(fromVector.x, toVector.x, ref velocity.x, smoothTime, maxVel, deltaTime),
            Mathf.SmoothDampAngle(fromVector.y, toVector.y, ref velocity.y, smoothTime, maxVel, deltaTime),
            Mathf.SmoothDampAngle(fromVector.z, toVector.z, ref velocity.z, smoothTime, maxVel, deltaTime),
            Mathf.SmoothDampAngle(fromVector.w, toVector.w, ref velocity.w, smoothTime, maxVel, deltaTime)
        );
    }

    // Ref: http://en.wikipedia.org/wiki/Smoothstep
    public static float Smootherstep(float x, float edge0, float edge1)
	{
		// Scale, and clamp x to 0..1 range
		x = Mathf.Clamp01((x - edge0)/(edge1 - edge0));

		// Evaluate polynomial
		return x * x * x * (x * (x * 6 - 15) + 10);
	}

	// Ref: http://en.wikipedia.org/wiki/Smoothstep
	public static float Smoothstep(float x, float edge0, float edge1)
	{
		// Scale, and clamp x to 0..1 range
		x = Mathf.Clamp01((x - edge0)/(edge1 - edge0));

		// Evaluate polynomial
		return x * x * (3 - 2 * x);
	}

    public static bool ContainsWithoutTranslation(Bounds container, Bounds target)
    {
        container.center = target.center;
        return container.Contains(target.min) && container.Contains(target.max);
    }

    public static bool Contains(Bounds container, Bounds target)
    {
        return container.Contains(target.min) && container.Contains(target.max);
    }

    private static Vector3[] aabbVertices = { new Vector3 (1f, 1f, 1f), new Vector3 (1f, -1f, -1f), new Vector3 (1f, 1f, -1f), new Vector3 (1f, -1f, 1f),
        new Vector3 (-1f, 1f, 1f), new Vector3 (-1f, -1f, -1f), new Vector3 (-1f, 1f, -1f), new Vector3 (-1f, -1f, 1f),     
    };

    public static Bounds TransformBounds(ref Matrix4x4 t, Bounds bounds)
    {
        Vector3 max = Vector3.one * -float.MaxValue;
        Vector3 min = Vector3.one * float.MaxValue;

        for (int i = 0; i < aabbVertices.Length; i++)
        {
            Vector3 vertex = bounds.center + Vector3.Scale(bounds.extents, aabbVertices[i]);
            Vector3 transformedVertex = t.MultiplyPoint3x4(vertex);

            max = Vector3.Max(max, transformedVertex);
            min = Vector3.Min(min, transformedVertex);
        }

        bounds.SetMinMax(min, max);

        return bounds;
    }
}