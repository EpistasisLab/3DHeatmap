using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Script for managing camera movement, etc
public class CameraManager : MonoBehaviorSingleton<CameraManager> {

    /// <summary> Scaling for translational input </summary>
    public float translationScaleKeys = 1f;
    public float translationScaleMouse = 8f;
    public float rotationScale = 1f;
    public float zoomScale = 1f;
    public Vector3 defaultPosition;

    private Camera ourCamera;

    //The main object for the app
    private HeatVRML heatVRML;

    /// <summary> The look-at target for the camera </summary>
    private Vector3 lookAtTarget;

    //Use this instead of Awake
    //void Awake()
    protected override void Initialize()
    {

    }

    // Use this for initialization
    void Start () {
        //Expect a Camera component in this class' object
        ourCamera = transform.GetComponent<Camera>() as Camera;
        if (ourCamera == null)
            Debug.LogError("camera == null");
        heatVRML = GameObject.Find("Prefab objectify").GetComponent<HeatVRML>();
        if (heatVRML == null)
            Debug.LogError("heatVRML == null");

        lookAtTarget = Vector3.zero;
    }

    /// <summary> Directly set the LookAt target for the camera </summary>
    /// <param name="_target">Position to look at </param>
    public void LookAt(Vector3 _target)
    {
        lookAtTarget = _target;
    }

    /// <summary> Reset the camera view to the default view. </summary>
    public void ResetView()
    {
        Vector3 center = heatVRML.GetPlotCenter();
        ourCamera.transform.position = new Vector3(center.x, defaultPosition.y, defaultPosition.z);
        LookAt(center);
    }

    public void OnResetViewButtonClick()
    {
        ResetView();
    }

    public void TranslateView(float lateralStep, float forwardsStep)
    {
        //Move both the camera and lookat target
        Vector3 lateral = ourCamera.transform.right * lateralStep;
        //Move fwd/back only parallel to ground plane
        Vector3 forwards = new Vector3(ourCamera.transform.forward.x, 0f, ourCamera.transform.forward.z).normalized * forwardsStep;
        lookAtTarget += lateral + forwards;
        ourCamera.transform.position += lateral + forwards;
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


    /// <summary> Zoom by moving camera along its fwd vector</summary>
    /// <param name="amount">Amount to move by - scale the zoomScale public property. Use +1 (zoom in) and -1 (zoom out) to control direction and use default zoom amount.</param>
    public void Zoom(float amount)
    {
        float zoomAmount = amount * zoomScale;
        if(ourCamera.orthographic)
        {
            ourCamera.orthographicSize += zoomAmount;
        }
        else
        {
            //perspective. move along fwd vector
            ourCamera.transform.position += ourCamera.transform.forward * zoomAmount;
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

        //Check for touch events and proces them
        TouchManager.Instance.Process();

        //Zoom
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            Zoom(-1f);
        }
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            Zoom(1f);
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
