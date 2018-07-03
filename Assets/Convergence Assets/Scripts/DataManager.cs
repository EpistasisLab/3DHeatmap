using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;

/// <summary>
/// A single data variable, i.e. a set of observational values for a particular experiment, state, condition, etc.
/// Initially, the 2D data imported from a single csv file.
/// </summary>
public class DataVariable : CSVReaderData
{
    //Derive from CSVReaderData for now.
    //Will need to rework if we take this project further and have other data sources.

    public override float[][] Data { get { return _data; } set { minMaxReady = false; _data = value; } }

    private float minValue;
    private float maxValue;
    public float MinValue { get { if(!minMaxReady) CalcMinMax(); return minValue; } private set { minValue = value; } }
    public float MaxValue { get { if(!minMaxReady) CalcMinMax(); return maxValue; } private set { maxValue = value; } }

    private bool minMaxReady;

    /// <summary> Filename of the file used to load this variable </summary>
    private string filename;
    public string Filename { get { return filename; } set { filename = value; } }

    /// <summary> A label for this data variable. Set (at least initially) via GUI and used for id'ing by user and displaying on heatmap. </summary>
    private string label;
    public string Label { get { return label; } set { label = value; } }

    public DataVariable()
    {
        //(parameter-less) Base ctor is called implicitly
    }

    public override void Clear()
    {
        //Debug.Log("DataVariable:Clear()");
        base.Clear();
        MinValue = float.MinValue;
        MaxValue = float.MaxValue;
        minMaxReady = false;
        label = "DefaultLabel";
        filename = "None";
    }

    /// <summary>
    /// Run verification on this data variable without getting an error message return.
    /// </summary>
    /// <returns>True if ok. False otherwise.</returns>
    public bool VerifyData()
    {
        string dummy;
        return VerifyData(out dummy);
    }
    /// <summary>
    /// Run verification on this data variable. 
    /// </summary>
    /// <param name="error">Message when verification fails</param>
    /// <returns>True if ok. False otherwise and puts message in 'error'</returns>
    public bool VerifyData(out string error)
    {
        if( Data.Length <= 0) { error = "No data loaded"; return false; }
        if( numDataCols <= 0) { error = "numDataCols <= 0"; return false; }
        if( numDataRows <= 0) { error = "numDataRows <= 0"; return false; }
        if( Data.Length != numDataRows) { error = "Number of rows in data array != numDataRows."; return false; }
        if( hasColumnHeaders && columnHeaders.Count != numDataCols ) { error = "Number of available column headers != number expected."; return false; }
        if (hasRowHeaders && rowHeaders.Count != numDataRows) { error = "Number of available row headers != number expected."; return false; }
        for( int r = 0; r < _data.Length; r++)
        {
            if(_data[r].Length != numDataCols)
            {
                error = "Data array row " + r + " does not have expected number of rows: " + numDataCols;
                return false;
            }
        }
        error = "Data is valid.";
        return true;
    }

    private void CalcMinMax()
    {
        //Debug.Log("in CalcMinMax");
        if (numDataCols > 0 && numDataRows > 0)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach ( float[] row in Data)
                foreach( float val in row)
                {
                    if (val > max)
                        max = val;
                    if (val < min)
                        min = val;
                }
            MaxValue = max;
            MinValue = min;
            minMaxReady = true;
        }
    }

    public override void DumpNonData()
    {
        Debug.Log("Label:    " + Label);
        Debug.Log("Filename: " + Filename);
        Debug.Log("Min, Max: " + MinValue + ", " + MaxValue);
        base.DumpNonData();
    }
}

/// <summary>
/// Data manager object. Singleton
/// Holds data objects for individual variables, along with options and state.
/// </summary>
public class DataManager : MonoBehaviour {

    private UIManager uiMgr;

    /// <summary>
    /// List of loaded variables. These may or may not be assigned to visual parameters.
    /// </summary>
    private List<DataVariable> variables;

    /// <summary> Check if a variable has been assigned to the height param </summary>
    public bool HeightVarIsAssigned { get { return(HeightVar != null && HeightVar.VerifyData()); } }
    public bool TopColorVarIsAssigned { get { return (TopColorVar != null && TopColorVar.VerifyData()); } }
    public bool SideColorVarIsAssigned { get { return (SideColorVar != null && SideColorVar.VerifyData()); } }

    /// <summary> Accessor to variable currently assigned to height param 
    /// Note - returns null if not assigned. </summary>
    private DataVariable heightVar;
    public DataVariable HeightVar
    {
        get { return heightVar; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning heightVar to variable not in list.");
                heightVar = value;
                //Debug.Log("HeightVar set to var with label " + value.Label);
            }
    }
    private DataVariable topColorVar;
    public DataVariable TopColorVar
    {
        get { return topColorVar; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning topColorVar to variable not in list.");
            topColorVar = value; }
    }
    private DataVariable sideColorVar;
    public DataVariable SideColorVar
    {
        get { return sideColorVar; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning sideColorVar to variable not in list.");
            sideColorVar = value; }
    }

    public void AssignHeightVarByLabel(string label)
    {
        HeightVar = GetVariableByLabel(label);
    }
    public void AssignTopColorVarByLabel(string label)
    {
        TopColorVar = GetVariableByLabel(label);
    }
    public void AssignSideColorVarByLabel(string label)
    {
        SideColorVar = GetVariableByLabel(label);
    }

