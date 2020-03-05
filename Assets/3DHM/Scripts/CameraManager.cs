using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script for managing camera movement, etc
public class CameraManager : MonoBehaviorSingleton<CameraManager> {

    /// <summary> The default camera view position relative to center of front of plot area </summary>
    public Vector3 defaultViewOffset;

    private Camera ourCamera;

    /// <summary> The look-at target for the camera </summary>
    private Vector3 lookAtTarget;

    /// <summary> Flag to enable/disable follow-HMD mode </summary>
    public bool followHmdEnabled;
    /// <summary> Offset along HMD fwd vector for the smooth follow </summary>
    public float followHmdFwdOffset = -1;
    /// <summary> Smooth time for position for follow mode </summary>
    public float followHmdPosTime = 0.7f;
    public float followHmdPosMaxVel = 4;
    public float followHmdRotTime = 1;
    public float followHmdRotMaxVel = 100;

    /// <summary> Power to which to zoom scale based on distance </summary>
    public float zoomScalePower = 1.5f;
    /// <summary> Distance from graph at which to start accelerating zoom scaling </summary>
    public float zoomScaleHeight = 10;

    /// <summary> The VR HMD transform to use for FollowHMD mode in VR mode.
    /// If not set up yet, ie during init IIRC, then just return a dummy transform </summary>
    private Transform hmdTransform { get { return VRManager.I.hmdTransform == null ? gameObject.transform : VRManager.I.hmdTransform; } }
    /// <summary> For VR FollowHmd mode. Helper class for SmoothDamp. </summary>
    private SmoothVector3 posSmoother;
    /// <summary> For VR FollowHmd mode. Helper class for SmoothDamp. </summary>
    private SmoothQuaternion rotSmoother;

    //Use this instead of Awake since this is a MonoBehaviorSingleton
    //void Awake()
    protected override void Initialize()
    {
        //Expect a Camera component in this class' object
        ourCamera = transform.GetComponent<Camera>() as Camera;
        if (ourCamera == null)
            Debug.LogError("camera == null");

        lookAtTarget = Vector3.zero;

        //Smoothing helpers
        posSmoother = new SmoothVector3(Vector3.zero);
        rotSmoother = new SmoothQuaternion(Quaternion.identity);

        followHmdEnabled = false;
    }

    // Use this for initialization
    void Start () {
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
        Vector3 center = Graph.I.GetPlotCenter();
        SetCameraPositionWithBounds( new Vector3(center.x, Graph.I.sceneCorner.y + defaultViewOffset.y, Graph.I.sceneCorner.z + defaultViewOffset.z) );
        LookAt(center);
    }

    public void OnResetViewButtonClick()
    {
        ResetView();
    }

    IEnumerator FollowHMDenableCoroutine(bool enable)
    {
        ourCamera.enabled = false;
        //Empirically, wait for a little bit. Waiting for two frames doesn't work, didn't try more.
        yield return new WaitForSeconds(0.5f);
        ourCamera.enabled = true;
        followHmdEnabled = enable;
        //Refresh the UI for VR stuff. Awkward to have to call it here.
        VRManager.I.UIupdate();
    }

    /// <summary>
    /// Enable/disable followHMD mode. This is used with VR mode to change between showing VR headset
    /// view (when this is disabled), and regullar desktop view/controls (when this is enabled)
    /// </summary>
    /// <param name="enable"></param>
    public void FollowHMDenable(bool enable)
    {
        //Workaround - need to disable and reenable the camera after VR rig has been enabled
        StartCoroutine(FollowHMDenableCoroutine(enable));
    }

    public void TranslateView(float lateralStep, float forwardsStep)
    {
        //Move both the camera and lookat target
        Vector3 lateral = ourCamera.transform.right * lateralStep;
        //Move fwd/back only parallel to ground plane
        Vector3 forwards = new Vector3(ourCamera.transform.forward.x, 0f, ourCamera.transform.forward.z).normalized * forwardsStep;
        lookAtTarget += lateral + forwards;
        SetCameraPositionWithBounds( ourCamera.transform.position + lateral + forwards );
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
        //Change speed of zoom based on camera height. Below zoomScaleHeight, we slow down. Above that we speed up
        float zoomAmount = amount * Mathf.Pow( ourCamera.transform.position.y / zoomScaleHeight, zoomScalePower ) ;
        if(ourCamera.orthographic)
        {
            ourCamera.orthographicSize += zoomAmount;
        }
        else
        {
            //perspective. move along fwd vector
            Vector3 newPos = ourCamera.transform.position + ourCamera.transform.forward * zoomAmount;
            SetCameraPositionWithBounds( newPos );
        }
    }

    /// <summary> Set the camera position with some bounds checking and enforcement. </summary>
    /// <param name="newPos"></param>
    private void SetCameraPositionWithBounds(Vector3 newPos)
    {
        newPos.y = Mathf.Max(Mathf.Min(newPos.y, 200), 0.1f);
        ourCamera.transform.position = newPos;
    }

    private void FollowHmdUpdate()
    {
        //Update for FollowHMD mode
        if (followHmdEnabled && Time.deltaTime > 0)
        {
            //position
            posSmoother.Target = hmdTransform.position + (hmdTransform.forward * followHmdFwdOffset);
            posSmoother.Update(followHmdPosTime, Time.deltaTime, followHmdPosMaxVel);
            SetCameraPositionWithBounds( posSmoother.Value );

            //rotation
            rotSmoother.Target = hmdTransform.rotation;
            rotSmoother.Update(followHmdRotTime, Time.deltaTime, followHmdRotMaxVel);
            ourCamera.transform.rotation = rotSmoother.Value;
        }
    }

    // Update is called once per frame
    void Update () {
        //Update target
        if(followHmdEnabled)
            FollowHmdUpdate();
        else
            ourCamera.transform.LookAt(lookAtTarget);
        //Debug.Log("target: " + target);
	}


}
