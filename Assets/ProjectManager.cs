using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//Standalone file browser
using SFB;

[System.Serializable]
public class Project
{
    public DataStorage data;
    public GraphStorage graphSettings;
    public string name;

    private static string Extension { get { return "3DHMprj"; } }

    Project()
    {
        data = null;
        graphSettings = null;
        name = DateTime.Now.ToString();
    }

    /// <summary> Save the current app state as a project </summary>
    public static void Save()
    {
        Project prj = new Project();
        prj.data = DataStorage.Create();
        prj.graphSettings = GraphStorage.Create();

        string path = StandaloneFileBrowser.SaveFilePanel("Save your project", "", "", Extension);
        if (path == "")
            return;

        string jsonString = JsonUtility.ToJson(prj);
        try
        {
            File.WriteAllText(path, jsonString);
        }
        catch (Exception e)
        {
            string msg = "Error saving project to file " + path + "\n\n" + e.Message;
            UIManager.I.ShowMessageDialog(msg);
            return;
        }
        UIManager.I.ShowMessageDialog("Project Saved");
    }

    /// <summary> Prompt user to choose a project file and load it </summary>
    public static bool Load()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Choose a project file", "", Extension, false);
        if (path.Length == 0 || path[0] == "")
            return false;

        string jsonString = "";
        try
        {
            jsonString = File.ReadAllText(path[0]);
        }
        catch (Exception e)
        {
            string msg = "Error reading project file: \n " + path + "\n\n" + e.Message;
            UIManager.I.ShowMessageDialog(msg);
            return false;
        }

        Project newPrj = JsonUtility.FromJson<Project>(jsonString);
        //Restore. Will show error messages on failure.
        if (!newPrj.data.Restore())
            return false;
        newPrj.graphSettings.Restore();
        return true;
    }
}

public class ProjectManager : MonoBehaviorSingleton<ProjectManager>
{

    //Call this instead of Awake
    protected override void Initialize()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary> Save the current state as a project </summary>
    public void Save()
    {
        Project.Save();
    }

    IEnumerator RestoreCoroutine()
    {
        int id = UIManager.I.StatusShow("Loading Project...");
        yield return null;
        bool result = Project.Load();
        UIManager.I.StatusComplete(id);
        if (result)
        {
            UIManager.I.ShowMessageDialog("Project loaded.\nReady for Redraw.");
            UIManager.I.StartRedrawPrompt();
        }

    }

    /// <summary> Choose a project file from disk and load it and restore it</summary>
    public void Restore()
    {
        StartCoroutine(RestoreCoroutine());
    }

    ///////////////////////////////////////////////////////////
    ///Older routines for quickly loading some sample/demo data
    ///Not using the Project classes, but still works for loading 
    /// demo data that's stored in streaming assets as thereby is distributed
    /// with the build.
    
    /// <summary> Hacked-in routine to quickly load some sample data and assign visual mappings.
    /// We eventually want a proper project capability to handle this and user-saved projects. </summary>
    /// <returns></returns>
    public void DemoDataLoadAndDraw()
    {
        StartCoroutine(DemoData_LoadAndMapDataCoroutine());
    }

    IEnumerator DemoData_LoadAndMapDataCoroutine()
    {
        int id = UIManager.I.StatusShow("Loading Demo Data...");
        yield return null;
        DemoData_LoadAndMapDataHandler();
        UIManager.I.StatusComplete(id);
    }

    private bool DemoData_LoadAndMapDataHandler()
    {
        DataManager.I.Clear();

        //Sample files are in Assets/StreamingAssets
        DataVariable dataVar;
        int count = 0;

        if (!DemoData_LoadSingleDataFile("200x200-R80C110.csv", out dataVar, DataManager.Mapping.Height, count))
        {
            return false;
        }

        if (!DemoData_LoadSingleDataFile("200x200-R100C200.csv", out dataVar, DataManager.Mapping.SideColor, ++count))
        {
            return false;
        }
        UIManager.I.SetColorTableByMappingAndIndex(DataManager.Mapping.SideColor, 2);

        if (!DemoData_LoadSingleDataFile("200x200-R150C12.csv", out dataVar, DataManager.Mapping.TopColor, ++count))
        {
            return false;
        }
        UIManager.I.SetColorTableByMappingAndIndex(DataManager.Mapping.TopColor, 0);

        UIManager.I.RefreshUI();

        Graph.I.Redraw();

        return true;
    }

    private bool DemoData_LoadSingleDataFile(string filename, out DataVariable dataVar, DataManager.Mapping mapping, int count)
    {
        string path = Application.streamingAssetsPath + "/sampleData/" + filename;
        string errorMsg;
        if (!DataManager.I.LoadAddFile(path, true, true, out dataVar, out errorMsg))
        {
            UIManager.I.ShowMessageDialog("Loading sample data failed.\n" + filename + "\n" + errorMsg);
            return false;
        }
        DataManager.I.AssignVariableMapping(mapping, dataVar);
        DataVarUIHandler.SetDataVarAtIndex(dataVar, count, false, false);
        return true;
    }
}
