using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for the DataVariablePanel UI element.
/// Handles events for all children.
/// Tracks DataVariable that it's assigned to.
/// </summary>
public class DataVarUIHandler : MonoBehaviour {

    /// <summary> The DataVariable that this UI panel is responsible for.
    /// null if hasn't been set or has been cleared. </summary>
    public DataVariable dataVar;

    /// <summary> Return true if this UI panel is currently assigned to a DataVariable </summary>
    public bool IsAssigned { get { return dataVar != null; } }

    //Ref to manager objects from the scene
    private DataManager dataMgr;
    private UIManager uiMgr;

    //Internal convenience ref
    private InputField inputField;

    private bool filenameTextMouseHovering;
    private float filenameTextMouseEnterTime;
    private bool filenameTextTooltipShowing;

    // Use this for initialization
    void Start () {
        dataMgr = GameObject.Find("DataManager").GetComponent<DataManager>();
        if (dataMgr == null)
            Debug.LogError("dataMgr == null");

        uiMgr = GameObject.Find("UIManager").GetComponent<UIManager>();
        if (uiMgr == null)
            Debug.LogError("uiMgr == null");

        //There's only one comp. of type InputField, so this is easy...
        inputField = transform.GetComponentInChildren<InputField>(); //should recurse
        if (inputField == null)
            Debug.LogError("inputField == null");

        Clear();
	}

    /// <summary>
    /// Remove any association with a DataVariable. </summary>
    public void Clear()
    {
        dataVar = null;
        filenameTextMouseHovering = false;
        filenameTextMouseEnterTime = float.MaxValue;
        filenameTextTooltipShowing = false;
        RefreshUI();
    }

    /// <summary> Update the UI for this panel, given the current state of the assoc'ed DataVariable </summary>
    public void RefreshUI()
    {
        //Update according to DataVariable
        string label = "_unassigned_";
        string filename = "None loaded";
        if( dataVar != null)
        {
            label = dataVar.Label;
            filename = Path.GetFileName(dataVar.Filename);
        }
        //There are multiple Text components in the whole panel, so seems I have to do this. Probably there is a better way...
        //And from what I read, transform.Find doesn't recurse, so you have to do this to go down beyond immeditate children.
        //.Find("LabelPanel/InputField").gameObject.GetComponentInChildren<InputField>().text = label;
        inputField.text = label;
        transform.Find("FilePanel/FilenameText").gameObject.GetComponentInChildren<Text>().text = filename;
    }

    // Update is called once per frame
    void Update () {
		if( filenameTextMouseHovering && !filenameTextTooltipShowing)
        {
            if( (Time.time - filenameTextMouseEnterTime) > 0.3f)
            {
                filenameTextTooltipShowing = true;
                string path = dataVar != null ? dataVar.Filename : "none";
                Vector3[] corners = new Vector3[4];
                transform.GetComponent<RectTransform>().GetWorldCorners(corners);
                Vector3 pos = corners[3]; //0th is lower-left, then 
                uiMgr.TooltipShow(path, pos);
            }
        }
	}

    public void OnLabelEdit()
    { 
        Debug.Log("OnLabelEdit");
        if (dataVar != null)
            dataVar.Label = GetLabel();
        uiMgr.DataUpdated();
    }

    private string GetLabel()
    {
        return inputField.text;
    }

    public void OnLoadClick()
    {

    }

    public void OnHeaderChoice()
    {

    }

    /// <summary> Handle button to choose and load file </summary>
    public void OnFileChooseClick()
    {
        //Debug.Log("OnFileChooseClick. this.GetInstanceID(): " + this.GetInstanceID());
        //filename = "button clicked";

        //Choose filename and try reading into a new DataVariable
        DataVariable newDataVar;
        bool success = dataMgr.ChooseLoadAddFile(out newDataVar);
        if (success)
        {
            //dataVar already added to variable list by above method call
            //Error handling and reporting handled by ChooseLoadAddFile()
            Debug.Log("Success: file loaded.");
            //If this already had a data var assigned, remove it. (Ignores if null)
            dataMgr.Remove(dataVar);
            dataVar = newDataVar;
        }

        RefreshUI();
    }

    //Mouse enter
    public void OnFilenameTextEnter()
    {
        filenameTextMouseHovering = true;
        filenameTextMouseEnterTime = Time.time;
    }

    public void OnFilenameTextExit()
    {
        if (filenameTextTooltipShowing)
            uiMgr.TooltipHide();
        filenameTextTooltipShowing = false;
        filenameTextMouseHovering = false;
    }
}
