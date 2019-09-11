using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handle all inputs
/// </summary>
public class InputManager : MonoBehaviorSingleton<InputManager> {

    //// KEYBOARD Input ////
    /// <summary> Scaling for translational input </summary>
    public float translationScaleKeys = 1.5f;
    public float zoomScaleKeys = 2f;

    //// MOUSE Input ////
    public float translationScaleMouse = 25f;
    public float rotationScaleMouse = 1f;
    /// <summary> Time below which a mouse click-and-release is considered a single click </summary>
    public float mouseSingleClickThreshold;
    private float leftMouseButtonDownTime;
    public float zoomScaleScrollWheel;

    //// TOUCH Input ////

    //Use this time and TouchActions to try and avoid unintentional actions as fingers go on and come off the screen.
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
    public float pinchZoomScaleTouch = 0.2f;
    /// <summary> Scaling exponent applied to touch-finger motion before other scaling</summary>
    public float pinchZoomExpScaleTouch = 1.5f;
    /// <summary> Tolerance for dot product of touch movement vectors to call them as moving along the same line for zoom </summary>
    public float pinchZoomDotThresholdTouch = 0.9f;

    public float rotationScaleTouch = 0.1f;
    public float rotationExpScaleTouch = 1.5f;
    /// <summary> Tolerance for dot product of touch movement vectors to call them as moving along the same line for zoom </summary>
    public float rotationDotThresholdTouch = 0.9f;

    /// <summary> Scales the measure touch movment to scale translation amount. </summary>
    public float translationScaleTouch = 0.1f;
    /// <summary> Raises the measured touch movement to this power to get non-linear speed response </summary>
    public float translationExpScaleTouch = 1.5f;
    /// <summary> Threshold above which dot product of movement vectors are considered parallel and thus translating </summary>
    public float translateDotThresholdTouch = 0.95f;

    //// Other fields ////

    //Data Inspection
    //
    /// <summary> Flag to enable continuous data inspection, so data under the mouse pointer is queried under continuously as pointer is moved </summary>
    public bool contDataInspectionEnabled = true;
    /// <summary> Flag to temporarily disable continuous data inspection. Used to lock the inspection when mouse is clicked </summary>
    private bool contDataInpectionTempDisable = false;
    /// <summary> How often (seconds) to do continous data inspection </summary>
    public float contDataInspectionInterval = 0.2f;
    private float contDataInspectionPrevTime;

    //Use this instead of Awake
    protected override void Initialize()
    {
        contDataInspectionPrevTime = 0f;
    }

	// Use this for 
	void Start () {
        firstTouchTime = 0;
        currentTouchAction = prevTouchActionDbg = TouchAction.None;
    }
	
