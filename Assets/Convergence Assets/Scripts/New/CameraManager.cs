using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Script for managing camera movement, etc
public class CameraManager : MonoBehaviour {

    /// <summary> Scaling for translational input </summary>
    public float translationScaleKeys = 1f;
    public float translationScaleMouse = 8f;
    public float rotationScale = 1f;
    public float zoomScale = 1f;

    private Camera ourCamera;

    /// <summary> The look-at target for the camera </summary>
    private Vector3 lookAtTarget;

	// Use this for initialization
	void Start () {
        //Expect a Camera component in this class' object
        ourCamera = transform.GetComponent<Camera>() as Camera;
        if (ourCamera == null)
            Debug.LogError("camera == null");

        lookAtTarget = Vector3.zero;
    }

    /// <summary> Directly set the LookAt target for the camera </summary>
    /// <param name="_target">Position to look at </param>
    public void LookAt(Vector3 _target)
    {
        lookAtTarget = _target;
    }

    private void TranslateView(float lateralStep, float longitudinalStep)
    {
        //Move both the camera and lookat target
        Vector3 lateral = ourCamera.transform.right * lateralStep;
        //Move fwd/back only parallel to ground plane
        Vector3 longitudinal = new Vector3(ourCamera.transform.forward.x, 0f, ourCamera.transform.forward.z).normalized * longitudinalStep;
        lookAtTarget += lateral + longitudinal;
        ourCamera.transform.position += lateral + longitudinal;
    }

    private void RotateView(float rotXstep, float rotYstep)
    {
        //Prevent the camera from dropping down below the ground plane
        if (rotXstep < 0f)
            rotXstep = Mathf.Max(3f - ourCamera.transform.rotation.eulerAngles.x, rotXstep );
        else
            //Prevent the camera from rotating over the top (which makes Y flip in LoogAt call and isn't terrible, but still not what we want)
            rotXstep = Mathf.Min((89.95f - ourCamera.transform.rotation.eulerAngles.x), rotXstep);

        //Rotate camera around the target so it keeps looking at target
        ourCamera.transform.RotateAround(lookAtTarget, ourCamera.transform.right, rotXstep);
        ourCamera.transform.RotateAround(lookAtTarget, Vector3.up, rotYstep);
    }

    private void TouchHandler()
    {
        //Input.touch stuff seems to be for multitouch screens, not for trackpad
        //Not sure yet how to handle trackpad/touchpad gestures
        if (Input.touchCount > 0)
            Debug.Log("touchCount: " + Input.touchCount);
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            Debug.Log("touch delta: " + touchDeltaPosition);
        }
    }

    private void Zoom(float amount)
    {
        if(ourCamera.orthographic)
        {
            ourCamera.orthographicSize += -amount;
        }
        else
        {
            //perspective. move along fwd vector
            ourCamera.transform.position += ourCamera.transform.forward * amount;
        }
    }

    private void CheckForInput()
    {
        float vertButton = Input.GetAxisRaw("Vertical");
        float horzButton = Input.GetAxisRaw("Horizontal");
        //These two were used in orig code, but I'm not using, at least not at this point.
        //float turnButton = Input.GetAxisRaw("Turn");
        //float spaceButton = Input.GetAxisRaw("Jump");

        //Forward / backward translation parallel to ground plane.
        //NOT movement along camera-forward
        if ((vertButton != 0f || horzButton != 0f))
        {
            TranslateView(horzButton * translationScaleKeys, vertButton * translationScaleKeys);
            //Debug.Log("horzButton " + horzButton);
        }
        if (Input.GetButton("Fire2"/*right mouse button*/))
        {
            // Read the mouse input axis
            float trX = Input.GetAxis("Mouse X");
            float trY = Input.GetAxis("Mouse Y");
            TranslateView(-trX * translationScaleMouse, -trY * translationScaleMouse);
        }

        //Rotation
        if (Input.GetButton("Fire1"/*left mouse button*/))
        {
            // Read the mouse input axis
            float rotX = Input.GetAxis("Mouse Y");
            float rotY = Input.GetAxis("Mouse X");
            RotateView(-rotX * rotationScale, rotY * rotationScale);
        }

        //TODO
        TouchHandler();

        //Zoom
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            Zoom(-zoomScale);
        }
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            Zoom(zoomScale);
        }

    }
    // Update is called once per frame
    void Update () {

        //Look for input controls if the UI isn't being used
        //if (!EventSystem.current.IsPointerOverGameObject() ) //NOTE - this method fails when cursor leaves area of UI control but still is controlling it, e.g. with a slider when you move up or down off of it while still holding click on it
        if (EventSystem.current.currentSelectedGameObject == null)
            CheckForInput();

        //Update target
        ourCamera.transform.LookAt(lookAtTarget);
        //Debug.Log("target: " + target);
	}


}
