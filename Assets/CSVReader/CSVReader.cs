using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;


/// <summary>
/// Data container, output by CSVReader
/// </summary>
public class CSVReaderData
{
    public float[,] data;
    /// <summary>
    /// Flag. Set if there are headers for the data columns.
    /// </summary>
    public bool hasColumnHeaders;
    /// <summary>
    /// Flag. Set if there are headers for each data row.
    /// </summary>
    public bool hasRowHeaders;
    public List<string> columnHeaders;
    public List<string> rowHeaders;
    public int numDataRows;
    public int numDataCols;

    public CSVReaderData()
    {
        Clear();
    }

    /// <summary>
    /// Clear/empty the object. Array size is set to 0
    /// </summary>
    public void Clear()
    {
        //Header lists will be empty if not available.
        columnHeaders = new List<string>();
        rowHeaders = new List<string>();
        data = new float[0, 0];
        hasColumnHeaders = false;
        hasRowHeaders = false;
        numDataCols = 0;
        numDataRows = 0;
    }

    /// <summary>
    /// Debug dump of non-data properties of this class
    /// </summary>
    public void DumpNonData()
    {
        Debug.Log("hasColumnHeaders: " + hasColumnHeaders + " hasRowHeaders: " + hasRowHeaders);
        Debug.Log("numDataRows: " + numDataRows + " numDataCols: " + numDataCols);
        string str = "";
        foreach (string s in rowHeaders)
            str += s + ", ";
        Debug.Log("rowHeaders: " + str);
        str = "";
        foreach (string s in columnHeaders)
            str += s + ", ";
        Debug.Log("colHeaders: " + str);
        str = "";
    }
    /// <summary>
    /// Dump the data
    /// </summary>
    /// <param name="numRows">How many rows to dump. 0 for all.</param>
    /// <param name="numCols">How many columns to dump. 0 for all.</param>
    public void DumpData(int numRows = 0, int numCols = 0)
    {
        numRows = (int) Mathf.Min(numRows, numDataRows);
        numCols = (int) Mathf.Min(numCols, numDataCols);

        for (int i = 0; i < (numRows == 0 ? numDataRows : numRows); i++)
        {
            string str = "Row " + i + ":";
            for (int j = 0; j < (numCols == 0 ? numDataCols : numCols); j++)
                str += " " + data[i, j].ToString().PadLeft(8, ' ');
            Debug.Log(str);
        }
    }
}

/// <summary>
/// Simple csv reader (comma- and tab-delimited files) that expects all numeric values other than an optional header column and/or row.
/// Returns an object that includes float[,] with the data.
/// From https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/
/// Heavily modified to
///     pull out header row and column and return separately in csv data object
///     expect float data 
///     return array instead of dictionary
/// </summary>
public class CSVReader
{
    static string SPLIT_RE = @",|\t(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        //this splits on both comma and tab  
        //quotes - works with quoted fields (header and numeric fields). Double-quoted fields yield field with single quotes (which I believe is proper csv behavior)
        //  Double-quoted numeric fields work too - presumably the ToFloat parser discards the quotes.
        //@ is for string literal, to avoid escaping: http://www.rexegg.com/regex-csharp.html
        //?: is for non-capturing group (matches, but doesn't get output) - I'm guessing this is to match quotes and not split on them, thereby removing them
        //?= - don't know what this is for
        //regex quick ref: https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference
        //intro: https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions
    
