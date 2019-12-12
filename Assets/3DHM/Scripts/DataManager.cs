using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Standalone file browser
using SFB;

/// <summary> Data values and data labels for a point on the data grid.
/// For a given row and column, the three data values from each mapped DataVariable.
/// Include the labels of the DataVariables for each value, for convenience. </summary>
public class TriDataPoint
{
    /// <summary> Flag. Is the data in this object valid? </summary>
    public bool isValid;

    /// <summary> Data row (corresponds to a ridge) of this data point </summary>
    public int row;
    /// <summary> The first non-empty row header from data vars, in order height, top, side </summary>
    public string rowHeader;
    /// <summary> Data column (position within a ridge) of this data point </summary>
    public int col;
    /// <summary> The first non-empty column header from data vars, in order height, top, side </summary>
    public string colHeader;

    /// <summary> Bin number - always 0 - not used currently. Orig code included a bin # for data in the database. 
    /// Have this here for compatibility with older code, until we decide for sure whether to not re-implement the bin option. </summary>
    public int bin;

    public float heightValue;
    public float topValue;
    public float sideValue;

    /// <summary> The user-assigned label/name for this data variable </summary>
    public string heightLabel;
    /// <summary> Data row label/header string. Emptry-string if not assigned. </summary>
    public string heightRowHeader;
    /// <summary> Data column label/header string. Emptry-string if not assigned. </summary>
    public string heightColHeader;

    public string topLabel;
    public string topRowHeader;
    public string topColHeader;

    public string sideLabel;
    public string sideRowHeader;
    public string sideColHeader;

    public TriDataPoint()
    {
        isValid = false;
        row = col = -1;
        bin = 0;
    }

    public TriDataPoint(int row, int col)
    {
        Initialize(row, col);
    }

    public void Initialize(int rowIn, int colIn)
    {
        isValid = false;
        string dummy;
        //Returns silently if data isn't ready or is out of bounds
        if (DataManager.I.PrepareAndVerify(out dummy) == false)
            return;
        if (rowIn < 0 || rowIn >= DataManager.I.Rows)
            return;
        if (colIn < 0 || colIn >= DataManager.I.Cols)
            return;

        row = rowIn;
        col = colIn;
        bin = 0;

        DataVariable v;
        v = DataManager.I.GetVariableByMapping(DataManager.Mapping.Height);
        heightValue = v.Data[row][col]; //should really have a general accessor for this
        heightLabel = v.Label;
        heightRowHeader = v.hasRowHeaders ? v.rowHeaders[row] : "";        
        heightColHeader = v.hasColumnHeaders ? v.columnHeaders[col] : "";

        v = DataManager.I.GetVariableByMapping(DataManager.Mapping.TopColor);
        topValue = v.Data[row][col];
        topLabel = v.Label;
        topRowHeader = v.hasRowHeaders ? v.rowHeaders[row] : "";
        topColHeader = v.hasColumnHeaders ? v.columnHeaders[col] : "";

        v = DataManager.I.GetVariableByMapping(DataManager.Mapping.SideColor);
        sideValue = v.Data[row][col];
        sideLabel = v.Label;
        sideRowHeader = v.hasRowHeaders ? v.rowHeaders[row] : "";
        sideColHeader = v.hasColumnHeaders ? v.columnHeaders[col] : "";

        rowHeader = heightRowHeader != "" ? heightRowHeader : (topRowHeader != "" ? topRowHeader : sideRowHeader);
        colHeader = heightColHeader != "" ? heightColHeader : (topColHeader != "" ? topColHeader : sideColHeader);

        isValid = true;
    }

    public void DebugDump()
    {
        Debug.Log("TriDataPoint.DebugDump: " + isValid + " r,c: " + row + " " + col + "\nval, label\n" + heightValue.ToString("F4") + " " + heightLabel +
                    "\n" + topValue.ToString("F4") + " " + topLabel + "\n" + sideValue.ToString("F4") + " " + sideLabel);
    }
}


