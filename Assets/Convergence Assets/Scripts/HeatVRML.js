import Mono.Data.Sqlite;
import System.IO;
/*
	Heatmap.js Contains almost all code for interacting with user and drawing 3D heatmaps.  Coded by Douglas P. Hill.
    Copyright (C) 2010  Dartmouth College

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

// Version
var programVersion : String = "1.2";

// window numbers
private var DSwin : int = 0;
private var STYLEwin : int = 1;
private var ZIPwin : int = 2;
private var SLwin : int = 3;
private var NUMwin : int = 4;

private var isWindowOpen : boolean[];
private var windowRects : Rect[];
private var sizeRect : Function[];
private var windChanged : boolean[];
private var doWind : GUI.WindowFunction[];
private var closefunc : GUI.WindowFunction;
private var isLayout : boolean[] = [true, true, true, true];
private var allHidden : boolean = false;

// window extents
var dsRect : Rect = Rect(2, 2, 120, 100);
var styleRect : Rect = Rect(130, 2, 120, 40);
var zipRect : Rect = Rect(258, 2, 120, 40);
var slidersRect : Rect = Rect(258, 2, 120, 40);
var pointedWindowRect : Rect = Rect(0, 300, 120, 60);
var xrayWindowRect : Rect = Rect(200, 200, 800, 800);
var helpWindowRect : Rect = Rect(10, 10, 10, 10);

var windNames : String[] = ["Data Selection", "Chart Style", "Zip to Viewpoint", "Chart View"];

// Input related
private var waitCount : int = 0;
var scrollOnLowTexture : Texture;
var scrollOffLowTexture : Texture;
var scrollOnHighTexture : Texture;
var scrollOffHighTexture : Texture;
var scrollStyle : GUIStyle;

var grayscaleStyle : GUIStyle;
var grayscaleOnTexture : Texture;
var grayscaleOffTexture : Texture;

var constantStyle : GUIStyle;
var constantOnTexture : Texture;
var constantOffTexture : Texture;

var rainbowStyle : GUIStyle;
var rainbowOnTexture : Texture;
var rainbowOffTexture : Texture;

var redgreenStyle : GUIStyle;
var redgreenOnTexture : Texture;
var redgreenOffTexture : Texture;

var yellowblueStyle : GUIStyle;
var yellowblueOnTexture : Texture;
var yellowblueOffTexture : Texture;

private var initDone : boolean = false;
private var scrollAmount : float = 0.0;

// scene related
var xSceneSize : float = 400.0;
var ySceneSize : float = 400.0;
var zSceneSize : float = 200.0;
var xzySceneCorner : Vector3 = Vector3(0, 0, 0);
private var fullYSceneSize : float;

var maxXs : int = 1800;

// Database related
private var dbPos = new Vector2(0, 0);
private var numDB : int = 0;
private var currDB : int = -1;
private var dbChoices : String[];
private var selTable : String;
private var isDetecting : int = 0;
private var rowLabels : String[];
private var numRowLabels : int = 0;

private var connStrn : String;
private var reader : SqliteDataReader;
private var connection : SqliteConnection;
private var dbcmd : SqliteCommand;
private var dataChanged : boolean = false;
private var wantRedraw : boolean = false;
private var wantVRML : boolean = false;
private var wantTriangles : boolean = false;

private var numFields : int = 0;
private var allFields : FieldData[];
	
// data values for the current row
private var colVals : int[];
private var heightVals : float[];
private var topVals : int[];
private var sideVals : int[];

var serverURL : String = "draco.dhcp.hitchcock.org";
var beServer : boolean = true;

// feature inclusion
var includeVRML : boolean = false;
var includeBalls : boolean = false;
var includeTriangles : boolean = true;

// chart related
private var bInterpolateY : boolean = false;
private var bConnectX : boolean = false;
private var bExtendZ : boolean = true;
private var bHaveLabels : boolean = false;
private var bShowLabels : boolean = true;
private var topColorChoice : int = 0;
private var sideColorChoice : int = 0;
private var proto : GameObject;
private var baseCube : GameObject;
private var protolabel : GameObject;
private var drawn : boolean = false;

private var xRidges : XRidge[];
private var shaveCount : int = 3;
private var numRidges : int = 0;
private var tokenWidth : float;	// Y dimension of tokens used to represent values
private var xScale : float;
private var zScale : float;

private var binIncrement : float;
private var rowIncrement : float;
private var plotIncrement : float;

private var minRow : int;
private var maxRow : int;
private var minCol : int;
private var maxCol : int;
private var minBin : int;
private var maxBin : int;
private var minHeight : float;
private var maxHeight : float;
private var numRows : int;
private var numCols : int;
private var numBins : int;
private var heightRange : float;
private var minMarker : int;
private var maxMarker : int;
private var numMarkers : int;
private var currBin : int = 0;
private var bScrollBin : boolean = false;
private var lastScrollTime : float = 0.0; //Stauffer mod
var minScrollSecs : float = 0.1;	// minimum time between changes of scrolling value
private var choiceHeight : int = 0;
private var choiceFOV : int = 1;
private var choiceThick : int = 2;
private var choiceSep : int = 3;
private var choiceGap : int = 4;
private var choiceBin : int = 5;

private var sideStyleChoice : int = 0;
private var topStyleChoice : int = 0;

private var scrollChoice : int = -1;

private var lowGraphHeightRange : float = 0.002; private var highGraphHeightRange : float = 1.0; private var currGraphHeight : float = 0.5;
private var lowFOVRange : float = 20.0; private var highFOVRange : float = 170.0; private var currFOV : float;
private var lowThickRange : float = -4.0; private var highThickRange : float = 4.0; private var currThick : float = 0.0;
private var lowSepRange : float = 0.0; private var highSepRange : float = 4.0; private var currSep : float = 1.1;
private var lowGapRange : float = 0.0; private var highGapRange : float = 4.0; private var currGap : float = 1.0;

private var pointedData : PointedData = new PointedData();
private var signPost : GameObject;

// Layout
var pixPerLine : int = 25;
var rowHeight : int = 25;
var colWidth : int = 100;
var lineWidth : int = 20;

// Specific windows
var scrollHeight : int = 112;

// Help window
private var showHelp : boolean = true;
private var helpPage : int = 0;
private var oldHelpPage : int = 1;
private var menuScrollPos : Vector2 = new Vector2(0.0, 0.0);
private var helpCount : int = 0;

// Components
var myCamera : Camera;
var myController : GameObject;

// VRML
private var fileVRML : File;
var vrmlModelMM : float = 200.0;

// Ball dropping
private var doDrop : boolean = false;

// Debugging
private var xray : Texture2D;
private var showXRay : boolean = false;
private var colLimit = 32000;
	

function Start()
{
	lastScrollTime = Time.time; //Stauffer move init here

	isWindowOpen = new boolean[NUMwin];
	windowRects = new Rect[NUMwin];
	windowRects[DSwin] = dsRect;
	windowRects[STYLEwin] = styleRect;
	windowRects[ZIPwin] = zipRect;
	windowRects[SLwin] = slidersRect;
	sizeRect = new Function[NUMwin];
	sizeRect[DSwin] = SizeDS;
	sizeRect[STYLEwin] = SizeStyle;
	sizeRect[ZIPwin] = SizeZip;
	sizeRect[SLwin] = SizeSliders;
	doWind = new GUI.WindowFunction[NUMwin];
	doWind[DSwin] = DoDS;
	doWind[STYLEwin] = DoStyle;
	doWind[ZIPwin] = DoZip;
	doWind[SLwin] = DoSliders;
	windChanged = new boolean[NUMwin];
	var i : int;
	for(i = 0; i < NUMwin; ++i)
	{
		windChanged[i] = true;
		isWindowOpen[i] = true;
		sizeRect[i]();
		if(i > 0)
		{
			windowRects[i].x = windowRects[i - 1].x + windowRects[i - 1].width + 2;
		}
	}
	closefunc = DoClosedWind;
	// Can't figure out how to have ArrayFromQuery re-allocate these, so pick a big size
	dbChoices = new String[100];
	if(beServer)
	{
		if(Network.InitializeServer(40, 25000) != NetworkConnectionError.NoError)
		{
			ErrorGUI.ShowError("Could not initialize server");
		}
	}
	else
	{
		if(Network.Connect(serverURL, 25000) != NetworkConnectionError.NoError)
		{
			//ErrorGUI.ShowError("Could not connect to server");
		}
	}
	// If the falling ball feature is present, we want to have a small scene so gravity looks more realistic
	if(!includeBalls)
	{
		xSceneSize *= 4.0;
		ySceneSize *= 4.0;
		zSceneSize *= 4.0;
	}
	
	// Find the prototype mesh object, if present
	proto = GameObject.Find("protomesh");
	if(!includeBalls) Destroy(proto.GetComponent.<Collider>());
	baseCube = GameObject.Find("basecube");
	MakeUnitCube(baseCube);
	protolabel = GameObject.Find("protolabel");
	signPost = Instantiate(GameObject.Find("SignPost"), Vector3(-1000, -1000, -1000), Quaternion.identity);
	
	// Open database
	connStrn = "URI=file:" + Const.dataBase;
	connection = new SqliteConnection(connStrn);
	connection.Open();
	dbcmd = connection.CreateCommand();
	myCamera = GameObject.FindWithTag("MainCamera").GetComponent("Camera");
	currFOV = lowFOVRange + highFOVRange - myCamera.fieldOfView; // hokey, but we want currFOV to increase as fieldOfView decreases
	myController = GameObject.Find("FPC");
	
	allFields = new FieldData[2];
	
	allFields[0] = new FieldData();
	allFields[0].SetFloat("height", 0.0, 10.0);
	
	allFields[1] = new FieldData();
	allFields[1].SetInt("bin", 0, 2);
	
	numFields = 2;
	xray = new Texture2D(Screen.width / 2, Screen.height / 2);
	//SpreadBalls(16, 10.0);
}

private var haveBalls : boolean = false;
private var allBalls : Rigidbody[];
var protoBall : Rigidbody;

function SpreadBalls(rowcols : int, spread : float)
{
	if(haveBalls)
	{
		//var i : int;
		//for(i = 0; i < allBalls.length; ++i) Destroy(allBalls[i].gameObject);
	}
	spread = (xSceneSize * spread) / rowcols;
	var numBalls : int = rowcols * rowcols;
	allBalls = new Rigidbody[numBalls];
	var startX : float = xzySceneCorner.x + (xSceneSize * 0.5) - (rowcols * 0.5 * spread);
	var startZ : float = xzySceneCorner.z + (ySceneSize * 0.5) - (rowcols * 0.5 * spread);
	var startY : float = zSceneSize * 2.0;
	var row : int;
	var col : int;
	i = 0;
	protoBall.transform.localScale = Vector3(tokenWidth, tokenWidth, tokenWidth);
	for(row = 0; row < rowcols; ++row)
	{
		var zPos : float = startZ + row * spread;
		for(col = 0; col < rowcols; ++col)
		{
			allBalls[i] = Instantiate(protoBall, Vector3(startX + col * spread + Random.Range(-spread, spread), startY, zPos + Random.Range(-spread, spread)), Quaternion.identity);
			allBalls[i++].velocity = Vector3(0, 0, 0);
		}
	}
	haveBalls = true;
}

function Update()
{
	if(Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.F5)) allHidden = !allHidden;
	if(Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1)) showHelp = !showHelp;
	
	if(Const.menuScrolling && (Time.time > (lastScrollTime + minScrollSecs)))
	{
		scrollAmount = Input.GetAxis("Mouse ScrollWheel");
		if(scrollAmount != 0.0)
		{
			lastScrollTime = Time.time;
		}
	}
	/*
	var i : int;
	for(i = 0; i < numRidges; ++i)
	{
		var adv : Vector2 = Vector2(i / 100028.0, (i % 30) / 100028.0);
		xRidges[i].AdvanceUV(Vector2(i * 0.1 / numRidges, (i % 6) / 360.0), adv);
	}
	*/
	/*
	if(bScrollBin && (scrollAmount != 0))
	{
		if(currBin < 0) currBin = 0;
		var oldBin : int = currBin;
		if(scrollAmount < 0) ++currBin;
		if(scrollAmount > 0) --currBin;
		scrollAmount = 0;
		if(currBin < 0) currBin = numBins - 1;
		if(currBin >= numBins) currBin = 0;
		if(currBin != oldBin)
		{
			if(beServer) networkView.RPC("VisBins", RPCMode.All, currBin);
			else VisBins(currBin);
		}
	}
	*/
}

