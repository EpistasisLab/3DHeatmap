using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for managing camera movement, etc
public class CameraManager : MonoBehaviour {

    /// <summary> Scaling for translational input </summary>
    public float translationScale = 1f;
    public float rotationScale = 1f;

    private Camera ourCamera;

    /// <summary> The look-at target for the camera </summary>
    private Vector3 target;

	// Use this for initialization
	void Start () {
        //Expect a Camera component in this class' object
        ourCamera = transform.GetComponent<Camera>() as Camera;
        if (ourCamera == null)
            Debug.LogError("camera == null");

        target = Vector3.zero;
    }

    /// <summary> Directly set the LookAt target for the camera </summary>
    /// <param name="_target">Position to look at </param>
    public void LookAt(Vector3 _target)
    {
        target = _target;
    }

    private void TranslateView(float lateralStep, float longitudinalStep)
    {
        //Move both the camera and lookat target
        Vector3 lateral = ourCamera.transform.right * lateralStep * translationScale;
        //Move fwd/back only parallel to ground plane
        Vector3 longitudinal = new Vector3(ourCamera.transform.forward.x, 0f, ourCamera.transform.forward.z) * longitudinalStep * translationScale;
        target += lateral + longitudinal;
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
        ourCamera.transform.RotateAround(target, ourCamera.transform.right, rotXstep);
        ourCamera.transform.RotateAround(target, Vector3.up, rotYstep);
    }

    // Update is called once per frame
    void Update () {

        float vertButton = Input.GetAxisRaw("Vertical");
        float horzButton = Input.GetAxisRaw("Horizontal");
        float turnButton = Input.GetAxisRaw("Turn");
        float spaceButton = Input.GetAxisRaw("Jump");

        //Forward / backward translation parallel to ground plane.
        //NOT movement along camera-forward
        if( vertButton != 0f || horzButton != 0f)
        {
            TranslateView(horzButton, vertButton);
        }

        //Rotation
        if (Input.GetButton("Fire1") )
        {
            // Read the mouse input axis
            float rotX = Input.GetAxis("Mouse Y");
            float rotY = Input.GetAxis("Mouse X");
            RotateView(-rotX * rotationScale, rotY * rotationScale);
        }

        //Update target
        ourCamera.transform.LookAt(target);
        //Debug.Log("target: " + target);
	}
}
