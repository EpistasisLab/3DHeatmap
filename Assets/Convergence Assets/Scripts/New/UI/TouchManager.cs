using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle touch
/// </summary>
public class TouchManager : MonoBehaviorSingleton<TouchManager> {

    //Use these to try and avoid unintentional actions as fingers go on and come off the screen.
    private float firstTouchTime;
    //Only need to track when we're starting a touch and when we're rotating, so we don't end up zooming 
    // or scaling during or at the end of a rotation.
    private enum TouchAction { None, TouchStarting, Rotate };
    private TouchAction currentTouchAction;
    private TouchAction prevTouchActionDbg;
    /// <summary> How long to wait after the first finger is down before we start looking for actions.
    /// This help prevent unintentional tranlsations or zooms when user is trying to rotate and
    ///  doesn't get all fingers down at the same time. </summary>
    public float touchStartDelay = 0.15f;

    /// <summary> Scaling factor applied to touch-finger motion to zoom amount </summary>
    public float pinchZoomScale = 0.2f;
    /// <summary> Scaling exponent applied to touch-finger motion before other scaling</summary>
    public float pinchZoomExpScale = 1.5f;
    /// <summary> Tolerance for dot product of touch movement vectors to call them as moving along the same line for zoom </summary>
    public float pinchZoomDotThreshold = 0.9f;

    public float rotationScale = 0.1f;
    public float rotationExpScale = 1.5f;
    /// <summary> Tolerance for dot product of touch movement vectors to call them as moving along the same line for zoom </summary>
    public float rotationDotThreshold = 0.9f;

    /// <summary> Scales the measure touch movment to scale translation amount. </summary>
    public float translationScale = 0.1f;
    /// <summary> Raises the measured touch movement to this power to get non-linear speed response </summary>
    public float translationExpScale = 1.5f;
    /// <summary> Threshold above which dot product of movement vectors are considered parallel and thus translating </summary>
    public float translateDotThreshold = 0.95f;

    //Use this instead of Awake
    protected override void Initialize()
    {

    }

	// Use this for 
	void Start () {
        firstTouchTime = 0;
        currentTouchAction = prevTouchActionDbg = TouchAction.None;
	}
	
    /// <summary> Call this when you want to check for touch actions and respond to them </summary>
    public void Process()
    {
        if( prevTouchActionDbg != currentTouchAction)
        {
            //Debug.Log("touch action change: " + currentTouchAction.ToString());
            prevTouchActionDbg = currentTouchAction;
        }

        //Input.touch stuff seems to be for multitouch screens, not for trackpad
        //Not sure yet how to handle trackpad/touchpad gestures
        if (Input.touchCount == 0)
        {
            currentTouchAction = TouchAction.None;
            return;
        }

        //Debug.Log("touch: " + Input.touchCount + " " + currentTouchAction.ToString());

        //If a touch event/action is just starting, setup state and skeedaddle.
        if ( currentTouchAction == TouchAction.None)
        {
            firstTouchTime = Time.time;
            currentTouchAction = TouchAction.TouchStarting;
            return;
        }
        //Wait a minimum amount of time to allow allow fingers to get on the screen
        // for a multi-touch action.
        if (currentTouchAction == TouchAction.TouchStarting && (Time.time - firstTouchTime) < touchStartDelay)
            return;

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

        //Look for two-finger touch/movement, but only if not in/ending a rotate action.
        //  User may lift a finger while still rotating, or be ending a rotate and lift one finger and move the other two before lifting them.
        //Pinch-zoom
        //Translation
        if (Input.touchCount == 2 && currentTouchAction != TouchAction.Rotate)
        {
            //Check for translation, but only if not already zooming
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
                    //Debug.Log("touch delta: " + d0.ToString("F2") + " " + d1.ToString("F2") + " " + d2.ToString("F2"));
                    //Debug.Log("avg trans vec: " + avg.ToString("F2"));
                    float lateral = Mathf.Pow(Mathf.Abs(avg.x), translationExpScale) * translationScale * Mathf.Sign(avg.x) * -1;
                    float forward = Mathf.Pow(Mathf.Abs(avg.y), translationExpScale) * translationScale * Mathf.Sign(avg.y) * -1;
                    CameraManager.Instance.TranslateView(lateral, forward);
                    return;
                }
            }

            //Check for pinch-zoom
            //But if we already are translating, we assume the user has paused motion, so skeedaddle
            if ( mv0 || mv1)
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
                //Debug.Log("zoom dot: " + dot);
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

        //3-finger touch for rotation
        if( Input.touchCount == 3)
        {
            float dot3 = (Vector2.Dot(d0, d1) + Vector2.Dot(d0, d2) + Vector2.Dot(d1, d2)) / 3f;
            if( dot3 > rotationDotThreshold)
            {
                Vector2 avg = (d0 + d1 + d2) / 3f;
                //Debug.Log("rot avg: " + avg);
                float rotRight = Mathf.Pow(Mathf.Abs(avg.y), rotationExpScale) * -Mathf.Sign(avg.y) * rotationScale;
                float rotUp    = Mathf.Pow(Mathf.Abs(avg.x), rotationExpScale) * Mathf.Sign(avg.x) * rotationScale;
                CameraManager.Instance.RotateView(rotRight, rotUp);
                currentTouchAction = TouchAction.Rotate;
                return;
            }
        }
    }


    // Update is called once per frame
    //void Update () {

    //}
}
