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

    /// <summary> Flag for whether a controller grab is currently happening </summary>
    private bool[] grabDown = new bool[2];

    /// <summary> Controller position at previous frame </summary>
    private Vector3[] handPosPrev = new Vector3[2];
    private Vector3[] handPos = new Vector3[2];

    // Use this for initialization
    override protected void Initialize() {
        grabDown[0] = grabDown[1] = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate()
    {
        if( grabDown[0])
        {
            Vector3 d = handPos[0] - handPosPrev[0];
            Debug.Log("d: " + d);
            HeatVRML.Instance.TranslateRidges(d.x, d.z);
        }

        handPosPrev[0] = handPos[0];
        handPosPrev[1] = handPos[1];
    }

    //Event handlers from SteamVR Input system
    //
    public void OnGrabGripChange(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        //I figure we should only get grip changes from left and right hand controllers, but just in case...
        if (sis != SteamVR_Input_Sources.LeftHand && sis != SteamVR_Input_Sources.RightHand)
            return;

        Debug.Log("Grab with " + GetHand(sis).ToString() + " state " + state);

        grabDown[(int)GetHand(sis)] = state;
        handPosPrev[(int)GetHand(sis)] = handPos[(int)GetHand(sis)];
    }

    public void OnControllerTransformChange(SteamVR_Behaviour_Pose sbb, SteamVR_Input_Sources sis)
    {
        handPos[(int)GetHand(sis)] = sbb.origin.position;
    }


    /*
    public void OnGrabGripChangeLeft(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        Debug.Log("OnGrabGribChangeLeft, sbb " + sbb.actionSet.ToString() + " sis " + sis.ToString() + " " + state);
        //Grab(Hand.left, state);
    }

    public void OnGrabGripChangeRight(SteamVR_Behaviour_Boolean sbb, SteamVR_Input_Sources sis, bool state)
    {
        Debug.Log("OnGrabGribChangeRight, sbb " + sbb.actionSet.ToString() + " sis " + sis.ToString() + " " + state);
        //Grab(Hand.right, state);
    }
    */

}