/// <summary>
/// A single data variable, i.e. a set of observational values for a particular experiment, state, condition, etc.
/// Initially, the 2D data imported from a single csv file.
/// </summary>
public class DataVariable : CSVReaderData
{
    //Derive from CSVReaderData for now.
    //Will need to rework if we take this project further and have other data sources.

    //Make Data private here so we can force user to use accessors, because of NaN/NoData values
    private new float[][] Data { get { return _data; } set { minMaxReady = false; _data = value; } }

    private float minValue;
    private float maxValue;
    public float MinValue { get { if(!minMaxReady) CalcMinMax(); return minValue; } private set { minValue = value; } }
    public float MaxValue { get { if(!minMaxReady) CalcMinMax(); return maxValue; } private set { maxValue = value; } }
    private float range;
    public float Range { get { return range; } }

    private bool minMaxReady;

    /// <summary> Filename of the file used to load this variable </summary>
    private string filename;
    public string Filepath { get { return filename; } set { filename = value; } }

    /// <summary> A user-friendly label/name for this data variable. Set (at least initially) via GUI and used for id'ing by user and displaying on heatmap. </summary>
    private string label;
    /// <summary> A user-friendly label/name for this data variable. Set (at least initially) via GUI and used for id'ing by user and displaying on heatmap. </summary>
    public string Label { get { return label; } set { label = value; DataManager.I.ForceUniqueLabels(); } }

    /// <summary> The UI handler that this dataVar has been assignd to. Null if not yet set. </summary>
    public DataVarUIHandler UIhandler;

    public DataVariable()
    {
        //(parameter-less) Base ctor is called implicitly
    }

    public override void Clear()
    {
        //Debug.Log("DataVariable:Clear()");
        base.Clear();
        DataVarUIHandler.Clear(this);
        MinValue = float.MinValue;
        MaxValue = float.MaxValue;
        range = 0f;
        minMaxReady = false;
        label = "DefaultLabel";
        filename = "None";
        UIhandler = null;
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
                    //Skip NaN's
                    if( ! float.IsNaN(val))
                    {
                        if (val > max)
                            max = val;
                        if (val < min)
                            min = val;
                    }
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

    public override void DumpMetaData()
    {
        Debug.Log("Label:    " + Label);
        Debug.Log("Filename: " + Filepath);
        Debug.Log("Min, Max, Range: " + MinValue + ", " + MaxValue + ", " + range);
        base.DumpMetaData();
    }
}


/// <summary> Class holding required fields from single data var for project save/load </summary>
[System.Serializable]
public class DataVarStorage
{
    //Store path and filename separately so we can mod path easily if need be
    public string pathOnly;
    public string filename;
    public string label;
    public bool hasRowHeaders;
    public bool hasColumnHeaders;
    /// <summary> The index into the list of DataVarUIHandlers that this variable had,
    /// so we can repopulate in the same order.</summary>
    public int UIindex;

    /// <summary> Store which mappings this variable is used in.
    /// Do it this way since variable may be mapped to more than mapping. </summary>
    public List<DataManager.Mapping> mappings;

    public DataVarStorage()
    {
        pathOnly = "";
        filename = "";
        label = "";
        hasRowHeaders = false;
        hasColumnHeaders = false;
        mappings = new List<DataManager.Mapping>();
        UIindex = -1;
    }

    public DataVarStorage(DataVariable dv)
    {
        Init(dv);
    }

