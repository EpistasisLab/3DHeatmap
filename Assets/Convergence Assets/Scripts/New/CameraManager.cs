using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script for managing camera movement, etc
public class CameraManager : MonoBehaviorSingleton<CameraManager> {

    /// <summary> The default camera view position relative to center of front of plot area </summary>
    public Vector3 defaultViewOffset;

    private Camera ourCamera;

    //The main object for the app
    private HeatVRML heatVRML;

    /// <summary> The look-at target for the camera </summary>
    private Vector3 lookAtTarget;

    //Use this instead of Awake since this is a MonoBehaviorSingleton
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
        ourCamera.transform.position = new Vector3(center.x, heatVRML.sceneCorner.y + defaultViewOffset.y, heatVRML.sceneCorner.z + defaultViewOffset.z);
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

    /// <summary> Rotate camera around the lookAtTarget point </summary>
    /// <param name="rotRightStep">rotation step around camera's Right vector</param>
    /// <param name="rotUpStep">rotation step around camera's Up vector</param>
    public void RotateView(float rotRightStep, float rotUpStep)
    {
        //Prevent the camera from dropping down below the ground plane
        if (rotRightStep < 0f)
            rotRightStep = Mathf.Max(2f - ourCamera.transform.rotation.eulerAngles.x, rotRightStep );
        else
            //Prevent the camera from rotating over the top (which makes Y flip in LoogAt call and isn't terrible, but still not what we want)
            rotRightStep = Mathf.Min((89.95f - ourCamera.transform.rotation.eulerAngles.x), rotRightStep);

        //Rotate camera around the target so it keeps looking at target
        ourCamera.transform.RotateAround(lookAtTarget, ourCamera.transform.right, rotRightStep);
        ourCamera.transform.RotateAround(lookAtTarget, Vector3.up, rotUpStep);
    }

   /// <summary> Zoom by moving camera along its fwd vector</summary>
    /// <param name="amount">Amount to move by. Use + (zoom in) and - (zoom out) to control direction.</param>
    public void Zoom(float amount)
    {
        float zoomAmount = amount;
        if(ourCamera.orthographic)
        {
            ourCamera.orthographicSize += zoomAmount;
        }
        else
        {
            //perspective. move along fwd vector
            Vector3 newPos = ourCamera.transform.position + ourCamera.transform.forward * zoomAmount;
            if (newPos.y < 0.1f)
                return;
            ourCamera.transform.position = newPos;
        }
    }

    // Update is called once per frame
    void Update () {
        //Update target
        ourCamera.transform.LookAt(lookAtTarget);
        //Debug.Log("target: " + target);
	}


}
