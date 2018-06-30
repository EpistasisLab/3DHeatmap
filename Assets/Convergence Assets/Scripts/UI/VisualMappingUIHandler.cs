using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler code for the PANEL that contains UI for visual mapping selections.
/// </summary>
public class VisualMappingUIHandler : MonoBehaviour {


    //Ref to DataManager object from the scene
    private DataManager dataMgr;

    //Convenience refs to the dropdown UI elements for each visual mapping
    private Dropdown heightDropdown;
    private Dropdown topColorDropdown;
    private Dropdown sideColorDropdown;

    // Use this for initialization
    void Start()
    {
        dataMgr = GameObject.Find("DataManager").GetComponent<DataManager>();
        if (dataMgr == null)
            Debug.LogError("dataMgr == null");

        //Assign the dropdown object refs. 
        heightDropdown = transform.Find("HeightPanel").gameObject.GetComponentInChildren<Dropdown>();
        if (heightDropdown == null)
            Debug.LogError("heightDropdown == null");
        topColorDropdown = transform.Find("TopColorPanel").gameObject.GetComponentInChildren<Dropdown>();
        if (topColorDropdown == null)
            Debug.LogError("topColorDropdown == null");
        sideColorDropdown = transform.Find("SideColorPanel").gameObject.GetComponentInChildren<Dropdown>();
        if (sideColorDropdown == null)
            Debug.LogError("sideColorDropdown == null");
    }

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// Receive a value change event from one of the dropdown elements in the panel.
    /// </summary>
    /// <param name="go"></param>
    public void OnValueChange(GameObject go)
    {
        Debug.Log("Value Change. go.GetInstanceID() " + go.GetInstanceID());
        Dropdown dd = go.GetComponentInChildren<Dropdown>();
        string label = dd.captionText.text;
        DataVariable var = dataMgr.GetVariableByLabel( label );
        if( var == null)
        {
            Debug.LogError("null var returned for label " + label);
            return;
        }
        if (dd == heightDropdown)
            dataMgr.AssignHeightVarByLabel(label);
        else if (dd == topColorDropdown)
            dataMgr.AssignTopColorVarByLabel(label);
        else if (dd == sideColorDropdown)
            dataMgr.AssignSideColorVarByLabel(label);
        else
            Debug.LogError("Unmatched dropdown component");
    }

    /// <summary>
    /// Queries the DataManager for available variables and fills the dropdowns. </summary>
    public void RefreshUI()
    {
        PopulateDropdownItems();
    }

    void PopulateDropdownItems()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        foreach (string label in dataMgr.GetLabels()) //NOTE - I don't really like exposing variables property like this
        {
            Dropdown.OptionData od = new Dropdown.OptionData
            {
                text = label
            };
            list.Add(od);
        }
        Dropdown[] dds = transform.GetComponentsInChildren<Dropdown>(); //is recursive
        foreach( Dropdown dd in dds)
        {
            dd.options = list;
        }
    }
}