    private void Init(DataVariable dv)
    {
        pathOnly = Path.GetDirectoryName(dv.Filepath);
        filename = Path.GetFileName(dv.Filepath);
        label = dv.Label;
        mappings = new List<DataManager.Mapping>();

        hasRowHeaders = dv.hasRowHeaders;
        hasColumnHeaders = dv.hasColumnHeaders;

        if(dv.UIhandler != null)
        {
            UIindex = dv.UIhandler.UIindex;
        }

        //Find which mappings this is assigned to
        if (DataManager.I.GetVariableByMapping(DataManager.Mapping.Height) == dv)
            mappings.Add(DataManager.Mapping.Height);
        if (DataManager.I.GetVariableByMapping(DataManager.Mapping.TopColor) == dv)
            mappings.Add(DataManager.Mapping.TopColor);
        if (DataManager.I.GetVariableByMapping(DataManager.Mapping.SideColor) == dv)
            mappings.Add(DataManager.Mapping.SideColor);
    }
}

[System.Serializable]
/// <summary> Class that holds settings required for save/load of full data state as part of a project </summary>
public class DataStorage
{
    /// <summary> Each loaded data variable will have its own storage object, that includes the variable's mappings </summary>
    public List<DataVarStorage> dataVars;

    /// <summary> Index of the color table. 
    /// NOTE - It's bad to be using an index since it's tied to UI layout, but go with it for now. </summary>
    public int colorTableIndexSide;
    public int colorTableIndexTop;

    DataStorage()
    {
        dataVars = new List<DataVarStorage>();
        colorTableIndexSide = 0;
        colorTableIndexTop = 0;
    }

    /// <summary> Retrieve a DataStorage object filled with current data state, suitable for saving. </summary>
    /// <returns></returns>
    public static DataStorage Create()
    {
        DataStorage result = new DataStorage
        {
            dataVars = new List<DataVarStorage>(),
            colorTableIndexSide = DataManager.I.GetColorTableIdByMapping(DataManager.Mapping.SideColor),
            colorTableIndexTop = DataManager.I.GetColorTableIdByMapping(DataManager.Mapping.TopColor)
        };

        //Get all the data vars
        foreach (DataVariable dv in DataManager.I.Variables)
        {
            result.dataVars.Add(new DataVarStorage(dv));
        }

        return result;
    }

    /// <summary> Restore from a DataStorage object - load data files, applying mappings, set color tables </summary>
    /// <param name="data"></param>
    /// <returns>True on success </returns>
    public bool Restore()
    {
        bool success = true;
        string failedPath = "";

        //Clear whatever's currently loaded
        DataManager.I.Clear();

        foreach ( DataVarStorage dvs in dataVars)
        {
            DataVariable newDataVar = null;
            //Load the dataVar's file, and add it to the UI
            if (DataManager.I.LoadDataVarFromStorage(dvs, out newDataVar))
            {
                //We have a list cuz variable might be mapped to more than one visual param
                foreach (DataManager.Mapping mapping in dvs.mappings)
                {
                    DataManager.I.AssignVariableMapping(mapping, newDataVar);
                }
            }
            else
            {
                failedPath = dvs.pathOnly + dvs.filename;
                success = false;
            }
        }

        //color tables
        UIManager.I.SetColorTableByMappingAndIndex(DataManager.Mapping.SideColor, colorTableIndexSide);
        UIManager.I.SetColorTableByMappingAndIndex(DataManager.Mapping.TopColor, colorTableIndexTop);

        if (!success)
        {
            string msg = "One or more variables failed to load. The last one that failed: \n" + failedPath;
            UIManager.I.ShowMessageDialog(msg);
        }

        return success;
    }
}

///////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Singleton class for managing much of the data stuff.
/// See MonoBehaviorSingleton class for usage as a singleton.
/// Holds data objects for individual variables, along with options and state.
/// </summary>
public class DataManager : MonoBehaviorSingleton<DataManager> {

    public enum Mapping { Height, TopColor, SideColor };

    /// <summary>
    /// List of loaded variables. These may or may not be assigned to visual parameters.
    /// This list is separate from variableMappings to allow for > 3 variables to be loaded at once.
    /// </summary>
    private List<DataVariable> variables;
    public List<DataVariable> Variables { get { return variables; } }

    /// <summary>
    /// List that holds mappings of variable to visual params. Indexed via enum DataManagerMapping.
    /// </summary>
    private List<DataVariable> variableMappings;

