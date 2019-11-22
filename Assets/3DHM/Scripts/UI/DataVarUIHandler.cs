using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for the DataVariablePanel UI element, which contains several UI controls
/// for managing file loading for a single variable.
/// Handles events for all children.
/// Tracks DataVariable that it's assigned to.
/// NOTE - this is a separate handler for each DataVariablePanel
/// </summary>
public class DataVarUIHandler : MonoBehaviour {

    /// <summary> This is used to populate the header dropdown </summary>
    private enum HeaderDropdownValues { Headers, None, Row_Only, Col_Only, Both }

    public GameObject ChooseFileButton;
    public GameObject HeadersDropdown;
    public GameObject LoadButton;
    /// <summary> The label object for Name and instance number </summary>
    public GameObject labelLabel;
    public GameObject InputField;

    /// <summary> The index within of this UI panel is within the list of all panels in scene.
    /// -1 if hasn't been set. 
    /// </summary>
    private int _UIindex = -1;
    public int UIindex { get { return _UIindex; } }

    /// <summary> Get the variable assoc'ed with this UI handler. null if not yet assigned </summary>
    private DataVariable _dataVar;
    /// <summary> Assign the dataVar that this panel is handling, and refresh UI </summary>
    public DataVariable DataVar
    {
        get { return _dataVar; }
        set
        {
            _dataVar = value;
            if(IsLoaded)
                filepathSelected = _dataVar.Filepath;
            SetFileNeedsLoading(false);
            _dataVar.UIhandler = this;
            RefreshUI();
        }
    }

    /// <summary> Return true if this UI panel is currently assigned to a loaded DataVariable </summary>
    public bool IsLoaded { get { return DataVar != null; } }

    //Internal convenience ref
    private InputField inputField;

    /// <summary> Filename/path that's been selected but not necessarily loaded </summary>
    private string filepathSelected;

    private bool filenameTextMouseHovering;
    private float filenameTextMouseEnterTime;
    private bool filenameTextTooltipShowing;

    private GameObject loadButton;
    
    /// <summary> Static list of all handlers of this type in the scene </summary>
    private static DataVarUIHandler[] allHandlers;

    /// <summary> Find all instances of this class in the scene, make a list, and assign their index values in order.
    /// This should be run on startup, and if more instances of this class are made at runtime. 
    /// 1st run during startup happens automatically from UIManager </summary>
    public static void InitializeListOfAll()
    {
        //*NOTE* we might want to make sure these are sorted by height/position, but skip that for now
        //
        allHandlers = GameObject.FindObjectsOfType<DataVarUIHandler>();
        for (int i = 0; i < allHandlers.Length; i++)
        {
            allHandlers[i]._UIindex = i;
            //The children get returned out-of-order, so set them straight here
            allHandlers[i].transform.SetSiblingIndex(i);
            allHandlers[i].ShowIndexNumber();
        }
    }

    /// <summary>
    /// Return the total # of these handlers instantiated in the app. Only valid
    /// after InitializeListOfAll has been called.
    /// </summary>
    /// <returns></returns>
    public static int GetNumberOfHandlers()
    {
        return allHandlers.Length;
    }

    public static DataVarUIHandler GetHandlerAtIndex(int index)
    {
        if (index < GetNumberOfHandlers())
            return allHandlers[index];
        else
        {
            Debug.LogError("Out of range handler index");
            return null;
        }
    }

    /// <summary>
    /// Return the handler assigned to the passed dataVariable .
    /// </summary>
    /// <param name="dataVar"></param>
    /// <returns>Null if not found</returns>
    public static DataVarUIHandler GetHandlerForDataVar(DataVariable dataVar)
    {
        foreach(DataVarUIHandler dvh in allHandlers)
        {
            if (dvh.DataVar == dataVar)
                return dvh;
        }
        return null;
    }

    /// <summary> Assign a new dataVar to one of the available handlers, specified by the hander's index.
    /// This is useful for loading files and settings from script, then updating things here for UI display.</summary>
    /// <param name="newVar"></param>
    /// <param name="index">index within the list of all handlers. Is ignored if out of range.</param>
    /// <returns>True on success, false if index out of range </returns>
    public static bool SetDataVarAtIndex(DataVariable newVar, int index)
    {
        if (index < 0 || index >= allHandlers.Length)
        {
            UIManager.I.ShowMessageDialog("SetDataVarAtIndex: out-of-range index passed: " + index + ". Skipping this data variabled.");
            return false;
        }
        //This will call refreshUI which will take care of headers and label and get it all showing 
        allHandlers[index].DataVar = newVar;
        return true;
    }

