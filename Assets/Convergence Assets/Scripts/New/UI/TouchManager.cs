using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle touch
/// </summary>
public class TouchManager : MonoBehaviorSingleton<TouchManager> {

    public float pinchZoomScale;
    public float pinchZoomExpScale;
    /// <summary> Tolerance for dot product of touch movement vectors to call them as moving along the same line for zoom </summary>
    public float pinchZoomDotThreshold = 0.9f;

    /// <summary> Scales the measure touch movment to scale translation amount. </summary>
    public float translationScale = 0.1f;
    /// <summary> Raises the measured touch movement to this power to get non-linear speed response </summary>
    public float translationExpScale = 2.5f;
    /// <summary> Threshold above which dot product of movement vectors are considered parallel and thus translating </summary>
    public float translateDotThreshold = 0.95f;


    //Use this instead of Awake
    protected override void Initialize()
    {

    }

	// Use this for 
	void Start () {
		
	}
	
    /// <summary> Call this when you want to check for touch actions and respond to them </summary>
    public void Process()
    {
        //Input.touch stuff seems to be for multitouch screens, not for trackpad
        //Not sure yet how to handle trackpad/touchpad gestures
        if (Input.touchCount == 0)
            return;
        //Debug.Log("touchCount: " + Input.touchCount);

        // Get movement of the fingers since last frame
        Vector2 d0, d1 = new Vector2(), d2 = new Vector2();
        d0 = Input.GetTouch(0).deltaPosition;
        if( Input.touchCount >= 2 )
            d1 = Input.GetTouch(1).deltaPosition;
        if (Input.touchCount >= 3)
            d2 = Input.GetTouch(2).deltaPosition;

        bool mv0, mv1 = false, mv2 = false;
        mv0 = Input.GetTouch(0).phase == TouchPhase.Moved;
        if (Input.touchCount >= 2)
            mv1 = Input.GetTouch(1).phase == TouchPhase.Moved;
        if (Input.touchCount >= 3)
            mv2 = Input.GetTouch(2).phase == TouchPhase.Moved;

        //Look for two-finger touch/movement
        if (Input.touchCount == 2)
        {

            //Check for translation
            //Two fingers moving in parallel
            if (mv0 && mv1)
            {
                //dot product between two vectors
                float dot = Vector2.Dot(d0, d1);
                //Debug.Log("dot: " + dot);
                //If dot product positive and above threshold, fingers are moving in parallel so translate
                if ( dot >= translateDotThreshold)
                {
                    Vector2 avg = (d0 + d1) / 2f;
                    Debug.Log("touch delta: " + d0.ToString("F2") + " " + d1.ToString("F2") + " " + d2.ToString("F2"));
                    Debug.Log("avg trans vec: " + avg.ToString("F2"));
                    float lateral = Mathf.Pow(Mathf.Abs(avg.x), translationExpScale) * translationScale * Mathf.Sign(avg.x) * -1;
                    float forward = Mathf.Pow(Mathf.Abs(avg.y), translationExpScale) * translationScale * Mathf.Sign(avg.y) * -1;
                    CameraManager.Instance.TranslateView(lateral, forward);

                    return;
                }
            }

            //Check for pinch-zoom
            if (mv0 || mv1)
            {
                //Method:
                //Make a vector from first touch to tip of vector made from 2nd Touch's position + deltaPosition.
                //If this vector is parallel or anti-parallel to the 2nd touch's deltaPosition, then we're pinching.
                //This lets us zoom when one touch isn't moving.
                //If one of the touches is stationary, use that as the still/reference point.
                //If both are moving, it doesn't matter which is which.
                Touch still, moving;
                moving = mv0 ? Input.GetTouch(0) : Input.GetTouch(1);
                still = mv0 ? Input.GetTouch(1) : Input.GetTouch(0);
                Vector2 dH = ( moving.deltaPosition + moving.position - still.position);
                float dot = Vector2.Dot(dH, moving.deltaPosition);
                Debug.Log("zoom dot: " + dot);
                //Check if the two touches are moving parallel or anti-parallel
                if ( Mathf.Abs(dot) > pinchZoomDotThreshold)
                {
                    float magnitude = mv0 && mv1 ? (d0.magnitude + d1.magnitude) / 2f : Mathf.Max(d0.magnitude,d1.magnitude);
                    float zoom = Mathf.Pow(magnitude, pinchZoomExpScale) * pinchZoomScale * Mathf.Sign(dot);
                    CameraManager.Instance.Zoom(zoom);
                    return;
                }
            }
        }

    }


    // Update is called once per frame
    //void Update () {

    //}
}
