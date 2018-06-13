using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB; //StandaloneFileBrowser https://github.com/gkngkc/UnityStandaloneFileBrowser


/// <summary>
/// Simple test of CSVReader.
/// Run. Press 'o' to choose a file and results are printed to console.
/// </summary>
public class CSVReaderTest : MonoBehaviour {

    /// <summary>
    /// In the editor, specify what headers are expected
    /// </summary>
    public bool hasColumnsHeader; //First row/ine of file
    public bool hasRowHeader; //First column of file

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if( Input.GetKeyDown(KeyCode.O))
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", true);
            foreach( string s in paths)
                Debug.Log(s);
            if (paths.Length == 0)
                return;

            bool success = false;
            string errorMsg = "Unknown Error";
            //CSVReaderData data = new CSVReaderData();
            DataVariable dataVariable = new DataVariable();
            CSVReaderData dataObj = (CSVReaderData) dataVariable; // new CSVReaderData();
            try
            {
                success = CSVReader.Read(paths[0], hasColumnsHeader, hasRowHeader, ref dataObj, out errorMsg);
            }
            catch (Exception e)
            {
                Debug.Log("Exception caugt: " + e.ToString());
                return;
            }
            if ( ! success)
            {
                Debug.Log("Error msg from csv read: ");
                Debug.Log(errorMsg);
            }
            else
            {
                dataVariable.DumpNonData();
                dataVariable.DumpData();
                dataVariable.Clear();
            }
        }

    }
}
