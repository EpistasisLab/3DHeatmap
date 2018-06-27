using System;
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
        base.DumpNonData();
        Debug.Log("Min, Max: " + MinValue + ", " + MaxValue);
    }
}

/// <summary>
/// Data manager object. Singleton
/// Holds data objects for individual variables, along with options and state.
/// </summary>
public class DataManager {

    List<DataVariable> variables;

    /// <summary> Check if a variable has been assigned to the height param </summary>
    public bool HeightVarIsAssigned { get { return(heightVarInd >= 0 && heightVarInd < variables.Count); } }
    public bool TopColorVarIsAssigned { get { return (topColorVarInd >= 0 && topColorVarInd < variables.Count); } }
    public bool SideColorVarIsAssigned { get { return (sideColorVarInd >= 0 && sideColorVarInd < variables.Count); } }

    /// <summary> Accessor to variable currently assigned to height param </summary>
    public DataVariable HeightVar { get { return variables[heightVarInd]; } }
    public DataVariable TopColorVar { get { return variables[topColorVarInd]; } }
    public DataVariable SideColorVar { get { return variables[sideColorVarInd]; } }

    //Index values for variable assignments to visual params
    /// <summary> Index into array of variables of the var currently assigned to height param.
    /// -1 if not set. </summary>
    private int heightVarInd;
    private int topColorVarInd;
    private int sideColorVarInd;

    public DataManager()
    {
        Clear();
    }

    private void Clear()
    {
        variables = new List<DataVariable>();
        heightVarInd = -1;
        topColorVarInd = -1;
        sideColorVarInd = -1;
    }

    /// <summary>
    /// Assign the passed DataVariable to be used for height
    /// If invalid, the height var remains effectively unassigned.
    /// </summary>
    /// <param name="var"></param>
    public void AssignHeightVar(DataVariable var)
    {
        heightVarInd = variables.IndexOf(var); //-1 if not found
    }
    public void AssignTopColorVar(DataVariable var)
    {
        topColorVarInd = variables.IndexOf(var); //-1 if not found
    }
    public void AssignSideColorVar(DataVariable var)
    {
        sideColorVarInd = variables.IndexOf(var); //-1 if not found
    }

    /// <summary>
    /// For Debugging. Choose and load a file and assign it to height param.
    /// </summary>
    /// <returns></returns>
    public bool DebugQuickChooseAndLoadFile()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false/*mutli-select*/);
        if (path.Length == 0)
            return false;
        foreach (string s in path)
            Debug.Log(s);

        bool success = false;
        string errorMsg = "Unknown Error";
        DataVariable dataVariable = new DataVariable();
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
            variables.Add(dataVariable);
            AssignHeightVar(dataVariable);
        }
        else
        {
            Debug.Log("Error msg from csv read: ");
            Debug.Log(errorMsg);
        }
        return success;
    }
}