function FixedUpdate()
{
	if(doDrop)
	{
		SpreadBalls(16, 0.9);
		doDrop = false;
	}
}

function OnGUI()
{
	if(!initDone)
	{
		initDone = true;
		scrollStyle = new GUIStyle(GUI.skin.toggle);
		scrollStyle.imagePosition = ImagePosition.ImageLeft;
		scrollStyle.onActive.background = scrollOnLowTexture;
		scrollStyle.onNormal.background = scrollOnLowTexture;
		scrollStyle.onHover.background = scrollOnHighTexture;
		scrollStyle.active.background = scrollOffLowTexture;
		scrollStyle.normal.background = scrollOffLowTexture;
		scrollStyle.hover.background = scrollOffHighTexture;
		scrollStyle.stretchWidth = false;
		scrollStyle.stretchHeight = false;
		
		grayscaleStyle = new GUIStyle();
		grayscaleStyle.imagePosition = ImagePosition.ImageOnly;
		grayscaleStyle.onActive.background = grayscaleOnTexture;
		grayscaleStyle.onNormal.background = grayscaleOnTexture;
		grayscaleStyle.onHover.background = grayscaleOnTexture;
		grayscaleStyle.active.background = grayscaleOffTexture;
		grayscaleStyle.normal.background = grayscaleOffTexture;
		grayscaleStyle.hover.background = grayscaleOffTexture;
		grayscaleStyle.stretchWidth = false;
		grayscaleStyle.stretchHeight = false;
		grayscaleStyle.fixedWidth = 16;
		grayscaleStyle.fixedHeight = 16;
		grayscaleStyle.margin = GUI.skin.toggle.margin;
		grayscaleStyle.margin.left = 2;
		grayscaleStyle.margin.right = 2;
		grayscaleStyle.margin.top = 1;
		grayscaleStyle.margin.bottom = 1;
		grayscaleStyle.border = GUI.skin.toggle.border;
		grayscaleStyle.border.left = 0;
		grayscaleStyle.border.right = 0;
		grayscaleStyle.border.top = 0;
		grayscaleStyle.border.bottom = 0;
		grayscaleStyle.padding = GUI.skin.toggle.padding;
		grayscaleStyle.padding.left = 0;
		grayscaleStyle.padding.right = 0;
		grayscaleStyle.padding.top = 0;
		grayscaleStyle.padding.bottom = 0;
		grayscaleStyle.overflow = GUI.skin.toggle.overflow;
		grayscaleStyle.overflow.left = 0;
		grayscaleStyle.overflow.right = 0;
		grayscaleStyle.overflow.top = 0;
		grayscaleStyle.overflow.bottom = 0;
		
		constantStyle = new GUIStyle(grayscaleStyle);
		constantStyle.onActive.background = constantOnTexture;
		constantStyle.onNormal.background = constantOnTexture;
		constantStyle.onHover.background = constantOnTexture;
		constantStyle.active.background = constantOffTexture;
		constantStyle.normal.background = constantOffTexture;
		constantStyle.hover.background = constantOffTexture;
		
		rainbowStyle = new GUIStyle(grayscaleStyle);
		rainbowStyle.onActive.background = rainbowOnTexture;
		rainbowStyle.onNormal.background = rainbowOnTexture;
		rainbowStyle.onHover.background = rainbowOnTexture;
		rainbowStyle.active.background = rainbowOffTexture;
		rainbowStyle.normal.background = rainbowOffTexture;
		rainbowStyle.hover.background = rainbowOffTexture;
		
		redgreenStyle = new GUIStyle(grayscaleStyle);
		redgreenStyle.onActive.background = redgreenOnTexture;
		redgreenStyle.onNormal.background = redgreenOnTexture;
		redgreenStyle.onHover.background = redgreenOnTexture;
		redgreenStyle.active.background = redgreenOffTexture;
		redgreenStyle.normal.background = redgreenOffTexture;
		redgreenStyle.hover.background = redgreenOffTexture;
		
		yellowblueStyle = new GUIStyle(grayscaleStyle);
		yellowblueStyle.onActive.background = yellowblueOnTexture;
		yellowblueStyle.onNormal.background = yellowblueOnTexture;
		yellowblueStyle.onHover.background = yellowblueOnTexture;
		yellowblueStyle.active.background = yellowblueOffTexture;
		yellowblueStyle.normal.background = yellowblueOffTexture;
		yellowblueStyle.hover.background = yellowblueOffTexture;
		
	}
	//if((!Const.controlBusy) && Input.GetKeyDown(KeyCode.I)) CapturePointedAt();
	//if((!Const.controlBusy) && Input.GetKeyDown(KeyCode.O)) MakeXRay();
	
	if(showHelp)
	{
		if(helpCount++ == 2)
		{
			// Center screen after first sizing; after that let the user control position
			helpWindowRect.x = (Screen.width - helpWindowRect.width) / 2.0;
			helpWindowRect.y = (Screen.height - helpWindowRect.height) / 3.0;
		}
		if(helpPage != oldHelpPage)
		{
			helpWindowRect.width = 800.0;
			helpWindowRect.height = 10.0;
		}
		helpWindowRect = GUILayout.Window(NUMwin + 100, helpWindowRect, DoHelp, "3D Heatmap " + programVersion);
	}
	
	var i : int;
	if(!allHidden)
	{
		for(i = 0; i < NUMwin; ++i)
		{
			if(windChanged[i]) DoSize(i);
			DoWind(i);
		}
	}
	
	if(pointedData.ready) pointedWindowRect = GUILayout.Window(NUMwin, pointedWindowRect, DoPointedWindow, "Data Point");
	if(showXRay) xrayWindowRect = GUILayout.Window(NUMwin + 1, xrayWindowRect, DoXRayWindow, "xray");
}

function DoWind(windowID : int)
{
	if(isLayout[windowID])
	{
		if(isWindowOpen[windowID]) windowRects[windowID] = GUILayout.Window(windowID, windowRects[windowID], doWind[windowID], windNames[windowID]);
		else windowRects[windowID] = GUI.Window(windowID, windowRects[windowID], DoClosedWind, windNames[windowID]);
	}
	else
	{
		windowRects[windowID] = GUI.Window(windowID, windowRects[windowID], (isWindowOpen[windowID]) ? doWind[windowID] : closefunc, windNames[windowID]);
	}
}

function DoSize(windowID : int)
{
	if(isWindowOpen[windowID]) sizeRect[windowID]();
	else
	{
		windowRects[windowID].height = 46;
	}
	windChanged[windowID] = false;
}

function DoClosedWind(windowID : int) : void
{
	if(GUI.Toggle(Rect(2, 0, 20, 20), false, ""))
	{
		isWindowOpen[windowID] = true;
		windChanged[windowID] = true;
	}
	GUI.DragWindow();
}

function SizeDS()
{
	// These are going to expand because of GUILayout
	windowRects[DSwin].height = 60;
	windowRects[DSwin].width = 280;
	scrollHeight = numDB * pixPerLine + 20;
	if(scrollHeight > 200) scrollHeight = 200;
}

function SizeStyle() : void
{
	//windowRects[STYLEwin].height = (12 * pixPerLine) + 6;
	windowRects[STYLEwin].height = 60;
	windowRects[STYLEwin].width = 450;
}

function SizeZip()
{
	windowRects[ZIPwin].height = (5 * pixPerLine) + 6;
	windowRects[ZIPwin].width = 130;
}

function SizeSliders()
{
	windowRects[SLwin].height = windowRects[ZIPwin].height;
	windowRects[SLwin].width = 150;
}

