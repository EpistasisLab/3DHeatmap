using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class BallBump : MonoBehaviour
{
    public static float minMag;
    public static float bigMag;
    public virtual void OnCollisionEnter(Collision collision)//Debug.Log("Had a collision");
    {
        float mag = collision.relativeVelocity.magnitude;
        if (mag > BallBump.minMag)
        {
            if (mag > BallBump.bigMag)
            {
                mag = BallBump.bigMag;
            }
            this.GetComponent<AudioSource>().volume = mag / BallBump.bigMag;
            this.GetComponent<AudioSource>().Play();
        }
    }

    static BallBump()
    {
        BallBump.minMag = 12f;
        BallBump.bigMag = 60f;
    }

}