    /// <summary>
    /// Return a loaded DataVariable by label.
    /// Note that labels aren't guaranteed to be unique, so this returns first match.
    /// </summary>
    /// <param name="label"></param>
    /// <returns>null if no match</returns>
    public DataVariable GetVariableByLabel(string label)
    {
        foreach( DataVariable var in variables)
        {
            if (var.Label == label)
                return var;
        }
        return null;
    }


    void Start()
    {
        uiMgr = GameObject.Find("UIManager").GetComponent<UIManager>();
        if (uiMgr == null)
            Debug.LogError("uiMgs == null");
        Clear();
    }

    private void Clear()
    {
        variables = new List<DataVariable>();
        heightVar = null;
        topColorVar = null;
        sideColorVar = null;
    }

    public void Remove(DataVariable var)
    {
        if (var == null)
            return;

        if( variables.Contains(var))
        {
            variables.Remove(var);
            if( heightVar == var)
                heightVar = null;
            if (topColorVar == var)
                topColorVar = null;
            if (sideColorVar == var)
                sideColorVar = null;
        }
        else
        {
            Debug.LogWarning("Tried removing variable that's not in variable list.");
        }

        //Update UI
        uiMgr.DataUpdated();
    }

    /// <summary>
    /// Return a list of the label for each loaded DataVariable
    /// Note that labels aren't guaranteed to be unique.
    /// </summary>
    /// <returns>Empty string if none loaded</returns>
    public List<string> GetLabels()
    {
        List<string> labels = new List<string>();
        foreach (DataVariable var in variables)
        {
            labels.Add(var.Label);
        }
        return labels;
    }

    /// <summary> Choose a file via file picker, try to load/read it, and add to variable list if successful. </summary>
    /// <returns></returns>
    public bool ChooseLoadAddFile(out DataVariable dataVar)
    {
        bool cancelled;
        bool success = ChooseAndReadFile(out dataVar, out cancelled);
        if (success)
        {
            Debug.Log("Success choosing and loading file.");
            variables.Add(dataVar);
            //Get filename and set it to label as default
            dataVar.Label = Path.GetFileNameWithoutExtension(dataVar.Filename);
            //Update UI
            uiMgr.DataUpdated();
        }
        else
        {
            if (cancelled)
            {
                Debug.Log("User cancelled file choice.");
            }
            else
            {
                //Error message will have been reported by method above
                Debug.Log("Other error while reading file.");
            }
        }
        return success;
    }

    /// <summary>
    /// Prompt user to choose a file with file picker.
    /// If file is chosen, read it and assign to a new DataVariable.
    /// </summary>
    /// <param name="dataVariable">Returns a new DataVariable. Is valid but empty object if error or canceled.</param>
    /// <param name="cancelled">Returns true if operation failed because user cancelled file picker.</param>
    /// <returns>True on success. False if user cancels file picker or if there's an error.</returns>
    public bool ChooseAndReadFile(out DataVariable dataVariable, out bool cancelled)
    {
        dataVariable = new DataVariable();
        cancelled = false;
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open .csv or Tab-Delimited File", "", "", false/*mutli-select*/);
        if (path.Length == 0)
        {
            //User cancelled filepicker
            cancelled = true;
            return false;
        }

        //foreach (string s in path)
        //    Debug.Log(s);

        bool success = false;
        string errorMsg = "Unknown Error";

        //Cast to base class for reading in the file
        CSVReaderData data = (CSVReaderData)dataVariable; // new CSVReaderData();
        try
        {
            success = CSVReader.Read(path[0], true, true, ref data, out errorMsg);
        }
        catch (Exception e)
        {
            Debug.Log("Exception caugt: " + e.ToString());
            return false;
        }
        if (success)
        {
            dataVariable.Filename = path[0];
        }
        else
        {
            Debug.Log("Error msg from csv read: ");
            Debug.Log(errorMsg);
        }
        return success;
    }

    public void DebugDumpVariables(bool verbose)
    {
        Debug.Log("============= Variable Mappings: ");
        Debug.Log("HeightVar: " + HeightVar == null ? "unassigned" : HeightVar.Label);
        Debug.Log("TopColorVar: " + TopColorVar == null ? "unassigned" : TopColorVar.Label);
        Debug.Log("SideColorVar: " + SideColorVar == null ? "unassigned" : SideColorVar.Label);
        Debug.Log("------------------------------");
        Debug.Log("Dumping data variable headers: ");
        foreach(DataVariable var in variables)
        {
            if (verbose)
                var.DumpNonData();
            else
                Debug.Log("Label: " + var.Label);
            if(verbose)
                Debug.Log("------------------------------");
        }
        Debug.Log("=============================== end");
    }

    /// <summary>
    /// For Debugging. Choose and load a file and assign it to height param.
    /// </summary>
    /// <returns></returns>
    public bool DebugQuickChooseLoadDisplayFile()
    {
        DataVariable dataVar;
        bool cancelled;
        bool success = ChooseAndReadFile(out dataVar, out cancelled);
        if (success)
        {
            Debug.Log("DEBUG: Success choosing and loading file.");
            variables.Add(dataVar);
            HeightVar = dataVar;
        }
        else if (cancelled)
        {
            Debug.Log("User cancelled file choice.");
        }
        else
            Debug.Log("Other error while reading file.");
        return success;
    }

}