function DoDS(windowID : int) : void
{
	ShrinkIt(windowID);
	if(isDetecting++ < 4)
	{
		GUILayout.Label("Analyzing Datasets . . .", Const.greenCenterLabel);
		// Give the above time to draw before lengthy examination of database
		if(isDetecting == 4)
		{
			GetDatabaseChoices();
			SizeDS();
		}
	}
	else
	{
		if(dataChanged) GUILayout.Label("Drawing. . .", Const.greenCenterLabel);
		else
		{
			dbPos = GUILayout.BeginScrollView(dbPos, GUILayout.Width (260), GUILayout.Height (scrollHeight));
			GUILayout.BeginVertical();
			for(var dbnum = 0; dbnum < numDB; ++dbnum)
			{
				var oldVal : boolean = (dbnum == currDB);
				if(GUILayout.Toggle(oldVal, dbChoices[dbnum], Const.realToggle))
				{
					// Note: can't turn off a DB; can only turn on another
					currDB = dbnum;
					selTable = dbChoices[currDB];
					if(!oldVal) dataChanged = true;
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}
	}
	var infoString : String = ("   " + numRows + " rows   " + numCols + " cols   ");
	if(numBins > 1) infoString += (numBins + " bins");
	else infoString += "1 bin";
	if((currDB >= 0) && !dataChanged) GUILayout.Label(infoString);
	GUI.DragWindow();
	if(dataChanged && (++waitCount > 4))
	{
		if(beServer) GetComponent.<NetworkView>().RPC("DatasetSelected", RPCMode.All, selTable, bConnectX, bExtendZ, bInterpolateY, topColorChoice, sideColorChoice, currGraphHeight);
		else DatasetSelected(selTable, bConnectX, bExtendZ, bInterpolateY, topColorChoice, sideColorChoice, currGraphHeight);
	}
}

function DoStyle(windowID : int) : void
{
	ShrinkIt(windowID);
	GUILayout.BeginHorizontal();
	
	GUILayout.BeginVertical();
	GUILayout.BeginHorizontal();
	
	GUILayout.BeginVertical(Const.grayStyle);
	bConnectX = GUILayout.Toggle(bConnectX, "Ribbon", Const.realToggle);
	bConnectX = !GUILayout.Toggle(!bConnectX, "Tile", Const.realToggle);
	GUILayout.EndVertical();
	
	GUILayout.BeginVertical();
	bExtendZ = GUILayout.Toggle(bExtendZ, "Fill", Const.realToggle);
	if(bHaveLabels) bShowLabels = GUILayout.Toggle(bShowLabels, "Show Labels", Const.realToggle);
	bInterpolateY = GUILayout.Toggle(bInterpolateY, "Interleave", Const.realToggle);
	GUILayout.EndVertical();
	
	GUILayout.EndHorizontal();
	if(drawn && !wantRedraw && GUILayout.Button("Draw", Const.buttonToggle)) wantRedraw = true;
	if(!drawn && !wantRedraw) GUILayout.Label("Draw", Const.grayButton);
	if(wantRedraw) GUILayout.Label("Drawing . . .", Const.greenCenterLabel);
	if(includeVRML && drawn && !wantVRML && GUILayout.Button("Write VRML", Const.buttonToggle)) wantVRML = true;
	if(wantVRML) GUILayout.Label("Writing VRML . . .", Const.greenCenterLabel);
	if(includeTriangles && drawn && !wantTriangles && GUILayout.Button("Write Triangles", Const.buttonToggle)) wantTriangles = true;
	if(wantTriangles) GUILayout.Label("Writing Triangles . . .", Const.greenCenterLabel);
	if(includeBalls)
	{
		if(GUILayout.Button("Drop Balls", Const.buttonToggle)) doDrop = true;
	}
	
	GUILayout.EndVertical();
	
	GUILayout.BeginVertical(Const.grayStyle);
	GUILayout.Label("Top Color", Const.littleCenterLabel);
	
	GUILayout.BeginHorizontal();
	UpdateColorStyleFromToggle(0, redgreenStyle, false);
	UpdateColorStyleFromToggle(1, rainbowStyle, false);
	UpdateColorStyleFromToggle(2, yellowblueStyle, false);
	UpdateColorStyleFromToggle(3, grayscaleStyle, false);
	UpdateColorStyleFromToggle(4, constantStyle, false);
	GUILayout.EndHorizontal();
	
	if(topColorChoice >= numFields) topColorChoice = 0;
	if(sideColorChoice >= numFields) sideColorChoice = 0;
	
	var choiceInd : int;
	
	for(choiceInd = 0; choiceInd < numFields; ++choiceInd)
	{
		if(GUILayout.Toggle(topColorChoice == choiceInd, allFields[choiceInd].fieldName, Const.realToggle)) topColorChoice = choiceInd;
	}
	
	GUILayout.EndVertical();
	
	GUILayout.BeginVertical(Const.grayStyle);
	GUILayout.Label("Side Color", Const.littleCenterLabel);
	
	GUILayout.BeginHorizontal();
	UpdateColorStyleFromToggle(0, redgreenStyle, true);
	UpdateColorStyleFromToggle(1, rainbowStyle, true);
	UpdateColorStyleFromToggle(2, yellowblueStyle, true);
	UpdateColorStyleFromToggle(3, grayscaleStyle, true);
	UpdateColorStyleFromToggle(4, constantStyle, true);
	GUILayout.EndHorizontal();
	
	for(choiceInd = 0; choiceInd < numFields; ++choiceInd)
	{
		if(GUILayout.Toggle(sideColorChoice == choiceInd, allFields[choiceInd].fieldName, Const.realToggle)) sideColorChoice = choiceInd;
	}
	GUILayout.EndVertical();
	GUILayout.EndHorizontal();
	
	GUI.DragWindow();
	if(wantRedraw && (++waitCount > 4))
	{
		if(beServer) GetComponent.<NetworkView>().RPC("DatasetSelected", RPCMode.All, selTable, bConnectX, bExtendZ, bInterpolateY, topColorChoice, sideColorChoice, currGraphHeight);
		else DatasetSelected(selTable, bConnectX, bExtendZ, bInterpolateY, topColorChoice, sideColorChoice, currGraphHeight);
	}
	if(wantVRML && (++waitCount > 4))
	{
		if(beServer) GetComponent.<NetworkView>().RPC("DrawVRML", RPCMode.All);
		else DrawVRML();
	}
	if(wantTriangles && (++waitCount > 4))
	{
		if(beServer) GetComponent.<NetworkView>().RPC("DrawTriangle", RPCMode.All);
		else DrawVRML();
	}
}

function DoSliders(windowID : int) : void
{
	var oldScrollChoice : int = scrollChoice;
	var oldGraphHeight : float = currGraphHeight;
	var oldFOV : float = currFOV;
	var oldThick : float = currThick;
	var oldSep : float = currSep;
	var oldGap : float = currGap;
	var oldBin : int = currBin;
	ShrinkIt(windowID);
	
	if(scrollAmount != 0.0)
	{
		currGraphHeight = UpdateFromScroll(choiceHeight, currGraphHeight, lowGraphHeightRange, highGraphHeightRange, 0.05);
		currFOV = UpdateFromScroll(choiceFOV, currFOV, lowFOVRange, highFOVRange, 0.05);
		currThick = UpdateFromScroll(choiceThick, currThick, lowThickRange, highThickRange, 0.05);
		currSep = UpdateFromScroll(choiceSep, currSep, lowSepRange, highSepRange, 0.05);
		currGap = UpdateFromScroll(choiceGap, currGap, lowGapRange, highGapRange, 0.05);
		currBin = UpdateIntFromScroll(choiceBin, currBin, numBins, true);
		scrollAmount = 0.0;
	}

	UpdateSliderFromToggle(choiceHeight, "height");
	currGraphHeight = GUILayout.HorizontalSlider(currGraphHeight, lowGraphHeightRange, highGraphHeightRange);
	UpdateSliderFromToggle(choiceFOV, "zoom");
	currFOV = GUILayout.HorizontalSlider(currFOV, lowFOVRange, highFOVRange);
	UpdateSliderFromToggle(choiceThick, "thickness");
	currThick = GUILayout.HorizontalSlider(currThick, lowThickRange, highThickRange);
	UpdateSliderFromToggle(choiceSep, "bin separation");
	currSep = GUILayout.HorizontalSlider(currSep, lowSepRange, highSepRange);
	UpdateSliderFromToggle(choiceGap, "row gap");
	currGap = GUILayout.HorizontalSlider(currGap, lowGapRange, highGapRange);
	UpdateSliderFromToggle(choiceBin, "scroll bins");

	GUI.DragWindow();
	if(oldScrollChoice != scrollChoice)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("ScrollingSelected", RPCMode.All, scrollChoice);
		else ScrollingSelected(scrollChoice);
		
		if(oldScrollChoice == choiceBin)
		{
			if(beServer) GetComponent.<NetworkView>().RPC("VisBins", RPCMode.All, -1);
			else VisBins(-1);
		}
		if(scrollChoice == choiceBin)
		{
			if(beServer) GetComponent.<NetworkView>().RPC("VisBins", RPCMode.All, currBin);
			else VisBins(currBin);
		}
	}
	
	if(oldGraphHeight != currGraphHeight)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("GraphHeightSelected", RPCMode.All, currGraphHeight);
		else GraphHeightSelected(currGraphHeight);
	}
	if(oldFOV != currFOV)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("FOVSelected", RPCMode.All, currFOV);
		else FOVSelected(currFOV);
	}
	
	if((currThick != oldThick) || (currSep != oldSep) || (currGap != oldGap))
	{
		if(beServer) GetComponent.<NetworkView>().RPC("Redistribute", RPCMode.All, currThick, currSep, currGap);
		else Redistribute(currThick, currSep, currGap);
	}
	
	if(currBin != oldBin)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("VisBins", RPCMode.All, currBin);
		else VisBins(currBin);
	}
	Const.menuScrolling = (scrollChoice >= 0);
}

function UpdateSliderFromToggle(choiceNumber : int, sliderName : String)
{
	var thisVal : boolean = GUILayout.Toggle((scrollChoice == choiceNumber), sliderName, scrollStyle);
	if((scrollChoice == choiceNumber) && !thisVal) scrollChoice = -1;
	if(thisVal) scrollChoice = choiceNumber;
}

function UpdateColorStyleFromToggle(choiceNumber : int, style : GUIStyle, bSide : boolean)
{
	var thisVal : boolean;
	if(bSide)
	{
		thisVal = GUILayout.Toggle((sideStyleChoice == choiceNumber), "", style);
		if((sideStyleChoice == choiceNumber) && !thisVal) sideStyleChoice = -1;
		if(thisVal) sideStyleChoice = choiceNumber;
	}
	else
	{
		thisVal = GUILayout.Toggle((topStyleChoice == choiceNumber), "", style);
		if((topStyleChoice == choiceNumber) && !thisVal) topStyleChoice = -1;
		if(thisVal) topStyleChoice = choiceNumber;
	}
}

function UpdateFromScroll(thisChoice : int, currValue : float, lowValue : float, highValue, frac : float) : float
{
	Debug.Log("UpdateFromScroll " + thisChoice + " " + scrollChoice + " " + scrollAmount + " " + currValue);
	if(scrollChoice != thisChoice) return currValue;
	var increment : float = (highValue - lowValue) * frac;
	if(scrollAmount < 0) currValue -= increment;
	if(scrollAmount > 0) currValue += increment;
	if(currValue < lowValue) currValue = lowValue;
	if(currValue > highValue) currValue = highValue;
	return currValue;
}

