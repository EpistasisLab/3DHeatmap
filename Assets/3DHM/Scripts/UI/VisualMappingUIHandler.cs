using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SMView;

/// <summary>
/// Handler code for the TOP PANEL that contains multiple UI group for visual mapping selections, i.e
/// for setting which data variables should be mapped to which visual parameter.
/// </summary>
public class VisualMappingUIHandler : MonoBehaviour {

    //Convenience refs to the dropdown UI elements for each visual mapping
    private Dropdown heightLabelDropdown;
    private Dropdown topColorLabelDropdown;
    private Dropdown sideColorLabelDropdown;
    private Dropdown topColortableDropdown;
    private Dropdown sideColortableDropdown;

    //Accessors for dropdown values via SMV.
    //The main value of using SMV for these is so that we can have these in both
    // desktop and VR menus and they automatically stay sync'ed.
    private int HeightLabelDropdownValue
    {
        get { return SMV.I.GetValueInt(SMVmapping.VarMapHeight); }
        set { SMV.I.SetValue(SMVmapping.VarMapHeight, value); }
    }
    private int TopColorLabelDropdownValue
    {
        get { return SMV.I.GetValueInt(SMVmapping.VarMapTop); }
        set { SMV.I.SetValue(SMVmapping.VarMapTop, value); }
    }
    private int SideColorLabelDropdownValue
    {
        get { return SMV.I.GetValueInt(SMVmapping.VarMapSide); }
        set { SMV.I.SetValue(SMVmapping.VarMapSide, value); }
    }
    private int TopColortableDropdownValue
    {
        get { return SMV.I.GetValueInt(SMVmapping.VarMapTopColorTable); }
        set { SMV.I.SetValue(SMVmapping.VarMapTopColorTable, value); }
    }
    private int SideColortableDropdownValue
    {
        get { return SMV.I.GetValueInt(SMVmapping.VarMapSideColorTable); }
        set { SMV.I.SetValue(SMVmapping.VarMapSideColorTable, value); }
    }
       
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
        assigns[(int)DataManager.Mapping.TopColor] = TopColortableDropdownValue;
        assigns[(int)DataManager.Mapping.SideColor] = SideColortableDropdownValue;
        //assigns[(int)DataManager.Mapping.TopColor] = topColortableDropdown.value; // transform.Find("TopColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
        //assigns[(int)DataManager.Mapping.SideColor] = sideColortableDropdown.value; // transform.Find("SideColorPanel").transform.Find("ColorDropdown").gameObject.GetComponentInChildren<Dropdown>().value;
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
            SideColortableDropdownValue = index;
        else
            TopColortableDropdownValue = index;
    }

    /// <summary>
    /// Receive a value change event from one of the label dropdown elements in the panel.
    /// </summary>
    public void OnLabelValueChange()
    {
        //Debug.Log("Value Change. go.GetInstanceID() " + go.GetInstanceID());
        /*
        string label = go.GetComponentInChildren<Dropdown>().captionText.text;
        DataVariable var = DataManager.I.GetVariableByLabel( label );
        if( var == null)
        {
            Debug.LogWarning("null var returned for label " + label);
            return;
        }
        */
        AssignVarsByCurrentLabelChoices();
        UIManager.I.ShowNextUIActionPrompt();
        //This method will only do anything if auto-prompting has already finished.
        UIManager.I.StartRedrawPrompt();
        //DataManager.I.DebugDumpVariables(false);
    }

    /// <summary> The color table dropdown has changed </summary>
    public void OnColorValueChange()
    {
        //We don't do anything else here because we query the selected color table
        // on demand rather than store it somewhere.
        UIManager.I.StartRedrawPrompt();
    }

    /// <summary>
    /// Take the current dropdown choices and use them to assign vars to visual mapping, if vars are valid
    /// </summary>
    private void AssignVarsByCurrentLabelChoices()
    {
        //*NOTE* these dropdowns use SMVview componenets, but those only provide the 'value', i.e. chosen dropdown field index.
        // So we really on refs to the dropdowns themselves here to get chosen text.
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.Height, heightLabelDropdown.captionText.text);
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.TopColor, topColorLabelDropdown.captionText.text);
        DataManager.I.AssignVariableMappingByLabel(DataManager.Mapping.SideColor, sideColorLabelDropdown.captionText.text);
    }

    private void SetDropdownsValueByCurrentVars()
    {
        SetDropdownValueByVarLabel(DataManager.Mapping.Height, DataManager.I.HeightVar);
        SetDropdownValueByVarLabel(DataManager.Mapping.TopColor, DataManager.I.TopColorVar);
        SetDropdownValueByVarLabel(DataManager.Mapping.SideColor, DataManager.I.SideColorVar);
    }

    private void SetDropdownValueByMapping(DataManager.Mapping mapping, int value)
    {
        //Set value via SMV so VR menu gets updated too
        switch (mapping)
        {
            case DataManager.Mapping.Height: HeightLabelDropdownValue = value; break;
            case DataManager.Mapping.TopColor: TopColorLabelDropdownValue = value; break;
            case DataManager.Mapping.SideColor: SideColorLabelDropdownValue = value; break;
        }
    }

    /// <summary>
    /// Set the UI dropdown choice (Value) according to data variable label.
    /// This lets us change the dropdown options/list and set the
    /// choice back to what it was pointing at before (or the default if
    /// its variable was removed).
    /// Note that labels guaranteed to be unique, see DataManager.ForceUniqueLabels()
    /// </summary>
    /// <param name="dd"></param>
    /// <param name="var"></param>
    private void SetDropdownValueByVarLabel(DataManager.Mapping mapping, DataVariable var)
    {
        Dropdown dd = null;
        switch (mapping)
        {
            case DataManager.Mapping.Height: dd = heightLabelDropdown; break;
            case DataManager.Mapping.TopColor: dd = topColorLabelDropdown; break;
            case DataManager.Mapping.SideColor: dd = sideColorLabelDropdown; break;
        }
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
        SetDropdownValueByMapping(mapping, index);
    }

    /// <summary>
    /// Queries the DataManager for available variables and fills the dropdowns. </summary>
    public void RefreshUI()
    {
        PopulateLabelDropdownItems();
        //Since items in the dropdown may (probably) have changed, find ones that match
        // current variable mappings and set the dropdown to match that.
        SetDropdownsValueByCurrentVars();
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
        //This will set the options for all dropdowns mapped to
        // each mapping, i.e. in desktop and VR menus
        SMV.I.SetSpecial(SMVmapping.VarMapHeight, list);
        SMV.I.SetSpecial(SMVmapping.VarMapSide, list);
        SMV.I.SetSpecial(SMVmapping.VarMapTop, list);
        /* orig
        heightLabelDropdown.options = list;
        topColorLabelDropdown.options = list;
        sideColorLabelDropdown.options = list;
        */
    }
}
