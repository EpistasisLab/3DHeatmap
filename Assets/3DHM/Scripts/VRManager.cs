using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using SMView;

public class VRManager : MonoBehaviorSingleton<VRManager> {

    enum Hand { left, right };

    /*
    private Hand GetHand(SteamVR_Input_Sources sis)
    {
        return sis == SteamVR_Input_Sources.LeftHand ? Hand.left : Hand.right;
    }
    */

    /// <summary> UI element for the enable/disable button </summary>
    public Text VRenableButtonText;

    /// <summary> The default player/hmd offset from the plot's center/front edge </summary>
    public Vector3 defaultPlayerOffset;

    /// <summary> The headset, camera rig object </summary>
    public GameObject HmdRig;

    /// <summary> Scaling in each direction for movement of data by grab-and-move </summary>
    public Vector3 grabMoveScale;
    /// <summary> Exponent of scaling of movement during grab </summary>
    public float grabMoveScaleExp = 1;

    /// <summary> Angle in degrees around which to rotate the fwd vector of the 
    ///  controller to get the direciton we want, i.e. a more natural forward direction. 
    ///  Will need to modify this based on which VR system/controller is in use. </summary>
    public float laserPointerXRot = 0;

    public Vector3 UserHeadPosition { get { return HmdRig.transform.position; } }

    /// <summary> Flags for whether a controller grab is currently happening.
    /// Array with element for each hand. </summary>
    private bool[] grabDown = new bool[2];
    private bool[] triggerDown = new bool[2];

    /// <summary> Current hand/controller position. Vector with one elem for each hand. </summary>
    private Vector3[] handPos = new Vector3[2];
    private Quaternion[] handRot = new Quaternion[2];

    /// <summary> Controller position at previous frame </summary>
    private Vector3[] handPosPrev = new Vector3[2];
    private Quaternion[] handRotPrev = new Quaternion[2];

    /// <summary> For now we'll just use OpenVR </summary>
    private string vrDeviceName = "OpenVR";

    /// <summary> Flag. Is VR currently available on this computer? \
    /// NOTE - this returns true if device is connected before running this app,
    /// but it doesn't start returning true if a device (Oculus via SteamVR, at least)
    /// is connected only after app starts. </summary>
    public bool VRdevicePresent { get { return XRDevice.isPresent; } }

    /// <summary> Flag. Is the VR device enabled? </summary>
    public bool VRdeviceEnabled { get { return XRSettings.enabled; } }

    /// <summary> Check if vr device is loaded and ready to use. May not be enabled, but it's ready to be enabled. </summary>
    private bool VRisAvailable { get { return VRdevicePresent && (String.Compare(XRSettings.loadedDeviceName, vrDeviceName, true) == 0); } }

    /// <summary> True if 3DHM is running VR mode </summary>
    public bool VRmodeIsEnabled { get; private set; }

    // Use this for initialization instead of Awake()
    override protected void Initialize()
    {
        grabDown[0] = grabDown[1] = false;

        //StartCoroutine("DeviceCheckDebug");

        //Enter VR mode if available
        VRmodeEnable(VRisAvailable);
    }


    void Start()
    {
    }

	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate()
    {
        //Do things in late update so we can grab current action states first via the event handlers.

        //Check for trigger
        LookForTriggerActivity();

        //Check for grip button
        LookForGrabActivity();

        //Update hand position
        handPosPrev[0] = handPos[0];
        handPosPrev[1] = handPos[1];
        handRotPrev[0] = handRot[0];
        handRotPrev[1] = handRot[1];
    }

    /// <summary>
    /// NOTE - this does not load VR device, it just takes care of everything else needed when
    ///  VR is enabled or disabled. 
    ///  To load VR device and enable VR mode, call DeviceLoadAndEnable()
    /// </summary>
    /// <param name="enable"></param>
    private void VRmodeEnable(bool enable)
    {
        //Debug.Log("Entering VRmodeEnable: " + enable);
        VRmodeIsEnabled = false;

        if( enable == true && !VRisAvailable)
        {
            Debug.LogError("VrmodeEnable called, but VR is not available.");
            return;
        }

        //VR setup
        XRSettings.enabled = enable;
        
        //Hack this setting to prevent errors with steamvr behaviors when we disable vr mode
        /*
        SteamVR_Input.initialized = enable;
        */

        //Do this before enabling followHMD mode in desktop camera.
        //When this is enabled, it will take over drawing to the app desktop window.
        HmdRig.SetActive(enable);

        //Tell desktop camera to go into follow-hmd mode.
        //Do this after enabling the hmd rig.
        CameraManager.I.FollowHMDenable(enable);

        VRmodeIsEnabled = enable && VRisAvailable;

        if (enable)
            ShowInstructions();

        UIupdate();
    }