function UpdateIntFromScroll(thisChoice : int, currValue : int, numValues : int, doWrap : boolean)
{
	if(scrollChoice != thisChoice) return currValue;
	if(scrollAmount < 0) currValue--;
	if(scrollAmount > 0) currValue++;
	if(currValue < 0) currValue = doWrap ? numValues - 1 : 0;
	if(currValue >= numValues) currValue = doWrap ? 0 : numValues - 1;
	return currValue;
}

function DoZip(windowID : int) : void
{
	var zipChoice : int = -1;
	var lookChoice : int = -1;
	ShrinkIt(windowID);
	if(GUILayout.Button("Top", Const.buttonToggle)) zipChoice = 0;
	GUILayout.BeginHorizontal();
	if(GUILayout.Button("Left", Const.buttonToggle)) zipChoice = 3;
	if(GUILayout.Button("Right", Const.buttonToggle)) zipChoice = 4;
	GUILayout.EndHorizontal();
	GUILayout.BeginHorizontal();
	if(GUILayout.Button("Front", Const.buttonToggle)) zipChoice = 1;
	if(GUILayout.Button("Back", Const.buttonToggle)) zipChoice = 2;
	GUILayout.EndHorizontal();
	if(zipChoice >= 0)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("ZipSelected", RPCMode.All, zipChoice);
		else ZipSelected(zipChoice);
	}
	
	GUILayout.Label("Look", Const.littleCenterLabel);
	if(GUILayout.Button("Down", Const.buttonToggle)) lookChoice = 0;
	GUILayout.BeginHorizontal();
	if(GUILayout.Button("Left", Const.buttonToggle)) lookChoice = 3;
	if(GUILayout.Button("Right", Const.buttonToggle)) lookChoice = 4;
	GUILayout.EndHorizontal();
	GUILayout.BeginHorizontal();
	if(GUILayout.Button("Ahead", Const.buttonToggle)) lookChoice = 1;
	if(GUILayout.Button("Back", Const.buttonToggle)) lookChoice = 2;
	GUILayout.EndHorizontal();
	if(lookChoice >= 0)
	{
		if(beServer) GetComponent.<NetworkView>().RPC("LookSelected", RPCMode.All, lookChoice);
		else LookSelected(lookChoice);
	}
	GUI.DragWindow();
}

function ShrinkIt(windowID : int) : boolean
{
	var shrink : boolean;
	shrink = GUI.Toggle(Rect(2, 0, 20, 20), true, "");
	if(!shrink)
	{
		isWindowOpen[windowID] = false;
		windChanged[windowID] = true;
	}
	return shrink;
}

function DoPointedWindow(windowID : int) : void
{
	GUILayout.Label("x:" + pointedData.position.x);
	GUILayout.Label("y:" + pointedData.position.y);
	GUILayout.Label("z:" + pointedData.position.z);
	GUILayout.Label("row:" + pointedData.row);
	GUILayout.Label("col:" + pointedData.col);
	GUILayout.Label("bin:" + pointedData.bin);
	GUILayout.Label("height:" + pointedData.height);
	GUI.DragWindow();
}

function DoXRayWindow(windowID : int) : void
{
	GUILayout.Label(xray);
	if(GUILayout.Button("close")) showXRay = false;
	GUI.DragWindow();
}

function DoHelp(windowID : int) : void
{
	oldHelpPage = helpPage;
	GUILayout.Label("Press F1 or H to hide/view this window.  Press F5 or M to hide/view menus.", Const.greenCenterLabel);
	GUILayout.Label("Press ESC to exit program", Const.littleCenterLabel);
	GUILayout.BeginHorizontal();
	if(GUILayout.Toggle((helpPage == 0), "About", Const.bigToggle)) helpPage = 0;
	if(GUILayout.Toggle((helpPage == 1), "Data Preparation", Const.bigToggle)) helpPage = 1;
	if(GUILayout.Toggle((helpPage == 2), "Navigation", Const.bigToggle)) helpPage = 2;
	if(GUILayout.Toggle((helpPage == 3), "Menus", Const.bigToggle)) helpPage = 3;
	GUILayout.EndHorizontal();
	if(helpPage != oldHelpPage) menuScrollPos = Vector2(0.0, 0.0);
	switch(helpPage)
	{
		case 0:
			GUILayout.Label("3D Heatmap was developed by Dr. Jason H. Moore and the Bioinformatics Visualization Laboratory at Dartmouth Medical School with support from the Institute for Quantitative Biomedical Sciences and the Norris-Cotton Cancer Center and funding from NIH grants LM009012 and RR018787.");
			GUILayout.Label("The goal of this project is to evaluate the use of 3D video game engines for the interactive visual exploration of more than two dimensions of data.");
			GUILayout.Label("3D Heatmap is open source software released unter the GNU General Public License, Version 3.");
			GUILayout.Label("We hope to receive comments, suggestions and feature requests from users who have tried this program using their own data. (Press \"Data Preparation\" button for instructions on formatting your data) Please e-mail Jason.H.Moore@Dartmouth.edu to offer feedback.");
			GUILayout.Label("This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.");
			GUILayout.Label("Powered by the Unity 3D Game Engine", Const.littleCenterLabel);
		break;
		
		case 1:
			menuScrollPos = GUILayout.BeginScrollView(menuScrollPos, GUILayout.Width (780), GUILayout.Height (400));
			GUILayout.Label("Design", Const.fixLabel);
			GUILayout.Label("A conventional heat map consists of a 2D grid of colored squares where each square represents an observation of a random variable and the color of the square is proportional to the value of that observation. 2D heat maps are used pervasively in the Biological sciences and both the grid and the dimension mapped to the grid can represent a variety of concepts. Genes, experimental conditions, subjects, genomic elements, etc... are distributed on the grid where a color palette is used to encode transcript abundance, protein concentration, conservation, activation, etc...");
			GUILayout.Label("It is often desirable to map several dimensions to the same grid. This situation is usually resolved by plotting a separate 2D heat map for each dimension. The analysis of relationships between multiple dimensions is usually hindered by this design due to the loss of context and orientation when transitioning between dimensions in large data sets. It is our goal to explore alternative representations that superimpose and interleave several dimensions onto the same grid. Through this approach we aim to find a solution that decreases the disorienting effect of transitioning between dense and separately graphed volumes of data and to increase the interpretability of multidimensional data without overwhelming the user's senses.");
			GUILayout.Label("The current version includes the following features:");
			GUILayout.Label("- Updating graphical parameters in real time.");
			GUILayout.Label("The parameters that govern the graphing of data can be changed in real time. This allows for the seamless transition between dimensions without losing the current perspective and arrangement of the 3D heat map.");
			GUILayout.Label("- Superimposing dimensions.");
			GUILayout.Label("In order to map several dimensions onto the same grid we have chosen simple yet multifaceted geometries. The graphical unit can hold one dimension as its height, a second dimension as the color on its horizontal surface and a third dimension as the color of its vertical surfaces.");
			GUILayout.Label("- Interleaving dimensions.");
			GUILayout.Label("An alternative to superimposition that allows for an arbitrary number of dimensions to be mapped to the same grid is interleaving. This is achieved by consolidating the same row in the grid across all dimensions and plotting the consolidated rows adjacently. Spacer of different widths are used to convey the hierarchical structure of rows.");
			GUILayout.Label("All features can be explored in combination. It is possible to superimpose, interleave and switch between dimensions without interrupting the path of flight through the data or losing the point of view. It is important in this exercise that the user is able to start from a conventional 2D heat map and incrementally add dimensions as they elaborate and refine their analysis and interpretation. It is also up to the user to decide which variables are better represented by height or color.");
			GUILayout.Label("DATA IMPORT", Const.fixLabel);
			GUILayout.Label("See the README.txt for ways to import your data.");
			GUILayout.EndScrollView();
		break;
		
		case 2:
	#if UNITY_STANDALONE_WIN
			GUILayout.Label("We recommend a 3DConnexion SpaceNavigator for 3D navigation.  The following mouse and keyboard controls may also be used.");
	#endif
			GUILayout.Label("To look in a different direction, hold down the right mouse button (or Alt key, if that is more convenient) and move the mouse (or touchpad).");
			GUILayout.Label("You can also turn left and right by holding down a shift key and pressing left or right arrow keys.  Hold down a shift key and press up and down arrow keys to tilt vertically.");
			GUILayout.Label("If you become disoriented, use the \"Look\" menu to select one of 5 fixed orientations.");
			GUILayout.Label("To move forward, press up arrow or \"w\".  The longer you hold it down, the faster you will go, and your motion will continue after you release the key.");
			GUILayout.Label("To stop, press the left mouse button while your cursor is within the scene, not in a menu.");
			GUILayout.Label("You can careen wildly and rapidly about the scene using only the mouse, right button and up arrow.  Onlookers may experience motion discomfort.");
			GUILayout.Label("To modify your velocity in a more controlled way, the following keys add velocity relative to your orientation:");
			GUILayout.Label("left arrow or \"a\" = left   right arrow or \"d\" = right");
			GUILayout.Label("down arrow or \"s\" = backwards");
			GUILayout.Label("space bar + up or down arrow = up or down");
		break;
		
		case 3:
			menuScrollPos = GUILayout.BeginScrollView(menuScrollPos, GUILayout.Width (780), GUILayout.Height (400));
			GUILayout.Label("Toggle menus on and off by pressing F5.");
			GUILayout.Label("Data Selection", Const.fixLabel);
			GUILayout.Label("Clicking on a dataset causes it to be drawn immediately using the current chart style.");
			GUILayout.Label("If you prefer, choose chart style before selecting a dataset.");
			GUILayout.Label("Chart Style", Const.fixLabel);
			GUILayout.Label("Ribbon style draws a line from each point to the next along the row; the 3D equivalent of a line graph.  Selecting the \"fill\" option extends the ribbon down to the base.");
			GUILayout.Label("Tile style draws a horizontal tile for each point.  Selecting the \"fill\" option extends the tile down to the base, making the 3D equivalent of a bar chart.");
			GUILayout.Label("The Interleave option applies when the dataset has values in more than one bin.  If Interleave is on, the chart will show all the bins for one row before drawing the next row.  If it is off, all the rows for one bin will be shown before beginning the next bin.");
			GUILayout.Label("The color of the top and side of each point is driven by data values.  The data value can be height or bin number.  If your dataset has other integer fields, these will also appear as choices.  There are five color schemes for mapping data to colors, based on a linear interpolation between the lowest and highest values of the chosen data element.");
			GUILayout.Label("As described in Data Preparation, you can also provide a table of color assignments for each possible data value.  If this table is present, it will be used when no other color scheme is chosen.");
			GUILayout.Label("A Draw button appears on the Chart Style menu only if a dataset has been selected.  Changes chosen in the Chart Style menu only take effect when Draw is clicked or a new dataset is chosen in the Data Selection menu.");
			GUILayout.Label("Zip to Viewpoint", Const.fixLabel);
			GUILayout.Label("These buttons reposition and reorient the viewpoint so that your 3D chart can be viewd from fixed angles.  If you change the size of your chart using sliders in the Chart View menu, the Zip buttons will recalculate the positions needed to view your data.");
			GUILayout.Label("The Look submenu reorients the viewpoint without moving it.");
			GUILayout.Label("Chart View", Const.fixLabel);
			GUILayout.Label("There are 5 sliders for adjusting the view of your data without requiring a redraw.");
			GUILayout.Label("You may set a value by clicking on the slider bar or dragging the slider indicator.");
			GUILayout.Label("To the left of each slider is a stylized \"S\".  Clicking on this attaches that slider to the mouse scrollwheel, so it can be adjusted even when the menu is toggled off.");
			GUILayout.Label("Height - changes the height of the entire graph.");
			GUILayout.Label("Zoom - Like changing the focal length of a camera.  Zooming out (left) gives more feeling of being surrounded by your data, and provides smoother motion when turning.  Zooming in (and moving back) gives a more distant view, with less distortion at the edges of the view.  If you want to make screen shots of your chart, they make look better if you move the viewpoint back and zoom in to compensate.");
			GUILayout.Label("Thickness - the depth of each data element.  If there are many rows and few columns, you may wish to make the rows narrow to fit in a reasonable area.");
			GUILayout.Label("Bin Separation - the distance from the start of one bin to the start of the next.  Normally this is the size of one data point, so that when Interleave is chosen, each bin is next to the next.  Sliding this down to zero causes the bins to overlap.  If Interleave is off, you may want to choose a large value to give visible separation between bins.");
			GUILayout.Label("Row Gap - the distance between rows.  Defaults to the size of one data point.");
			GUILayout.Label("When the \"S\" on Scroll Bins is chosen, only one bin is visible at a time.  Moving the scroll wheel cycles through the bins.  By setting Interleave On, Bin Separation 0, and scrolling bins, you create a blink comparator for noticing small changes between bins.");
			GUILayout.EndScrollView();
		break;
		
	}
	GUI.DragWindow();
}

