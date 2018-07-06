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
    private Dropdown heightLabelDropdown;
    private Dropdown topColorLabelDropdown;
    private Dropdown sideColorLabelDropdown;

    // Use this for initialization
    void Start()
    {
        dataMgr = GameObject.Find("DataManager").GetComponent<DataManager>();
        if (dataMgr == null)
            Debug.LogError("dataMgr == null");

        //Assign the dropdown object refs. 
        //These seems like a clumsy way to handle this.
        heightLabelDropdown = transform.Find("HeightPanel").transform.Find("LabelDropdown").gameObject.GetComponentInChildren<Dropdown>();
        if (heightLabelDropdown == null)
            Debug.LogError("heightDropdown == null");
        topColorLabelDropdown = transform.Find("TopColorPanel").transform.Find("LabelDropdown").gameObject.GetComponentInChildren<Dropdown>();
        if (topColorLabelDropdown == null)
            Debug.LogError("topColorDropdown == null");
        sideColorLabelDropdown = transform.Find("SideColorPanel").transform.Find("LabelDropdown").gameObject.GetComponentInChildren<Dropdown>();
        if (sideColorLabelDropdown == null)
            Debug.LogError("sideColorDropdown == null");
    }

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// Return color table assignments based on current dropdown states.
    /// </summary>
    /// <returns>An array of color table ID's, indexed via DataManager.Mapping values</returns>
    public int[] GetColorTableAssignments()
    {
        int[] assigns = new int[3];
        assigns[(int)DataManager.Mapping.Height] = -1; //N/A
        assigns[(int)DataManager.Mapping.TopColor] = transform.Find("TopColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
        assigns[(int)DataManager.Mapping.SideColor] = transform.Find("SideColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
        return assigns;
    }

    /// <summary>
    /// Receive a value change event from one of the label dropdown elements in the panel.
    /// </summary>
    /// <param name="go"></param>
    public void OnLabelValueChange(GameObject go)
    {
        //Debug.Log("Value Change. go.GetInstanceID() " + go.GetInstanceID());
        string label = go.GetComponentInChildren<Dropdown>().captionText.text;
        DataVariable var = dataMgr.GetVariableByLabel( label );
        if( var == null)
        {
            Debug.LogWarning("null var returned for label " + label);
            return;
        }
        AssignVarsByCurrentLabelChoices();
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
    private void AssignVarsByCurrentLabelChoices()
    {
        
        dataMgr.AssignVariableMappingByLabel(DataManager.Mapping.Height, heightLabelDropdown.captionText.text);
        dataMgr.AssignVariableMappingByLabel(DataManager.Mapping.TopColor, topColorLabelDropdown.captionText.text);
        dataMgr.AssignVariableMappingByLabel(DataManager.Mapping.SideColor, sideColorLabelDropdown.captionText.text);
        /*
        string label;
        //label = heightDropdown.captionText.text;
        //if (dataMgr.GetVariableByLabel(label) != null)
        //    dataMgr.AssignHeightVarByLabel(label);
        label = topColorDropdown.captionText.text;
        if (dataMgr.GetVariableByLabel(label) != null)
            dataMgr.AssignTopColorVarByLabel(label);
        label = sideColorDropdown.captionText.text;
        if (dataMgr.GetVariableByLabel(label) != null)
            dataMgr.AssignSideColorVarByLabel(label);
        */
    }

    private void SetLabelChoicesByCurrentVars()
    {
        SetLabelChoiceIndexByVarLabel(heightLabelDropdown, dataMgr.HeightVar);
        SetLabelChoiceIndexByVarLabel(topColorLabelDropdown, dataMgr.TopColorVar);
        SetLabelChoiceIndexByVarLabel(sideColorLabelDropdown, dataMgr.SideColorVar);
    }

    /// <summary>
    /// Set the UI dropdown choice (Value) according to data variable label.
    /// This lets us change the dropdown options/list and set the
    /// choice back to what it was pointing at before (or the default if
    /// its variable was removed).
    /// </summary>
    /// <param name="dd"></param>
    /// <param name="var"></param>
    private void SetLabelChoiceIndexByVarLabel(Dropdown dd, DataVariable var)
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
        PopulateLabelDropdownItems();
        //Since items in the dropdown may (probably) have changed, find ones that match
        // current variable mappings and set the dropdown to match that.
        SetLabelChoicesByCurrentVars();
        //Now call this since we want to make default assignments when a new
        // var is added or only one var and can't use choice dropdown.
        AssignVarsByCurrentLabelChoices();
    }

    void PopulateLabelDropdownItems()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        foreach (string label in dataMgr.GetLabels())
        {
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = label;
            list.Add(od);
        }
        heightLabelDropdown.options = list;
        topColorLabelDropdown.options = list;
        sideColorLabelDropdown.options = list;
    }
}
