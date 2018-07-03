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
            Debug.LogWarning("null var returned for label " + label);
            return;
        }
        AssignVarsByCurrentChoices();
        dataMgr.DebugDumpVariables(false);
        /*
        if (dd == heightDropdown)
            dataMgr.AssignHeightVarByLabel(label);
        else if (dd == topColorDropdown)
            dataMgr.AssignTopColorVarByLabel(label);
        else if (dd == sideColorDropdown)
            dataMgr.AssignSideColorVarByLabel(label);
        else
            Debug.LogError("Unmatched dropdown component");
        */
    }

    /// <summary>
    /// Take the current dropdown choices and use them to assign vars to visual mapping, if vars are valid
    /// </summary>
    private void AssignVarsByCurrentChoices()
    {
        string label;
        label = heightDropdown.captionText.text;
        if (dataMgr.GetVariableByLabel(label) != null)
            dataMgr.AssignHeightVarByLabel(label);
        label = topColorDropdown.captionText.text;
        if (dataMgr.GetVariableByLabel(label) != null)
            dataMgr.AssignTopColorVarByLabel(label);
        label = sideColorDropdown.captionText.text;
        if (dataMgr.GetVariableByLabel(label) != null)
            dataMgr.AssignSideColorVarByLabel(label);
    }

    private void SetChoicesByCurrentVars()
    {
        SetChoiceIndexByVarLabel(heightDropdown, dataMgr.HeightVar);
        SetChoiceIndexByVarLabel(topColorDropdown, dataMgr.TopColorVar);
        SetChoiceIndexByVarLabel(sideColorDropdown, dataMgr.SideColorVar);
    }

    private void SetChoiceIndexByVarLabel(Dropdown dd, DataVariable var)
    {
        int index;
        if (var == null)
            index = 0;
        else
            //Look at each item in the choice list until we get a match
            for ( index = 0; index < dd.options.Count; index++)
            {
                if (dd.options[index].text == var.Label)
                    break;
            }
        //If no match, defaults to first item in dropdown
        if (index == dd.options.Count)
            index = 0;
        dd.value = index;
    }

    /// <summary>
    /// Queries the DataManager for available variables and fills the dropdowns. </summary>
    public void RefreshUI()
    {
        PopulateDropdownItems();
        //Since items in the dropdown may (probably) have changed, find ones that match
        // current variable mappings and set the dropdown to match that.
        SetChoicesByCurrentVars();
        //Now call this since we want to make default assignments when a new
        // var is added or only one var and can't use choice dropdown.
        AssignVarsByCurrentChoices();
    }

    void PopulateDropdownItems()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        foreach (string label in dataMgr.GetLabels())
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