function GetDatabaseChoices()
{
	//numDB = ArrayFromQuery(dbChoices, "name", "from SQLITE_MASTER where name like 'heat_%' order by name;");
	numDB = ArrayFromQuery(dbChoices, "name", "from SQLITE_MASTER where like('heat!_%', name, '!') order by name;");
	if(currDB >= numDB) currDB = 0;
	//Debug.Log("currDB is " + currDB + ", size of dbChoices is " + dbChoices.length);
	selTable = (currDB >= 0) ? dbChoices[currDB] : "";
}

function GetAxisExtents()
{
	dbcmd.CommandText = "SELECT MIN(row), MAX(row), MIN(col), MAX(col), MIN(bin), MAX(bin), MIN(height), MAX(height) from " + selTable + " where col <= " + colLimit + ";";
	Debug.Log("GetAxisExtents() query is " + dbcmd.CommandText);
	reader = dbcmd.ExecuteReader();
	reader.Read();
	minRow = reader.GetInt32(0);
	maxRow = reader.GetInt32(1);
	minCol = reader.GetInt32(2);
	maxCol = reader.GetInt32(3);
	minBin = reader.GetInt32(4);
	maxBin = reader.GetInt32(5);
	minHeight = reader.GetFloat(6);
	maxHeight = reader.GetFloat(7);
	reader.Close();
	numRows = maxRow - minRow + 1;
	// debugging
	if(maxCol > colLimit) maxCol = colLimit;
	
	numCols = maxCol - minCol + 1;
	
	numBins = maxBin - minBin + 1;
	heightRange = maxHeight - minHeight;
}

function ArrayFromQuery(inarray : String[], field : String, fromClause : String) : int
{
	dbcmd.CommandText = "SELECT count(" + field + ") " + fromClause;
	//Debug.Log("Query is " + dbcmd.CommandText);
	var numvals = dbcmd.ExecuteScalar();
	//Debug.Log("numvals is " + numvals);
	//inarray = new String[numvals];
	dbcmd.CommandText = "SELECT " + field + " " + fromClause;
	//Debug.Log("Query is " + dbcmd.CommandText);
	reader = dbcmd.ExecuteReader();
	var thisChoice : int = 0;
	var astring : String;
	var anint : int;
	while(reader.Read())
	{
		try
		{
			astring = reader.GetString(0);
		}
		catch(err)
		{
			anint = reader.GetInt32	(0);
			astring = anint.ToString();
		}
		inarray[thisChoice++] = astring;
		//Debug.Log("Member " + (thisChoice - 1) + " is " + inarray[thisChoice - 1]);
	}
	reader.Close();
	return thisChoice;
}

function ScaleRidges(frac : float)
{
	var newSize : float = frac * zSceneSize;
	var i : int;
	for(i = 0; i < numRidges; ++i)
	{
		xRidges[i].NewHeight(newSize);
	}
}

function CalcDimensions()
{
	tokenWidth = (xSceneSize * Mathf.Pow(2, currThick)) / numCols;
	binIncrement = tokenWidth * currSep;
	if(bInterpolateY)
	{
		rowIncrement = (binIncrement * (numBins - 1)) + ((1.0 + currGap) * tokenWidth);
		plotIncrement = rowIncrement * numRows- currGap * tokenWidth;
		ySceneSize = plotIncrement;
		fullYSceneSize = ySceneSize;
		
		
	}
	else
	{
		rowIncrement = (1.0 + currGap) * tokenWidth;
		//plotIncrement = (rowIncrement * numRows) + (currSep * tokenWidth);
		//ySceneSize = rowIncrement * numRows - currGap;
		//fullYSceneSize = (plotIncrement * numBins) - (currSep * tokenWidth);
		ySceneSize = tokenWidth * (numRows + (numRows - 1) * currGap);
		plotIncrement = ySceneSize + binIncrement;
		fullYSceneSize = (ySceneSize * numBins) + ((numBins - 1) * binIncrement);
	}
	var cubeHeight : float = xSceneSize * 0.05;
//	baseCube.transform.localScale = Vector3(xSceneSize + 2.0, cubeHeight, fullYSceneSize + 2.0);
//	baseCube.transform.position = Vector3(xzySceneCorner.x - 1.0, xzySceneCorner.y - (cubeHeight * 0.9), xzySceneCorner.z - 1.0);
}

function ShowData()
{
	drawn = true;
	if(!proto) return;	// must be functioning only as user interface
	
	if(numRidges > 0) for(var iridge : int = 0; iridge < numRidges; ++iridge)
	{
		Destroy(xRidges[iridge].myMeshObject);
		Destroy(xRidges[iridge].myLabel);
	}
	numRidges = 0;
	
	CalcDimensions();

	xScale = 1.0 / numCols;
	zScale = 1.0 / (maxHeight - minHeight);
	
	xRidges = new XRidge[numRows * numBins];
	
	// In case we have changed datasets
	if(topColorChoice >= numFields) topColorChoice = 0;
	if(sideColorChoice >= numFields) sideColorChoice = 0;
	var extra1 : String = (topColorChoice > 1) ? (", " + allFields[topColorChoice].fieldName) : ", 0";
	var extra2 : String = (sideColorChoice > 1) ? (", " + allFields[sideColorChoice].fieldName) : ", 0";
				
	dbcmd.CommandText = "SELECT col, row, height, bin" + extra1 + extra2 + " from " + selTable + " where col <= " + colLimit + " order by bin, row, col;";
		
	Debug.Log("Query is " + dbcmd.CommandText);
	reader = dbcmd.ExecuteReader();
	var prow : int;
	var pbin : int;
	var col : int;
	var row : int;
	var abin : int;
	var hght : float;
	var top : int;
	var side : int;
	colVals = new int[numCols];
	heightVals = new float[numCols];
	topVals = new int[numCols];
	sideVals = new int[numCols];
	var recNum : int = 0;
	var xslot : int = 0;
	while(reader.Read())
	{
		col = reader.GetInt32(0);
		row = reader.GetInt32(1);
		hght = reader.GetFloat(2);
		abin = reader.GetInt32(3);
		top = reader.GetInt32(4);
		side = reader.GetInt32(5);
		if((recNum > 0) && (row != prow))
		{
			BuildRidge(prow, xslot, pbin - minBin);
			prow = row;
			xslot = 0;
		}
		if(xslot < numCols)
		{
			colVals[xslot] = col;
			heightVals[xslot] = hght;
			topVals[xslot] = top;
			sideVals[xslot] = side;
			++xslot;
		}
		prow = row;
		pbin = abin;
		++recNum;
	}
	reader.Close();
	BuildRidge(prow, xslot, pbin - minBin);
}

var doingEdges : boolean = true;
var bevelFraction : float = 0.05;

