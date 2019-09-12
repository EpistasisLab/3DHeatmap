using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler code for the PANEL that contains UI for visual mapping selections, i.e
/// for setting which data variables should be mapped to which visual parameter.
/// </summary>
public class VisualMappingUIHandler : MonoBehaviour {

    //Convenience refs to the dropdown UI elements for each visual mapping
    private Dropdown heightLabelDropdown;
    private Dropdown topColorLabelDropdown;
    private Dropdown sideColorLabelDropdown;
    private Dropdown topColortableDropdown;
    private Dropdown sideColortableDropdown;

    // Use this for initialization
    void Start()
    {
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
        topColortableDropdown = transform.Find("TopColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>();
        if (topColortableDropdown == null)
            Debug.LogError("topColortableDropdown == null");
        sideColortableDropdown = transform.Find("SideColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>();
        if (sideColortableDropdown == null)
            Debug.LogError("sideColortableDropdown == null");
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
        assigns[(int)DataManager.Mapping.TopColor] = topColortableDropdown.value; // transform.Find("TopColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
        assigns[(int)DataManager.Mapping.SideColor] = sideColortableDropdown.value; // transform.Find("SideColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
        return assigns;
    }

    /// <summary>
    /// Set the color table selection for a given mapping. If we just set it here and then call
    /// Redraw, the new values will get picked up.
    /// </summary>
    /// <param name="index">Index into color table dropdown</param>
    public void SetColorTableByMappingAndIndex(DataManager.Mapping mapping, int index)
    {
        if (mapping == DataManager.Mapping.Height)
            return;
        if (mapping == DataManager.Mapping.SideColor)
            sideColortableDropdown.value = index;
        else
            topColortableDropdown.value = index;
    }

    /// <summary>
    /// Receive a value change event from one of the label dropdown elements in the panel.
    /// </summary>
    /// <param name="go"></param>
    public void OnLabelValueChange(GameObject go)
    {
        //Debug.Log("Value Change. go.GetInstanceID() " + go.GetInstanceID());
        string label = go.GetComponentInChildren<Dropdown>().captionText.text;
        DataVariable var = DataManager.I.GetVariableByLabel( label );
        if( var == null)
        {
            Debug.LogWarning("null var returned for label " + label);
            return;
        }
        AssignVarsByCurrentLabelChoices();
        UIManager.I.ShowNextUIActionPrompt(go);
        //This method will only do anything if auto-prompting has already finished.
        UIManager.I.StartRedrawPrompt();
        //DataManager.I.DebugDumpVariables(false);
    }

    /// <summary>
    /// Take the current dropdown choices and use them to assign vars to visual mapping, if vars are valid
    /// </summary>
    private void AssignVarsByCurrentLabelChoices()
    {
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.Height, heightLabelDropdown.captionText.text);
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.TopColor, topColorLabelDropdown.captionText.text);
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.SideColor, sideColorLabelDropdown.captionText.text);
    }

    private void SetLabelChoicesByCurrentVars()
    {
        SetLabelChoiceIndexByVarLabel(heightLabelDropdown, DataManager.I.HeightVar);
        SetLabelChoiceIndexByVarLabel(topColorLabelDropdown, DataManager.I.TopColorVar);
        SetLabelChoiceIndexByVarLabel(sideColorLabelDropdown, DataManager.I.SideColorVar);
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

        foreach (string label in DataManager.I.GetLabels())
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
