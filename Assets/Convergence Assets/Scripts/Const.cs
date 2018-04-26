using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class Const : MonoBehaviour
{
    public static GUIStyle greenButton;
    public static GUIStyle redButton;
    public static GUIStyle greenLabel;
    public static GUIStyle redLabel;
    private bool done;
    public static float noiseMagnitude;
    public static float dampFactor;
    public bool useNoise;
    public bool enableProfiling;
    public static bool controlBusy;
    public static bool usesNoise;
    public static bool menuScrolling;
    public string dataBaseName;
    public static string dataBase;
    public static GUIStyle bigLabel;
    public static GUIStyle fixLabel;
    public static GUIStyle centerLabel;
    public static GUIStyle littleCenterLabel;
    public static GUIStyle littleBlackLabel;
    public static GUIStyle bigRedLabel;
    public static GUIStyle greenCenterLabel;
    public static GUIStyle rightLabel;
    public static GUIStyle bigToggle;
    public static GUIStyle buttonToggle;
    public static GUIStyle realToggle;
    public static GUIStyle bigInput;
    public static GUIStyle grayStyle;
    public static GUIStyle grayButton;
    public static GUIStyle errorStyle;
    public static GUIStyle solidWindowStyle;
    public Font bigFont;
    public Texture2D inputTexture; //Stauffer - change these to Texture2D
    public Texture2D grayTexture;
    public Texture2D errorTexture;
    public virtual void Awake()
    {
        Debug.Log("Awaking in const");
        Const.dataBase = this.dataBaseName;
        Const.greenButton = new GUIStyle();
        Const.redButton = new GUIStyle();
        Const.greenLabel = new GUIStyle();
        Const.redLabel = new GUIStyle();
        Const.usesNoise = this.useNoise;
        if (this.enableProfiling)
        {
            UnityEngine.Profiling.Profiler.logFile = "mylog.log";
            UnityEngine.Profiling.Profiler.enabled = true;
        }
    }

    private int gcount;
    public virtual void OnGUI()
    {
        if (++this.gcount < 3)
        {
        }
         //Debug.Log("in const OnGUI " + gcount);
        if (!this.done)
        {
            Const.greenButton = new GUIStyle(GUI.skin.button);
            Const.redButton = new GUIStyle(GUI.skin.button);
            Const.redButton.normal.textColor = new Color(1f, 0f, 0f, 1f);
            Const.greenButton.normal.textColor = new Color(0f, 1f, 0f, 1f);
            Const.greenLabel = new GUIStyle(GUI.skin.label);
            Const.redLabel = new GUIStyle(GUI.skin.label);
            Const.redLabel.normal.textColor = new Color(1f, 0f, 0f, 1f);
            Const.greenLabel.normal.textColor = new Color(0f, 1f, 0f, 1f);
            Const.bigLabel = new GUIStyle(GUI.skin.label);
            Const.bigLabel.font = this.bigFont;
            Const.bigLabel.alignment = TextAnchor.UpperLeft;
            Const.fixLabel = new GUIStyle(GUI.skin.label);
            Const.fixLabel.font = this.bigFont;
            Const.fixLabel.alignment = TextAnchor.UpperLeft;
            Const.fixLabel.normal.textColor = new Color(0f, 0f, 0f, 1f);
            Const.centerLabel = new GUIStyle(GUI.skin.label);
            Const.centerLabel.font = this.bigFont;
            Const.centerLabel.alignment = TextAnchor.UpperCenter;
            Const.centerLabel.normal.textColor = new Color(0f, 0f, 0f, 1f);
            Const.littleCenterLabel = new GUIStyle(GUI.skin.label);
            Const.littleCenterLabel.alignment = TextAnchor.UpperCenter;
            Const.littleBlackLabel = new GUIStyle(GUI.skin.label);
            Const.littleBlackLabel.normal.textColor = new Color(0f, 0f, 0f, 1f);
            Const.greenCenterLabel = new GUIStyle(GUI.skin.label);
            Const.greenCenterLabel.font = this.bigFont;
            Const.greenCenterLabel.alignment = TextAnchor.UpperCenter;
            Const.greenCenterLabel.normal.textColor = new Color(0f, 0.8f, 0f, 1f);
            Const.bigRedLabel = new GUIStyle(GUI.skin.label);
            Const.bigRedLabel.font = this.bigFont;
            Const.bigRedLabel.alignment = TextAnchor.UpperLeft;
            Const.bigRedLabel.normal.textColor = new Color(1f, 0.2f, 0.2f, 1f);
            Const.rightLabel = new GUIStyle(GUI.skin.label);
            Const.rightLabel.font = this.bigFont;
            Const.rightLabel.alignment = TextAnchor.UpperRight;
            Const.bigToggle = new GUIStyle(GUI.skin.button);
            Const.bigToggle.font = this.bigFont;
            Const.bigToggle.stretchWidth = true;
            Const.bigToggle.stretchHeight = true;
            Const.bigToggle.alignment = TextAnchor.UpperCenter;
            Const.buttonToggle = new GUIStyle(GUI.skin.button);
            Const.buttonToggle.font = this.bigFont;
            Const.buttonToggle.alignment = TextAnchor.UpperCenter;
            Const.realToggle = new GUIStyle(GUI.skin.toggle);
            Const.realToggle.font = this.bigFont;
            Const.realToggle.alignment = TextAnchor.UpperLeft;
            Const.bigInput = new GUIStyle(GUI.skin.label);
            Const.bigInput.font = this.bigFont;
            Const.bigInput.alignment = TextAnchor.UpperLeft;
            Const.bigInput.normal.background = this.inputTexture;
            Const.grayStyle = new GUIStyle(GUI.skin.label);
            Const.grayStyle.font = this.bigFont;
            Const.grayStyle.alignment = TextAnchor.UpperLeft;
            Const.grayStyle.normal.background = this.grayTexture;
            Const.grayButton = new GUIStyle(GUI.skin.button);
            Const.grayButton.font = this.bigFont;
            Const.grayButton.alignment = TextAnchor.UpperCenter;
            Const.grayButton.normal.background = this.grayTexture;
            Const.errorStyle = new GUIStyle(GUI.skin.label);
            Const.errorStyle.font = this.bigFont;
            Const.errorStyle.alignment = TextAnchor.UpperLeft;
            Const.errorStyle.normal.background = this.errorTexture;
            Const.solidWindowStyle = new GUIStyle(GUI.skin.label);
            Const.solidWindowStyle.normal.background = this.inputTexture;
            this.done = true;
        }
    }

    private int upcount;
    public virtual void Update()//Debug.Log("if we quit, why are we still here?");
    {
        if (++this.upcount < 3)
        {
        }
         //Debug.Log("in const update");
        if (Input.GetKey("escape"))
        {
            Debug.Log("heard escape key in Const");
            Application.Quit();
        }
    }

    public Const()
    {
        this.dataBaseName = "///unitygames/testdata.sqlite";
    }

    static Const()
    {
        Const.noiseMagnitude = 0.2f;
        Const.dampFactor = 0.95f;
    }

}