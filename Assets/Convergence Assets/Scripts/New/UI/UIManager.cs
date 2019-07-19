using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton class for managing much of the UI stuff.
/// See MonoBehaviorSingleton class for usage as a singleton.
/// </summary>
public class UIManager : MonoBehaviorSingleton<UIManager>
{
    //The UI canvas
    private GameObject canvas;
    //A bunch of refs to various elements
    //A bunch of refs to various elements
    //Public ones to set in inspector. Probably should
    // all just be this way for clarity.
    public GameObject toolTipPanel;
    public GameObject statusPanel;
    //private ones
    private GameObject dataTopPanel;
    private GameObject optionsTopPanel;
    private GameObject visualMappingPanel;
    private VisualMappingUIHandler visualMappingUIHandler;
    //private GameObject dataVarsTopPanel; not using here currently
    public GameObject messageDialogPrefab;

    /// <summary> The UI object that's currently being highlighted in some way </summary>
    private int currentUIActionPromptee;
    /// <summary> List of UI elements to use for prompting user action, in order of
    /// which they should be activated </summary>
    public GameObject[] UIActionPromptees;

    private GameObject GetAndCheckGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
            Debug.LogError("gamobject not found: " + name);
        return go;
    }
    // Use this for initialization instead of Awake, since this is MonoBehaviorSingleton
    //void Awake () {
    protected override void Initialize() { 
        //UI Canvas
        canvas = GetAndCheckGameObject("CanvasScreenSpace");
        //Main panels
        dataTopPanel = GetAndCheckGameObject("DataTopPanel");
        optionsTopPanel = GetAndCheckGameObject("OptionsTopPanel");

        //Sub panels and components
        //assigned thru inspector
        if (toolTipPanel == null)
            Debug.LogError("toolTipPanel == null");
        if (statusPanel == null)
            Debug.LogError("statusPanel == null");
        //find and get ref
        visualMappingPanel = GetAndCheckGameObject("VisualMappingPanel");
        visualMappingUIHandler = visualMappingPanel.GetComponent<VisualMappingUIHandler>();
        if (visualMappingUIHandler == null)
            Debug.LogError("visualMappingUIHandler == null");
        //dataVarsTopPanel = GetAndCheckGameObject("DataVarsTopPanel");

        TooltipHide();

        currentUIActionPromptee = -1;
    }

    private void Start()
    {
        //Start the sequence of UI prompts to help user know what to do.
        StartUIActionPrompts();    
    }

    // Update is called once per frame
    void Update () {

        /* debug
        if (Input.GetKeyDown(KeyCode.T))
        {
            //Test status panel
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //Debug.Log("left shift down");
                StatusComplete(debugStatusPanelID[debugStatusPanelID.Count - 1]);
                debugStatusPanelID.RemoveAt(debugStatusPanelID.Count - 1);
            }
            else
            {
                int id = debugStatusPanelID.Count == 0 ? 1 : debugStatusPanelID[debugStatusPanelID.Count - 1];
                debugStatusPanelID.Add(StatusShow("Test status panel. Previous id: " + id));
            }
        }
        */
    }

    /// <summary>
    /// Post a message to the Status Panel/Window
    /// Meant for short messages, like "Loading file..."
    /// Store the return value (messageID) and pass it
    /// to StatusComplete() when done.
    /// The goal of this two-step process is to be able to overwrite the 
    /// current message in the panel, while also being able to have the
    /// previous message return automatically if it hasn't yet been completed.
    /// For asynchronous message updating.
    /// </summary>
    /// <param name="tip">message to show</param>
    /// <returns></returns>
    public int StatusShow(string tip)
    {
        int id = statusPanel.GetComponent<StatusHandler>().StatusShow(tip);
        statusPanel.transform.SetAsLastSibling();
        return id;
    }

    /// <summary>
    /// Remove a status message.
    /// Pass the id value that was returned from the call to StatusShow
    /// </summary>
    /// <param name="messageID"></param>
    public void StatusComplete(int messageID)
    {
        statusPanel.GetComponent<StatusHandler>().StatusComplete(messageID);
    }

    /// <summary> Disables and greys out the main UI elements.
    /// Meant for use when a message dialog or similar is shown, and you want to prevent other UI action. </summary>
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

    /// <summary>
    /// To Disables and greys out the main UI elements, i.e. data selection and mapping panels, and options panels.
    /// Meant for use when a message dialog or similar is shown, and you want to prevent other UI action.
    /// Does NOT do SetActive(False). Instead, uses a CanvasGroup and makes no interactable
    /// </summary>
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
    /// <param name="txf">The transform of the gameObject we want the tooltip placed next to</param>
    public void TooltipShow(string tip, Transform txf)
    {
        toolTipPanel.GetComponent<SimpleTextPanelHandler>().ShowNearTransform(tip, txf);
    }

    public void TooltipHide()
    {
        toolTipPanel.GetComponent<SimpleTextPanelHandler>().Hide();
    }

    /// <summary>
    /// Call this when something needs refershing, e.g. data has been updated in some way the will need a UI refresh (i.e. DataVariables)
    /// Awkware, I know.
    /// </summary>
    public void RefreshUI()
    {
        visualMappingUIHandler.RefreshUI();
        InputManager.Instance.Reset();
        
        //Debug
        //dataMgr.DebugDumpVariables(false/*verbose*/);   
    }

    /// <summary>
    /// Pass-thru func
    /// </summary>
    /// <returns></returns>
    public int[] GetColorTableAssignments()
    {
        return visualMappingUIHandler.GetColorTableAssignments();
    }

    public void OnRedrawButtonClick(GameObject button)
    {
        HeatVRML.Instance.Redraw();
    }

    public void OnMaxHeightSlider(GameObject go)
    {
        //float frac = go.GetComponent<Slider>().value;
        //HeatVRML.Instance.SetGraphHeightFrac(frac);
        //Don't need to redraw to see new height
        //StartCoroutine(RedrawCoroutine(true));
    }

    /// <summary>
    /// Start prompting the user for which UI item should be
    /// (typically) used next. Currently it flashes the item.
    /// </summary>
    /// <param name="newIndex">Index of UI object to start showing prompt for. Pass -1 to just stop anything currently showing a prompt.</param>
    private void ShowUIActionPrompt(int newIndex)
    {
        //Stop whatever currently is prompting
        if (currentUIActionPromptee >= 0 && currentUIActionPromptee < UIActionPromptees.Length)
            UIActionPromptees[currentUIActionPromptee].GetComponent<UIElementFlasher>().StopFlashing();
        currentUIActionPromptee = -1;
        if (newIndex < 0)
            return;
        if (newIndex >= UIActionPromptees.Length)
        {
            Debug.LogWarning("ShowUIActionPrompt: newIndex out of range: " + newIndex);
            return;
        }

        GameObject uiObj = UIActionPromptees[newIndex];
        if( uiObj == null || uiObj.GetComponent<UIElementFlasher>() == null)
        {
            Debug.LogWarning("ShowUIActionPrompt: passed UI is null or doesn't have UIElementFlasher");
            return;
        }

        //Debug.Log("calling StartFlashing. index: " + index);
        uiObj.GetComponent<UIElementFlasher>().StartFlashing();
        currentUIActionPromptee = newIndex;
    }

    /// <summary> Start the sequence of UI action prompts </summary>
    public void StartUIActionPrompts()
    {
        ShowUIActionPrompt(0);
    }

    /// <summary> Stop the sequence of UI action prompting </summary>
    public void StopAllUIActionPrompts()
    {
        ShowUIActionPrompt(-1);
    }
    
    /// <summary> If the passed Gameobject is currently prompting, then stop it and start prompting behavior on the next UI elements. If we reach the end, stop. </summary>
    public void ShowNextUIActionPrompt(GameObject go)
    {
        if (currentUIActionPromptee < 0 || currentUIActionPromptee >= UIActionPromptees.Length)
            return;
        if ( go == null)
        {
            Debug.LogError("Passed go is null. Returning.");
            return;
        }
        if (go == UIActionPromptees[currentUIActionPromptee])
            ShowNextUIActionPrompt();
    }

    /// <summary> Start prompting behavior on the next UI elements. If we reach the end, stop. </summary>
    public void ShowNextUIActionPrompt()
    {
        int newIndex = currentUIActionPromptee < UIActionPromptees.Length - 1 ? currentUIActionPromptee+1 : -1; //-1 will tell the system to stop, kinda awkward
        //This will first stop anything that's currently prompting.
        ShowUIActionPrompt(newIndex);
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