    /// <summary>
    /// Color table IDs in use for the various mappings (initially just top and side colors).
    /// Use the Mapping enums to index this for simplicity, and just ignore the value at Mapping.Height index.
    /// NOTE this is filled as needed from UI before a redraw is done. See comments elsewhere.
    /// </summary>
    private int[] variableColorTableIDs;

    /// <summary> Returns true if one or more data variables are loaded. Does NOT verify data or variable mappings. </summary>
    public bool DataIsLoaded { get { return variables.Count > 0; } }

    /// <summary>
    /// Get the number of rows of data. 
    /// NOTE - only returns size of Height variable. You should already know that all the data is valid before using this.
    /// </summary>
    public int Rows { get { return HeightVarIsAssigned ? HeightVar.numDataRows : 0; } }
    public int Cols { get { return HeightVarIsAssigned ? HeightVar.numDataCols : 0; } }

    /// <summary> Check if a variable has been assigned to the height param </summary>
    public bool HeightVarIsAssigned { get { return (HeightVar != null && HeightVar.VerifyData()); } }
    public bool TopColorVarIsAssigned { get { return (TopColorVar != null && TopColorVar.VerifyData()); } }
    public bool SideColorVarIsAssigned { get { return (SideColorVar != null && SideColorVar.VerifyData()); } }

    public int TopColorColorTableID { get { return variableColorTableIDs[(int)Mapping.TopColor]; } }
    public int SideColorColorTableID { get { return variableColorTableIDs[(int)Mapping.SideColor]; } }

    /// <summary> Return a variable by its index into the list of assigned/loaded variables.  </summary>
    /// <returns>null if index is out of range</returns>
    public DataVariable GetVariableByIndex(int index)
    {
        if (index >= variables.Count)
            return null;
        return variables[index];
    }

    public DataVariable GetVariableByMapping(Mapping mapping)
    {
        return variableMappings[(int)mapping];
    }

    public int GetColorTableIdByMapping(Mapping mapping)
    {
        return variableColorTableIDs[(int)mapping];
    }

    /// <summary> For the given row & column, return the value of the variable mapped to Height.
    /// If nothing mapped, returns 0.
    /// If row or col is out of range, returns 0 and prints error </summary>
    public float GetHeightValue(int row, int col, bool returnZeroForNaN)
    {
        if (!HeightVarIsAssigned)
            return 0;

        return GetValueByMapping(Mapping.Height, row, col, returnZeroForNaN);
    }

    public float GetTopValue(int row, int col, bool returnZeroForNaN)
    {
        if (!TopColorVarIsAssigned)
            return 0;

        return GetValueByMapping(Mapping.TopColor, row, col, returnZeroForNaN);
    }

    public float GetSideValue(int row, int col, bool returnZeroForNaN)
    {
        if (!SideColorVarIsAssigned) //???
            return 0;

        return GetValueByMapping(Mapping.SideColor, row, col, returnZeroForNaN);
    }

    /// <summary>
    /// For the given variable mapping and row, int, return if the value is NaN/NoData.
    /// If no data loaded, returns true. </summary>
    public bool GetIsNanByMapping(Mapping mapping, int row, int col)
    {
        if (!DataIsLoaded)
            return true;
        float val = GetValueByMapping(mapping, row, col, false);
        return float.IsNaN(val);
    }

    /// <summary> For the given row & column, return the value of the variable mapped to 'mapping'.
    /// If nothing mapped, returns 0.
    /// If row or col is out of range, returns 0 and prints error </summary>
    public float GetValueByMapping(Mapping mapping, int rowIn, int colIn, bool returnZeroForNaN)
    {
        if (!DataIsLoaded)
            return 0;

        if ( rowIn < 0 || rowIn >= DataManager.I.Rows ||
             colIn < 0 || colIn >= DataManager.I.Cols)
        {
            Debug.LogError("GetValueByMapping: row or col out of range: " + rowIn + ", " + colIn);
            return 0;
        }

        float val = GetVariableByMapping(mapping).Data[rowIn][colIn];
        return (returnZeroForNaN && float.IsNaN(val)) ? 0 : val;
    }

