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
    private float range;
    public float Range { get { return range; } }

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
        range = 0f;
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
            float t = 0.001f;
            if (max - min < t)
                Debug.LogWarning("Variable range is tiny: " + (max-min));
            range = Mathf.Max( max - min, t); //just make sure it's not 0
            minMaxReady = true;
        }
    }

    public override void DumpNonData()
    {
        Debug.Log("Label:    " + Label);
        Debug.Log("Filename: " + Filename);
        Debug.Log("Min, Max, Range: " + MinValue + ", " + MaxValue + ", " + range);
        base.DumpNonData();
    }
}

///////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Data manager object. Singleton
/// Holds data objects for individual variables, along with options and state.
/// </summary>
public class DataManager : MonoBehaviour {

    public enum Mapping { Height, TopColor, SideColor };

    private UIManager uiMgr;

    /// <summary>
    /// List of loaded variables. These may or may not be assigned to visual parameters.
    /// This list is separate from variableMappings to allow for > 3 variables to be loaded at once.
    /// </summary>
    private List<DataVariable> variables;
    
    /// <summary>
    /// List that holds mappings of variable to visual params. Index via enum DataManagerMapping.
    /// </summary>
    private List<DataVariable> variableMappings;

    /// <summary>
    /// Color table IDs for the various mapping (initially just top and side colors).
    /// Use the Mapping enums to index this for simplicity, and just ignore the value at Mapping.Height index.
    /// </summary>
    private int[] variableColorTableIDs;

    /// <summary> Check if a variable has been assigned to the height param </summary>
    public bool HeightVarIsAssigned { get { return(HeightVar != null && HeightVar.VerifyData()); } }
    public bool TopColorVarIsAssigned { get { return (TopColorVar != null && TopColorVar.VerifyData()); } }
    public bool SideColorVarIsAssigned { get { return (SideColorVar != null && SideColorVar.VerifyData()); } }

    public int TopColorColorTableID { get { return variableColorTableIDs[(int)Mapping.TopColor]; } }
    public int SideColorColorTableID { get { return variableColorTableIDs[(int)Mapping.SideColor]; } }

    public DataVariable GetVariableByMapping(Mapping mapping)
    {
        return variableMappings[(int)mapping];
    }

    public int GetColorTableIdByMapping(Mapping mapping)
    {
        return variableColorTableIDs[(int)mapping];
    }

    /// <summary> Accessor to variable currently assigned to height param 
    /// Note - returns null if not assigned. </summary>
    private DataVariable heightVar;
    public DataVariable HeightVar
    {
        get { return variableMappings[(int)Mapping.Height]; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning heightVar to variable not in list.");
                variableMappings[(int)Mapping.Height] = value;
                //Debug.Log("HeightVar set to var with label " + value.Label);
            }
    }
    private DataVariable topColorVar;
    public DataVariable TopColorVar
    {
        get { return variableMappings[(int)Mapping.TopColor]; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning topColorVar to variable not in list.");
            variableMappings[(int)Mapping.TopColor] = value; }
    }
    private DataVariable sideColorVar;
    public DataVariable SideColorVar
    {
        get { return variableMappings[(int)Mapping.SideColor]; }
        set { if (!variables.Contains(value)) Debug.LogError("Assigning sideColorVar to variable not in list.");
            variableMappings[(int)Mapping.SideColor] = value; }
    }

    public void AssignVariableMapping(Mapping mapping, DataVariable var)
    {
        //Silent return makes it easier to call this when we know sometimes
        // var will be unset.
        if (var == null)
            return;
        variableMappings[(int)mapping] = var;
    }

    public void AssignVariableMappingByLabel(Mapping mapping, string label)
    {
        AssignVariableMapping(mapping, GetVariableByLabel(label));
    }

    /* orig method - can be removed soon
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
    */

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
        variableMappings = new List<DataVariable>();
        foreach (Mapping map in Enum.GetValues(typeof(Mapping)))
        {
            //Make sure these starts as null to indicate no mapping
            variableMappings.Add(null);
        }
        variableColorTableIDs = new int[Enum.GetValues(typeof(Mapping)).Length];
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
            if( HeightVar == var)
                HeightVar = null;
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

    /// <summary>
    /// Call before drawing/rendering.
    /// Pulls any changed vals from UI as needed.
    /// Runs data verification
    /// </summary>
    /// <returns>True if ready. False if some issue. Error message returned in errorMsg</returns>
    public bool PrepareAndVerify(out string errorMsg)
    {
        errorMsg = "no error";

        //get color table ids
        //pull these from UI instead of pushing from UI so we don't
        // have to handle when there's not an assigned var mapping.
        //awkward
        variableColorTableIDs = uiMgr.GetColorTableAssignments();

        //Verify the data
        bool result = VerifyData(out errorMsg);

        return result;
    }

    /// <summary>
    /// Verify that data is ready for drawing
    /// </summary>
    /// <param name="errorMsg">Holds an error message when returns failed/false.</param>
    /// <returns>True on success. False on fail.</returns>
    private bool VerifyData(out string errorMsg)
    {
        if( ! HeightVarIsAssigned)
        {
            errorMsg = "HeightVar unassigned or invalid";
            return false;
        }
        //Top and side color vars should always be set, even if just set to same as heigh.
        if (!TopColorVarIsAssigned)
        {
            errorMsg = "TopColorVar unassigned or invalid";
            return false;
        }
        if (!SideColorVarIsAssigned)
        {
            errorMsg = "SideColorVar unassigned or invalid";
            return false;
        }

        if ( variableColorTableIDs.Length != Enum.GetValues(typeof(Mapping)).Length)
        {
            errorMsg = "Error with color tables. Incorrect array length.";
            return false;
        }

        //TODO
        //
        //Check for duplicate data variable labels. Error if found.
        //
        //Check that all data has same dims

        errorMsg = "no error";
        return true;
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
            success = CSVReader.Read(path[0], false, false, ref data, out errorMsg);
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
