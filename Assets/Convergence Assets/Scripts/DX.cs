using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

/* NOTE
 * Stauffer - got this from "Standard Assets/Scripts" folder in orig project
 * For using the 3Dconnexion SpaceNavigator 3D mouse.
 * Don't know where it came from, i.e. if from 3Dconnexion, from unity store/forum - prob not from 3DHM developer
 * Note that there's no 'using' directive above, and the dll import is commented out below.
 * I guess the use of namespace TDx.TDxInput below is resolved by compiler looking in plugin folder?
 */

public class DX : MonoBehaviour {

    //[DllImport ("TDxInput")]
    //private static extern TDxInput.Device Device();
    
    //[DllImport ("TDxInput")]
    //private static extern void Device();
    
    #if UNITY_STANDALONE_WIN
    
    private static TDx.TDxInput.Device device;
    private static bool hasOne = false;
    
	// Use this for initialization
	void Awake () {
		if(!hasOne)
		{
	        device = new TDx.TDxInput.Device();
	        device.Connect();
	        Debug.Log("Connecting DX");
	        DontDestroyOnLoad(this);
	        hasOne = true;
		}
	}
	
	public static float GetX()
	{
		return (float)(device.Sensor.Translation.X / 80.0);
	}
	
	public static bool hasController
	{
		get
		{
			return hasOne;
		}
	}

	public static float GetY()
	{
		return (float)(device.Sensor.Translation.Y / 80.0);
	}

	public static float GetZ()
	{
		return (float)(device.Sensor.Translation.Z / -80.0);
	}

	public static float GetRX()
	{
		return (float)(device.Sensor.Rotation.X * device.Sensor.Rotation.Angle / 80.0);;
	}

	public static float GetRY()
	{
		return (float)-(device.Sensor.Rotation.Y * device.Sensor.Rotation.Angle / 80.0);
	}

	public static float GetRZ()
	{
		return (float)device.Sensor.Rotation.Z;
	}

	#endif
}