    /// <summary> Accessor to variable currently assigned to height param 
    /// Note - returns null if not assigned. </summary>
    public DataVariable HeightVar
    {
        get { return variableMappings[(int)Mapping.Height]; }
        set { if (value != null && !variables.Contains(value)) Debug.LogError("Assigning heightVar to variable not in list.");
            variableMappings[(int)Mapping.Height] = value;
            //Debug.Log("HeightVar set to var with label " + value.Label);
        }
    }
    public DataVariable TopColorVar
    {
        get { return variableMappings[(int)Mapping.TopColor]; }
        set { if (value != null && !variables.Contains(value)) Debug.LogError("Assigning topColorVar to variable not in list.");
            variableMappings[(int)Mapping.TopColor] = value; }
    }
    public DataVariable SideColorVar
    {
        get { return variableMappings[(int)Mapping.SideColor]; }
        set { if (value != null && !variables.Contains(value)) Debug.LogError("Assigning sideColorVar to variable not in list.");
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

    /// <summary>
    /// Return a loaded DataVariable by label.
    /// Note that labels guaranteed to be unique, see ForceUniqueLabels()
    /// </summary>
    /// <param name="label"></param>
    /// <returns>null if no match</returns>
    public DataVariable GetVariableByLabel(string label)
    {
        if (variables.Count == 0)
            return null;
        foreach (DataVariable var in variables)
        {
            if (var.Label == label)
                return var;
        }
        return null;
    }

    // Use this for initialization instead of Awake, since this is MonoBehaviorSingleton
    //void Awake () {
    protected override void Initialize()
    {
    }

    void Start()
    {
        Clear();
    }

    public void Clear()
    {
        if(variables != null)
            foreach(DataVariable var in variables)
            {
                var.Clear();
            }
        variables = new List<DataVariable>();
        variableMappings = new List<DataVariable>();
        foreach (Mapping map in Enum.GetValues(typeof(Mapping)))
        {
            //Make sure these starts as null to indicate no mapping
            variableMappings.Add(null);
        }
        variableColorTableIDs = new int[Enum.GetValues(typeof(Mapping)).Length];
        UIManager.I.RefreshUI();
    }

    /// <summary> Remove and clear/empty/delete a data var </summary>
    /// <param name="var"></param>
    public void Remove(DataVariable var)
    {
        if (var == null)
            return;

        if (variables.Contains(var))
        {
            variables.Remove(var);

            //Remove possible variable mapping 
            foreach(Mapping mapping in Enum.GetValues(typeof(Mapping)))
            {
                if (variableMappings[(int)mapping] == var)
                    variableMappings[(int)mapping] = null;
            }

            var.Clear();
        }
        else
        {
            Debug.LogWarning("Tried removing variable that's not in variable list.");
        }

        //Update UI
        UIManager.I.RefreshUI();
    }

    /// <summary>
    /// Return a list of the label for each loaded DataVariable
    /// Note that labels guaranteed to be unique, see ForceUniqueLabels()
    /// </summary>
    /// <returns>Empty string if none loaded</returns>
    public List<string> GetLabels()
    {
        //Just in case, even though uniqueness should be forced each time Label is changed on a var.
        ForceUniqueLabels();

        List<string> labels = new List<string>();
        foreach (DataVariable var in variables)
        {
            labels.Add(var.Label);
        }
        return labels;
    }

    /// <summary>
    /// Got through the list of labels, and for any any loaded variables,
    ///  make sure the labels are unique by add _<N> to end of a label that's
    ///  a duplicate of a prior label
    ///NOTE - since we call this method from the DataVariable.Label property, it can
    /// end up recursing, but it's fine except that for multiple duplicates, instead of
    /// getting eg. label, label_1 and label_2, we get label, label_1 and label_1_1
    /// </summary>
    public void ForceUniqueLabels()
    {
        for (int refInd = 0; refInd < variables.Count; refInd++)
        {
            int dupCount = 0;
            string refLabel = variables[refInd].Label;
            //Go thru each var futher down the list to look for duplicates
            for(int chkInd = refInd+1; chkInd < variables.Count; chkInd++)
            {
                if(refLabel == variables[chkInd].Label)
                {
                    dupCount++;
                    variables[chkInd].Label += ("_" + dupCount.ToString());
                    UIManager.I.RefreshUI();
                }
            }
        }
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
        variableColorTableIDs = UIManager.I.GetColorTableAssignments();

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
        if (!HeightVarIsAssigned)
        {
            errorMsg = "Height Variable unassigned or invalid";
            return false;
        }
        //Top and side color vars should always be set, even if just set to same as heigh.
        if (!TopColorVarIsAssigned)
        {
            errorMsg = "Top Color Variable unassigned or invalid";
            return false;
        }
        if (!SideColorVarIsAssigned)
        {
            errorMsg = "Side Color Variable unassigned or invalid";
            return false;
        }

        if (variableColorTableIDs.Length != Enum.GetValues(typeof(Mapping)).Length)
        {
            errorMsg = "Error with color tables. Incorrect array length.";
            return false;
        }

        //Check that all data has same dims
        int m = HeightVar.numDataRows;
        int n = HeightVar.numDataCols;
        if (TopColorVar.numDataRows != m ||
            TopColorVar.numDataCols != n ||
            SideColorVar.numDataRows != m ||
            SideColorVar.numDataCols != n)
        {
            string msg = "Data variables do not have same dimensions: \n" + String.Format("{0}: {1}x{2}\n {3}: {4}x{5}\n {6}: {7}x{8}", HeightVar.Label, m, n, TopColorVar.Label, TopColorVar.numDataRows, TopColorVar.numDataCols, SideColorVar.Label, SideColorVar.numDataRows, SideColorVar.numDataCols);
            errorMsg = msg;
            return false;
        }
        
        //TODO
        //
        //Check for duplicate data variable labels. Error if found.
        //

        errorMsg = "no error";
        return true;
    }

    /// <summary> Given a path, try to load/read it, and add to variable list if successful. </summary>
    /// <returns></returns>
    public bool LoadAddFile(string path, bool hasRowHeaders, bool hasColumnHeaders, out DataVariable dataVar, out string errorMsg)
    {
        bool success = LoadFile(path, hasRowHeaders, hasColumnHeaders, out dataVar, out errorMsg);
        if (success)
        {
            Debug.Log("Success choosing and loading file.");
            variables.Add(dataVar);
            //Get filename and set it to label as default
            dataVar.Label = Path.GetFileNameWithoutExtension(dataVar.Filepath);
            //Update UI
            UIManager.I.RefreshUI();
        }
        else
        {
            //Error message will have been reported by method above
            Debug.Log("Other error while reading file.");
        }
        return success;
    }

    /// <summary>
    /// Choose a file via file picker
    /// </summary>
    /// <returns>Path, or "" if cancelled or some other issue.</returns>
    public string ChooseFile()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open .csv or Tab-Delimited File", "", "", false/*mutli-select*/);
        if (path.Length == 0)
        {
            return "";
        }
        return path[0];
    }

    /// <summary>
    /// Load a file path into a DataVariable. Does NOT add it to the DataManager
    /// </summary>
    /// <param name="hasRowHeaders">Flag. Set if data file is expected to have row headers.</param>
    /// <param name="hasColumnHeaders">Flag. Set if data file is expected to have column headers.</param>
    /// <param name="dataVariable">Returns a new DataVariable. Is valid but empty object if error or canceled.</param>
    /// <param name="errorMsg">Contains an error message on failure.</param>
    /// <returns>True on success. False if user cancels file picker or if there's an error.</returns>
    public bool LoadFile(string path, bool hasRowHeaders, bool hasColumnHeaders, out DataVariable dataVariable, out string errorMsg)
    {
        dataVariable = new DataVariable();

        //foreach (string s in path)
        //    Debug.Log(s);

        bool success = false;
        errorMsg = "No Error";

        //Cast to base class for reading in the file
        CSVReaderData data = (CSVReaderData)dataVariable; // new CSVReaderData();
        try
        {
            success = CSVReader.Read(path, hasColumnHeaders, hasRowHeaders, ref data, out errorMsg);
        }
        catch (Exception e)
        {
            errorMsg = "Exception caught: " + e.ToString();
            Debug.Log(errorMsg);
            return false;
        }
        if (success)
        {
            dataVariable.Filepath = path;
        }
        else
        {
            Debug.Log("Error msg from csv read: \n");
            Debug.Log(errorMsg);
        }
        return success;
    }

    /// <summary> Load a data var as its restored as part of a project </summary>
    /// <param name="dvs"></param>
    /// <param name="index">index within the collection of dataVarUIHandlers at which to place the data var</param>
    /// <param name="newDataVar"></param>
    /// <returns></returns>
    public bool LoadDataVarFromStorage(DataVarStorage dvs, out DataVariable newDataVar)
    {
        //NOTE - probably will need to improve this for x-plat needs, although maybe not
        // since path is stored from local path anyway
        string path = dvs.pathOnly + "\\" + dvs.filename;
        string errorMsg;
        newDataVar = null;

        //make sure the file exists, so we can give a simpler error than we'd get otherwise when calling LoadAddFile
        if(! File.Exists(path))
        {
            UIManager.I.ShowMessageDialog("File not found: " + path);
            return false;
        }

        //Actually load it now
        if (!DataManager.I.LoadAddFile(path, dvs.hasRowHeaders, dvs.hasColumnHeaders, out newDataVar, out errorMsg))
        {
            UIManager.I.ShowMessageDialog("Loading data failed.\n" + path + "\n" + errorMsg);
            return false;
        }
        newDataVar.Label = dvs.label;
        DataVarUIHandler.SetDataVarAtIndex(newDataVar, dvs.UIindex);
        return true;
    }

    public void DebugDumpVariables(bool verbose)
    {
        Debug.Log("============= Variable Mappings: ");
        foreach(Mapping mapping in Enum.GetValues(typeof(Mapping)))
        {
            DataVariable var = variableMappings[(int)mapping];
            string label = var == null ? "unassigned" : var.Label; //Note - make this string separately instead of directly in the Debug.Log call below, or else seems to evaluate both options of the ? operator and then fails
            Debug.Log(Enum.GetName(typeof(Mapping), mapping) + ": " + label);
        }
        Debug.Log("------------------------------");
        Debug.Log("Dumping data variable headers: ");
        foreach(DataVariable var in variables)
        {
            if (verbose)
                var.DumpMetaData();
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

        string path = ChooseFile();
        if( path == "")
        {
            Debug.Log("User cancelled file choice.");
            return false;
        }

        string errorMsg;
        bool success = LoadAddFile(path, true, true, out dataVar, out errorMsg);
        if (success)
        {
            Debug.Log("DEBUG: Success choosing and loading file.");
            variables.Add(dataVar);
            HeightVar = dataVar;
        }
        else
            Debug.Log("Other error while reading file: \n" + errorMsg);
        return success;
    }


    /// <summary>
    /// For Debugging. load a hardcoded file and assign it to height param and display it
    /// </summary>
    public bool DebugQuickLoadDefaultAndDraw()
    {
        DataVariable dataVar;

        string path = "C:\\Users\\mgsta\\Documents\\Penn\\IBI\\3dHeatMap\\testData\\simple\\10x10-monotonic-DEcrease-no_headers.csv";
        string errorMsg;
        bool success = LoadAddFile(path, true, true, out dataVar, out errorMsg);
        if (success)
        {
            Debug.Log("DEBUG: Success choosing and loading file.");
            variables.Add(dataVar);
            HeightVar = dataVar;
            TopColorVar = dataVar;
            SideColorVar = dataVar;
            Graph.I.Redraw();
        }
        else
            Debug.Log("Other error while reading file: \n" + errorMsg);
        return success;
    }

}