    // Use this for initialization
    void Start () {
        //There's only one comp. of type InputField, so this is easy...
        inputField = transform.GetComponentInChildren<InputField>(); //should recurse
        if (inputField == null)
            Debug.LogError("inputField == null");

        loadButton = transform.Find("FilePanel").transform.Find("LoadButton").gameObject;
        if (loadButton == null)
            Debug.LogError("loadButton == null");

        //Set headers dropdown
        List<string> options = new List<string>(typeof(HeaderDropdownValues).GetEnumNames());
        HeadersDropdown.GetComponent<Dropdown>().ClearOptions();
        HeadersDropdown.GetComponent<Dropdown>().AddOptions(options);

        Clear();
	}

    /// <summary> Update the display within this panel of its index number within all panels 
    /// This is the label for the text input UI that holds var's friendly name/label</summary>
    private void ShowIndexNumber()
    {
        labelLabel.GetComponent<Text>().text = _UIindex.ToString() + ". Name";
    }

    /// <summary>
    /// Remove any association with a DataVariable. </summary>
    public void Clear()
    {
        if(_dataVar != null)
            _dataVar.UIhandler = null;
        _dataVar = null;
        filenameTextMouseHovering = false;
        filenameTextMouseEnterTime = float.MaxValue;
        filenameTextTooltipShowing = false;
        //Empty string indicates not yet specified
        filepathSelected = "";
        SetFileNeedsLoading(false);
        RefreshUI();
    }

    public static void Clear(DataVariable dataVar)
    {
        DataVarUIHandler h = GetHandlerForDataVar(dataVar);
        if (h != null)
            h.Clear();
    }

