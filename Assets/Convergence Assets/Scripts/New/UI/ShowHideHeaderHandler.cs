using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for UI element that acts as a section header and show/hide button for 
/// certain UI elements
/// </summary>
public class ShowHideHeaderHandler : MonoBehaviour {

    /// <summary>
    /// Assign one or more UI elements to this list, and
    /// there visibility will be controlled. Typically this
    /// will be a list of UI panels.
    /// </summary>
    public List<GameObject> itemsToShowHide;

    public void OnShowHideButtonClick(GameObject button)
    {
        //Debug.Log("OnShowHideButtonClick. Obj name: " + button.name);
        //Show-hide all items assigned to this header.
        if( itemsToShowHide == null || itemsToShowHide.Count < 1)
        {
            Debug.LogWarning("itemsToShowHide is empty");
            return;
        }

        //Just look at first one to see if we should show or hide
        bool setActive = ! itemsToShowHide[0].activeInHierarchy;
        foreach ( GameObject go in itemsToShowHide)
        {
            go.SetActive(setActive);
        }

        //Change this button's +/- text
        string text = setActive ? "-" : "+";
        button.GetComponentInChildren<Text>().text = text;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