function BuildRidge(row : int, numx : int, binindex : int)
{
	var yoff : float = (row - minRow) * rowIncrement;
	if(bInterpolateY) yoff += (binindex * binIncrement);
	else yoff += (binindex * plotIncrement);
	
	var newRidge : GameObject = Instantiate(proto, Vector3(xzySceneCorner.x, xzySceneCorner.y, xzySceneCorner.z + yoff), Quaternion.identity);
	newRidge.transform.localScale = Vector3(xSceneSize, zSceneSize * currGraphHeight, tokenWidth);

	var amesh : Mesh = newRidge.gameObject.GetComponent(MeshFilter).mesh;
	xRidges[numRidges] = new XRidge();
	var idScript : IdentifyRidge = newRidge.gameObject.GetComponent(IdentifyRidge);
	idScript.row = row;
	idScript.bin = binindex + minBin;

	var newLabel : GameObject = Instantiate(protolabel, Vector3(xzySceneCorner.x + xSceneSize, xzySceneCorner.y + 1.0, xzySceneCorner.z + yoff + (tokenWidth * 0.1)), protolabel.transform.rotation);
	

	if((row > numRowLabels) || (rowLabels[row] == null))
	{
		newLabel.GetComponent(TextMesh).text = row.ToString();
	}
	else
	{
		newLabel.GetComponent(TextMesh).text = rowLabels[row];
	}

	/*
	// The following causes an error in setting the sharedMesh whenever it traps an error
	try{
		newLabel.GetComponent(TextMesh).text = rowLabels[row];
	}
	catch(fair)
	{
		newLabel.GetComponent(TextMesh).text = row.ToString();
	}
	*/
	
	newLabel.transform.localScale.x = tokenWidth * 0.5;
	newLabel.transform.localScale.y = tokenWidth * 0.5;
	newLabel.transform.localScale.x = tokenWidth * 0.5;
	var thisColor : Color;
	var sideColor : Color;
	var passColor : float  = 0.0;
	var thisX : float;
	var thisZ : float;
	var prevX : float;
	var prevZ : float;
	var nextX : float;
	var nextZ : float;
	var leftZ : float;
	var rightZ : float;
	var leftX : float;
	var rightX : float;
	var minZ : float = 0.1;
	var lastInd = numx - 1;
	var isFirst : boolean = true;
	var isLast : boolean = false;
	var slabZ : float = 0.006;
	var front : float;
	var back : float;
	var edgeBite : float = bevelFraction / numCols;
	// Note: this makes a 45 degree bevel at the curreent graph height, but it will be a different angle when height is changed.
	var topBite : float = edgeBite * xSceneSize / (zSceneSize * currGraphHeight);
	var mm : MeshMaker = new MeshMaker();
	
	for(var i : int = 0; i <= lastInd; ++i)
	{
		if(i % 2 == 0)
		{
			front = 0.0;
			back = 1.0;
		}
		else
		{
			front = 0.001;
			back = 1.001;
		}
		thisColor = MakeColor(i, binindex, false);
		sideColor = MakeColor(i, binindex, true);
		if(i > 0)
		{
			isFirst = false;
			prevX = thisX;
			prevZ = thisZ;
			thisX = nextX;
			thisZ = nextZ;
		}
		else
		{
			thisX = (colVals[0] + 0.5 - minCol) * xScale;
			thisZ = (heightVals[0] - minHeight) * zScale + minZ;
			prevX = thisX - xScale;
			prevZ = thisZ;
		}
		if(i < lastInd)
		{
			nextX = (colVals[i + 1] + 0.5 - minCol) * xScale;
			nextZ = (heightVals[i + 1] - minHeight) * zScale + minZ;
		}
		else
		{
			isLast = true;
			nextX += xScale;
		}
		
		leftZ = (prevZ + thisZ) / 2.0;
		leftX = (prevX + thisX) / 2.0;
		
		rightZ = (thisZ + nextZ) / 2.0;
		rightX = (thisX + nextX) / 2.0;
		
		mm.SetColor(thisColor);
		if(bConnectX)	// ribbon
		{
			mm.Verts(leftX, leftZ, front, 1, 0);
			mm.Verts(leftX, leftZ, back, 0, 0);
			mm.Verts(thisX, thisZ, front, 1, 1);
			mm.Verts(thisX, thisZ, back, 0, 1);
			mm.Verts(rightX, rightZ, front, 1, 0);
			mm.Verts(rightX, rightZ, back, 0, 0);
			mm.Tris(0, 1, 2,  2, 1, 3,  2, 3, 4,  5, 4, 3);
			
			// make bottom
			if(!bExtendZ)
			{		
				mm.Verts(leftX, leftZ - slabZ, front, 1, 0);
				mm.Verts(leftX, leftZ - slabZ, back, 0, 0);
				mm.Verts(thisX, thisZ - slabZ, front, 1, 1);
				mm.Verts(thisX, thisZ - slabZ, back, 0, 1);
				mm.Verts(rightX, rightZ - slabZ, front, 1, 0);
				mm.Verts(rightX, rightZ - slabZ, back, 0, 0);
				mm.Tris(2, 1, 0,  3, 1, 2,  4, 3, 2,  3, 4, 5);
			}
			
			
			// make sides
			mm.SetColor(sideColor);
			mm.Verts(leftX, leftZ, front, 0, 1);
			mm.Verts(leftX, leftZ, back, 1, 1);
			mm.Verts(leftX, bExtendZ ? 0.0 : leftZ - slabZ, front, 0, 0);
			mm.Verts(leftX, bExtendZ ? 0.0 : leftZ - slabZ, back, 1, 0);
			mm.Verts(thisX, thisZ, front, 0.5, 1);
			mm.Verts(thisX, thisZ, back, 0.5, 1);
			mm.Verts(thisX, bExtendZ ? 0.0 : thisZ - slabZ, front, 0.5, 0);
			mm.Verts(thisX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0.5, 0);
			mm.Verts(rightX, rightZ, front, 0, 1);
			mm.Verts(rightX, rightZ, back, 1, 1);
			mm.Verts(rightX, bExtendZ ? 0.0 : rightZ - slabZ, front, 0, 0);
			mm.Verts(rightX, bExtendZ ? 0.0 : rightZ - slabZ, back, 0, 0);
			
			mm.Tris(0, 4, 6,  0, 6, 2,  1, 7, 5,  1, 3, 7);
			mm.Tris(4, 10, 6,  4, 8, 10,  5, 7, 11,  5, 11, 9);
		}
		else	// tile
		{
			var edgeZ : float;
			if(doingEdges)
			{
				edgeZ = thisZ - topBite;
				// draw top
				mm.SetColor(thisColor);
				mm.Verts(leftX + edgeBite, thisZ, front, 1, 1);
				mm.Verts(leftX + edgeBite, thisZ, back, 0, 1);
				mm.Verts(rightX - edgeBite, thisZ, front, 1, 1);
				mm.Verts(rightX - edgeBite, thisZ, back, 0, 1);
				
				// draw bevel
				mm.Verts(leftX, edgeZ, front, 1, 0.9);
				mm.Verts(leftX, edgeZ, back, 0, 0.9);
				mm.Verts(rightX, edgeZ, front, 1, 0.9);
				mm.Verts(rightX, edgeZ, back, 0, 0.9);
				mm.Tris(0, 1, 2,  2, 1, 3);
				mm.Tris(4, 2, 6,  2, 4, 0,  6, 3, 7,  3, 6, 2);
				mm.Tris(7, 1, 5,  1, 7, 3,  5, 0, 4,  0, 5, 1);
				
				// draw bottom
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 1);
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 1);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 1);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 1);
				mm.Tris(2, 1, 0,  3, 1, 2);
				// draw sides
				mm.SetColor(sideColor);
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, front, 0, 0);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 0);
				mm.Verts(leftX, edgeZ, front, 0, 0.9);
				mm.Verts(rightX, edgeZ, front, 1, 0.9);
	
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, back, 1, 0);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 0);
				mm.Verts(leftX, edgeZ, back, 1, 0.9);
				mm.Verts(rightX, edgeZ, back, 0, 0.9);
				mm.Tris(0, 2, 1,  1, 2, 3);
				mm.Tris(4, 5, 6,  5, 7, 6,  0, 4, 2,  2, 4, 6);
				mm.Tris(1, 3, 5,  3, 7, 5);
			}
			else
			{
				edgeZ = thisZ;
				// draw top
				mm.SetColor(thisColor);
				
				mm.Verts(leftX, edgeZ, front, 1, 1);
				mm.Verts(leftX, edgeZ, back, 0, 1);
				mm.Verts(rightX, edgeZ, front, 1, 1);
				mm.Verts(rightX, edgeZ, back, 0, 1);
				mm.Tris(0, 1, 2,  2, 1, 3);
				
				// draw bottom
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 1);
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 1);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 1);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 1);
				mm.Tris(0, 1, 2,  2, 1, 3);
				
				// draw sides
				mm.SetColor(sideColor);
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, front, 0, 0);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, front, 1, 0);
				mm.Verts(leftX, edgeZ, front, 0, 1);
				mm.Verts(rightX, edgeZ, front, 1, 1);
	
				mm.Verts(leftX, bExtendZ ? 0.0 : thisZ - slabZ, back, 1, 0);
				mm.Verts(rightX, bExtendZ ? 0.0 : thisZ - slabZ, back, 0, 0);
				mm.Verts(leftX, edgeZ, back, 1, 1);
				mm.Verts(rightX, edgeZ, back, 0, 1);
				mm.Tris(0, 2, 1,  1, 2, 3);
				mm.Tris(4, 5, 6,  5, 7, 6,  0, 4, 2,  2, 4, 6);
				mm.Tris(1, 3, 5,  3, 7, 5);
			}
		}
	}
	
	mm.Attach(amesh);
	try
	{
		newRidge.transform.GetComponent.<Collider>().sharedMesh = amesh;
	}
	catch(err)
	{
		//Debug.Log("Failed to set collider mesh in dataset " + selTable + " row " + row + ", bin " + binindex);
	}
	
	
	xRidges[numRidges].AddRidge(newRidge, amesh, binindex, row);
	xRidges[numRidges++].AddLabel(newLabel);
}

function MakeUnitCube(ac : GameObject)
{
	var amesh : Mesh = ac.GetComponent(MeshFilter).mesh;
	var vertices : Vector3[] = new Vector3[8];
	var triangles : int[] = [0, 2, 1,  2, 0, 3,  1, 6, 5,  6, 1, 2,  4, 6, 7,  6, 4, 5,  4, 3, 0,  3, 4, 7,  4, 1, 5,  1, 4, 0,  3, 6, 2,  6, 3, 7];
	var uv : Vector2[] = [Vector2(0, 0), Vector2(1, 0), Vector2(1, 1), Vector2(0, 1), Vector2(1, 0), Vector2(0, 0), Vector2(0, 1), Vector2(1, 1)];
	vertices[0] = Vector3(0, 0, 0);
	vertices[1] = Vector3(1, 0, 0);
	vertices[2] = Vector3(1, 1, 0);
	vertices[3] = Vector3(0, 1, 0);
	vertices[4] = Vector3(0, 0, 1);
	vertices[5] = Vector3(1, 0, 1);
	vertices[6] = Vector3(1, 1, 1);
	vertices[7] = Vector3(0, 1, 1);
	amesh.Clear();
	amesh.vertices = vertices;
	amesh.uv = uv;
	amesh.triangles = triangles;
	amesh.RecalculateNormals();
	amesh.RecalculateBounds();
}