    /// <summary>
    /// NOTE - Made this to be able to check at runtime if VR is available and give the user option
    ///  to enable it for use. But if steamVR is not already running, XRDevice.isPresent never returns true.
    /// </summary>
    /// <returns></returns>
    IEnumerator DeviceCheckDebug()
    {
        while (true)
        {
            Debug.Log("time " + Time.time + " XRDevice.isPresent: " + VRdevicePresent + " XRSettings.isDeviceActive: " + XRSettings.isDeviceActive + " XRSettings.loadedDeviceName: " + XRSettings.loadedDeviceName);
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator DeviceLoadAndEnableCoroutine()
    {
        int id = UIManager.I.StatusShow("Attempting to start VR device " + vrDeviceName + "...");
        XRSettings.LoadDeviceByName(vrDeviceName);
        float startTime = Time.time;
        //Wait for a few seconds for VR system to load.
        while( Time.time < startTime + 6)
        {
            yield return null;
            if (VRisAvailable)
                break;
        }
        UIManager.I.StatusComplete(id);
        if (String.Compare(XRSettings.loadedDeviceName, vrDeviceName, true) != 0 || !VRisAvailable)
        {
            UIManager.I.ShowMessageDialog("VR Device load for " + vrDeviceName + " failed.\nCheck that headset is attached\nand SteamVR is running properly.");
            VRmodeEnable(false);
        }
        else
        {
            VRmodeEnable(true);
        }
    }

    /// <summary>
    /// Try to load the VR device. Needs coroutine, so no immediate return value.
    /// </summary>
    public void DeviceLoadAndEnable()
    {
        if (VRisAvailable)
            return;
        //Using just openvr for now, so it's simple.
        //make sure it's not loaded already
        if (String.Compare(XRSettings.loadedDeviceName, vrDeviceName, true) != 0)
        {
            StartCoroutine("DeviceLoadAndEnableCoroutine");
        }
        else if( XRDevice.isPresent)
        {
            //If the load times out because it takes longer than I wait in the coroutine, this 
            // condition will handle it when this routine is called again.
            VRmodeEnable(true);
        }
    }

    public void DeviceDisconnect()
    {
        VRmodeEnable(false);
        XRSettings.LoadDeviceByName("");
    }

    private void LookForTriggerActivity()
    {
        if( triggerDown[1])
        {
            Quaternion correction = new Quaternion();
            correction.eulerAngles = new Vector3(laserPointerXRot, 0, 0);
            Ray ray = new Ray(handPos[1], handRot[1] * (correction * Vector3.forward));
            DataInspector.I.InspectDataWithRay(ray, true);
        }
    }

    private void LookForGrabActivity()
    {
        if (grabDown[0] || grabDown[1])
        {
            Vector3 d = grabDown[0] ? handPos[0] - handPosPrev[0] : handPos[1] - handPosPrev[1];

            //Debug.Log("d: " + d.ToString("F4"));

            Graph.I.TranslateGraph(Mathf.Pow(Mathf.Abs(d.x), grabMoveScaleExp) * Mathf.Sign(d.x) * grabMoveScale.x,
                                              Mathf.Pow(Mathf.Abs(d.y), grabMoveScaleExp) * Mathf.Sign(d.y) * grabMoveScale.y,
                                              Mathf.Pow(Mathf.Abs(d.z), grabMoveScaleExp) * Mathf.Sign(d.z) * grabMoveScale.z);
        }
    }

    //Event handlers for events from SteamVR Input system
    //
    /*
    //Controller grip button has changed state
    public void OnGrabGripChange(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        //I figure we should only get grip changes from left and right hand controllers, but just in case...
        if (sis != SteamVR_Input_Sources.LeftHand && sis != SteamVR_Input_Sources.RightHand)
            return;

        //Debug.Log("GrabGrip with " + GetHand(sis).ToString() + " state " + state);

        grabDown[(int)GetHand(sis)] = state;
        handPosPrev[(int)GetHand(sis)] = handPos[(int)GetHand(sis)];
    }

    /// <summary> GrabPinch is trigger press </summary>
    public void OnGrabPinchChange(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        //I figure we should only get grip changes from left and right hand controllers, but just in case...
        if (sis != SteamVR_Input_Sources.LeftHand && sis != SteamVR_Input_Sources.RightHand)
            return;

        //Debug.Log("GrabPinch with " + GetHand(sis).ToString() + " state " + state);

        triggerDown[(int)GetHand(sis)] = state;
    }

    //Controller/hand has changed position/rotation
    public void OnControllerTransformChange(SteamVR_Behaviour_Pose sbb, SteamVR_Input_Sources sis)
    {
        handPos[(int)GetHand(sis)] = sbb.transform.position;
        handRot[(int)GetHand(sis)] = sbb.transform.rotation;
        //Debug.Log("handPos " + handPos[0].ToString("F3") + " " + handPos[1].ToString("F3"));
    }
    */

    //Reset the player/hmd to default position
    public void ResetPlayerPosition()
    {
        Vector3 center = Graph.I.GetPlotCenter();
        HmdRig.transform.position = new Vector3(center.x, Graph.I.sceneCorner.y + defaultPlayerOffset.y, Graph.I.sceneCorner.z + defaultPlayerOffset.z);
    }

    //////////
    // VR UI 
    //
    public void OnEnableButtonClick()
    {
        if (VRmodeIsEnabled)
            DeviceDisconnect();
        else
            DeviceLoadAndEnable();
    }

    public void OnDesktopViewDropdown(int choice)
    {
        bool followHMD = choice == 1;
        CameraManager.I.FollowHMDenable(followHMD);
    }

    /// <summary> Update the VR UI based on VR system's current state </summary>
    public void UIupdate()
    {
        SMV.I.SetValue(SMVmapping.VRdesktopViewMode, CameraManager.I.followHmdEnabled ? 1 : 0);
        VRenableButtonText.text = VRmodeIsEnabled ? "Disable" : "Enable";
    }

    /// <summary>
    /// Simple message disalog with VR instructions
    /// </summary>
    public void ShowInstructions()
    {
        UIManager.I.ShowMessageDialog("VR Instructions\n\nData Inspection - hold the controller trigger button and point at the data.\n\nMoving the Data - hold the controller grip button and move the controller to move the data in all directions to change your view.");
    }
}