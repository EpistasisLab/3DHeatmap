using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton class for managing much of the UI stuff.
/// Some UI handling is done in classes tied to particular complex UI elements,
/// e.g. DataVarUIHandler and VisualMappingUIHandler
/// This has gotten somewhat outta hand with putting many things UI-related in here.
/// See MonoBehaviorSingleton class for usage as a singleton.
/// </summary>
public class UIManager : MonoBehaviorSingleton<UIManager>
{
    /// <summary> The UI canvas for the desktop </summary>
    public GameObject canvasDesktop { get; private set; }
    //A bunch of refs to various elements
    //A bunch of refs to various elements
    //Public ones to set in inspector. Probably should
    // all just be this way for clarity.
    public GameObject toolTipPanel;
    public GameObject statusPanel;
    //private ones
    private GameObject mainPanel;
    private GameObject optionsTopPanel;
    private GameObject visualMappingPanel;
    private VisualMappingUIHandler visualMappingUIHandler;
    //private GameObject dataVarsTopPanel; not using here currently
    public GameObject messageDialogPrefab;
    public GameObject redrawButton;
    public GameObject mappingHeightDropdown;
    public GameObject mappingTopColorDropdown;
    public GameObject mappingSideColorDropdown;
    /// <summary> The panel in the VR/world canvas in which to drop copies of menus </summary>
    public GameObject VRCanvasLayoutPanel;
    
    /// <summary> Index within the auto-prompt list of the UI object that's currently being highlighted in some way </summary>
    private int currentAutoUIActionPrompteeIndex;
    /// <summary> Obj pointer to UI element that's currently being flashed. </summary>
    GameObject mostRecentUIActionPrompteeObj;
    /// <summary> List of UI elements to use for prompting user action, in order of
    /// which they should be activated </summary>
    private List<GameObject> UIActionPromptees;

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
        canvasDesktop = GetAndCheckGameObject("CanvasScreenSpace");
        //Graph panels
        mainPanel = GetAndCheckGameObject("MainPanel");
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

//        SetupVRmenus();

        TooltipHide();

        //Make sure this is called to init the list of dataVar handlers
        DataVarUIHandler.InitializeListOfAll();