function MakeColor(col : int, bin : int, isSide : boolean)
{
	var inv : float;
	
	var colorChoice : int = (isSide) ? sideColorChoice : topColorChoice;
	var styleChoice : int = (isSide) ? sideStyleChoice : topStyleChoice;
	
	// side
	if(colorChoice == 0)
	{
		// height
		inv = (heightVals[col] - minHeight) / heightRange;
		//return GreenRed(inv, isSide);
	}
	else if(colorChoice == 1)
	{
		// bin
		if(numBins < 2) inv = 0.0;
		else inv = (bin - minBin + 0.0) / (numBins - 1);
	}
	else
	{
		var thisVal = isSide ? sideVals[col] : topVals[col];
		var thisField = isSide ? allFields[sideColorChoice] : allFields[topColorChoice];
		if((styleChoice < 0) && thisField.Fields.ContainsKey(thisVal))
		{
			var thisR : float = thisField.Fields[thisVal].r;
			var thisG : float = thisField.Fields[thisVal].g;
			var thisB : float = thisField.Fields[thisVal].b;
			return new Color(thisR, thisG, thisB, (isSide) ? 0.7 : 0.9);
		}
		// no specified color, so interpolate and use the default coloring method
		if(thisField.range < 0.5) inv = 0.0;	// really if it's zero, but this is a float so test for < 0.5
		else inv = (thisVal - thisField.lowInt + 0.0) / thisField.range;
	}
	var retColor : Color;
	switch(styleChoice)
	{
		case 0:
			retColor = GreenRed(inv, isSide);
		break;
		
		case 1:
			retColor = Rainbow(inv, isSide);
		break;
		
		case 2:
			retColor = YellowBlue(inv, isSide);
		break;
		
		case 3:
			retColor = GrayScale(inv, isSide);
		break;
		
		case 4:
			retColor = ConstantColor(inv, isSide);
		break;
		
		default:
			retColor = GreenRed(inv, isSide);
		break;
	}
	return retColor;
}

function GreenRed(inv : float, isSide : boolean) : Color
{
	var trans : float = (isSide) ? 0.7 : 0.9;
	var green : float;
	var red : float;
	if(inv > 0.5) green = 0.0;
	else green = 1.0 - (2.0 * inv);
	if(inv < 0.5) red = 0.0;
	else red = (inv - 0.5) * 2.0;
	return new Color(red, green, 0.0, trans);
}

function YellowBlue(inv : float, isSide : boolean) : Color
{
	var trans : float = (isSide) ? 0.7 : 0.9;
	return new Color(1.0 - inv, 1.0 - inv, inv, trans);
}

function Spectrum(inv : float, isSide : boolean) : Color
{
	var trans : float = (isSide) ? 0.7 : 0.9;
	if(inv < 0.25) return new Color(0.0, inv * 4.0, 1.0, trans);
	if(inv < 0.5) return new Color(0.0, 1.0, (0.5 - inv) * 2.0, trans);
	if(inv < 0.75) return new Color((inv - .5) * 4, 0.0, 0.0, trans);
	return new Color(1.0, (1.0 - inv) * 4.0, trans);
}

function Rainbow(inv : float, isSide : boolean) : Color
{
	var aColor = Colors.HSLtoColor(inv * 0.93, 1.0, 0.5);
	return new Color(aColor.r, aColor.g, aColor.b, (isSide) ? 0.7 : 0.9);
}

function GrayScale(inv : float, isSide : boolean) : Color
{
	return new Color(inv, inv, inv, (isSide) ? 0.7 : 0.9);
}

function ConstantColor(inv : float, isSide : boolean) : Color
{
	return new Color(0.5, 0.5, 0.5, (isSide) ? 0.7 : 0.9);
}

static var hit : RaycastHit;
static var dataPointMask : int = (1 << 8);
function CapturePointedAt()
{
		pointedData.ready = false;
		var ray : Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		ray.origin = Camera.main.transform.position;
		if(!Physics.Raycast(ray, hit, Mathf.Infinity, dataPointMask)) return;
		
		pointedData.position = hit.point;
		var idScript : IdentifyRidge = hit.transform.gameObject.GetComponent(IdentifyRidge);
		pointedData.row = idScript.row;
		pointedData.bin = idScript.bin;
		// Calculate column from x position of hit
		// It is possible for this to be one too high if it hits on the right side of a column, so check for this condition
		var floatCol : float = ((hit.point.x - xzySceneCorner.x) * numCols / xSceneSize) + minCol;
		if(((floatCol - Mathf.Floor(floatCol)) < 0.1) && (hit.normal.x > 0.95)) floatCol -= 1.0;
		pointedData.col = Mathf.Floor(floatCol);
		
		dbcmd.CommandText = "Select height from " + selTable + " where row = " + pointedData.row + " and col = " + pointedData.col + " and bin = " + pointedData.bin + ";";
		Debug.Log(dbcmd.CommandText);
		reader = dbcmd.ExecuteReader();
		if(reader.Read())
		{
			pointedData.height = reader.GetFloat(0);
			pointedData.ready = true;
		}
		reader.Close();
		ShowPointedData();
}

function ShowPointedData()
{
	if(pointedData.ready)
	{
		var sceney : float = (pointedData.height - minHeight) * zScale * zSceneSize * currGraphHeight + xzySceneCorner.y;
		var scenex : float = ((pointedData.col + 0.5 - minCol) * xSceneSize / numCols) + xzySceneCorner.x;
		var yoff : float = pointedData.row * rowIncrement;
		if(bInterpolateY) yoff += (pointedData.bin * binIncrement);
		else yoff += (pointedData.bin * plotIncrement);
		var scenez : float = xzySceneCorner.z + yoff + (tokenWidth / 2.0);
		signPost.transform.position = Vector3(scenex, sceney, scenez);
		var signSize : float = Mathf.Min(tokenWidth, xSceneSize / numCols);
		signPost.transform.localScale = Vector3(signSize, signSize, signSize);
		signPost.GetComponent.<Renderer>().enabled = true;
	}
	else signPost.GetComponent.<Renderer>().enabled = false;
}

// debugging
function MakeXRay()
{
	var maxx : int = Screen.width;
	var maxy : int = Screen.height;
	var x : int;
	var y : int;
	var backColor : Color = new Color(0, 0, 0);
	var foreColor : Color = new Color(1, 1, 1);
	var pColor : Color;
	for(y = 0; y < maxy; y += 2)
	{
		for(x = 0; x < maxx; x += 2)
		{
			var ray : Ray = Camera.main.ScreenPointToRay(Vector3(x , y, 0));
			ray.origin = Camera.main.transform.position;
			if(!Physics.Raycast(ray, hit, Mathf.Infinity, dataPointMask)) pColor = backColor;
			else
			{
				pColor = foreColor;
			}
			xray.SetPixel(x / 2, y / 2, pColor);
		}
	}
	xray.Apply();
	showXRay = true;
}

@RPC
function ScrollingSelected(newScroll : int)
{
	scrollChoice = newScroll;
	if(scrollChoice >= 0)
	{
		Const.menuScrolling = true;
		//VisBins(currBin);
	}
	else
	{
		Const.menuScrolling = false;
		//VisBins(-1);
	}
}

@RPC
function Redistribute(newThick : float, newSep : float, newGap : float)
{
	currThick = newThick;
	currSep = newSep;
	currGap = newGap;
	CalcDimensions();
	var i : int;
	var yoff : float;
	var thisz : float;
	for(i = 0; i < numRidges; ++i)
	{
		yoff = (xRidges[i].myRow - minRow) * rowIncrement;
		if(bInterpolateY) yoff += (xRidges[i].myBin * binIncrement);
		else yoff += (xRidges[i].myBin * plotIncrement);
		
		thisz = xzySceneCorner.z + yoff;
		xRidges[i].trans.position.z = thisz;
		xRidges[i].trans.localScale.z = tokenWidth;
		xRidges[i].myLabel.transform.position.z = xzySceneCorner.z + yoff + (tokenWidth * 0.1);
		xRidges[i].myLabel.transform.localScale.x = tokenWidth * 0.5;
		xRidges[i].myLabel.transform.localScale.y = tokenWidth * 0.5;
		xRidges[i].myLabel.transform.localScale.z = tokenWidth * 0.5;
	}
	var cubeHeight : float = xSceneSize * 0.05;
	//baseCube.transform.localScale = Vector3(xSceneSize + 2.0, cubeHeight, fullYSceneSize + 2.0);
	//baseCube.transform.position = Vector3(xzySceneCorner.x - 1.0, xzySceneCorner.y - (cubeHeight * 0.9), xzySceneCorner.z - 1.0);
	ShowPointedData();
}

// if selBin < 0, all bins visible
@RPC
function VisBins(selBin : int)
{
	if(selBin >= 0) currBin = selBin;
	var i : int;
	if(selBin >= 0)
	{
		for(i = 0; i < numRidges; ++i)
		{
			xRidges[i].Show(xRidges[i].myBin == selBin);
		}
	}
	else
	{
		for(i = 0; i < numRidges; ++i)
		{
			xRidges[i].Show(true);
		}
	}
}

@RPC
function DatasetSelected(newDB : String, newBConnectX : boolean, newBExtendZ : boolean, newBInterpolateY : boolean, newTopColorChoice : int, newSideColorChoice : int, newGraphHeight : float)
{
	//Debug.Log("DatasetSelected " + newDB);
	selTable = newDB;
	bConnectX = newBConnectX;
	bExtendZ = newBExtendZ;
	bInterpolateY = newBInterpolateY;
	topColorChoice = newTopColorChoice;
	sideColorChoice = newSideColorChoice;
	currGraphHeight = newGraphHeight;
	var i : int;
	for(i = 0; i < numDB; ++i)
	{
		if(dbChoices[i] == newDB)
		{
			currDB = i;
		}
	}
	GetAxisExtents();
	
	// Find the fields in this database
	dbcmd.CommandText = "PRAGMA table_info(" + selTable + ");";
	var nameArray = new Array();
	reader = dbcmd.ExecuteReader();
	while(reader.Read())
	{
		var fname : String = reader.GetString(1);
		var fieldtype : String = reader.GetString(2);
		if(fieldtype != "INTEGER") continue;
		if(fname == "row") continue;
		if(fname == "col") continue;
		if(fname == "bin") continue;
		nameArray.push(fname);
	}
	reader.Close();
	
	var thisField : int;
	for(thisField = 0; thisField < numFields; ++thisField)
	{
		allFields[thisField] = new FieldData(); // just to destroy old values
	}
	
	allFields = new FieldData[2 + nameArray.length];
	
	allFields[0] = new FieldData();
	allFields[0].SetFloat("height", minHeight, maxHeight);
	
	allFields[1] = new FieldData();
	allFields[1].SetInt("bin", minBin, maxBin);
	
	numFields = 2;
	for(var fieldname in nameArray)
	{
		dbcmd.CommandText = "SELECT MIN(" + fieldname + "), MAX(" + fieldname + ") from " + selTable + ";";
		//Debug.Log("trying " + dbcmd.CommandText);
		reader = dbcmd.ExecuteReader();
		reader.Read();
		var intField : FieldData = new FieldData();
		intField.SetInt(fieldname, reader.GetInt32(0), reader.GetInt32(1));
		reader.Close();
		var infoTable : String = "heatfield_" + selTable.Substring(5) + "_" + fieldname;
		try
		{
			dbcmd.CommandText = "SELECT value, r, g, b, name from " + infoTable + ";";
			//Debug.Log(dbcmd.CommandText);
			reader = dbcmd.ExecuteReader();
			while(reader.Read())
			{
				var oneF = new OneField();
				oneF.r = reader.GetInt32(1) / 255.0;
				oneF.g = reader.GetInt32(2) / 255.0;
				oneF.b = reader.GetInt32(3) / 255.0;
				oneF.name = reader.GetString(4);
				var value : int = reader.GetInt32(0);
				intField.Fields[value] = oneF;
				//Debug.Log("setting Fields of " + value + " to " + oneF.r + " " + oneF.g + " " + oneF.b + " " + oneF.name);
			}
			reader.Close();
		}
		catch(err)
		{
		}
		allFields[numFields++] = intField;
	}
	
	var rowtable : String = "heatrows_" + selTable.Substring(5);
	dbcmd.CommandText = "SELECT count(*) from " + rowtable + ";";
	try
	{
		numRowLabels = dbcmd.ExecuteScalar();
		dbcmd.CommandText = "SELECT row, name from " + rowtable + ";";
		rowLabels = new String[numRowLabels + 1];
		reader = dbcmd.ExecuteReader();
		while(reader.Read())
		{
			rowLabels[reader.GetInt32(0)] = reader.GetString(1);
		}
		reader.Close();
	}
	catch(err)
	{
		numRowLabels = 0;
		rowLabels = new String[1];
	}
	ShowData();
	if(bScrollBin) VisBins(currBin);
	dataChanged = false;
	wantRedraw = false;
	waitCount = 0;
	pointedData.ready = false;
	ShowPointedData();
}

