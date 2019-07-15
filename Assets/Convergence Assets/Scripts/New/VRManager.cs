using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

public class VRManager : MonoBehaviorSingleton<VRManager> {

    enum Hand { left, right };

    private Hand GetHand(SteamVR_Input_Sources sis)
    {
        return sis == SteamVR_Input_Sources.LeftHand ? Hand.left : Hand.right;
    }

    /// <summary> The default player/hmd offset from the plot's center/front edge </summary>
    public Vector3 defaultPlayerOffset;

    /// <summary> The headset, camera rig object </summary>
    public GameObject HmdRig;

    /// <summary> Flags for whether a controller grab is currently happening.
    /// Array with element for each hand. </summary>
    private bool[] grabDown = new bool[2];

    /// <summary> Current hand/controller position. Vector with one elem for each hand. </summary>
    private Vector3[] handPos = new Vector3[2];

    /// <summary> Controller position at previous frame </summary>
    private Vector3[] handPosPrev = new Vector3[2];

    // Use this for initialization
    override protected void Initialize() {
        grabDown[0] = grabDown[1] = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate()
    {
        //Do things in late update so we can grab current action states first via the event handlers


        LookForGrabActivity();

        //Update hand position
        handPosPrev[0] = handPos[0];
        handPosPrev[1] = handPos[1];
    }


    private void LookForGrabActivity()
    {
        if (grabDown[0] || grabDown[1])
        {
            Vector3 d = grabDown[0] ? handPos[0] - handPosPrev[0] : handPos[1] - handPosPrev[1];
            
            //Debug.Log("d: " + d.ToString("F4"));

            HeatVRML.Instance.TranslateRidges(d.x * 250f, d.y*100f, d.z * 250f); //hack in a larger movement until we get scaling figured out in VR
        }
    }

    //Event handlers for events from SteamVR Input system
    //

    //Controller grip button has changed state
    public void OnGrabGripChange(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        //I figure we should only get grip changes from left and right hand controllers, but just in case...
        if (sis != SteamVR_Input_Sources.LeftHand && sis != SteamVR_Input_Sources.RightHand)
            return;

        //Debug.Log("Grab with " + GetHand(sis).ToString() + " state " + state);

        grabDown[(int)GetHand(sis)] = state;
        handPosPrev[(int)GetHand(sis)] = handPos[(int)GetHand(sis)];
    }

    //Controller/hand has changed position/rotation
    public void OnControllerTransformChange(SteamVR_Behaviour_Pose sbb, SteamVR_Input_Sources sis)
    {
        handPos[(int)GetHand(sis)] = sbb.transform.position;
        //Debug.Log("handPos " + handPos[0].ToString("F3") + " " + handPos[1].ToString("F3"));
    }

    //Reset the player/hmd to default position
    public void ResetPlayerPosition()
    {
        Vector3 center = HeatVRML.Instance.GetPlotCenter();
        HmdRig.transform.position = new Vector3(center.x, HeatVRML.Instance.xzySceneCorner.y + defaultPlayerOffset.y, HeatVRML.Instance.xzySceneCorner.z + defaultPlayerOffset.z);
    }
}