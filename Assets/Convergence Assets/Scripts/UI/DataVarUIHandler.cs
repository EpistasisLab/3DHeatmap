using System.Collections;
using System.Collections.Generic;
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

    //Ref to DataManager object from the scene
    private DataManager dataMgr;

    private InputField inputField;

    // Use this for initialization
    void Start () {
        dataMgr = GameObject.Find("DataManager").GetComponent<DataManager>();
        if (dataMgr == null)
            Debug.LogError("dataMgr == null");

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
        RefreshUI();
    }

    /// <summary> Update the UI for this panel, given the current state of this object and the assoc'ed DataVariable </summary>
    public void RefreshUI()
    {
        string label = "_unassigned_";
        string filename = "None loaded";
        if( dataVar != null)
        {
            label = dataVar.Label;
            filename = dataVar.Filename;
        }
        //There are multiple Text components in the whole panel, so seems I have to do this. Probably there is a better way...
        //And from what I read, transform.Find doesn't recurse, so you have to do this to go down beyond immeditate children.
        //.Find("LabelPanel/InputField").gameObject.GetComponentInChildren<InputField>().text = label;
        inputField.text = label;
        transform.Find("FilePanel/FilenameText").gameObject.GetComponentInChildren<Text>().text = filename;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnLabelEdit()
    {
        if (dataVar != null)
            dataVar.Label = GetLabel(); 
    }

    private string GetLabel()
    {
        return inputField.text;
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
            Debug.Log("Success choosing and loading file.");
            //If this already had a data var assigned, remove it. (Ignores if null)
            dataMgr.Remove(dataVar);
            dataVar = newDataVar;
            dataVar.Label = GetLabel();
        }

        RefreshUI();
    }

}