    /// <summary> Call this when you want to check for touch actions and respond to them </summary>
    public void CheckForTouch()
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
                if ( dot >= translateDotThresholdTouch)
                {
                    Vector2 avg = (d0 + d1) / 2f;
                    //Debug.Log("touch delta: " + d0.ToString("F2") + " " + d1.ToString("F2") + " " + d2.ToString("F2"));
                    //Debug.Log("avg trans vec: " + avg.ToString("F2"));
                    float lateral = Mathf.Pow(Mathf.Abs(avg.x), translationExpScaleTouch) * translationScaleTouch * Mathf.Sign(avg.x) * -1;
                    float forward = Mathf.Pow(Mathf.Abs(avg.y), translationExpScaleTouch) * translationScaleTouch * Mathf.Sign(avg.y) * -1;
                    CameraManager.I.TranslateView(lateral, forward);
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
                if ( Mathf.Abs(dot) > pinchZoomDotThresholdTouch)
                {
                    float magnitude = mv0 && mv1 ? (d0.magnitude + d1.magnitude) / 2f : Mathf.Max(d0.magnitude,d1.magnitude);
                    float zoom = Mathf.Pow(magnitude, pinchZoomExpScaleTouch) * pinchZoomScaleTouch * Mathf.Sign(dot);
                    CameraManager.I.Zoom(zoom);
                    return;
                }
            }
        }

        //3-finger touch-and-drag for rotation
        if( Input.touchCount == 3 && mv0 && mv1 && mv2)
        {
            //float dot3 = (Vector2.Dot(d0, d1) + Vector2.Dot(d0, d2) + Vector2.Dot(d1, d2)) / 3f;
            //if( dot3 > rotationDotThresholdTouch)
            if (Vector2.Dot(d0, d1) > rotationDotThresholdTouch &&
                Vector2.Dot(d0, d2) > rotationDotThresholdTouch &&
                Vector2.Dot(d1, d2) > rotationDotThresholdTouch)
            {
                Vector2 avg = (d0 + d1 + d2) / 3f;
                //Debug.Log("rot avg: " + avg);
                float rotRight = Mathf.Pow(Mathf.Abs(avg.y), rotationExpScaleTouch) * -Mathf.Sign(avg.y) * rotationScaleTouch;
                float rotUp    = Mathf.Pow(Mathf.Abs(avg.x), rotationExpScaleTouch) * Mathf.Sign(avg.x) * rotationScaleTouch;
                CameraManager.I.RotateView(rotRight, rotUp);
                currentTouchAction = TouchAction.Rotate;
                return;
            }
        }
    }

    /// <summary> Check for keyboard input that's ok only if the UI does NOT have input focus. </summary>
    private void CheckForNonUIKeyboard()
    {
        //// VIEW-CONTROL INPUTS

        float vertButton = Input.GetAxisRaw("Vertical");
        float horzButton = Input.GetAxisRaw("Horizontal");
        //These two were used in orig code, but I'm not using, at least not at this point.
        //float turnButton = Input.GetAxisRaw("Turn");
        //float spaceButton = Input.GetAxisRaw("Jump");

        //test translation via moving the graph instead of camera
        /*
        if( Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if ((vertButton != 0f || horzButton != 0f))
            {
                Graph.I.TranslateRidges(horzButton * translationScaleKeys * 10, 0f, vertButton * translationScaleKeys * 10);
                return;
            }

        }
        */

        //Forward / backward translation parallel to ground plane.
        //NOT movement along camera-forward
        if ((vertButton != 0f || horzButton != 0f))
        {
            CameraManager.I.TranslateView(horzButton * translationScaleKeys, vertButton * translationScaleKeys);
            //Debug.Log("horzButton " + horzButton);
        }

        //Zoom
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            CameraManager.I.Zoom(-1f * zoomScaleKeys);
        }
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            CameraManager.I.Zoom(1f * zoomScaleKeys);
        }
    }

    /// <summary> Check for keyboard input that's ok if the UI has input focus. </summary>
    private void CheckForUIKeyboard() { 

        if (Input.GetKeyDown(KeyCode.O))
        {/*
            if (DataManager.I.DebugQuickChooseLoadDisplayFile())
            {
                //take the new data and draw it
                Debug.Log("Loaded file with success");
                Graph.I.Redraw();
            }
            */
        }
        if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.RightShift))
        {
            //Quick load a test file and view it
            //DataManager.I.DebugQuickLoadDefaultAndDraw();
            DataManager.I.LoadAndMapSampleData();
        }
        if (/*Input.GetKeyDown(KeyCode.H) ||*/ Input.GetKeyDown(KeyCode.F1))
        {
            //this.showHelp = !this.showHelp;
            UIManager.I.ShowIntroMessage();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Graph.I.Redraw();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            CameraManager.I.FollowHMDenable(CameraManager.I.followHmdEnabled == false);
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //Debug VR device loading
            VRManager.I.DeviceLoadAndEnable();
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            //Debugging

            DataInspector.I.DbgSelectWithDebugRay();

            //GameObject newRidge = UnityEngine.Object.Instantiate(Graph.I.Proto, new Vector3(Graph.I.sceneCorner.x, Graph.I.sceneCorner.y, Graph.I.sceneCorner.z), Quaternion.identity);
            //newRidge.name = "testRidge";

            //UIManager.I.StartAutoUIActionPrompts();

            //TriDataPoint data = new TriDataPoint(0, 1);
            //data.DebugDump();
        }
    }

    private void CheckForMouse()
    {
        //Check for single click. Stash time and swallow if found.
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseButtonDownTime = Time.time;
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            //Look for single click
            if( Time.time - leftMouseButtonDownTime < mouseSingleClickThreshold)
            {
                //Process single click

                //Initiate a data inspection
                TriDataPoint triPoint = DataInspector.I.InspectDataAtScreenPosition(Input.mousePosition);
                //If we get a valid point, lock the data inspection to this point by setting this flag.
                contDataInpectionTempDisable = triPoint.isValid;
                //triPoint.DebugDump();
            }
            return;
        }

        //Translate
        if (Input.GetMouseButton(1/*right mouse button held down*/))
        {
            // Read the mouse input axis
            float trX = Input.GetAxis("Mouse X"); //delta position, from what I understand
            float trY = Input.GetAxis("Mouse Y");
            CameraManager.I.TranslateView(-trX * translationScaleMouse, -trY * translationScaleMouse);
            return;
        }

        //Rotation - mouse
        if (Input.GetMouseButton(0/*left button*/))
        {
            // Read the mouse input axis
            float rotX = Input.GetAxis("Mouse X");
            float rotY = Input.GetAxis("Mouse Y");
            CameraManager.I.RotateView(-rotY * rotationScaleMouse, rotX * rotationScaleMouse);
            return;
        }

        //Scroll wheel for zoom
        //NOTE - this isn't picking up scroll from trackpad - why not?
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if( scroll != 0 )
        {
            CameraManager.I.Zoom(scroll * zoomScaleScrollWheel);
            return;
        }

        //No mouse button or scroll-wheel activity and no button held
        //
        //Do continuous data inspector
        if ( contDataInspectionEnabled &&
            ! contDataInpectionTempDisable &&
            (Time.time - contDataInspectionPrevTime) > contDataInspectionInterval)
        {
            DataInspector.I.InspectDataAtScreenPosition(Input.mousePosition);
            contDataInspectionPrevTime = Time.time;
        }

    }

    /// <summary> Reset anything that should be reset when new data loaded, etc. </summary>
    public void Reset()
    {
        DataInspector.I.Hide(); //probably not a great place to do this - DataInspector could get its own Reset() method
    }

    // Update is called once per frame
    void Update ()
    {
        CheckForUIKeyboard();

        //Look for input controls if the UI isn't being used
        //if (!EventSystem.current.IsPointerOverGameObject() ) //NOTE - this method fails when cursor leaves area of UI control but still is controlling it, e.g. with a slider when you move up or down off of it while still holding click on it
        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        CheckForNonUIKeyboard();

        //Check for touch events and proces them
        CheckForTouch();

        CheckForMouse();
    }
}