    //static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r"; used in orig version to split into lines from TextAsset
    static char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// Read the csv or tab-delimited file specified by 'file'.
    /// </summary>
    /// <param name="file">Full path for file to read.</param>
    /// <param name="columnHeadersExpected">Flag. Set this if columns headers are expected in the file. This is headers in the first row/line of the file.</param>
    /// <param name="rowHeadersExpected">Flag. Set this is row headers are expected. This means the first field/column of each row is a header/name/string for the row</param>
    /// <param name="success">Return value. True if file read successfully.</param>
    /// <param name="errorMsg">Return value. Error message string if failed (including exception string if an excpetion occured).</param>
    /// <returns>New data object containing data, headers, etc.</returns>
    public static CSVReaderData Read(string file, bool columnHeadersExpected, bool rowHeadersExpected, out bool success, out string errorMsg)
    {
        success = true;
        errorMsg = "no error";

        CSVReaderData result = new CSVReaderData();

        //var list = new List<Dictionary<string, object>>();
        //TextAsset textData = Resources.Load(file) as TextAsset;
        //string[] lines = Regex.Split(textData.text, LINE_SPLIT_RE);

        //NOTE - reads whole file into memory. If we get into big data, we'll want
        // to read line-by-line. See https://stackoverflow.com/questions/46405067/reading-a-file-line-by-line-in-c-sharp-using-streamreader
        //NOTE - will this work with non-windows line endings?
        string[] lines;
        try
        {
            //NOTE - reads in all lines at once. Won't be good for very large files.
            lines = File.ReadAllLines(file);
        }
        catch (Exception e)
        {
            errorMsg = "Failed loading file: " + file + ".  Exception: " + e.ToString();
            success = false;
            Debug.Log(errorMsg);
            result.Clear();
            return result;
        }

        if (lines.Length < 1)
        {
            result.Clear();
            return result;
        }

        int numDataColumns = 0;
        int numDataRows = 0;

        result.hasRowHeaders = rowHeadersExpected;
        result.hasColumnHeaders = columnHeadersExpected;

        //number of data rows
        result.numDataRows = numDataRows = lines.Length - (columnHeadersExpected ? 1 : 0);

        //number of data columns and columns headers
        var firstRow = Regex.Split(lines[0], SPLIT_RE);
        int numAllColumns = firstRow.Length;
        if (columnHeadersExpected)
        {
            //If we have row headers, skip the first value since it's not a data-column header
            int ind = rowHeadersExpected ? 1 : 0;
            for (int i = ind; i < firstRow.Length; i++)
                result.columnHeaders.Add(firstRow[i]);
        }
        result.numDataCols = numDataColumns = firstRow.Length - (rowHeadersExpected ? 1 : 0);

        //Alloc the data array
        try
        {
            result.data = new float[numDataRows, numDataColumns];
        }
        catch( Exception e)
        {
            errorMsg = "Aborting. Failed allocating data array, with exception: " + e.ToString();
            success = false;
            Debug.Log(errorMsg);
            result.Clear();
            return result;
        }

        //Read in the lines
        int start = columnHeadersExpected ? 1 : 0;
        for (var fileRow = start; fileRow < lines.Length; fileRow++)
        {

            var values = Regex.Split(lines[fileRow], SPLIT_RE);
            //or try string.split for simpler: https://docs.microsoft.com/en-us/dotnet/csharp/how-to/parse-strings-using-split

            //Empty line or not enough columns? Abort.
            if (values.Length != numAllColumns)
            {
                errorMsg = "Row " + fileRow + " is empty or otherwise incorrect length: " + values.Length + " instead of " + numAllColumns + ". Aborting.";
                success = false;
                Debug.Log(errorMsg);
                result.Clear();
                return result;
            }

            //Parse a line
            for (var fileCol = 0; fileCol < numAllColumns; fileCol++)
            {
                string value = values[fileCol];
                //empty data cell?
                if (value == "" && (!columnHeadersExpected || (columnHeadersExpected && fileCol > 0)))
                {
                    errorMsg = "Empty data cell. Row, col: " + fileRow + ", " + fileCol + ". Aborting.";
                    success = false;
                    Debug.Log(errorMsg);
                    result.Clear();
                    return result;
                }
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                if (rowHeadersExpected && fileCol == 0)
                {
                    //Header
                    result.rowHeaders.Add(value);
                }
                else
                {
                    //data
                    float f;
                    if (float.TryParse(value, out f))
                    {
                        int ii = fileRow - (columnHeadersExpected ? 1 : 0);
                        int jj = fileCol - (rowHeadersExpected ? 1 : 0);
                        result.data[ii, jj] = f;
                    }
                    else
                    {
                        string ex = "";
                        if (fileRow == 0 && !columnHeadersExpected)
                            ex = "Wasn't expecting first row to be headers. But is it maybe? ";
                        else if (fileCol == 0 && !rowHeadersExpected)
                            ex = "Wasn't expecting first columns to be headers. But is it maybe? ";
                        errorMsg = ex + "Expected a number but got non-numeric value: " + value + ". At row, col: " + fileRow + ", " + fileCol + ". Aborting.";
                        success = false;
                        Debug.Log(errorMsg);
                        result.Clear();
                        return result;
                    }
                }
            }
        }
        return result;
    }
}

/*** ORIG
 * 
 *
public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load(file) as TextAsset;

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}

    */