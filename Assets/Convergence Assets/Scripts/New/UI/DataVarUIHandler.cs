using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for the DataVariablePanel UI element.
/// Handles events for all children.
/// Tracks DataVariable that it's assigned to.
/// NOTE - this is a separate handler for each DataVariablePanel
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

    /// <summary> Filename/path that's been selected but not necessarily loaded </summary>
    private string filepathLocal;

    private bool filenameTextMouseHovering;
    private float filenameTextMouseEnterTime;
    private bool filenameTextTooltipShowing;

    private Button loadButton;
    private Color loadButtonNormalColor;
    private Color loadButtonNeedsLoadingColor;

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

        loadButton = transform.Find("FilePanel").transform.Find("LoadButton").GetComponent<Button>();
        if (loadButton == null)
            Debug.LogError("loadButton == null");
        loadButtonNormalColor = loadButton.colors.normalColor;

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
        //Empty string indicates not yet specified
        filepathLocal = "";
        SetFileNeedsLoading(false);
        RefreshUI();
    }

    /// <summary> Update the UI for this panel, given the current state of the assoc'ed DataVariable </summary>
    public void RefreshUI()
    {
        //Update according to DataVariable
        string label = "_unassigned_";
        string filename = Path.GetFileName(filepathLocal);
        if( dataVar != null)
        {
            label = dataVar.Label;
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
                //If the file hasn't yet been loaded, and thus the dataVar not setup, just
                // show filepathLocal
                string path = dataVar != null ? dataVar.Filepath : filepathLocal;
                uiMgr.TooltipShow(path, transform);
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

    public void OnHeaderChoice()
    {
        if( filepathLocal != "")
            SetFileNeedsLoading(true);
    }

    /// <summary>
    /// Do what's needed to alert user when something changes that requires a file reload.
    /// </summary>
    /// <param name="needsIt"></param>
    public void SetFileNeedsLoading(bool needsIt)
    {
        ColorBlock cb = loadButton.colors;
        cb.normalColor = needsIt ? new Color(0.5f,0.8f,0.4f) : loadButtonNormalColor;
        loadButton.colors = cb;
    }

    /// <summary> Handle button to choose (but not load/read) a file</summary>
    public void OnFileChooseClick()
    {
        string result = dataMgr.ChooseFile();
        if (result == "") //Cancelled
            return;
        if( dataVar != null)
        {
            dataMgr.Remove(dataVar);
        }
        filepathLocal = result;
        SetFileNeedsLoading(true);
        RefreshUI();
    }

    private void GetHeaderSelection(out bool hasRowHeaders, out bool hasColumnHeaders)
    {
        //Dropdown selction options:
        //{No Headers, Row Only, Column Only, Both}
        int selection = transform.Find("FilePanel").transform.Find("HeadersDropdown").GetComponent<Dropdown>().value;
        hasRowHeaders = (selection == 1 || selection == 3);
        hasColumnHeaders = (selection == 2 || selection == 3);
    }

    public void OnLoadClick()
    {
        //Debug.Log("OnFileChooseClick. this.GetInstanceID(): " + this.GetInstanceID());

        //Make sure we've chosen a filepath already
        if (filepathLocal == "")
            return;

        //Try reading into a new DataVariable
        DataVariable newDataVar;
        string errorMsg;
        //Read the currently selected option for headers
        bool hasRowHeaders, hasColumnHeaders;
        GetHeaderSelection(out hasRowHeaders, out hasColumnHeaders);
        bool success = dataMgr.LoadAddFile(filepathLocal, hasRowHeaders, hasColumnHeaders, out newDataVar, out errorMsg);
        if (success)
        {
            //dataVar already added to variable list by above method call
            //Error handling and reporting handled by ChooseLoadAddFile()
            Debug.Log("Success: file loaded.");
            //If this already had a data var assigned, remove it. (Ignores if null)
            dataMgr.Remove(dataVar);
            dataVar = newDataVar;
        }
        else
        {
            string msg = "Error loading file \n\n" + filepathLocal + ". \n\n" + errorMsg;
            Debug.LogError(msg);
            uiMgr.ShowMessageDialog(msg);
        }
        SetFileNeedsLoading(!success);
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
