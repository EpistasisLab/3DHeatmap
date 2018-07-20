using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    //The UI canvas
    private GameObject canvas;
    //A bunch of refs to various elements
    private GameObject dataTopPanel;
    private GameObject optionsTopPanel;
    private GameObject visualMappingPanel;
    private VisualMappingUIHandler visualMappingUIHandler;
    private GameObject dataVarsTopPanel;
    private GameObject toolTipPanel;
    private Text toolTipText;
    public GameObject messageDialogPrefab;

    //The main object for the app
    private HeatVRML heatVRML;
    private DataManager dataMgr;

    /// <summary>
    /// Hack for odd UI behavior. When we switch tooltip text to new string in TooltipShow()
    /// and make the panel active, the panel doesn't resize properly, and instead retains
    /// the size of the previous time tooltip was shown. If we leave the object that's
    /// generating the tooltip and then go right back to it, it then takes on the correct size.
    /// So this hack sets this works in Update to make inactive and the active again, and
    /// it then properly resizes.
    /// Might work better to instantiate a prefab, since that's what I do with MessageDialog and 
    /// I'm not having this same issue.
    /// </summary>
    private int toolTipHackState;

    private GameObject GetAndCheckGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
            Debug.LogError("gamobject not found: " + name);
        return go;
    }
	// Use this for initialization
	void Awake () {
        canvas = GetAndCheckGameObject("Canvas");
        //Main panels
        dataTopPanel = GetAndCheckGameObject("DataTopPanel");
        optionsTopPanel = GetAndCheckGameObject("OptionsTopPanel");

        //Sub panels and components
        visualMappingPanel = GetAndCheckGameObject("VisualMappingPanel");
        visualMappingUIHandler = visualMappingPanel.GetComponent<VisualMappingUIHandler>();
        if (visualMappingUIHandler == null)
            Debug.LogError("visualMappingUIHandler == null");
        toolTipPanel = GetAndCheckGameObject("ToolTipPanel");
        dataVarsTopPanel = GetAndCheckGameObject("DataVarsTopPanel");
        toolTipText = toolTipPanel.transform.Find("ToolTipText").GetComponent<Text>();
        if( toolTipText == null)
            Debug.LogError("toolTipText == null");
        heatVRML = GameObject.Find("Prefab objectify").GetComponent<HeatVRML>();
        if (heatVRML == null)
            Debug.LogError("heatVRML == null");
        dataMgr = GameObject.Find("DataManager").GetComponent<DataManager>();
        if (dataMgr == null)
            Debug.LogError("dataMgr == null");
        TooltipHide();

        toolTipHackState = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (toolTipHackState == 1)
        {
            toolTipPanel.SetActive(false);
            toolTipHackState++;
        }
        else if(toolTipHackState == 2)
        {
            toolTipPanel.SetActive(true);
            toolTipHackState = 0;
        }
	}

    /// <summary>
    /// Disables and greys out the main UI elements, i.e. data selection and mapping panels, and options panels.
    /// Meant for use when a message dialog or similar is shown, and you want to prevent other UI action.
    /// Does NOT do SetActive(False). Instead, uses a CanvasGroup and makes no interactable
    /// </summary>
    public void DisableMainUI()
    {
        MainUIEnableDisable(false);
    }

    /// <summary>
    /// Opposite of DisableMainUI
    /// </summary>
    public void EnableMainUI()
    {
        MainUIEnableDisable(true);
    }

    private void MainUIEnableDisable(bool enable)
    {
        float alpha = enable ? 1f : 0.5f;
        bool interactable = enable;

        //Top data panel
        CanvasGroup cg = dataTopPanel.GetComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.interactable = interactable;

        //Options top panel
        cg = optionsTopPanel.GetComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.interactable = interactable;
    }

    /// <summary>
    /// Show a simple message dialog. \n in string should be recognized as newline
    /// </summary>
    /// <param name="message"></param>
    /// <param name="preferredWidth">Optionally change the width of the message box.
    /// Do this if you know you want it wider than its prefab setting. 0 means no change.</param>
    public void ShowMessageDialog(string message, float preferredWidth = 0f, bool leftAlign = false)
    {
        //Debug.Log("in ShowMessageDialog");
        GameObject go = Instantiate(messageDialogPrefab, canvas.GetComponent<RectTransform>());
        go.GetComponent<MessageDialog>().ShowMessage(message, preferredWidth, leftAlign);

        //disable main UI
        DisableMainUI();
    }

    /// <summary>
    /// MessageDialog component should call this when it closes.
    /// </summary>
    public void OnMessageDialogDone()
    {
        EnableMainUI();
    }

    /// <summary>
    /// Show a tool tip. Caller must also call TooltipHide() when done.
    /// </summary>
    /// <param name="tip"></param>
    /// <param name="position">Desired position of middle left of tooltip window</param>
    public void TooltipShow(string tip, Vector3 position)
    {
        position.x += toolTipPanel.GetComponent<RectTransform>().rect.width/2;
        toolTipPanel.transform.position = position;
        toolTipText.text = tip;
        toolTipPanel.SetActive(true);
        toolTipHackState = 1;
    }

    public void TooltipHide()
    { 
        toolTipPanel.SetActive(false);
    }

    /// <summary>
    /// Call this when data has been updated in some way the will need a UI refresh (i.e. DataVariables)
    /// </summary>
    public void DataUpdated()
    {
        visualMappingUIHandler.RefreshUI();
        //Debug
        dataMgr.DebugDumpVariables(false/*verbose*/);   
    }

    /// <summary>
    /// Pass-thru func
    /// </summary>
    /// <returns></returns>
    public int[] GetColorTableAssignments()
    {
        return visualMappingUIHandler.GetColorTableAssignments();
    }

    public void OnRedrawButtonClick()
    {
        heatVRML.NewPrepareAndDrawData();
    }

    public void OnMaxHeightSlider(GameObject go)
    {
        float frac = go.GetComponent<Slider>().value;
        heatVRML.SetNewGraphHeightAndRedraw(frac);
    }

    /// <summary>
    /// Hack this in here for now.
    /// Show an intro message dialog with basic usage instructions.
    /// </summary>
    public void ShowIntroMessage()
    {
        string msg = "<b>3D Heatmap</b> allows you to visualize up to three dependent variables (Data Variables) simultaneously. The variables must be derived from the same independent variables. Each variable can be assigned to any of three visual parameters used to make the 3D heatmap: height, top color and side color.\n\n" +
                "<b>Data Format</b>\n\nThe program expects each variable from a separate text data file of 2D array data, in either csv or tab-delimited formats. Each data file must have the same dimensions.\n\n" +
                "<b>Instructions</b>\n\n" +
                "1) Select and Load Data Variables in the GUI to the left. Note that you must indicate what type of headers your data file might have before you click Load.\n" +
                "2) Assign which variables should be mapped to Height, Top Color, and Side Color.\n" +
                "3) Choose color tables for Top and Side colors.\n" +
                "4) Click the Redraw button, or press F2.\n\n" +
                "<b>View Controls</b>\n\n" +
                "Move   - right-mouse-click & drag, or arrow keys\n" +
                "Rotate - left-mouse-click &drag\n" +
                "Zoom   - -/+ keys\n\n" +
                "Press F1 to see this message again";
        ShowMessageDialog(msg, 600f, true);
    }
}
