using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add this as a component to a UI Game Object that has an Image component (e.g. Button or Dropdown).
/// It will find the Image component and flash it on demand.
/// </summary>
public class UIElementFlasher : MonoBehaviour {

    private Image imageComp;
    private bool isFlashing;
    private float flashStartTime;
    private Color origColor;
    private static Color flashColor = new Color(0.2f,0.8f,0.2f);

	// Use this for initialization
	void Start () {
        imageComp = GetComponent<Image>();
        if (imageComp == null)
            Debug.LogError("imageComp == null");
        isFlashing = false;
        origColor = imageComp.color;
    }

    //co-routine
    IEnumerator Flash()
    {
        //Debug.Log("Starting Flash coroutine. imageComp: " + imageComp.GetInstanceID());
        while (isFlashing)
        {
            float phase = Mathf.Abs((Time.time - flashStartTime + 0.5f) % 1f - 0.5f);
            imageComp.color = Color.Lerp(origColor, flashColor, phase);
            //Debug.Log("Flash: phase  Color: " + phase + "  " + imageComp.ToString());
            yield return null;
        }
        imageComp.color = origColor;
    }

    public void StartFlashing()
    {
        if (imageComp == null)
            return;
        if (isFlashing)
        {
            Debug.LogWarning("UIElementFlasher: already flashing. Ignoring");
            return;
        }
        flashStartTime = Time.time;
        isFlashing = true;
        StartCoroutine(Flash());
    }

    public void StopFlashing()
    {
        //Debug.Log("StopFlashing called. imageComp: " + imageComp.GetInstanceID());
        isFlashing = false;
    }

}
