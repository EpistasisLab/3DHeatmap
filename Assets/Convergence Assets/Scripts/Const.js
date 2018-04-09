static var greenButton : GUIStyle;
static var redButton : GUIStyle;
static var greenLabel : GUIStyle;
static var redLabel : GUIStyle;
private var done : boolean = false;
static var noiseMagnitude : float = 0.2;
static var dampFactor : float = 0.95;
var useNoise : boolean = false;
var enableProfiling : boolean = false;
static var controlBusy : boolean = false;
static var usesNoise : boolean;
static var menuScrolling : boolean = false;
var dataBaseName : String = "///unitygames/testdata.sqlite";
static var dataBase : String;
static var bigLabel : GUIStyle;
static var fixLabel : GUIStyle;
static var centerLabel : GUIStyle;
static var littleCenterLabel : GUIStyle;
static var littleBlackLabel : GUIStyle;
static var bigRedLabel : GUIStyle;
static var greenCenterLabel : GUIStyle;
static var rightLabel : GUIStyle;
static var bigToggle : GUIStyle;
static var buttonToggle : GUIStyle;
static var realToggle : GUIStyle;
static var bigInput : GUIStyle;
static var grayStyle : GUIStyle;
static var grayButton : GUIStyle;
static var errorStyle : GUIStyle;
static var solidWindowStyle : GUIStyle;
var bigFont : Font;
var inputTexture : Texture;
var grayTexture : Texture;
var errorTexture : Texture;

function Awake()
{
	Debug.Log("Awaking in const");
	Const.dataBase = dataBaseName;
	greenButton = new GUIStyle();
	redButton = new GUIStyle();
	greenLabel = new GUIStyle();
	redLabel = new GUIStyle();
	usesNoise = useNoise;
	if(enableProfiling)
	{
		UnityEngine.Profiling.Profiler.logFile = "mylog.log";
		UnityEngine.Profiling.Profiler.enabled = true;
	}
}

private var gcount : int = 0;

function OnGUI()
{
	if(++gcount < 3)
	{
		//Debug.Log("in const OnGUI " + gcount);
	}
	if(!done)
	{
		greenButton = new GUIStyle(GUI.skin.button);
		redButton = new GUIStyle(GUI.skin.button);
		redButton.normal.textColor = new Color(1.0, 0.0, 0.0, 1.0);
		greenButton.normal.textColor = new Color(0.0, 1.0, 0.0, 1.0);
		greenLabel = new GUIStyle(GUI.skin.label);
		redLabel = new GUIStyle(GUI.skin.label);
		redLabel.normal.textColor = new Color(1.0, 0.0, 0.0, 1.0);
		greenLabel.normal.textColor = new Color(0.0, 1.0, 0.0, 1.0);
		bigLabel = new GUIStyle(GUI.skin.label);
		bigLabel.font = bigFont;
		bigLabel.alignment = TextAnchor.UpperLeft;
		fixLabel = new GUIStyle(GUI.skin.label);
		fixLabel.font = bigFont;
		fixLabel.alignment = TextAnchor.UpperLeft;
		fixLabel.normal.textColor = new Color(0.0, 0.0, 0.0, 1.0);
		
		centerLabel = new GUIStyle(GUI.skin.label);
		centerLabel.font = bigFont;
		centerLabel.alignment = TextAnchor.UpperCenter;
		centerLabel.normal.textColor = new Color(0.0, 0.0, 0.0, 1.0);
		
		littleCenterLabel = new GUIStyle(GUI.skin.label);
		littleCenterLabel.alignment = TextAnchor.UpperCenter;
		
		littleBlackLabel = new GUIStyle(GUI.skin.label);
		littleBlackLabel.normal.textColor = new Color(0.0, 0.0, 0.0, 1.0);
		
		greenCenterLabel = new GUIStyle(GUI.skin.label);
		greenCenterLabel.font = bigFont;
		greenCenterLabel.alignment = TextAnchor.UpperCenter;
		greenCenterLabel.normal.textColor = new Color(0.0, 0.8, 0.0, 1.0);
		
		bigRedLabel = new GUIStyle(GUI.skin.label);
		bigRedLabel.font = bigFont;
		bigRedLabel.alignment = TextAnchor.UpperLeft;
		bigRedLabel.normal.textColor = new Color(1.0, 0.2, 0.2, 1.0);
		
		rightLabel = new GUIStyle(GUI.skin.label);
		rightLabel.font = bigFont;
		rightLabel.alignment = TextAnchor.UpperRight;
		bigToggle = new GUIStyle(GUI.skin.button);
		bigToggle.font = bigFont;
		bigToggle.stretchWidth = true;
		bigToggle.stretchHeight = true;
		bigToggle.alignment = TextAnchor.UpperCenter;
		
		buttonToggle = new GUIStyle(GUI.skin.button);
		buttonToggle.font = bigFont;
		buttonToggle.alignment = TextAnchor.UpperCenter;
		
		realToggle = new GUIStyle(GUI.skin.toggle);
		realToggle.font = bigFont;
		realToggle.alignment = TextAnchor.UpperLeft;
		bigInput = new GUIStyle(GUI.skin.label);
		bigInput.font = bigFont;
		bigInput.alignment = TextAnchor.UpperLeft;
		bigInput.normal.background = inputTexture;
		
		grayStyle = new GUIStyle(GUI.skin.label);
		grayStyle.font = bigFont;
		grayStyle.alignment = TextAnchor.UpperLeft;
		grayStyle.normal.background = grayTexture;

		grayButton = new GUIStyle(GUI.skin.button);
		grayButton.font = bigFont;
		grayButton.alignment = TextAnchor.UpperCenter;
		grayButton.normal.background = grayTexture;

		errorStyle = new GUIStyle(GUI.skin.label);
		errorStyle.font = bigFont;
		errorStyle.alignment = TextAnchor.UpperLeft;
		errorStyle.normal.background = errorTexture;
		
		solidWindowStyle = new GUIStyle(GUI.skin.label);
		solidWindowStyle.normal.background = inputTexture;
	
		done = true;
	}
}

private var upcount : int = 0;
function Update ()
{
	if(++upcount < 3)
	{
		//Debug.Log("in const update");
	}
	if (Input.GetKey ("escape"))
	{
		Debug.Log("heard escape key in Const");
		Application.Quit();
		//Debug.Log("if we quit, why are we still here?");
	}
}