@RPC
function GraphHeightSelected(newGraphHeight : float)
{
	currGraphHeight = newGraphHeight;
	ScaleRidges(currGraphHeight);
	ShowPointedData();
}

@RPC
function FOVSelected(newFOV : float)
{
	currFOV = newFOV;
	myCamera.fieldOfView = lowFOVRange + (highFOVRange - currFOV);
}

@RPC
function LookSelected(newLook : int)
{
	switch(newLook)
	{
		case 0:	// Down
			Fly.NewRotation(0.0, -90.0);
		break;
		
		case 1:	// Forward
			Fly.NewRotation(0.0, 0.0);
		break;
		
		case 2:	// Back
			Fly.NewRotation(180.0, 0.0);
		break;
		
		case 3:	// Left
			Fly.NewRotation(-90.0, 0.0);
		break;
		
		case 4:	// Right
			Fly.NewRotation(90.0, 0.0);
		break;
		
	}
}

@RPC
function ZipSelected(newZip : int)
{
	var myX : float;
	var myY : float;
	var myZ : float;
	var hFOV : float;
	
	hFOV = Mathf.Atan(Screen.width * Mathf.Tan(myCamera.fieldOfView * Mathf.PI / 360.0) / Screen.height);
	
	switch(newZip)
	{
		case 0:	// top
			myY = xzySceneCorner.z + (ySceneSize / 2.0);
			myX = xzySceneCorner.x + (xSceneSize / 2.0);
			//Debug.Log("xzySceneCorner.y is " + xzySceneCorner.y);
			//Debug.Log("(zSceneSize * currGraphHeight) is " + (zSceneSize * currGraphHeight));
			//Debug.Log("myCamera.fieldOfView is " + myCamera.fieldOfView);
			//Debug.Log("Mathf.Tan(myCamera.fieldOfView / 2.0) is " + Mathf.Tan(myCamera.fieldOfView * Mathf.PI / 360.0));
			myZ = xzySceneCorner.y + (zSceneSize * currGraphHeight) + (ySceneSize / 2.0) / Mathf.Tan(myCamera.fieldOfView * Mathf.PI / 360.0);
			//Debug.Log("myZ is " + myZ);
			Fly.NewRotation(0.0, -90.0);
		break;
		
		case 1:	// front
			myX = xzySceneCorner.x + (xSceneSize / 2.0);
			myZ = xzySceneCorner.y + (zSceneSize * currGraphHeight / 2.0);
			myY = xzySceneCorner.z - ((xSceneSize / 2.0) / Mathf.Tan(hFOV));
			Fly.NewRotation(0.0, 0.0);
		break;
		
		case 2:	// back
			myX = xzySceneCorner.x + (xSceneSize / 2.0);
			myZ = xzySceneCorner.y + (zSceneSize * currGraphHeight / 2.0);
			myY = xzySceneCorner.z + ySceneSize + ((xSceneSize / 2.0) / Mathf.Tan(hFOV));
			if((numBins > 1) && !bInterpolateY) myY += (ySceneSize * (numBins - 1));
			Fly.NewRotation(180.0, 0.0);
		break;
		
		case 3:	// left
			myY = xzySceneCorner.z + (ySceneSize / 2.0);
			myZ = xzySceneCorner.y + (zSceneSize * currGraphHeight / 2.0);
			myX = xzySceneCorner.x - ((ySceneSize / 2.0) / Mathf.Tan(hFOV));
			Fly.NewRotation(90.0, 0.0);
		break;
		
		case 4:	// right
			myY = xzySceneCorner.z + (ySceneSize / 2.0);
			myZ = xzySceneCorner.y + (zSceneSize * currGraphHeight / 2.0);
			myX = xzySceneCorner.x + xSceneSize + ((ySceneSize / 2.0) / Mathf.Tan(hFOV));
			Fly.NewRotation(-90.0, 0.0);
		break;
	}
	myController.transform.position = Vector3(myX, myZ, myY);
}

private var vrout : StreamWriter;
private var vrmlScale : float;
@RPC
function DrawVRML()
{
	vrmlScale = vrmlModelMM / ((fullYSceneSize > xSceneSize) ? fullYSceneSize : xSceneSize);
	vrout = File.CreateText("heat.wrl");
	vrout.WriteLine("#VRML V2.0 utf8");
	vrout.WriteLine("Group { children [");
	var r : int;
	for(r = 0; r < numRidges; ++r)
	{
		//Debug.Log("Ridge " + r);
		WriteVRMLMesh(xRidges[r].myMesh, xRidges[r].trans, false);
	}
	//WriteVRMLMesh(baseCube.GetComponent(MeshFilter).mesh, baseCube.transform, true);
	vrout.WriteLine("]}"); // close children and Group
	vrout.Close();
	wantVRML = false;
	waitCount = 0;
}

private var trout : StreamWriter;
@RPC
function DrawTriangle()
{
	Debug.Log("in DrawTriangle");
	vrmlScale = vrmlModelMM / ((fullYSceneSize > xSceneSize) ? fullYSceneSize : xSceneSize);
	trout = File.CreateText("triangles.txt");
	var r : int;
	for(r = 0; r < numRidges; ++r)
	{
		//Debug.Log("Ridge " + r);
		WriteTriangleMesh(xRidges[r].myMesh, xRidges[r].trans, false);
	}
	trout.Close();
	wantTriangles = false;
	waitCount = 0;
}

function WriteTriangleMesh(amesh : Mesh, trans : Transform, makeColors : boolean)
{
	var colors : Color[] = amesh.colors;
	var vertices : Vector3[] = amesh.vertices;
	var triangles : int[] = amesh.triangles;
	var numVerts = vertices.length;
	var numTris = triangles.length;
	var thisTri : int = 0;
	var cstring : String;
	for(thisTri = 0; thisTri < numTris; )
	{
		var thisVertex : int = triangles[thisTri];
		var thisColor : Color = colors[thisVertex];
		cstring = (thisColor.r + " " + thisColor.g + " " + thisColor.b + " ");
		for(var corner : int = 0; corner < 3; ++corner)
		{
			if(corner > 0) cstring += " ";
			thisVertex = triangles[thisTri++];
			var apos : Vector3 = trans.TransformPoint(vertices[thisVertex]);
			apos -= Vector3(xzySceneCorner.x, xzySceneCorner.y, xzySceneCorner.z);
			apos *= vrmlScale;
			cstring += (apos.x + " " + apos.y + " " + apos.z);
		}
		trout.WriteLine(cstring);
	}
}

function WriteVRMLMesh(amesh : Mesh, trans : Transform, makeColors : boolean)
{
		vrout.WriteLine("Transform {");
		vrout.WriteLine("children Shape {");
		vrout.WriteLine("geometry IndexedFaceSet {");
		var colors : Color[] = amesh.colors;
		var vertices : Vector3[] = amesh.vertices;
		var triangles : int[] = amesh.triangles;
		var numVerts = vertices.length;
		var numTris = triangles.length;
		//numVerts = 6;
		//numTris = 18;
		vrout.WriteLine("coord Coordinate {");
		var cstring : String = "point [";
		var avert : int;
		for(avert = 0; avert < numVerts; ++avert)
		{
			var apos : Vector3 = trans.TransformPoint(vertices[avert]);
			apos -= Vector3(xzySceneCorner.x, xzySceneCorner.y, xzySceneCorner.z);
			apos *= vrmlScale;
			if(avert > 0) cstring += ", ";
			cstring += (apos.x + " " + apos.y + " " + apos.z);
		}
		cstring += "]";
		vrout.WriteLine(cstring);
		vrout.WriteLine("}");	// closes Coordinate
		
		cstring = "coordIndex [";
		var thisp : int;
		for(thisp = 0; thisp < numTris;)
		{
			cstring += (" " + triangles[thisp++]);
			cstring += (" " + triangles[thisp++]);
			cstring += (" " + triangles[thisp++]);
			cstring += " -1";
		}
		cstring += "]";
		vrout.WriteLine(cstring);
		
		cstring = "color Color { color [";
		//Debug.Log("Number of vertices " + numVerts);
		for(avert = 0; avert < numVerts; ++avert)
		{
			//Debug.Log("vertex " + avert);
			if(avert > 0) cstring += ", ";
			if(makeColors)
			{
				//cstring += "0.5 0.5 0.5, 0.5 0.5 0.5, 0.5 0.5 0.5";
				cstring += "0.5 0.5 0.5";
			}
			else
			{
				/*
				cstring += (colors[avert].r + " ");
				cstring += (colors[avert].g + " ");
				cstring += (colors[avert++].b + ", ");
				cstring += (colors[avert].r + " ");
				cstring += (colors[avert].g + " ");
				cstring += (colors[avert++].b + ", ");
				*/
				cstring += (colors[avert].r + " ");
				cstring += (colors[avert].g + " ");
				cstring += colors[avert].b;
			}
		}
		cstring += ", 0 0 0";
		cstring += " ]}";
		vrout.WriteLine(cstring);
		
		vrout.WriteLine("}");	// closes IndexedFaceSet
		vrout.WriteLine("}");	// closes Shape
		vrout.WriteLine("}");	// closes Transform
}