    /// <summary> Update the UI for this panel, given the current state of the assoc'ed DataVariable </summary>
    public void RefreshUI()
    {
        //Update according to DataVariable
        string label = "_name unassigned_";
        string filename = Path.GetFileName(filepathSelected);
        if ( IsLoaded )
        {
            label = _dataVar.Label;

            //Header options. Need to set these too for when we assign a new dataVar to this handler (e.g. when loading sample data).
            SetHeaderSelection(_dataVar.hasRowHeaders, _dataVar.hasColumnHeaders);
        }
        else
            HeadersDropdown.GetComponent<Dropdown>().value = 0;

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
                string path = filepathSelected;
                UIManager.I.TooltipShow(path, transform);
            }
        }
	}

    public void OnLabelEdit(GameObject go)
    { 
        //Debug.Log("OnLabelEdit");
        if (IsLoaded)
            _dataVar.Label = GetLabel();
        UIManager.I.RefreshUI();
        //Switch prompting behavior to the next UI element if this element is currently prompting.
        UIManager.I.ShowNextUIActionPrompt(go);
    }

    private string GetLabel()
    {
        return inputField.text;
    }

    public void OnHeaderChoice(GameObject go)
    {
        if( filepathSelected != "")
            SetFileNeedsLoading(true);
        //Switch prompting behavior to the next UI element if this element is currently prompting.
        UIManager.I.ShowNextUIActionPrompt(go);
    }

    /// <summary>
    /// Do what's needed to alert user when something changes that requires a file reload.
    /// </summary>
    /// <param name="needsIt"></param>
    public void SetFileNeedsLoading(bool needsIt)
    {
        //Debug.Log("obj id: " + this.GetInstanceID() + " SetFileNeedsLoading: " + needsIt);
        //This call will make sure the auto-prompting is done before following through with this request.
        UIManager.I.StartStopManualUIActionPrompt(loadButton, false);
    }

    /// <summary> Handle button to choose (but not load/read) a file</summary>
    public void OnFileChooseClick(GameObject go)
    {
        string result = DataManager.I.ChooseFile();
        if (result == "") //Cancelled
            return;
        if( IsLoaded )
        {
            DataManager.I.Remove(_dataVar);
        }
        Clear();
        filepathSelected = result;
        SetFileNeedsLoading(true);
        //Switch prompting behavior to the next UI element if this element is currently prompting.
        UIManager.I.ShowNextUIActionPrompt(go);
        RefreshUI();
    }

    /// <summary> Get the current selection of header options based on the drop-down box. </summary>
    /// <param name="hasRowHeaders"></param>
    /// <param name="hasColumnHeaders"></param>
    /// <returns>true if header option is valid, false if nothing selected</returns>
    private bool GetHeaderSelection(out bool hasRowHeaders, out bool hasColumnHeaders)
    {
        hasRowHeaders = hasColumnHeaders = false;
        int selection = HeadersDropdown.GetComponent<Dropdown>().value;
        if( selection == 0)
        {
            return false;
        }
        hasRowHeaders = (selection == (int)HeaderDropdownValues.Row_Only || selection == (int)HeaderDropdownValues.Both);
        hasColumnHeaders = (selection == (int)HeaderDropdownValues.Col_Only || selection == (int)HeaderDropdownValues.Both);
        return true;
    }

    public void SetHeaderSelection(bool hasRowHeaders, bool hasColumnHeaders)
    {
        int value;
        if (hasRowHeaders && hasColumnHeaders)
            value = (int)HeaderDropdownValues.Both;
        else if (hasRowHeaders)
            value = (int)HeaderDropdownValues.Row_Only;
        else if (hasColumnHeaders)
            value = (int)HeaderDropdownValues.Col_Only;
        else
            value = (int)HeaderDropdownValues.None;

        HeadersDropdown.GetComponent<Dropdown>().value = value;
        HeadersDropdown.GetComponent<Dropdown>().RefreshShownValue();
    }

    public void OnLoadClick(GameObject go)
    {
        //Start a coroutine to do the loading
        StartCoroutine("LoadCoroutine");
    }

    /// <summary> Coroutine allows us to show the status window by yielding a frame so it gets drawn,
    /// then call the method to do the actual loading. </summary>
    IEnumerator LoadCoroutine()
    {
        string loadingMsg = "Loading...";
        //Check if we already have something loaded in here.
        //Normally a loaded dataVar will be cleared from this panel when
        // a new file is chosen. But user might click load button and get to
        // this method more than once, or may have changed the file on the filesystem
        // and wants to reload it.
        if (IsLoaded)
        {
            if (_dataVar.Filepath == filepathSelected)
            {
                //We've already loaded this file, but we'll reload
                loadingMsg = "Reloading...";
                Debug.Log("LoadCoroutine: Reloading...");
            }
            else
            {
                //Something weird happened, and the loaded data var didn't get cleared
                // when a new filepath was selected.
                UIManager.I.ShowMessageDialog("Data is already loaded for a different file.\nThis is unexpected.\nClearing previous file's data and\nloading new one.\n\nPlease tell the app developers.");
            }
            DataManager.I.Remove(_dataVar);
        }

        int statusID = UIManager.I.StatusShow(loadingMsg);
        yield return null;
        LoadHandler();
        UIManager.I.StatusComplete(statusID);
    }

    /// <summary> Do the actual loading, based on class fields. </summary>
    private void LoadHandler() { 
        //Debug.Log("OnFileChooseClick. this.GetInstanceID(): " + this.GetInstanceID());

        //Make sure we've chosen a filepath already
        if (filepathSelected == "")
            return;

        //Try reading into a new DataVariable
        DataVariable newDataVar;
        string errorMsg;
        //Read the currently selected option for headers
        bool hasRowHeaders, hasColumnHeaders;
        if (!GetHeaderSelection(out hasRowHeaders, out hasColumnHeaders))
        {
            UIManager.I.ShowMessageDialog("Please select header option first.");
            return;
        }
        bool success = DataManager.I.LoadAddFile(filepathSelected, hasRowHeaders, hasColumnHeaders, out newDataVar, out errorMsg);
        if (success)
        {
            //dataVar has already added to DataManager's variable list by above method call
            //Error handling and reporting handled by LoadAddFile()
            //Debug.Log("Success: file loaded.");
            DataVar = newDataVar;
            //Switch prompting behavior to the next UI element if this element is currently prompting.
            UIManager.I.ShowNextUIActionPrompt(loadButton.gameObject);
            UIManager.I.StartRedrawPrompt();
        }
        else
        {
            string msg = "Error loading file \n\n" + filepathSelected + ". \n\n" + errorMsg;
            Debug.LogError(msg);
            UIManager.I.ShowMessageDialog(msg);
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
            UIManager.I.TooltipHide();
        filenameTextTooltipShowing = false;
        filenameTextMouseHovering = false;
    }
}