        UIActionPromptInit();
    }

    /// <summary>
    /// This will copy 
    /// </summary>
    private void SetupVRmenus()
    {
        GameObject mainMenuVR = Instantiate(mainPanel, VRCanvasLayoutPanel.transform);
        GameObject optionMenuVR = Instantiate(optionsTopPanel, VRCanvasLayoutPanel.transform);
    }

    IEnumerator StartAutoUIActionPromptsCoroutine()
    {
        yield return null;
        StartAutoUIActionPrompts();
    }

    private void Start()
    {
        //Start the sequence of UI prompts to help user know what to do.
        StartCoroutine(StartAutoUIActionPromptsCoroutine());
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

        CanvasGroup[] cgs = GameObject.FindObjectsOfType<CanvasGroup>();
        //Debug.Log("cgs count " + cgs.Length);
        foreach( CanvasGroup cg in cgs)
        {
            cg.alpha = alpha;
            cg.interactable = interactable;
        }
/*
        //Top data panel
        CanvasGroup cg = dataTopPanel.GetComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.interactable = interactable;

        //Options top panel
        cg = optionsTopPanel.GetComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.interactable = interactable;
        */
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
        GameObject go = Instantiate(messageDialogPrefab, canvasDesktop.GetComponent<RectTransform>());
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
    /// Awkward, I know.
    /// </summary>
    public void RefreshUI()
    {
        visualMappingUIHandler.RefreshUI();
        InputManager.I.Reset();
        VRManager.I.UIupdate();
        
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

    /// <summary>
    /// Set the color table selection for a given mapping. If we just set it here and then call
    /// Redraw, the new values will get picked up.
    /// </summary>
    /// <param name="index">Index into color table dropdown</param>
    public void SetColorTableByMappingAndIndex(DataManager.Mapping mapping, int index)
    {
        visualMappingUIHandler.SetColorTableByMappingAndIndex(mapping, index);
    }

    public void OnRedrawButtonClick(GameObject button)
    {
        Graph.I.Redraw();
    }

    public void OnResetViewButtonClick()
    {
        Graph.I.ResetView();
    }

    /// <summary> Set up the list of game objects for action prompting </summary>
    private void UIActionPromptInit()
    {
        UIActionPromptees = new List<GameObject>();
        //Add relevant UI elemetns for first two data vars
        for(int index = 0; index < 2; index++)
        {
            DataVarUIHandler obj = DataVarUIHandler.GetHandlerAtIndex(index);
            UIActionPromptees.Add(obj.ChooseFileButton);
            UIActionPromptees.Add(obj.HeadersDropdown);
            UIActionPromptees.Add(obj.LoadButton);
            UIActionPromptees.Add(obj.InputField);
        }

        //Now add the mapping UI elements
        UIActionPromptees.Add(mappingHeightDropdown);
        UIActionPromptees.Add(mappingTopColorDropdown);
        UIActionPromptees.Add(mappingSideColorDropdown);

        UIActionPromptees.Add(redrawButton);

        currentAutoUIActionPrompteeIndex = -1;
        mostRecentUIActionPrompteeObj = null;
    }

    private bool UIActionPromptIsFinished { get { return currentAutoUIActionPrompteeIndex == -1; } }

    /// <summary>
    /// Start prompting the user for which UI item should be
    /// (typically) used next. Currently it flashes the item.
    /// </summary>
    /// <param name="newIndex">Index of UI object to start showing prompt for. Pass -1 to just stop anything currently showing a prompt.</param>
    private void ShowUIActionPrompt(int newIndex)
    {
        //Stop whatever currently is prompting.
        //Sets currentAutoUIActionPrompteeIndex to -1
        StopCurrentUIActionPrompt();
        if (newIndex < 0)
            return;
        if (newIndex >= UIActionPromptees.Count)
        {
            Debug.LogWarning("ShowUIActionPrompt: newIndex out of range: " + newIndex);
            return;
        }

        GameObject uiObj = UIActionPromptees[newIndex];
        //Debug.Log("calling StartFlashing. index: " + index);
        StartStopUIActionPrompt(uiObj, true);
        currentAutoUIActionPrompteeIndex = newIndex;
    }

    /// <summary> Start/stop falshing an individual ui object. It must have the UIElementFlasher component.
    /// Can be called separately from the 'automatic' method of using a list of which element to prompt next.
    /// If the automatic method is not yet complete, this call will be silently ignored.
    /// </summary>
    public void StartStopManualUIActionPrompt(GameObject uiObj, bool enable)
    {
        if (UIActionPromptIsFinished)
            StartStopUIActionPrompt(uiObj, enable);
    }

    /// <summary> Start/stop falshing an individual ui object. It must have the UIElementFlasher component.
    /// If another object is already flashing, it will stop flashing. </summary>
    /// <param name="uiObj"></param>
    /// <param name="enable"></param>
    private void StartStopUIActionPrompt(GameObject uiObj, bool enable)
    {
        if (uiObj == null || uiObj.GetComponent<UIElementFlasher>() == null)
        {
            Debug.LogWarning("ShowUIActionPrompt: passed UI is null or doesn't have UIElementFlasher");
            return;
        }

        if (enable)
        {
            if (mostRecentUIActionPrompteeObj != null)
                StartStopUIActionPrompt(mostRecentUIActionPrompteeObj, false);
            uiObj.GetComponent<UIElementFlasher>().StartFlashing();
            mostRecentUIActionPrompteeObj = uiObj;
        }
        else
        {
            uiObj.GetComponent<UIElementFlasher>().StopFlashing();
            //don't set this, in case we get mulitple calls here while setting up - don't want to lose track of what's actually flashing
            //mostRecentUIActionPrompteeObj = null;
        }
    }

    /// <summary> Prompt the user to redraw by flashing UI element </summary>
    public void StartRedrawPrompt()
    {
        //First make sure we've stopped auto-prompts
        StopAutoUIActionPrompts();
        StartStopManualUIActionPrompt(redrawButton, true);
    }

    public void StopCurrentUIActionPrompt()
    {
        if(mostRecentUIActionPrompteeObj != null)
        {
            StartStopUIActionPrompt(mostRecentUIActionPrompteeObj, false);
            currentAutoUIActionPrompteeIndex = -1;
        }
    }

    /// <summary> Start the semi-automatic sequence of UI action prompts </summary>
    public void StartAutoUIActionPrompts()
    {
        ShowUIActionPrompt(0);
    }

    /// <summary> Stop the sequence of UI action prompting </summary>
    public void StopAutoUIActionPrompts()
    {
        ShowUIActionPrompt(-1);
    }
    
    /// <summary> If the passed Gameobject is currently prompting, then stop it and start prompting behavior on the next UI elements. If we reach the end, stop. </summary>
    public void ShowNextUIActionPrompt(GameObject go)
    {
        if (currentAutoUIActionPrompteeIndex < 0 || currentAutoUIActionPrompteeIndex >= UIActionPromptees.Count)
            return;
        if ( go == null)
        {
            Debug.LogError("Passed go is null. Returning.");
            return;
        }
        if (go == UIActionPromptees[currentAutoUIActionPrompteeIndex])
            ShowNextUIActionPrompt();
    }

    /// <summary> Start prompting behavior on the next UI elements. If we reach the end, stop. </summary>
    public void ShowNextUIActionPrompt()
    {
        int newIndex = currentAutoUIActionPrompteeIndex < UIActionPromptees.Count - 1 ? currentAutoUIActionPrompteeIndex+1 : -1; //-1 will tell the system to stop, kinda awkward
        //This will first stop anything that's currently prompting.
        ShowUIActionPrompt(newIndex);
    }

    public void OnDemoDataClick()
    {
        ProjectManager.I.DemoDataLoadAndDraw();
    }

    public void OnHelpButtonClick()
    {
        ShowIntroMessage();
    }

    public void OnClearData()
    {
        Graph.I.ClearCurrentGraph();
        DataManager.I.Clear();
    }

    /// <summary>
    /// Hack this in here for now.
    /// Show an intro message dialog with basic usage instructions.
    /// </summary>
    public void ShowIntroMessage()
    {
        string msg = "<b>3D Heatmap</b> allows you to visualize two independent variables, and up to three dependent variables (DV) simultaneously. Each DV must be derived from the same two independent variables. Each DV can be assigned to any of three visual parameters used to make the 3D heatmap: height, top color and side color.\n\n" +
                "<b>Data Format</b>\n\nThe program expects each DV from a separate text data file of 2D array data, in either csv or tab-delimited formats. Each data file must have the same dimensions and can contain NaN values.\n\n" +
                "<b>Instructions</b>\n\n" +
                "0) Click Demo Data button to view some demo data. Otherwise..."+
                "1) Under <i>Data Variables</i> in the GUI to the left, select and load your data files.\n" +
                "2) Under <i>Visual Mappings</i>, assign the variables to each of Height, Top Color, and Side Color.\n" +
                "3) Choose color tables for Top and Side colors.\n" +
                "4) Click the Redraw button, or press F2.\n\n" +
                "<b>View Controls</b>\n\n" +
                "Move   - <i>Mouse:</i> left-click & drag  <i>Touch:</i> Two-finger drag   <i>Keyboard:</i> arrow keys\n" +
                "Rotate - <i>Mouse:</i> right-click & drag <i>Touch:</i> Three-finger drag <i>Keyboard:</i> Ctrl + arrows\n" +
                "Zoom   - <i>Mouse:</i> scroll wheel       <i>Touch:</i> Two-finger pinch  <i>Keyboard:</i> Shift + arrows, or +/- keys\n\n" +
                "<b>VR</b>\n\n" +
                "SteamVR/OpenVR devices are supported for viewing in VR.\nHold the controller's grip button to move the data, and the trigger to inspect it.\n\n"+
                "<b>Version</b> " + VersionNumber.I.Version;
        ShowMessageDialog(msg, 650f, true);
    }
}
