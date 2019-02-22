using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Main class. Has lots of state vars crammed in it. Plan is to break it up over time.
/// As a MonoBehaviorSingleton, there's a single global instance that is accessible via HeatVRML.Instance
/// </summary>
[System.Serializable]
public class HeatVRML : MonoBehaviorSingleton<HeatVRML>
{
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
    public string programVersion;

    //Stauffer - Data
    //See DataManager singleton - new model of self-contained data objects
    
    // window numbers
    private int DSwin = 0;
    private int STYLEwin;
    private int ZIPwin;
    private int SLwin;
    private int NUMwin;
    private bool[] isWindowOpen;
    private Rect[] windowRects;
    private System.Action[] sizeRect;
    private bool[] windChanged;
    private GUI.WindowFunction[] doWind;
    private GUI.WindowFunction closefunc;
    private bool[] isLayout;
    private bool allHidden;

    // window extents
    public Rect dsRect;
    public Rect styleRect;
    public Rect zipRect;
    public Rect slidersRect;
    public Rect pointedWindowRect;
    public Rect xrayWindowRect;
    public Rect helpWindowRect;
    public string[] windNames;

    //Stauffer - Textures - change these from Texture to Texture2D to avoid implicit downcast when assigned to GUIStyle later. 
    //These get assigned in the 'prefab objectify' GameObject in the editor Hierarchy for the scene. Seems like this HeatVRML script
    // was added to the instance of 'prefab objectify' that's in the scene, cuz the version in project Prefabs folder doesn't have it.
    //
    // Input related
    private int waitCount;
    public Texture2D scrollOnLowTexture;
    public Texture2D scrollOffLowTexture;
    public Texture2D scrollOnHighTexture;
    public Texture2D scrollOffHighTexture;
    public GUIStyle scrollStyle;
    public GUIStyle grayscaleStyle;
    public Texture2D grayscaleOnTexture;
    public Texture2D grayscaleOffTexture;
    public GUIStyle constantStyle;
    public Texture2D constantOnTexture;
    public Texture2D constantOffTexture;
    public GUIStyle rainbowStyle;
    public Texture2D rainbowOnTexture;
    public Texture2D rainbowOffTexture;
    public GUIStyle redgreenStyle;
    public Texture2D redgreenOnTexture;
    public Texture2D redgreenOffTexture;
    public GUIStyle yellowblueStyle;
    public Texture2D yellowblueOnTexture;
    public Texture2D yellowblueOffTexture;
    private bool initDone;
    private float scrollAmount;

    public float junk;

    // scene related
    //Stauffer - plotting area width in scene units. Row width. Seems hardcoded
    public float xSceneSize;
    /// <summary> Stauffer - full plotting area DEPTH in scene units, including possible multiple bins and bin increment
    /// (Named with "Y" and not "Z" since coder swapped y and z in variable naming). Gets updated when data is redrawn. </summary>
    private float ySceneSizeFull;
    /// <summary> Stauffer - added this. Max size of full plot in Y scene units of (i.e. Unity Z) dim, i.e. depth when viewed from default camera positiona long unity z axis.
    /// Added this to constrain row depth when data has many more rows than columns. </summary>
    private float ySceneSizeApproxMax;
    /// <summary> Stauffer - plotting area DEPTH in scene units, for single bin (or for all bins when interleaved) </summary>
    public float ySceneSizeByBin;
    /// <summary> Stauffer - plotting area HEIGHT in scene units. Gets updated when data is redrawn. </summary>
    public float zSceneSize;
    /// <summary> Stauffer - starting corner of plot area in scene units. The left/front, 1st row/1st column. </summary>
    public Vector3 xzySceneCorner;
    /// <summary> Stauffer - separtion in scene units between bins, whether bins are interleaved or not. So if not interleaved,
    /// it's separation between groups of rows of each bin. If interleaved, this is separation between each row (different than rowGap, however). </summary>
    public float binSeparation;
    /// <summary> Stauffer - full scene depth of each row, including gap beetween rows (and bin separation when interleaved) </summary>
    public float rowDepthFull;
    /// <summary> Stauffer - plotting area DEPTH in scene units, for single bin (or for all bins when interleaved) INCLUDING gap between bin groups </summary>
    public float ySceneSizeByBinWithSep;

    // chart related
    /// <summary> Stauffer - as best I can tell, this flag controls interleaving of bins within the plot. i.e. if each bin is shown as separate group of rows, or interleaved by row </summary>
    public bool binInterleave;
    private bool bConnectX; //Draw ribbon. Flag
    private bool bExtendZ;
    private bool bHaveLabels = false; //Stauffer - set default val to suppress warning
    private bool bShowLabels;
    private int topColorChoice;
    private int sideColorChoice;
    private GameObject proto;
    //private GameObject baseCube; Stauffer - seems unused
    private GameObject protolabel;
    private bool drawn;
    private XRidge[] xRidges;
    //private int shaveCount; Stauffer - never used
    private int numRidges;
    /// <summary> Stauffer added. Minimum height of mesh used to make a ridge. </summary>
    private float ridgeMeshMinHeight;
    /// <summary> Depth (unity z-dimension) of data representation, i.e. the depth of the tower representing the data point. Does NOT include gap between rows. </summary>
    public float rowDepthDataOnly;
    private float xScale;
    private int minRow;
    private int maxRow;
    public int minCol;
    private int maxCol;
    private int minBin;
    private int maxBin;
    /// <summary> the minimum value of the data assigned to height </summary>
    public float minDataHeight;
    /// <summary> the max value of the data assigned to height </summary>
    public float maxDataHeight;
    /// <summary> the range in data variable assigned to height </summary>
    public float dataHeightRange;
    /// <summary> 1 over (maxDataHeight - minDataHeight) </summary>
    public float dataHeightRangeScale;
    private int numRows;
    public int numCols;
    private int numBins;
    private int minMarker;
    private int maxMarker;
    private int numMarkers;
    private int currBin;
    private bool bScrollBin = false; //Stauffer - compiler says this val never changes from default of false, so set it to false explicitly
    private float lastScrollTime; //Stauffer mod
    public float minScrollSecs; // minimum time between changes of scrolling value
    private int choiceHeight = 0; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    private int choiceFOV;
    private int choiceThick;
    private int choiceSep;
    private int choiceGap;
    private int choiceBin;
    private int sideStyleChoice;
    private int topStyleChoice;
    private int scrollChoice;
    private float lowGraphHeightRange;
    private float highGraphHeightRange;
    /// <summary> Scaling of column/ridge height </summary>
    public float currGraphHeight;
    private float lowFOVRange;
    private float highFOVRange;
    private float currFOV;
    /// <summary> Stauffer - scaling factor for row depth/width - gets used as 2^x for some reason </summary>
    private float currDepthToWidthRatioExp;
    private float lowDepthToWidthRatioRange;
    private float highDepthToWidthRatioRange;
    /// <summary> Stauffer - fractional amount of data row scene depth to apply to determining bin separation. </summary>
    private float binSeparationFrac;
    private float lowSepRange = 0f; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    private float highSepRange =0f; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    /// <summary> Stauffer - fractional value applied to rowDepthDataOnly to calculate gap between rows. Separate from binSeparation </summary>
    private float rowGapFrac;
    private float lowGapRange = 0f; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    private float highGapRange;
    private PointedData pointedData;
    private GameObject signPost;

    // Database related
    private Vector2 dbPos;
    private int numDB;
    private int currDB;
    private string[] dbChoices;
    private string selTable;
    private int isDetecting;
    private string[] rowLabels;
    private int numRowLabels;
    private string connStrn;
    //Stauffer - removing Sqlite stuff to work on webGL build
    //private SqliteDataReader reader;
    //private SqliteConnection connection;
    //private SqliteCommand dbcmd;
    private bool dataChanged;
    private bool wantRedraw;
    private bool wantVRML;
    private bool wantTriangles;
    private int numFields;
    /// <summary> array of descriptions of all the variables (originally called Fields) being visualized </summary>
    private VariableDesc[] allVariableDescs;

    // data values for the current row
    private float[] heightVals;
    private int[] topVals;
    private int[] sideVals;
    // Stauffer. Seems to be an array of column numbers, used in BuildRidgeOld. Gets shifted to start at 0 anyway, so not sure what the purpose is. </summary>
    // Not using in new code.
    private int[] colVals;

    // feature inclusion
    public bool includeVRML;
    public bool includeBalls;
    public bool includeTriangles;

    // Layout
    public int pixPerLine;
    public int rowHeight;
    public int colWidth;
    public int lineWidth;

    // Specific windows
    public int scrollHeight;

    // Help window
    private bool showHelp;
    private int helpPage;
    private int oldHelpPage;
    private Vector2 menuScrollPos;
    private int helpCount;

    // Components
    public Camera myCameraOld; //Stauffer - add declare as type Camera
    public GameObject myController;

    // VRML
    //private File fileVRML; //Stauffer - removing this. Not used anywhere, and giving c# compilation error
    public float vrmlModelMM;

    // Ball dropping
    private bool doDrop;

    // Debugging
    private Texture2D xray;
    private bool showXRay;
    private int colLimit;

    //Use this instead of Awake since this is a MonoBehaviorSingleton
    //void Awake()
    protected override void Initialize()
    {

    }

    //
    public virtual void Start()
    {

        int i = 0;
        this.lastScrollTime = Time.time; //Stauffer move init here
        this.isWindowOpen = new bool[this.NUMwin];
        this.windowRects = new Rect[this.NUMwin];
        this.windowRects[this.DSwin] = this.dsRect;
        this.windowRects[this.STYLEwin] = this.styleRect;
        this.windowRects[this.ZIPwin] = this.zipRect;
        this.windowRects[this.SLwin] = this.slidersRect;
        this.sizeRect = new System.Action[this.NUMwin];
        this.sizeRect[this.DSwin] = (System.Action)this.SizeDS;
        this.sizeRect[this.STYLEwin] = (System.Action)this.SizeStyle;
        this.sizeRect[this.ZIPwin] = (System.Action)this.SizeZip;
        this.sizeRect[this.SLwin] = (System.Action)this.SizeSliders;
        this.doWind = new GUI.WindowFunction[this.NUMwin];
        this.doWind[this.DSwin] = this.DoDS;
        this.doWind[this.STYLEwin] = this.DoStyle;
        this.doWind[this.ZIPwin] = this.DoZip;
        this.doWind[this.SLwin] = this.DoSliders;
        this.windChanged = new bool[this.NUMwin];
        i = 0;
        while (i < this.NUMwin)
        {
            this.windChanged[i] = true;
            this.isWindowOpen[i] = true;
            this.sizeRect[i].DynamicInvoke(new object[] { });
            if (i > 0)
            {
                this.windowRects[i].x = (this.windowRects[i - 1].x + this.windowRects[i - 1].width) + 2;
            }
            ++i;
        }
        this.closefunc = this.DoClosedWind;
        // Can't figure out how to have ArrayFromQuery re-allocate these, so pick a big size
        this.dbChoices = new string[100];

        // If the falling ball feature is present, we want to have a small scene so gravity looks more realistic
        if (!this.includeBalls)
        {
            this.xSceneSize = this.xSceneSize * 4f;
            this.ySceneSizeByBin = this.ySceneSizeByBin * 4f;
            this.zSceneSize = this.zSceneSize * 4f;
        }
        // Find the prototype mesh object, if present
        this.proto = GameObject.Find("protomesh_SceneObj");
        if (this.proto == null)
            Debug.LogError("Failed to find object for proto ridge");

        //this.baseCube = GameObject.Find("basecube");
        //this.MakeUnitCube(this.baseCube);
        this.protolabel = GameObject.Find("protolabel");
        this.signPost = UnityEngine.Object.Instantiate(GameObject.Find("SignPost"), new Vector3(-1000, -1000, -1000), Quaternion.identity);

        // Open database
        this.connStrn = "URI=file:" + Const.dataBase;

        /* Stauffer - removing Sqlite stuff to work on webGL build
        this.connection = new SqliteConnection(this.connStrn);
        this.connection.Open();
        this.dbcmd = this.connection.CreateCommand();
        */
        //this.myCameraOld = GameObject.FindWithTag("MainCamera").GetComponent("Camera") as Camera;
        //Stauffer - change this to my new camera for now while new camera controls are implemented
        this.myCameraOld = GameObject.Find("Camera").GetComponent<Camera>() as Camera; 
        this.currFOV = (this.lowFOVRange + this.highFOVRange) - this.myCameraOld.fieldOfView; // hokey, but we want currFOV to increase as fieldOfView decreases
        this.myController = GameObject.Find("FPC");
        this.allVariableDescs = new VariableDesc[2];
        this.allVariableDescs[0] = new VariableDesc();
        this.allVariableDescs[0].SetAsFloat("height", 0f, 10f);
        this.allVariableDescs[1] = new VariableDesc();
        this.allVariableDescs[1].SetAsInt("bin", 0, 2);
        this.numFields = 2;
        this.xray = new Texture2D(Screen.width / 2, Screen.height / 2);
        //SpreadBalls(16, 10.0);

        //Stauffer
        //
        this.ridgeMeshMinHeight = 0.1f;
        //Quick intro message with instructions
        UIManager.Instance.ShowIntroMessage();
    }

    private bool haveBalls;
    private Rigidbody[] allBalls;
    public Rigidbody protoBall;
    public virtual void SpreadBalls(int rowcols, float spread)
    {
        int row = 0;
        int col = 0;
        if (this.haveBalls)
        {
        }
        //var i : int;
        //for(i = 0; i < allBalls.length; ++i) Destroy(allBalls[i].gameObject);
        spread = (this.xSceneSize * spread) / rowcols;
        int numBalls = rowcols * rowcols;
        this.allBalls = new Rigidbody[numBalls];
        float startX = (this.xzySceneCorner.x + (this.xSceneSize * 0.5f)) - ((rowcols * 0.5f) * spread);
        float startZ = (this.xzySceneCorner.z + (this.ySceneSizeByBin * 0.5f)) - ((rowcols * 0.5f) * spread);
        float startY = this.zSceneSize * 2f;
        int i = 0;
        this.protoBall.transform.localScale = new Vector3(this.rowDepthDataOnly, this.rowDepthDataOnly, this.rowDepthDataOnly);
        row = 0;
        while (row < rowcols)
        {
            float zPos = startZ + (row * spread);
            col = 0;
            while (col < rowcols)
            {
                this.allBalls[i] = UnityEngine.Object.Instantiate(this.protoBall, new Vector3((startX + (col * spread)) + UnityEngine.Random.Range(-spread, spread), startY, zPos + UnityEngine.Random.Range(-spread, spread)), Quaternion.identity);
                this.allBalls[i++].velocity = new Vector3(0, 0, 0);
                ++col;
            }
            ++row;
        }
        this.haveBalls = true;
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {/*
            if (DataManager.Instance.DebugQuickChooseLoadDisplayFile())
            {
                //take the new data and draw it
                Debug.Log("Loaded file with success");
                NewPrepareAndDrawData();
            }
            */
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Redraw();
        }
        if (/*Input.GetKeyDown(KeyCode.M) ||*/ Input.GetKeyDown(KeyCode.F5))
        {
            this.allHidden = !this.allHidden;
        }
        if (/*Input.GetKeyDown(KeyCode.H) ||*/ Input.GetKeyDown(KeyCode.F1))
        {
            //this.showHelp = !this.showHelp;
            UIManager.Instance.ShowIntroMessage();
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            //Debugging
            GameObject newRidge = UnityEngine.Object.Instantiate(this.proto, new Vector3(this.xzySceneCorner.x, this.xzySceneCorner.y, this.xzySceneCorner.z), Quaternion.identity);
            newRidge.name = "testRidge";

            //UIManager.Instance.StartUIActionPrompts();

            //TriDataPoint data = new TriDataPoint(0, 1);
            //data.DebugDump();
        }
        if (Const.menuScrolling && (Time.time > (this.lastScrollTime + this.minScrollSecs)))
        {
            this.scrollAmount = Input.GetAxis("Mouse ScrollWheel");
            if (this.scrollAmount != 0f)
            {
                this.lastScrollTime = Time.time;
            }
        }
    }

    public virtual void FixedUpdate()
    {
        if (this.doDrop)
        {
            this.SpreadBalls(16, 0.9f);
            this.doDrop = false;
        }
    }

    public virtual void OnGUI()
    {
        int i = 0;
        if (!this.initDone)
        {
            this.initDone = true;
            this.scrollStyle = new GUIStyle(GUI.skin.toggle);
            this.scrollStyle.imagePosition = ImagePosition.ImageLeft;
            this.scrollStyle.onActive.background = this.scrollOnLowTexture;
            this.scrollStyle.onNormal.background = this.scrollOnLowTexture;
            this.scrollStyle.onHover.background = this.scrollOnHighTexture;
            this.scrollStyle.active.background = this.scrollOffLowTexture;
            this.scrollStyle.normal.background = this.scrollOffLowTexture;
            this.scrollStyle.hover.background = this.scrollOffHighTexture;
            this.scrollStyle.stretchWidth = false;
            this.scrollStyle.stretchHeight = false;
            this.grayscaleStyle = new GUIStyle();
            this.grayscaleStyle.imagePosition = ImagePosition.ImageOnly;
            this.grayscaleStyle.onActive.background = this.grayscaleOnTexture;
            this.grayscaleStyle.onNormal.background = this.grayscaleOnTexture;
            this.grayscaleStyle.onHover.background = this.grayscaleOnTexture;
            this.grayscaleStyle.active.background = this.grayscaleOffTexture;
            this.grayscaleStyle.normal.background = this.grayscaleOffTexture;
            this.grayscaleStyle.hover.background = this.grayscaleOffTexture;
            this.grayscaleStyle.stretchWidth = false;
            this.grayscaleStyle.stretchHeight = false;
            this.grayscaleStyle.fixedWidth = 16;
            this.grayscaleStyle.fixedHeight = 16;
            this.grayscaleStyle.margin = GUI.skin.toggle.margin;
            this.grayscaleStyle.margin.left = 2;
            this.grayscaleStyle.margin.right = 2;
            this.grayscaleStyle.margin.top = 1;
            this.grayscaleStyle.margin.bottom = 1;
            this.grayscaleStyle.border = GUI.skin.toggle.border;
            this.grayscaleStyle.border.left = 0;
            this.grayscaleStyle.border.right = 0;
            this.grayscaleStyle.border.top = 0;
            this.grayscaleStyle.border.bottom = 0;
            this.grayscaleStyle.padding = GUI.skin.toggle.padding;
            this.grayscaleStyle.padding.left = 0;
            this.grayscaleStyle.padding.right = 0;
            this.grayscaleStyle.padding.top = 0;
            this.grayscaleStyle.padding.bottom = 0;
            this.grayscaleStyle.overflow = GUI.skin.toggle.overflow;
            this.grayscaleStyle.overflow.left = 0;
            this.grayscaleStyle.overflow.right = 0;
            this.grayscaleStyle.overflow.top = 0;
            this.grayscaleStyle.overflow.bottom = 0;
            this.constantStyle = new GUIStyle(this.grayscaleStyle);
            this.constantStyle.onActive.background = this.constantOnTexture;
            this.constantStyle.onNormal.background = this.constantOnTexture;
            this.constantStyle.onHover.background = this.constantOnTexture;
            this.constantStyle.active.background = this.constantOffTexture;
            this.constantStyle.normal.background = this.constantOffTexture;
            this.constantStyle.hover.background = this.constantOffTexture;
            this.rainbowStyle = new GUIStyle(this.grayscaleStyle);
            this.rainbowStyle.onActive.background = this.rainbowOnTexture;
            this.rainbowStyle.onNormal.background = this.rainbowOnTexture;
            this.rainbowStyle.onHover.background = this.rainbowOnTexture;
            this.rainbowStyle.active.background = this.rainbowOffTexture;
            this.rainbowStyle.normal.background = this.rainbowOffTexture;
            this.rainbowStyle.hover.background = this.rainbowOffTexture;
            this.redgreenStyle = new GUIStyle(this.grayscaleStyle);
            this.redgreenStyle.onActive.background = this.redgreenOnTexture;
            this.redgreenStyle.onNormal.background = this.redgreenOnTexture;
            this.redgreenStyle.onHover.background = this.redgreenOnTexture;
            this.redgreenStyle.active.background = this.redgreenOffTexture;
            this.redgreenStyle.normal.background = this.redgreenOffTexture;
            this.redgreenStyle.hover.background = this.redgreenOffTexture;
            this.yellowblueStyle = new GUIStyle(this.grayscaleStyle);
            this.yellowblueStyle.onActive.background = this.yellowblueOnTexture;
            this.yellowblueStyle.onNormal.background = this.yellowblueOnTexture;
            this.yellowblueStyle.onHover.background = this.yellowblueOnTexture;
            this.yellowblueStyle.active.background = this.yellowblueOffTexture;
            this.yellowblueStyle.normal.background = this.yellowblueOffTexture;
            this.yellowblueStyle.hover.background = this.yellowblueOffTexture;
        }
        //if((!Const.controlBusy) && Input.GetKeyDown(KeyCode.I)) CapturePointedAt();
        //if((!Const.controlBusy) && Input.GetKeyDown(KeyCode.O)) MakeXRay();
        if (this.showHelp)
        {
            if (this.helpCount++ == 2)
            {
                // Center screen after first sizing; after that let the user control position
                this.helpWindowRect.x = (Screen.width - this.helpWindowRect.width) / 2f;
                this.helpWindowRect.y = (Screen.height - this.helpWindowRect.height) / 3f;
            }
            if (this.helpPage != this.oldHelpPage)
            {
                this.helpWindowRect.width = 800f;
                this.helpWindowRect.height = 10f;
            }
            this.helpWindowRect = GUILayout.Window(this.NUMwin + 100, this.helpWindowRect, this.DoHelp, "3D Heatmap " + this.programVersion, new GUILayoutOption[] { });
        }
        if (!this.allHidden)
        {
            i = 0;
            while (i < this.NUMwin)
            {
                if (this.windChanged[i])
                {
                    this.DoSize(i);
                }
                this.DoWind(i);
                ++i;
            }
        }
        if (this.pointedData.ready)
        {
            this.pointedWindowRect = GUILayout.Window(this.NUMwin, this.pointedWindowRect, this.DoPointedWindow, "Data Point", new GUILayoutOption[] { });
        }
        if (this.showXRay)
        {
            this.xrayWindowRect = GUILayout.Window(this.NUMwin + 1, this.xrayWindowRect, this.DoXRayWindow, "xray", new GUILayoutOption[] { });
        }
    }

    public virtual void DoWind(int windowID)
    {
        if (this.isLayout[windowID])
        {
            if (this.isWindowOpen[windowID])
            {
                this.windowRects[windowID] = GUILayout.Window(windowID, this.windowRects[windowID], this.doWind[windowID], this.windNames[windowID], new GUILayoutOption[] { });
            }
            else
            {
                this.windowRects[windowID] = GUI.Window(windowID, this.windowRects[windowID], this.DoClosedWind, this.windNames[windowID]);
            }
        }
        else
        {
            this.windowRects[windowID] = GUI.Window(windowID, this.windowRects[windowID], this.isWindowOpen[windowID] ? this.doWind[windowID] : this.closefunc, this.windNames[windowID]);
        }
    }

    public virtual void DoSize(int windowID)
    {
        if (this.isWindowOpen[windowID])
        {
            this.sizeRect[windowID].DynamicInvoke(new object[] { });
        }
        else
        {
            this.windowRects[windowID].height = 46;
        }
        this.windChanged[windowID] = false;
    }

    public virtual void DoClosedWind(int windowID)
    {
        if (GUI.Toggle(new Rect(2, 0, 20, 20), false, ""))
        {
            this.isWindowOpen[windowID] = true;
            this.windChanged[windowID] = true;
        }
        GUI.DragWindow();
    }

    public virtual void SizeDS()
    {
        // These are going to expand because of GUILayout
        this.windowRects[this.DSwin].height = 60;
        this.windowRects[this.DSwin].width = 280;
        this.scrollHeight = (this.numDB * this.pixPerLine) + 20;
        if (this.scrollHeight > 200)
        {
            this.scrollHeight = 200;
        }
    }

    public virtual void SizeStyle()
    {
        //windowRects[STYLEwin].height = (12 * pixPerLine) + 6;
        this.windowRects[this.STYLEwin].height = 60;
        this.windowRects[this.STYLEwin].width = 450;
    }

    public virtual void SizeZip()
    {
        this.windowRects[this.ZIPwin].height = (5 * this.pixPerLine) + 6;
        this.windowRects[this.ZIPwin].width = 130;
    }

    public virtual void SizeSliders()
    {
        this.windowRects[this.SLwin].height = this.windowRects[this.ZIPwin].height;
        this.windowRects[this.SLwin].width = 150;
    }

    public virtual void DoDS(int windowID)
    {
        this.ShrinkIt(windowID);
        if (this.isDetecting++ < 4)
        {
            GUILayout.Label("Analyzing Datasets . . .", Const.greenCenterLabel, new GUILayoutOption[] { });
            // Give the above time to draw before lengthy examination of database
            if (this.isDetecting == 4)
            {
                this.GetDatabaseChoices();
                this.SizeDS();
            }
        }
        else
        {
            if (this.dataChanged)
            {
                GUILayout.Label("Drawing. . .", Const.greenCenterLabel, new GUILayoutOption[] { });
            }
            else
            {
                this.dbPos = GUILayout.BeginScrollView(this.dbPos, new GUILayoutOption[] { GUILayout.Width(260), GUILayout.Height(this.scrollHeight) });
                GUILayout.BeginVertical(new GUILayoutOption[] { });
                int dbnum = 0;
                while (dbnum < this.numDB)
                {
                    bool oldVal = dbnum == this.currDB;
                    if (GUILayout.Toggle(oldVal, this.dbChoices[dbnum], Const.realToggle, new GUILayoutOption[] { }))
                    {
                        // Note: can't turn off a DB; can only turn on another
                        this.currDB = dbnum;
                        this.selTable = this.dbChoices[this.currDB];
                        if (!oldVal)
                        {
                            this.dataChanged = true;
                        }
                    }
                    ++dbnum;
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }
        string infoString = ((("   " + this.numRows) + " rows   ") + this.numCols) + " cols   ";
        if (this.numBins > 1)
        {
            infoString = infoString + (this.numBins + " bins");
        }
        else
        {
            infoString = infoString + "1 bin";
        }
        if ((this.currDB >= 0) && !this.dataChanged)
        {
            GUILayout.Label(infoString, new GUILayoutOption[] { });
        }
        GUI.DragWindow();
        if (this.dataChanged && (++this.waitCount > 4))
        {
            this.DatasetSelected(this.selTable, this.bConnectX, this.bExtendZ, this.binInterleave, this.topColorChoice, this.sideColorChoice, this.currGraphHeight);
        }
    }

    public virtual void DoStyle(int windowID)
    {
        int choiceInd = 0;
        this.ShrinkIt(windowID);
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        GUILayout.BeginVertical(new GUILayoutOption[] { });
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        GUILayout.BeginVertical(Const.grayStyle, new GUILayoutOption[] { });
        this.bConnectX = GUILayout.Toggle(this.bConnectX, "Ribbon", Const.realToggle, new GUILayoutOption[] { });
        this.bConnectX = !GUILayout.Toggle(!this.bConnectX, "Tile", Const.realToggle, new GUILayoutOption[] { });
        GUILayout.EndVertical();
        GUILayout.BeginVertical(new GUILayoutOption[] { });
        this.bExtendZ = GUILayout.Toggle(this.bExtendZ, "Fill", Const.realToggle, new GUILayoutOption[] { });
        //Stauffer - note that bHaveLabels is never assigned to, and will always have default value of false
        if (this.bHaveLabels)
        {
            this.bShowLabels = GUILayout.Toggle(this.bShowLabels, "Show Labels", Const.realToggle, new GUILayoutOption[] { });
        }
        this.binInterleave = GUILayout.Toggle(this.binInterleave, "Interleave", Const.realToggle, new GUILayoutOption[] { });
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if ((this.drawn && !this.wantRedraw) && GUILayout.Button("Draw", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            this.wantRedraw = true;
        }
        if (!this.drawn && !this.wantRedraw)
        {
            GUILayout.Label("Draw", Const.grayButton, new GUILayoutOption[] { });
        }
        if (this.wantRedraw)
        {
            GUILayout.Label("Drawing . . .", Const.greenCenterLabel, new GUILayoutOption[] { });
        }
        if (((this.includeVRML && this.drawn) && !this.wantVRML) && GUILayout.Button("Write VRML", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            this.wantVRML = true;
        }
        if (this.wantVRML)
        {
            GUILayout.Label("Writing VRML . . .", Const.greenCenterLabel, new GUILayoutOption[] { });
        }
        if (((this.includeTriangles && this.drawn) && !this.wantTriangles) && GUILayout.Button("Write Triangles", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            this.wantTriangles = true;
        }
        if (this.wantTriangles)
        {
            GUILayout.Label("Writing Triangles . . .", Const.greenCenterLabel, new GUILayoutOption[] { });
        }
        if (this.includeBalls)
        {
            if (GUILayout.Button("Drop Balls", Const.buttonToggle, new GUILayoutOption[] { }))
            {
                this.doDrop = true;
            }
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical(Const.grayStyle, new GUILayoutOption[] { });
        GUILayout.Label("Top Color", Const.littleCenterLabel, new GUILayoutOption[] { });
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        this.UpdateColorStyleFromToggle(0, this.redgreenStyle, false);
        this.UpdateColorStyleFromToggle(1, this.rainbowStyle, false);
        this.UpdateColorStyleFromToggle(2, this.yellowblueStyle, false);
        this.UpdateColorStyleFromToggle(3, this.grayscaleStyle, false);
        this.UpdateColorStyleFromToggle(4, this.constantStyle, false);
        GUILayout.EndHorizontal();
        if (this.topColorChoice >= this.numFields)
        {
            this.topColorChoice = 0;
        }
        if (this.sideColorChoice >= this.numFields)
        {
            this.sideColorChoice = 0;
        }
        choiceInd = 0;
        while (choiceInd < this.numFields)
        {
            if (GUILayout.Toggle(this.topColorChoice == choiceInd, this.allVariableDescs[choiceInd].name, Const.realToggle, new GUILayoutOption[] { }))
            {
                this.topColorChoice = choiceInd;
            }
            ++choiceInd;
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical(Const.grayStyle, new GUILayoutOption[] { });
        GUILayout.Label("Side Color", Const.littleCenterLabel, new GUILayoutOption[] { });
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        this.UpdateColorStyleFromToggle(0, this.redgreenStyle, true);
        this.UpdateColorStyleFromToggle(1, this.rainbowStyle, true);
        this.UpdateColorStyleFromToggle(2, this.yellowblueStyle, true);
        this.UpdateColorStyleFromToggle(3, this.grayscaleStyle, true);
        this.UpdateColorStyleFromToggle(4, this.constantStyle, true);
        GUILayout.EndHorizontal();
        choiceInd = 0;
        while (choiceInd < this.numFields)
        {
            if (GUILayout.Toggle(this.sideColorChoice == choiceInd, this.allVariableDescs[choiceInd].name, Const.realToggle, new GUILayoutOption[] { }))
            {
                this.sideColorChoice = choiceInd;
            }
            ++choiceInd;
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.DragWindow();
        if (this.wantRedraw && (++this.waitCount > 4))
        {
            this.DatasetSelected(this.selTable, this.bConnectX, this.bExtendZ, this.binInterleave, this.topColorChoice, this.sideColorChoice, this.currGraphHeight);
        }
        if (this.wantVRML && (++this.waitCount > 4))
        {
            this.DrawVRML();
        }
        if (this.wantTriangles && (++this.waitCount > 4))
        {
            this.DrawVRML();
        }
    }

    public virtual void DoSliders(int windowID)
    {
        int oldScrollChoice = this.scrollChoice;
        float oldGraphHeight = this.currGraphHeight;
        float oldFOV = this.currFOV;
        float oldThick = this.currDepthToWidthRatioExp;
        float oldSep = this.binSeparationFrac;
        float oldGap = this.rowGapFrac;
        int oldBin = this.currBin;
        this.ShrinkIt(windowID);
        if (this.scrollAmount != 0f)
        {
            this.currGraphHeight = this.UpdateFromScroll(this.choiceHeight, this.currGraphHeight, this.lowGraphHeightRange, this.highGraphHeightRange, 0.05f);
            this.currFOV = this.UpdateFromScroll(this.choiceFOV, this.currFOV, this.lowFOVRange, this.highFOVRange, 0.05f);
            this.currDepthToWidthRatioExp = this.UpdateFromScroll(this.choiceThick, this.currDepthToWidthRatioExp, this.lowDepthToWidthRatioRange, this.highDepthToWidthRatioRange, 0.05f);
            this.binSeparationFrac = this.UpdateFromScroll(this.choiceSep, this.binSeparationFrac, this.lowSepRange, this.highSepRange, 0.05f);
            this.rowGapFrac = this.UpdateFromScroll(this.choiceGap, this.rowGapFrac, this.lowGapRange, this.highGapRange, 0.05f);
            this.currBin = this.UpdateIntFromScroll(this.choiceBin, this.currBin, this.numBins, true);
            this.scrollAmount = 0f;
        }
        this.UpdateSliderFromToggle(this.choiceHeight, "height");
        this.currGraphHeight = GUILayout.HorizontalSlider(this.currGraphHeight, this.lowGraphHeightRange, this.highGraphHeightRange, new GUILayoutOption[] { });
        this.UpdateSliderFromToggle(this.choiceFOV, "zoom");
        this.currFOV = GUILayout.HorizontalSlider(this.currFOV, this.lowFOVRange, this.highFOVRange, new GUILayoutOption[] { });
        this.UpdateSliderFromToggle(this.choiceThick, "thickness");
        this.currDepthToWidthRatioExp = GUILayout.HorizontalSlider(this.currDepthToWidthRatioExp, this.lowDepthToWidthRatioRange, this.highDepthToWidthRatioRange, new GUILayoutOption[] { });
        this.UpdateSliderFromToggle(this.choiceSep, "bin separation");
        this.binSeparationFrac = GUILayout.HorizontalSlider(this.binSeparationFrac, this.lowSepRange, this.highSepRange, new GUILayoutOption[] { });
        this.UpdateSliderFromToggle(this.choiceGap, "row gap");
        this.rowGapFrac = GUILayout.HorizontalSlider(this.rowGapFrac, this.lowGapRange, this.highGapRange, new GUILayoutOption[] { });
        this.UpdateSliderFromToggle(this.choiceBin, "scroll bins");
        GUI.DragWindow();
        if (oldScrollChoice != this.scrollChoice)
        {
            this.ScrollingSelected(this.scrollChoice);

            if (oldScrollChoice == this.choiceBin)
            {
                //Shows all ridges, regardless of bin #
                this.VisBins(-1);
            }
            if (this.scrollChoice == this.choiceBin)
            {
                //Show only ridges with this bin #
                this.VisBins(this.currBin);
            }
        }
        if (oldGraphHeight != this.currGraphHeight)
        {
            this.GraphHeightSelected(this.currGraphHeight);
        }
        if (oldFOV != this.currFOV)
        {
            this.FOVSelected(this.currFOV);
        }
        if (((this.currDepthToWidthRatioExp != oldThick) || (this.binSeparationFrac != oldSep)) || (this.rowGapFrac != oldGap))
        {
            this.Redistribute(this.currDepthToWidthRatioExp, this.binSeparationFrac, this.rowGapFrac);
        }
        if (this.currBin != oldBin)
        {
            //Show only ridges with this bin #
            this.VisBins(this.currBin);
        }
        Const.menuScrolling = this.scrollChoice >= 0;
    }

    public virtual void UpdateSliderFromToggle(int choiceNumber, string sliderName)
    {
        bool thisVal = GUILayout.Toggle(this.scrollChoice == choiceNumber, sliderName, this.scrollStyle, new GUILayoutOption[] { });
        if ((this.scrollChoice == choiceNumber) && !thisVal)
        {
            this.scrollChoice = -1;
        }
        if (thisVal)
        {
            this.scrollChoice = choiceNumber;
        }
    }

    public virtual void UpdateColorStyleFromToggle(int choiceNumber, GUIStyle style, bool bSide)
    {
        bool thisVal = false;
        if (bSide)
        {
            thisVal = GUILayout.Toggle(this.sideStyleChoice == choiceNumber, "", style, new GUILayoutOption[] { });
            if ((this.sideStyleChoice == choiceNumber) && !thisVal)
            {
                this.sideStyleChoice = -1;
            }
            if (thisVal)
            {
                this.sideStyleChoice = choiceNumber;
            }
        }
        else
        {
            thisVal = GUILayout.Toggle(this.topStyleChoice == choiceNumber, "", style, new GUILayoutOption[] { });
            if ((this.topStyleChoice == choiceNumber) && !thisVal)
            {
                this.topStyleChoice = -1;
            }
            if (thisVal)
            {
                this.topStyleChoice = choiceNumber;
            }
        }
    }

    public virtual float UpdateFromScroll(int thisChoice, float currValue, float lowValue, float highValue, float frac)
    {
        Debug.Log((((((("UpdateFromScroll " + thisChoice) + " ") + this.scrollChoice) + " ") + this.scrollAmount) + " ") + currValue);
        if (this.scrollChoice != thisChoice)
        {
            return currValue;
        }
        float increment = (highValue - lowValue) * frac;
        if (this.scrollAmount < 0)
        {
            currValue = currValue - increment;
        }
        if (this.scrollAmount > 0)
        {
            currValue = currValue + increment;
        }
        if (currValue < lowValue)
        {
            currValue = lowValue;
        }
        if (currValue > highValue)
        {
            currValue = highValue;
        }
        return currValue;
    }

    public virtual int UpdateIntFromScroll(int thisChoice, int currValue, int numValues, bool doWrap)
    {
        if (this.scrollChoice != thisChoice)
        {
            return currValue;
        }
        if (this.scrollAmount < 0)
        {
            currValue--;
        }
        if (this.scrollAmount > 0)
        {
            currValue++;
        }
        if (currValue < 0)
        {
            currValue = doWrap ? numValues - 1 : 0;
        }
        if (currValue >= numValues)
        {
            currValue = doWrap ? 0 : numValues - 1;
        }
        return currValue;
    }

    public virtual void DoZip(int windowID)
    {
        int zipChoice = -1;
        int lookChoice = -1;
        this.ShrinkIt(windowID);
        if (GUILayout.Button("Top", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            zipChoice = 0;
        }
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        if (GUILayout.Button("Left", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            zipChoice = 3;
        }
        if (GUILayout.Button("Right", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            zipChoice = 4;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        if (GUILayout.Button("Front", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            zipChoice = 1;
        }
        if (GUILayout.Button("Back", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            zipChoice = 2;
        }
        GUILayout.EndHorizontal();
        if (zipChoice >= 0)
        {
            this.ZipSelected(zipChoice);
        }
        GUILayout.Label("Look", Const.littleCenterLabel, new GUILayoutOption[] { });
        if (GUILayout.Button("Down", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            lookChoice = 0;
        }
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        if (GUILayout.Button("Left", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            lookChoice = 3;
        }
        if (GUILayout.Button("Right", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            lookChoice = 4;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        if (GUILayout.Button("Ahead", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            lookChoice = 1;
        }
        if (GUILayout.Button("Back", Const.buttonToggle, new GUILayoutOption[] { }))
        {
            lookChoice = 2;
        }
        GUILayout.EndHorizontal();
        if (lookChoice >= 0)
        {
            this.LookSelected(lookChoice);
        }
        GUI.DragWindow();
    }

    public virtual bool ShrinkIt(int windowID)
    {
        bool shrink = false;
        shrink = GUI.Toggle(new Rect(2, 0, 20, 20), true, "");
        if (!shrink)
        {
            this.isWindowOpen[windowID] = false;
            this.windChanged[windowID] = true;
        }
        return shrink;
    }

    public virtual void DoPointedWindow(int windowID)
    {
        GUILayout.Label("x:" + this.pointedData.position.x, new GUILayoutOption[] { });
        GUILayout.Label("y:" + this.pointedData.position.y, new GUILayoutOption[] { });
        GUILayout.Label("z:" + this.pointedData.position.z, new GUILayoutOption[] { });
        GUILayout.Label("row:" + this.pointedData.row, new GUILayoutOption[] { });
        GUILayout.Label("col:" + this.pointedData.col, new GUILayoutOption[] { });
        GUILayout.Label("bin:" + this.pointedData.bin, new GUILayoutOption[] { });
        GUILayout.Label("height:" + this.pointedData.height, new GUILayoutOption[] { });
        GUI.DragWindow();
    }

    public virtual void DoXRayWindow(int windowID)
    {
        GUILayout.Label(this.xray, new GUILayoutOption[] { });
        if (GUILayout.Button("close", new GUILayoutOption[] { }))
        {
            this.showXRay = false;
        }
        GUI.DragWindow();
    }

    public virtual void DoHelp(int windowID)
    {
        this.oldHelpPage = this.helpPage;
        GUILayout.Label("Press F1 or H to hide/view this window.  Press F5 or M to hide/view menus.", Const.greenCenterLabel, new GUILayoutOption[] { });
        GUILayout.Label("Press ESC to exit program", Const.littleCenterLabel, new GUILayoutOption[] { });
        GUILayout.BeginHorizontal(new GUILayoutOption[] { });
        if (GUILayout.Toggle(this.helpPage == 0, "About", Const.bigToggle, new GUILayoutOption[] { }))
        {
            this.helpPage = 0;
        }
        if (GUILayout.Toggle(this.helpPage == 1, "Data Preparation", Const.bigToggle, new GUILayoutOption[] { }))
        {
            this.helpPage = 1;
        }
        if (GUILayout.Toggle(this.helpPage == 2, "Navigation", Const.bigToggle, new GUILayoutOption[] { }))
        {
            this.helpPage = 2;
        }
        if (GUILayout.Toggle(this.helpPage == 3, "Menus", Const.bigToggle, new GUILayoutOption[] { }))
        {
            this.helpPage = 3;
        }
        GUILayout.EndHorizontal();
        if (this.helpPage != this.oldHelpPage)
        {
            this.menuScrollPos = new Vector2(0f, 0f);
        }
        switch (this.helpPage)
        {
            case 0:
                GUILayout.Label("3D Heatmap was developed by Dr. Jason H. Moore and the Bioinformatics Visualization Laboratory at Dartmouth Medical School with support from the Institute for Quantitative Biomedical Sciences and the Norris-Cotton Cancer Center and funding from NIH grants LM009012 and RR018787.", new GUILayoutOption[] { });
                GUILayout.Label("The goal of this project is to evaluate the use of 3D video game engines for the interactive visual exploration of more than two dimensions of data.", new GUILayoutOption[] { });
                GUILayout.Label("3D Heatmap is open source software released unter the GNU General Public License, Version 3.", new GUILayoutOption[] { });
                GUILayout.Label("We hope to receive comments, suggestions and feature requests from users who have tried this program using their own data. (Press \"Data Preparation\" button for instructions on formatting your data) Please e-mail Jason.H.Moore@Dartmouth.edu to offer feedback.", new GUILayoutOption[] { });
                GUILayout.Label("This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.", new GUILayoutOption[] { });
                GUILayout.Label("Powered by the Unity 3D Game Engine", Const.littleCenterLabel, new GUILayoutOption[] { });
                break;
            case 1:
                this.menuScrollPos = GUILayout.BeginScrollView(this.menuScrollPos, new GUILayoutOption[] { GUILayout.Width(780), GUILayout.Height(400) });
                GUILayout.Label("Design", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("A conventional heat map consists of a 2D grid of colored squares where each square represents an observation of a random variable and the color of the square is proportional to the value of that observation. 2D heat maps are used pervasively in the Biological sciences and both the grid and the dimension mapped to the grid can represent a variety of concepts. Genes, experimental conditions, subjects, genomic elements, etc... are distributed on the grid where a color palette is used to encode transcript abundance, protein concentration, conservation, activation, etc...", new GUILayoutOption[] { });
                GUILayout.Label("It is often desirable to map several dimensions to the same grid. This situation is usually resolved by plotting a separate 2D heat map for each dimension. The analysis of relationships between multiple dimensions is usually hindered by this design due to the loss of context and orientation when transitioning between dimensions in large data sets. It is our goal to explore alternative representations that superimpose and interleave several dimensions onto the same grid. Through this approach we aim to find a solution that decreases the disorienting effect of transitioning between dense and separately graphed volumes of data and to increase the interpretability of multidimensional data without overwhelming the user's senses.", new GUILayoutOption[] { });
                GUILayout.Label("The current version includes the following features:", new GUILayoutOption[] { });
                GUILayout.Label("- Updating graphical parameters in real time.", new GUILayoutOption[] { });
                GUILayout.Label("The parameters that govern the graphing of data can be changed in real time. This allows for the seamless transition between dimensions without losing the current perspective and arrangement of the 3D heat map.", new GUILayoutOption[] { });
                GUILayout.Label("- Superimposing dimensions.", new GUILayoutOption[] { });
                GUILayout.Label("In order to map several dimensions onto the same grid we have chosen simple yet multifaceted geometries. The graphical unit can hold one dimension as its height, a second dimension as the color on its horizontal surface and a third dimension as the color of its vertical surfaces.", new GUILayoutOption[] { });
                GUILayout.Label("- Interleaving dimensions.", new GUILayoutOption[] { });
                GUILayout.Label("An alternative to superimposition that allows for an arbitrary number of dimensions to be mapped to the same grid is interleaving. This is achieved by consolidating the same row in the grid across all dimensions and plotting the consolidated rows adjacently. Spacer of different widths are used to convey the hierarchical structure of rows.", new GUILayoutOption[] { });
                GUILayout.Label("All features can be explored in combination. It is possible to superimpose, interleave and switch between dimensions without interrupting the path of flight through the data or losing the point of view. It is important in this exercise that the user is able to start from a conventional 2D heat map and incrementally add dimensions as they elaborate and refine their analysis and interpretation. It is also up to the user to decide which variables are better represented by height or color.", new GUILayoutOption[] { });
                GUILayout.Label("DATA IMPORT", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("See the README.txt for ways to import your data.", new GUILayoutOption[] { });
                GUILayout.EndScrollView();
                break;
            case 2:
#if UNITY_STANDALONE_WIN
                GUILayout.Label("We recommend a 3DConnexion SpaceNavigator for 3D navigation.  The following mouse and keyboard controls may also be used.", new GUILayoutOption[] { });
#endif
                GUILayout.Label("To look in a different direction, hold down the right mouse button (or Alt key, if that is more convenient) and move the mouse (or touchpad).", new GUILayoutOption[] { });
                GUILayout.Label("You can also turn left and right by holding down a shift key and pressing left or right arrow keys.  Hold down a shift key and press up and down arrow keys to tilt vertically.", new GUILayoutOption[] { });
                GUILayout.Label("If you become disoriented, use the \"Look\" menu to select one of 5 fixed orientations.", new GUILayoutOption[] { });
                GUILayout.Label("To move forward, press up arrow or \"w\".  The longer you hold it down, the faster you will go, and your motion will continue after you release the key.", new GUILayoutOption[] { });
                GUILayout.Label("To stop, press the left mouse button while your cursor is within the scene, not in a menu.", new GUILayoutOption[] { });
                GUILayout.Label("You can careen wildly and rapidly about the scene using only the mouse, right button and up arrow.  Onlookers may experience motion discomfort.", new GUILayoutOption[] { });
                GUILayout.Label("To modify your velocity in a more controlled way, the following keys add velocity relative to your orientation:", new GUILayoutOption[] { });
                GUILayout.Label("left arrow or \"a\" = left   right arrow or \"d\" = right", new GUILayoutOption[] { });
                GUILayout.Label("down arrow or \"s\" = backwards", new GUILayoutOption[] { });
                GUILayout.Label("space bar + up or down arrow = up or down", new GUILayoutOption[] { });
                break;
            case 3:
                this.menuScrollPos = GUILayout.BeginScrollView(this.menuScrollPos, new GUILayoutOption[] { GUILayout.Width(780), GUILayout.Height(400) });
                GUILayout.Label("Toggle menus on and off by pressing F5.", new GUILayoutOption[] { });
                GUILayout.Label("Data Selection", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("Clicking on a dataset causes it to be drawn immediately using the current chart style.", new GUILayoutOption[] { });
                GUILayout.Label("If you prefer, choose chart style before selecting a dataset.", new GUILayoutOption[] { });
                GUILayout.Label("Chart Style", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("Ribbon style draws a line from each point to the next along the row; the 3D equivalent of a line graph.  Selecting the \"fill\" option extends the ribbon down to the base.", new GUILayoutOption[] { });
                GUILayout.Label("Tile style draws a horizontal tile for each point.  Selecting the \"fill\" option extends the tile down to the base, making the 3D equivalent of a bar chart.", new GUILayoutOption[] { });
                GUILayout.Label("The Interleave option applies when the dataset has values in more than one bin.  If Interleave is on, the chart will show all the bins for one row before drawing the next row.  If it is off, all the rows for one bin will be shown before beginning the next bin.", new GUILayoutOption[] { });
                GUILayout.Label("The color of the top and side of each point is driven by data values.  The data value can be height or bin number.  If your dataset has other integer fields, these will also appear as choices.  There are five color schemes for mapping data to colors, based on a linear interpolation between the lowest and highest values of the chosen data element.", new GUILayoutOption[] { });
                GUILayout.Label("As described in Data Preparation, you can also provide a table of color assignments for each possible data value.  If this table is present, it will be used when no other color scheme is chosen.", new GUILayoutOption[] { });
                GUILayout.Label("A Draw button appears on the Chart Style menu only if a dataset has been selected.  Changes chosen in the Chart Style menu only take effect when Draw is clicked or a new dataset is chosen in the Data Selection menu.", new GUILayoutOption[] { });
                GUILayout.Label("Zip to Viewpoint", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("These buttons reposition and reorient the viewpoint so that your 3D chart can be viewd from fixed angles.  If you change the size of your chart using sliders in the Chart View menu, the Zip buttons will recalculate the positions needed to view your data.", new GUILayoutOption[] { });
                GUILayout.Label("The Look submenu reorients the viewpoint without moving it.", new GUILayoutOption[] { });
                GUILayout.Label("Chart View", Const.fixLabel, new GUILayoutOption[] { });
                GUILayout.Label("There are 5 sliders for adjusting the view of your data without requiring a redraw.", new GUILayoutOption[] { });
                GUILayout.Label("You may set a value by clicking on the slider bar or dragging the slider indicator.", new GUILayoutOption[] { });
                GUILayout.Label("To the left of each slider is a stylized \"S\".  Clicking on this attaches that slider to the mouse scrollwheel, so it can be adjusted even when the menu is toggled off.", new GUILayoutOption[] { });
                GUILayout.Label("Height - changes the height of the entire graph.", new GUILayoutOption[] { });
                GUILayout.Label("Zoom - Like changing the focal length of a camera.  Zooming out (left) gives more feeling of being surrounded by your data, and provides smoother motion when turning.  Zooming in (and moving back) gives a more distant view, with less distortion at the edges of the view.  If you want to make screen shots of your chart, they make look better if you move the viewpoint back and zoom in to compensate.", new GUILayoutOption[] { });
                GUILayout.Label("Thickness - the depth of each data element.  If there are many rows and few columns, you may wish to make the rows narrow to fit in a reasonable area.", new GUILayoutOption[] { });
                GUILayout.Label("Bin Separation - the distance from the start of one bin to the start of the next.  Normally this is the size of one data point, so that when Interleave is chosen, each bin is next to the next.  Sliding this down to zero causes the bins to overlap.  If Interleave is off, you may want to choose a large value to give visible separation between bins.", new GUILayoutOption[] { });
                GUILayout.Label("Row Gap - the distance between rows.  Defaults to the size of one data point.", new GUILayoutOption[] { });
                GUILayout.Label("When the \"S\" on Scroll Bins is chosen, only one bin is visible at a time.  Moving the scroll wheel cycles through the bins.  By setting Interleave On, Bin Separation 0, and scrolling bins, you create a blink comparator for noticing small changes between bins.", new GUILayoutOption[] { });
                GUILayout.EndScrollView();
                break;
        }
        GUI.DragWindow();
    }

    public virtual void GetDatabaseChoices()
    {
        //numDB = ArrayFromQuery(dbChoices, "name", "from SQLITE_MASTER where name like 'heat_%' order by name;");
        this.numDB = this.ArrayFromQuery(this.dbChoices, "name", "from SQLITE_MASTER where like('heat!_%', name, '!') order by name;");
        if (this.currDB >= this.numDB)
        {
            this.currDB = 0;
        }
        //Debug.Log("currDB is " + currDB + ", size of dbChoices is " + dbChoices.length);
        this.selTable = this.currDB >= 0 ? this.dbChoices[this.currDB] : "";
    }


    public virtual void GetAxisExtentsOld()
    {
        Debug.LogError("Deprecated to be removed");
        //Stauffer - get ranges for row and col numbers, bin numbers, and height values (first observed value)
        //In DatasetSelected, the ranges get set for the optional subsequent int columns.
        //
        /* Stauffer - removing Sqlite stuff to work on webGL build
        this.dbcmd.CommandText = ((("SELECT MIN(row), MAX(row), MIN(col), MAX(col), MIN(bin), MAX(bin), MIN(height), MAX(height) from " + this.selTable) + " where col <= ") + this.colLimit) + ";";
        Debug.Log("GetAxisExtentsOld() query is " + this.dbcmd.CommandText);
        this.reader = this.dbcmd.ExecuteReader();
        this.reader.Read();
        this.minRow = this.reader.GetInt32(0);
        this.maxRow = this.reader.GetInt32(1);
        this.minCol = this.reader.GetInt32(2);
        this.maxCol = this.reader.GetInt32(3);
        this.minBin = this.reader.GetInt32(4);
        this.maxBin = this.reader.GetInt32(5);
        this.minDataHeight = this.reader.GetFloat(6);
        this.maxDataHeight = this.reader.GetFloat(7);
        this.reader.Close();
        this.numRows = (this.maxRow - this.minRow) + 1;
        // debugging
        if (this.maxCol > this.colLimit)
        {
            this.maxCol = this.colLimit;
        }
        this.numCols = (this.maxCol - this.minCol) + 1;
        this.numBins = (this.maxBin - this.minBin) + 1;
        this.dataHeightRange = this.maxDataHeight - this.minDataHeight;
        */
    }

    public virtual int ArrayFromQuery(string[] inarray, string field, string fromClause)
    {
        Debug.LogError("Deprecated to be removed");
        return 0;
        /* Stauffer - removing Sqlite stuff to work on webGL build
        int anint = 0;
        this.dbcmd.CommandText = (("SELECT count(" + field) + ") ") + fromClause;
        //Debug.Log("Query is " + dbcmd.CommandText);
        //object numvals = this.dbcmd.ExecuteScalar();
        //Debug.Log("numvals is " + numvals);
        //inarray = new String[numvals];
        this.dbcmd.CommandText = (("SELECT " + field) + " ") + fromClause;
        //Debug.Log("Query is " + dbcmd.CommandText);
        this.reader = this.dbcmd.ExecuteReader();
        int thisChoice = 0;
        string astring = null;
        while (this.reader.Read())
        {
            try
            {
                astring = this.reader.GetString(0);
            }

            catch
            {
                anint = this.reader.GetInt32(0);
                astring = anint.ToString();
            }
            inarray[thisChoice++] = astring;
        }
        //Debug.Log("Member " + (thisChoice - 1) + " is " + inarray[thisChoice - 1]);
        this.reader.Close();
        return thisChoice;
        */
    }

    public virtual void ScaleRidges(float frac)
    {
        int i = 0;
        float newSize = frac * this.zSceneSize;
        i = 0;
        while (i < this.numRidges)
        {
            this.xRidges[i].NewHeight(newSize);
            ++i;
        }
    }

    public virtual void CalcDimensions()//	baseCube.transform.position = Vector3(xzySceneCorner.x - 1.0, xzySceneCorner.y - (cubeHeight * 0.9), xzySceneCorner.z - 1.0);
    {
        //Stauffer xSceneSize is set to fixed val (400) during init
        this.rowDepthDataOnly = (this.xSceneSize * Mathf.Pow(2, this.currDepthToWidthRatioExp)) / this.numCols;
        //Constrain row depth to not exceed max total depth of plot. This is necessary for data that has many more
        // rows than columns
        if( this.numRows > (int)(this.numCols * 1.25f))
        {
            this.rowDepthDataOnly = this.ySceneSizeApproxMax / (this.numRows * (1f + this.rowGapFrac));
        }
        this.binSeparation = this.rowDepthDataOnly * this.binSeparationFrac;
        //Stauffer - this flag seems to control interleaving rows by bin or not
        if (this.binInterleave)
        {
            //Show bins interleaved
            this.rowDepthFull = (this.binSeparation * (this.numBins - 1)) + ((1f + this.rowGapFrac) * this.rowDepthDataOnly);
            this.ySceneSizeByBinWithSep = (this.rowDepthFull * this.numRows) - (this.rowGapFrac * this.rowDepthDataOnly);
            this.ySceneSizeByBin = this.ySceneSizeByBinWithSep;
            this.ySceneSizeFull = this.ySceneSizeByBin;
        }
        else
        {
            //Show bins as separate groups of rows
            this.rowDepthFull = (1f + this.rowGapFrac) * this.rowDepthDataOnly;
            //ySceneSizeByBinWithSep = (rowDepthFull * numRows) + (binSeparationFrac * tokenWidth);
            //ySceneSizeByBin = rowDepthFull * numRows - rowGapFrac;
            //ySceneSizeFull = (ySceneSizeByBinWithSep * numBins) - (binSeparationFrac * tokenWidth);
            this.ySceneSizeByBin = this.rowDepthDataOnly * (this.numRows + ((this.numRows - 1) * this.rowGapFrac));
            this.ySceneSizeByBinWithSep = this.ySceneSizeByBin + this.binSeparation;
            this.ySceneSizeFull = (this.ySceneSizeByBin * this.numBins) + ((this.numBins - 1) * this.binSeparation);
        }
        //float cubeHeight = this.xSceneSize * 0.05f;
    }

    public virtual void ShowDataOld()
    {
        Debug.LogError("Deprecated to be removed");
        /* Stauffer - removing Sqlite stuff to work on webGL build

        int prow = 0; //Stauffer - previous row?
        int pbin = 0; //Stauffer - previous bin?
        int col = 0;
        int row = 0;
        int abin = 0;
        float hght = 0.0f;
        int top = 0;
        int side = 0;
        this.drawn = true;
        if (!this.proto) // must be functioning only as user interface
        {
            return;
        }
        //Looks to be deleting old mesh/visualization
        if (this.numRidges > 0)
        {
            int iridge = 0;
            while (iridge < this.numRidges)
            {
                UnityEngine.Object.Destroy(this.xRidges[iridge].myMeshObject);
                UnityEngine.Object.Destroy(this.xRidges[iridge].myLabel);
                ++iridge;
            }
        }
        this.numRidges = 0;
        this.CalcDimensions();
        this.xScale = 1f / this.numCols;
        this.dataHeightRangeScale = 1f / (this.maxDataHeight - this.minDataHeight);
        this.xRidges = new XRidge[this.numRows * this.numBins];
        // In case we have changed datasets
        if (this.topColorChoice >= this.numFields)
        {
            this.topColorChoice = 0;
        }
        if (this.sideColorChoice >= this.numFields)
        {
            this.sideColorChoice = 0;
        }
        string extra1 = this.topColorChoice > 1 ? ", " + this.allVariableDescs[this.topColorChoice].name : ", 0";
        string extra2 = this.sideColorChoice > 1 ? ", " + this.allVariableDescs[this.sideColorChoice].name : ", 0";
        this.dbcmd.CommandText = (((((("SELECT col, row, height, bin" + extra1) + extra2) + " from ") + this.selTable) + " where col <= ") + this.colLimit) + " order by bin, row, col;";
        Debug.Log("Query is " + this.dbcmd.CommandText);
        this.reader = this.dbcmd.ExecuteReader();
        this.colVals = new int[this.numCols];
        this.heightVals = new float[this.numCols];
        this.topVals = new int[this.numCols];
        this.sideVals = new int[this.numCols];
        int recNum = 0; //Stauffer - record number. Each row spans multiple records, since each record has multiple values/observations
        int xslot = 0;
        while (this.reader.Read()) // Stauffer - order of fields below is spec'ed by query above. Each row in database will have fields: row #, column #, height value, bin #, and possibly two more int-valued fields
        {
            col = this.reader.GetInt32(0); //NOTE - row is first field in the db, and columns is 2nd, but above the query has 'col' first
            row = this.reader.GetInt32(1);
            hght = this.reader.GetFloat(2);
            abin = this.reader.GetInt32(3);
            top = this.reader.GetInt32(4); //presumably returns 0 if field not present?
            side = this.reader.GetInt32(5); //presumably returns 0 if field not present?
            //Build a ridge for a complete row's-worth of data
            //NOTE this block only gets called once the database read above 
            // has encountered a new row #. It visualizes everything read into the arrays in the next block. Awkward
            if ((recNum > 0) && (row != prow))
            {
                this.BuildRidgeOld(prow, xslot, pbin - this.minBin);
                prow = row;
                xslot = 0; //reset this so the data read above will now start to fill arrays from begin below, for new row 
            }
            //Fill the data for a full row, one column at a time
            if (xslot < this.numCols)
            {
                this.colVals[xslot] = col; //NOTE - these are class properties, that then get used in BuildRidgeOld
                this.heightVals[xslot] = hght;
                this.topVals[xslot] = top;
                this.sideVals[xslot] = side;
                ++xslot;
            }
            prow = row;
            pbin = abin;
            ++recNum; //next record in the table. This is only used in the check above to decide when to call BuildRidgeOld
        }
        this.reader.Close();
        this.BuildRidgeOld(prow, xslot, pbin - this.minBin);
        */
    }

    public bool doingEdges; //Stauffer - seems to determine if a bevel is drawn at top of column
    public float bevelFraction;
    public virtual void BuildRidgeOld(int row, int numx /*== num of columns*/, int binindex)
    {
        Color thisColor = default(Color);
        Color sideColor = default(Color);
        float thisX = 0.0f;
        float thisZ = 0.0f;
        float prevX = 0.0f;
        float prevZ = 0.0f;
        float nextX = 0.0f;
        float nextZ = 0.0f;
        float leftZ = 0.0f;
        float rightZ = 0.0f;
        float leftX = 0.0f;
        float rightX = 0.0f;
        float front = 0.0f;
        float back = 0.0f;
        float edgeZ = 0.0f;
        float yoff = (row - this.minRow) * this.rowDepthFull;
        if (this.binInterleave)
        {
            yoff = yoff + (binindex * this.binSeparation);
        }
        else
        {
            yoff = yoff + (binindex * this.ySceneSizeByBinWithSep);
        }
        //Stauffer - 'proto' is from protomesh prefab. It's a private global instanced above.
        GameObject newRidge = UnityEngine.Object.Instantiate(this.proto, new Vector3(this.xzySceneCorner.x, this.xzySceneCorner.y, this.xzySceneCorner.z + yoff), Quaternion.identity);
        newRidge.transform.localScale = new Vector3(this.xSceneSize, this.zSceneSize * this.currGraphHeight, this.rowDepthDataOnly);
        Mesh amesh = ((MeshFilter)newRidge.gameObject.GetComponent(typeof(MeshFilter))).mesh;
        this.xRidges[this.numRidges/*a class variable!*/] = new XRidge();
        IdentifyRidge idScript = (IdentifyRidge)newRidge.gameObject.GetComponent(typeof(IdentifyRidge));
        idScript.row = row;
        idScript.bin = binindex + this.minBin;
        GameObject newLabel = UnityEngine.Object.Instantiate(this.protolabel, new Vector3(this.xzySceneCorner.x + this.xSceneSize, this.xzySceneCorner.y + 1f, (this.xzySceneCorner.z + yoff) + (this.rowDepthDataOnly * 0.1f)), this.protolabel.transform.rotation);
        if ((row > this.numRowLabels) || (this.rowLabels[row] == null))
        {
            ((TextMesh)newLabel.GetComponent(typeof(TextMesh))).text = row.ToString();
        }
        else
        {
            ((TextMesh)newLabel.GetComponent(typeof(TextMesh))).text = this.rowLabels[row];
        }

        {
            float _39 = /*
	// The following causes an error in setting the sharedMesh whenever it traps an error
	try{
		newLabel.GetComponent(TextMesh).text = rowLabels[row];
	}
	catch(fair)
	{
		newLabel.GetComponent(TextMesh).text = row.ToString();
	}
	*/
            this.rowDepthDataOnly * 0.5f;
            Vector3 _40 = newLabel.transform.localScale;
            _40.x = _39;
            newLabel.transform.localScale = _40;
        }

        {
            float _41 = this.rowDepthDataOnly * 0.5f;
            Vector3 _42 = newLabel.transform.localScale;
            _42.y = _41;
            newLabel.transform.localScale = _42;
        }

        {
            float _43 = this.rowDepthDataOnly * 0.5f;
            Vector3 _44 = newLabel.transform.localScale;
            _44.x = _43;
            newLabel.transform.localScale = _44;
        }
        //float passColor = 0f;
        float minZ = 0.1f;
        int lastInd = numx - 1;
        float slabZ = 0.006f;
        float edgeBite = this.bevelFraction / this.numCols;
        // Note: this makes a 45 degree bevel at the curreent graph height, but it will be a different angle when height is changed.
        float topBite = (edgeBite * this.xSceneSize) / (this.zSceneSize * this.currGraphHeight);
        MeshMaker mm = new MeshMaker();
        int i = 0;
        while (i <= lastInd) //loop over columns
        {
            if ((i % 2) == 0)
            {
                front = 0f;
                back = 1f;
            }
            else
            {
                front = 0.001f;
                back = 1.001f;
            }
            thisColor = this.MakeColor(i, binindex, false);
            sideColor = this.MakeColor(i, binindex, true);
            if (i > 0)
            {
                prevX = thisX;
                prevZ = thisZ;
                thisX = nextX;
                thisZ = nextZ;
            }
            else
            {
                //Stauffer - colVals[] - seems to be just an array of column numbers. It gets shifted to always start at 0, so why bother with it???
                thisX = ((this.colVals[0] + 0.5f) - this.minCol) * this.xScale;
                //thisZ = ((this.heightVals[0] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
                thisZ = GetColumnMeshHeight(this.heightVals[0]);
                prevX = thisX - this.xScale;
                prevZ = thisZ;
            }
            if (i < lastInd)
            {
                nextX = ((this.colVals[i + 1] + 0.5f) - this.minCol) * this.xScale;
                //nextZ = ((this.heightVals[i + 1] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
                nextZ = GetColumnMeshHeight(this.heightVals[i+1]);
            }
            else
            {
                nextX = nextX + this.xScale;
            }
            leftZ = (prevZ + thisZ) / 2f;
            leftX = (prevX + thisX) / 2f;
            rightZ = (thisZ + nextZ) / 2f;
            rightX = (thisX + nextX) / 2f;
            mm.SetColor(thisColor);
            if (this.bConnectX) // ribbon
            {
                mm.Verts(leftX, leftZ, front, 1, 0);
                mm.Verts(leftX, leftZ, back, 0, 0);
                mm.Verts(thisX, thisZ, front, 1, 1);
                mm.Verts(thisX, thisZ, back, 0, 1);
                mm.Verts(rightX, rightZ, front, 1, 0);
                mm.Verts(rightX, rightZ, back, 0, 0);
                mm.Tris(0, 1, 2, 2, 1, 3, 2, 3, 4, 5, 4, 3);
                // make bottom
                if (!this.bExtendZ)
                {
                    mm.Verts(leftX, leftZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, leftZ - slabZ, back, 0, 0);
                    mm.Verts(thisX, thisZ - slabZ, front, 1, 1);
                    mm.Verts(thisX, thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, rightZ - slabZ, front, 1, 0);
                    mm.Verts(rightX, rightZ - slabZ, back, 0, 0);
                    mm.Tris(2, 1, 0, 3, 1, 2, 4, 3, 2, 3, 4, 5);
                }
                // make sides
                mm.SetColor(sideColor);
                mm.Verts(leftX, leftZ, front, 0, 1);
                mm.Verts(leftX, leftZ, back, 1, 1);
                mm.Verts(leftX, this.bExtendZ ? 0f : leftZ - slabZ, front, 0, 0);
                mm.Verts(leftX, this.bExtendZ ? 0f : leftZ - slabZ, back, 1, 0);
                mm.Verts(thisX, thisZ, front, 0.5f, 1);
                mm.Verts(thisX, thisZ, back, 0.5f, 1);
                mm.Verts(thisX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0.5f, 0);
                mm.Verts(thisX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0.5f, 0);
                mm.Verts(rightX, rightZ, front, 0, 1);
                mm.Verts(rightX, rightZ, back, 1, 1);
                mm.Verts(rightX, this.bExtendZ ? 0f : rightZ - slabZ, front, 0, 0);
                mm.Verts(rightX, this.bExtendZ ? 0f : rightZ - slabZ, back, 0, 0);
                mm.Tris(0, 4, 6, 0, 6, 2, 1, 7, 5, 1, 3, 7);
                mm.Tris(4, 10, 6, 4, 8, 10, 5, 7, 11, 5, 11, 9);
            }
            else
            {
                // tile
                if (this.doingEdges) //Seems to mean draw a bevel
                {
                    edgeZ = thisZ - topBite;
                    // draw top
                    mm.SetColor(thisColor);
                    mm.Verts(leftX + edgeBite, thisZ, front, 1, 1);
                    mm.Verts(leftX + edgeBite, thisZ, back, 0, 1);
                    mm.Verts(rightX - edgeBite, thisZ, front, 1, 1);
                    mm.Verts(rightX - edgeBite, thisZ, back, 0, 1);
                    // draw bevel
                    mm.Verts(leftX, edgeZ, front, 1, 0.9f);
                    mm.Verts(leftX, edgeZ, back, 0, 0.9f);
                    mm.Verts(rightX, edgeZ, front, 1, 0.9f);
                    mm.Verts(rightX, edgeZ, back, 0, 0.9f);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    mm.Tris(4, 2, 6, 2, 4, 0, 6, 3, 7, 3, 6, 2);
                    mm.Tris(7, 1, 5, 1, 7, 3, 5, 0, 4, 0, 5, 1);
                    // draw bottom
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Tris(2, 1, 0, 3, 1, 2);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, edgeZ, front, 0, 0.9f);
                    mm.Verts(rightX, edgeZ, front, 1, 0.9f);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(leftX, edgeZ, back, 1, 0.9f);
                    mm.Verts(rightX, edgeZ, back, 0, 0.9f);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
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
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw bottom
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, edgeZ, front, 0, 1);
                    mm.Verts(rightX, edgeZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(leftX, edgeZ, back, 1, 1);
                    mm.Verts(rightX, edgeZ, back, 0, 1);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
                }
            }
            ++i;
        }
        mm.Attach(amesh);

        try
        {
            //Stauffer
            //'sharedMesh' throws an error with #pragma strict - not a member of Collider.
            //There is a MeshCollider::sharedMesh, so my guess is that this addition of
            // amesh isn't doing anything in the original code.
            //newRidge is of type 'protomesh' prefab from project. It has a Box Collider.
            //Must be found by <Collider>, as it's presumably a sub-class. Is it also a subclass of MeshCollider? I guess not if it doesn't have sharedMesh member.
            //
            //Commenting out until can be sorted out
            //newRidge.transform.GetComponent.< Collider > ().sharedMesh = amesh;
        }
        catch
        {
            //Debug.Log("Failed to set collider mesh in dataset " + selTable + " row " + row + ", bin " + binindex);
        }
        this.xRidges[this.numRidges].AddRidge(newRidge, amesh, binindex, row);
        this.xRidges[this.numRidges++].AddLabel(newLabel);
    }

    //Stauffer - seems unused since I removed basecube member
    public virtual void MakeUnitCube(GameObject ac)
    {
        Mesh amesh = ((MeshFilter)ac.GetComponent(typeof(MeshFilter))).mesh;
        Vector3[] vertices = new Vector3[8];
        int[] triangles = new int[] { 0, 2, 1, 2, 0, 3, 1, 6, 5, 6, 1, 2, 4, 6, 7, 6, 4, 5, 4, 3, 0, 3, 4, 7, 4, 1, 5, 1, 4, 0, 3, 6, 2, 6, 3, 7 };
        Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(1, 0, 0);
        vertices[2] = new Vector3(1, 1, 0);
        vertices[3] = new Vector3(0, 1, 0);
        vertices[4] = new Vector3(0, 0, 1);
        vertices[5] = new Vector3(1, 0, 1);
        vertices[6] = new Vector3(1, 1, 1);
        vertices[7] = new Vector3(0, 1, 1);
        amesh.Clear();
        amesh.vertices = vertices;
        amesh.uv = uv;
        amesh.triangles = triangles;
        amesh.RecalculateNormals();
        amesh.RecalculateBounds();
    }

    // Stauffer - NOTE this seems to work with the currently-loaded row data,
    //   by accessing the HeatVRML class properties topVals[] and sideVals[]
    //Skipping 'bin' stuff for now
    public virtual Color NewMakeColor(DataManager.Mapping mapping, int row, int column)
    {
        bool isSide = mapping == DataManager.Mapping.SideColor;

        DataVariable var = DataManager.Instance.GetVariableByMapping(mapping);
        int colorTableID = DataManager.Instance.GetColorTableIdByMapping(mapping);
        float inv = 0f;

        float value = var.Data[row][column];
        inv = (value - var.MinValue) / var.Range;

        Color retColor;
        switch (colorTableID)
        {
            case 0:
                retColor = this.GreenRed(inv, isSide);
                break;
            case 1:
                retColor = this.Rainbow(inv, isSide);
                break;
            case 2:
                retColor = this.YellowBlue(inv, isSide);
                break;
            case 3:
                retColor = this.GrayScale(inv, isSide);
                break;
            case 4:
                retColor = this.ConstantColor(inv, isSide);
                break;
            default:
                retColor = this.GreenRed(inv, isSide);
                Debug.LogWarning("Unmatched color table ID: " + colorTableID);
                break;
        }
        return retColor;
    }




    // Stauffer - NOTE this seems to work with the currently-loaded row data,
    //   by accessing the HeatVRML class properties topVals[] and sideVals[]
    public virtual Color MakeColor(int col, int bin, bool isSide)
    {
        float inv = 0.0f;
        Color retColor = default(Color);
        int colorChoice = isSide ? this.sideColorChoice : this.topColorChoice;
        int styleChoice = isSide ? this.sideStyleChoice : this.topStyleChoice;
        // side
        if (colorChoice == 0)
        {
            // height
            inv = (this.heightVals[col] - this.minDataHeight) / this.dataHeightRange;
        }
        else
        {
            //return GreenRed(inv, isSide);
            if (colorChoice == 1)
            {
                // bin
                if (this.numBins < 2)
                {
                    inv = 0f;
                }
                else
                {
                    inv = ((bin - this.minBin) + 0f) / (this.numBins - 1);
                }
            }
            else
            {
                int thisVal = isSide ? this.sideVals[col] : this.topVals[col];
                //Stauffer this.[side|top]ColorChoice seem to be indicating which of the db record fields is being used for coloring,
                // i.e. which observational/dependent variable.
                VariableDesc thisField = isSide ? this.allVariableDescs[this.sideColorChoice] : this.allVariableDescs[this.topColorChoice];
                if ((styleChoice < 0) && thisField.ColorMap.ContainsKey(thisVal)) //Is this looking up a color via color index/lut?
                {
                    //Stauffer - thisField.Fields[n] is type Object from a hashtable, so r,g,b members not defined.
                    //So, do this via casting to get compiler happy for coversion to C#
                    OneColor oneField = thisField.ColorMap[thisVal] as OneColor;
                    return new Color(oneField.r, oneField.g, oneField.b, isSide ? 0.7f : 0.9f);
                }
                //ORIG:
                //var thisR : float = thisField.Fields[thisVal].r;
                //var thisG : float = thisField.Fields[thisVal].g;
                //var thisB : float = thisField.Fields[thisVal].b;
                //return new Color(thisR, thisG, thisB, (isSide) ? 0.7 : 0.9);
                // no specified color, so interpolate and use the default coloring method
                if (thisField.range < 0.5f)
                {
                    inv = 0f; // really if it's zero, but this is a float so test for < 0.5
                }
                else
                {
                    inv = ((thisVal - thisField.lowInt) + 0f) / thisField.range;
                }
            }
        }
        switch (styleChoice)
        {
            case 0:
                retColor = this.GreenRed(inv, isSide);
                break;
            case 1:
                retColor = this.Rainbow(inv, isSide);
                break;
            case 2:
                retColor = this.YellowBlue(inv, isSide);
                break;
            case 3:
                retColor = this.GrayScale(inv, isSide);
                break;
            case 4:
                retColor = this.ConstantColor(inv, isSide);
                break;
            default:
                retColor = this.GreenRed(inv, isSide);
                break;
        }
        return retColor;
    }

    //Color Maps
    public virtual Color GreenRed(float inv, bool isSide)
    {
        float green = 0.0f;
        float red = 0.0f;
        float trans = isSide ? 0.7f : 0.9f;
        if (inv > 0.5f)
        {
            green = 0f;
        }
        else
        {
            green = 1f - (2f * inv);
        }
        if (inv < 0.5f)
        {
            red = 0f;
        }
        else
        {
            red = (inv - 0.5f) * 2f;
        }
        return new Color(red, green, 0f, trans);
    }

    public virtual Color YellowBlue(float inv, bool isSide)
    {
        float trans = isSide ? 0.7f : 0.9f;
        return new Color(1f - inv, 1f - inv, inv, trans);
    }

    public virtual Color Spectrum(float inv, bool isSide)
    {
        float trans = isSide ? 0.7f : 0.9f;
        if (inv < 0.25f)
        {
            return new Color(0f, inv * 4f, 1f, trans);
        }
        if (inv < 0.5f)
        {
            return new Color(0f, 1f, (0.5f - inv) * 2f, trans);
        }
        if (inv < 0.75f)
        {
            return new Color((inv - 0.5f) * 4, 0f, 0f, trans);
        }
        return new Color(1f, (1f - inv) * 4f, trans);
    }

    public virtual Color Rainbow(float inv, bool isSide)
    {
        Color aColor = Colors.HSLtoColor(inv * 0.93f, 1f, 0.5f);
        return new Color(aColor.r, aColor.g, aColor.b, isSide ? 0.7f : 0.9f);
    }

    public virtual Color GrayScale(float inv, bool isSide)
    {
        return new Color(inv, inv, inv, isSide ? 0.7f : 0.9f);
    }

    public virtual Color ConstantColor(float inv, bool isSide)
    {
        return new Color(0.5f, 0.5f, 0.5f, isSide ? 0.7f : 0.9f);
    }

    public static RaycastHit hit;
    public static int dataPointMask;
    //Stauffer - NOTE this isn't called from anywhere.
    public virtual void CapturePointedAt()
    {
        this.pointedData.ready = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ray.origin = Camera.main.transform.position;
        if (!Physics.Raycast(ray, out HeatVRML.hit, Mathf.Infinity, HeatVRML.dataPointMask))
        {
            return;
        }
        this.pointedData.position = HeatVRML.hit.point;
        IdentifyRidge idScript = (IdentifyRidge)HeatVRML.hit.transform.gameObject.GetComponent(typeof(IdentifyRidge));
        this.pointedData.row = idScript.row;
        this.pointedData.bin = idScript.bin;
        // Calculate column from x position of hit
        // It is possible for this to be one too high if it hits on the right side of a column, so check for this condition
        float floatCol = (((HeatVRML.hit.point.x - this.xzySceneCorner.x) * this.numCols) / this.xSceneSize) + this.minCol;
        if (((floatCol - Mathf.Floor(floatCol)) < 0.1f) && (HeatVRML.hit.normal.x > 0.95f))
        {
            floatCol = floatCol - 1f;
        }
        this.pointedData.col = (int)Mathf.Floor(floatCol);

        Debug.LogError("Deprecated to be removed");
        /* Stauffer - removing Sqlite stuff to work on webGL build
        this.dbcmd.CommandText = ((((((("Select height from " + this.selTable) + " where row = ") + this.pointedData.row) + " and col = ") + this.pointedData.col) + " and bin = ") + this.pointedData.bin) + ";";
        Debug.Log(this.dbcmd.CommandText);
        this.reader = this.dbcmd.ExecuteReader();
        if (this.reader.Read())
        {
            this.pointedData.height = this.reader.GetFloat(0);
            this.pointedData.ready = true;
        }
        this.reader.Close();
        */
        this.ShowPointedData();
    }

    //OLD ROUTINE to be removed
    public virtual void ShowPointedData()
    {
        if (this.pointedData.ready)
        {
            float sceney = ((((this.pointedData.height - this.minDataHeight) * this.dataHeightRangeScale) * this.zSceneSize) * this.currGraphHeight) + this.xzySceneCorner.y;
            float scenex = ((((this.pointedData.col + 0.5f) - this.minCol) * this.xSceneSize) / this.numCols) + this.xzySceneCorner.x;
            float yoff = this.pointedData.row * this.rowDepthFull;
            if (this.binInterleave)
            {
                yoff = yoff + (this.pointedData.bin * this.binSeparation);
            }
            else
            {
                yoff = yoff + (this.pointedData.bin * this.ySceneSizeByBinWithSep);
            }
            float scenez = (this.xzySceneCorner.z + yoff) + (this.rowDepthDataOnly / 2f);
            this.signPost.transform.position = new Vector3(scenex, sceney, scenez);
            float signSize = Mathf.Min(this.rowDepthDataOnly, this.xSceneSize / this.numCols);
            this.signPost.transform.localScale = new Vector3(signSize, signSize, signSize);
            this.signPost.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            this.signPost.GetComponent<Renderer>().enabled = false;
        }
    }

    // OLD ROUTINE
    // debugging
    public virtual void MakeXRay()
    {
        int x = 0;
        int y = 0;
        Color pColor = default(Color);
        int maxx = Screen.width;
        int maxy = Screen.height;
        Color backColor = new Color(0, 0, 0);
        Color foreColor = new Color(1, 1, 1);
        y = 0;
        while (y < maxy)
        {
            x = 0;
            while (x < maxx)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                ray.origin = Camera.main.transform.position;
                if (!Physics.Raycast(ray, out HeatVRML.hit, Mathf.Infinity, HeatVRML.dataPointMask))
                {
                    pColor = backColor;
                }
                else
                {
                    pColor = foreColor;
                }
                this.xray.SetPixel(x / 2, y / 2, pColor);
                x = x + 2;
            }
            y = y + 2;
        }
        this.xray.Apply();
        this.showXRay = true;
    }

    public virtual void ScrollingSelected(int newScroll)
    {
        this.scrollChoice = newScroll;
        if (this.scrollChoice >= 0)
        {
            Const.menuScrolling = true;
        }
        else
        {
            //VisBins(currBin);
            Const.menuScrolling = false;
        }
    }

    public virtual void Redistribute(float newThick, float newSep, float newGap)
    {
        int i = 0;
        float yoff = 0.0f;
        float thisz = 0.0f;
        this.currDepthToWidthRatioExp = newThick;
        this.binSeparationFrac = newSep;
        this.rowGapFrac = newGap;

        this.CalcDimensions(); // Stauffer - new values for newThick, newSep, newGap get used here

        i = 0;
        while (i < this.numRidges)
        {
            yoff = (this.xRidges[i].myRow - this.minRow) * this.rowDepthFull;
            if (this.binInterleave)
            {
                yoff = yoff + (this.xRidges[i].myBin * this.binSeparation);
            }
            else
            {
                yoff = yoff + (this.xRidges[i].myBin * this.ySceneSizeByBinWithSep);
            }
            thisz = this.xzySceneCorner.z + yoff;

            {
                float _45 = thisz;
                Vector3 _46 = this.xRidges[i].trans.position;
                _46.z = _45;
                this.xRidges[i].trans.position = _46;
            }

            {
                float _47 = this.rowDepthDataOnly;
                Vector3 _48 = this.xRidges[i].trans.localScale;
                _48.z = _47;
                this.xRidges[i].trans.localScale = _48;
            }

            {
                float _49 = (this.xzySceneCorner.z + yoff) + (this.rowDepthDataOnly * 0.1f);
                Vector3 _50 = this.xRidges[i].myLabel.transform.position;
                _50.z = _49;
                this.xRidges[i].myLabel.transform.position = _50;
            }

            {
                float _51 = this.rowDepthDataOnly * 0.5f;
                Vector3 _52 = this.xRidges[i].myLabel.transform.localScale;
                _52.x = _51;
                this.xRidges[i].myLabel.transform.localScale = _52;
            }

            {
                float _53 = this.rowDepthDataOnly * 0.5f;
                Vector3 _54 = this.xRidges[i].myLabel.transform.localScale;
                _54.y = _53;
                this.xRidges[i].myLabel.transform.localScale = _54;
            }

            {
                float _55 = this.rowDepthDataOnly * 0.5f;
                Vector3 _56 = this.xRidges[i].myLabel.transform.localScale;
                _56.z = _55;
                this.xRidges[i].myLabel.transform.localScale = _56;
            }
            ++i;
        }
        //float cubeHeight = this.xSceneSize * 0.05f;
        //baseCube.transform.localScale = Vector3(xSceneSize + 2.0, cubeHeight, ySceneSizeFull + 2.0);
        //baseCube.transform.position = Vector3(xzySceneCorner.x - 1.0, xzySceneCorner.y - (cubeHeight * 0.9), xzySceneCorner.z - 1.0);
        this.ShowPointedData();
    }

    // if selBin < 0, all bins visible
    public virtual void VisBins(int selBin)
    {
        int i = 0;
        if (selBin >= 0)
        {
            this.currBin = selBin;
        }
        if (selBin >= 0)
        {
            i = 0;
            while (i < this.numRidges)
            {
                this.xRidges[i].Show(this.xRidges[i].myBin == selBin);
                ++i;
            }
        }
        else
        {
            i = 0;
            while (i < this.numRidges)
            {
                //Show all the ridges
                this.xRidges[i].Show(true);
                ++i;
            }
        }
    }


    //Stauffer
    //Return the scene/plot center in scene units.
    public Vector3 GetPlotCenter()
    {
        //Debug.Log("xzySceneCorner: " + xzySceneCorner);
        return new Vector3(xzySceneCorner.x + xSceneSize / 2f, 0f, xzySceneCorner.z + ySceneSizeFull / 2f);
    }

    IEnumerator RedrawCoroutine(bool quiet = false)
    {
        int statusID = UIManager.Instance.StatusShow("Drawing...");
        yield return null;
        NewPrepareAndDrawData(quiet);
        UIManager.Instance.StatusComplete(statusID);
    }

    /// <summary>
    /// Start redrawing the data.
    /// </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    public void Redraw(bool quiet = false)
    {
        StartCoroutine(RedrawCoroutine());
        UIManager.Instance.RefreshUI();
        //When doing UI prompts, this is the last one we do, so stop the whole process if we get here,
        // which also handles the case when user jumps ahead of the prompts to here.
        UIManager.Instance.StopAllUIActionPrompts();
    }

    /// <summary>
    /// Stauffer. New routine. Prep and draw data loaded in DataManager.
    /// Replacing functionality of DatasetSelected().
    /// Should generally not be called directly. See Redraw().
    /// </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    private void NewPrepareAndDrawData(bool quiet = false)
    {
        //TODO 
        // some verification of data, so we don't have to check things every time we access dataMgr
        // e.g. minimum variables are set (e.g. always expect a height var (or, actually maybe not??))
        string errorMsg;
        if( ! DataManager.Instance.PrepareAndVerify(out errorMsg))
        {
            if( ! quiet)
            {
                string msg = "Error with data prep and verification: \n\n" + errorMsg;
                Debug.LogError(msg);
                UIManager.Instance.ShowMessageDialog(msg);
            }
            return;
        }
        
        //Axis extents
        NewGetAxisExtents();

        //Old code for now - Clear the (old) variable (field) description list. 
        for (int i = 0; i < allVariableDescs.Length; i++)
        {
            allVariableDescs[i] = new VariableDesc(); // just to destroy old values
        }

        //Old code - at this point it checks for the optional int-valued columns/fields for top/side 
        // If found, they get setup and added to allVariableDescs
        // Also looks for custom colormap table for each additional column/field

        //Old code - the 2 required data fields (height & bin)
        this.allVariableDescs = new VariableDesc[2];
        this.allVariableDescs[0] = new VariableDesc();
        this.allVariableDescs[0].SetAsFloat("height", this.minDataHeight, this.maxDataHeight);
        this.allVariableDescs[1] = new VariableDesc();
        this.allVariableDescs[1].SetAsInt("bin", this.minBin, this.maxBin);
        this.numFields = 2;

        //Setup row headers (row labels)
        //
        this.numRowLabels = DataManager.Instance.HeightVar.hasRowHeaders ? DataManager.Instance.HeightVar.numDataRows : 0;
        this.rowLabels = new string[this.numRowLabels + 1];
        //Copy
        for( int i = 0; i < this.numRowLabels; i++)
        {
            this.rowLabels[i] = DataManager.Instance.HeightVar.rowHeaders[i];
        }

        //Draw it!
        this.NewShowData();

        //Point the camera to the middle of the plot 
        CameraManager.Instance.LookAt(GetPlotCenter());

        /* Stauffer - this code is never reached because bScrollBin never changes from its default val of false
        if (this.bScrollBin)
        {
            //Stauffer - show only ridges with this bin # (or all if currBin < 0)
            //Why is this done here? As a refresh-type action?
            this.VisBins(this.currBin);
        }
        */

        this.dataChanged = false;
        this.wantRedraw = false;
        this.waitCount = 0;
        //Stauffer - has to do with clicking on graph and showing details
        this.pointedData.ready = false;
        this.ShowPointedData();

    }

    /// <summary>
    /// Stauffer - new. 
    /// Get physical axes extents (num rows, columns and height range)
    /// </summary>
    public void NewGetAxisExtents()
    {
        //In DatasetSelected, the ranges get set for the optional subsequent int columns.
        //
        DataVariable heightVar = DataManager.Instance.HeightVar;
        this.minRow = 0;
        this.maxRow = heightVar.numDataRows - 1;
        this.minCol = 0;
        this.maxCol = heightVar.numDataCols - 1;
        this.minBin = 0; //Just always have 1 bin for now. Empirically, we want its number to be 0, otherwise a space for a phantom bin appears in render.
        this.maxBin = 0;
        this.minDataHeight = heightVar.MinValue;
        this.maxDataHeight = heightVar.MaxValue;
        this.numRows = (this.maxRow - this.minRow) + 1;
        // debugging
        if (this.maxCol > this.colLimit)
        {
            this.maxCol = this.colLimit;
        }
        this.numCols = (this.maxCol - this.minCol) + 1;
        this.numBins = (this.maxBin - this.minBin) + 1;
        this.dataHeightRange = this.maxDataHeight - this.minDataHeight;
    }
    
    
    public virtual void NewShowData()
    {
        this.drawn = true;
        if (!this.proto) // must be functioning only as user interface
        {
            return;
        }
        //Looks to be deleting old mesh/visualization
        if (this.numRidges > 0)
        {
            int iridge = 0;
            while (iridge < this.numRidges)
            {
                UnityEngine.Object.Destroy(this.xRidges[iridge].myMeshObject);
                UnityEngine.Object.Destroy(this.xRidges[iridge].myLabel);
                ++iridge;
            }
        }
        this.numRidges = 0;

        //Stauffer - this call should be fine for now with member vars we setup previously.
        //Calculates dimensions of full plot in unity scene units, among other things.
        this.CalcDimensions();

        this.xScale = 1f / this.numCols;
        this.dataHeightRangeScale = 1f / (this.maxDataHeight - this.minDataHeight);
        this.xRidges = new XRidge[this.numRows * this.numBins];
        // In case we have changed datasets
        if (this.topColorChoice >= this.numFields)
        {
            this.topColorChoice = 0;
        }
        if (this.sideColorChoice >= this.numFields)
        {
            this.sideColorChoice = 0;
        }
        //Stauffer - commented out these lines, the vars are unused. What are they for originally?
        //string extra1 = this.topColorChoice > 1 ? ", " + this.allVariableDescs[this.topColorChoice].name : ", 0";
        //string extra2 = this.sideColorChoice > 1 ? ", " + this.allVariableDescs[this.sideColorChoice].name : ", 0";

        //For each row, setup data and draw a ridge
        DataVariable hVar = DataManager.Instance.HeightVar;

        //Build the ridges
        for ( int row = 0; row < hVar.numDataRows; row++)
        {
            //NOTE - these are class properties, that then get used in BuildRidgeOld
            this.heightVals = hVar.Data[row];
            this.NewBuildRidge(row, this.numCols, this.minBin);//always one bin for now
        }
    }

    /// <summary> For the given data value (of data var assigned to height), return the *unscaled* column height, i.e. the height of the mesh before any scene scaling </summary>
    /// <param name="heightValue"></param>
    /// <returns></returns>
    public float GetColumnMeshHeight(float heightValue)
    {
        return ((heightValue - this.minDataHeight) * this.dataHeightRangeScale) + this.ridgeMeshMinHeight;
    }
    public float GetColumnSceneHeight(float heightValue)
    {
        return ( GetColumnMeshHeight(heightValue) * this.zSceneSize * this.currGraphHeight) + this.xzySceneCorner.y;
    }

    public virtual void NewBuildRidge(int row, int numx /*== num of columns*/, int binindex)
    {
        Color topColor = default(Color);
        Color sideColor = default(Color);
        float thisX = 0.0f;
        float thisZ = 0.0f;
        float prevX = 0.0f;
        float prevZ = 0.0f;
        float nextX = 0.0f;
        float nextZ = 0.0f;
        float leftZ = 0.0f;
        float rightZ = 0.0f;
        float leftX = 0.0f;
        float rightX = 0.0f;
        float front = 0.0f;
        float back = 0.0f;
        float edgeZ = 0.0f;
        float yoff = (row - this.minRow) * this.rowDepthFull;
        if (this.binInterleave)
        {
            yoff = yoff + (binindex * this.binSeparation);
        }
        else
        {
            yoff = yoff + (binindex * this.ySceneSizeByBinWithSep);
        }
        //Stauffer - 'proto' is from protomesh scene object. It's a private global instanced above.
        GameObject newRidge = UnityEngine.Object.Instantiate(this.proto, new Vector3(this.xzySceneCorner.x, this.xzySceneCorner.y, this.xzySceneCorner.z + yoff), Quaternion.identity);
        //Stauffer these vals used to set localScale are calc'ed in CalcDimensions, I believe.
        //NOTE z is used in variable names for up, i.e. y in unity.
        newRidge.transform.localScale = new Vector3(this.xSceneSize, this.zSceneSize * this.currGraphHeight, this.rowDepthDataOnly);
        Mesh amesh = ((MeshFilter)newRidge.gameObject.GetComponent(typeof(MeshFilter))).mesh;
        this.xRidges[this.numRidges/*a class variable!*/] = new XRidge();
        IdentifyRidge idScript = (IdentifyRidge)newRidge.gameObject.GetComponent(typeof(IdentifyRidge));
        idScript.row = row;
        idScript.bin = binindex + this.minBin;

        //Row labels
        GameObject newLabel = UnityEngine.Object.Instantiate(this.protolabel, new Vector3(this.xzySceneCorner.x + this.xSceneSize, this.xzySceneCorner.y + 1f, (this.xzySceneCorner.z + yoff) + (this.rowDepthDataOnly * 0.1f)), this.protolabel.transform.rotation);
        if ((row > this.numRowLabels) || (this.rowLabels[row] == null))
        {
            ((TextMesh)newLabel.GetComponent(typeof(TextMesh))).text = row.ToString();
        }
        else
        {
            ((TextMesh)newLabel.GetComponent(typeof(TextMesh))).text = this.rowLabels[row];
        }

        {
            float _39 = 
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
            this.rowDepthDataOnly * 0.5f;
            Vector3 _40 = newLabel.transform.localScale;
            _40.x = _39;
            newLabel.transform.localScale = _40;
        }

        {
            float _41 = this.rowDepthDataOnly * 0.5f;
            Vector3 _42 = newLabel.transform.localScale;
            _42.y = _41;
            newLabel.transform.localScale = _42;
        }

        {
            float _43 = this.rowDepthDataOnly * 0.5f;
            Vector3 _44 = newLabel.transform.localScale;
            _44.x = _43;
            newLabel.transform.localScale = _44;
        }
        float minZ = 0.1f;
        int lastInd = numx - 1;
        float slabZ = 0.006f;
        float edgeBite = this.bevelFraction / this.numCols;
        // Note: this makes a 45 degree bevel at the curreent graph height, but it will be a different angle when height is changed.
        float topBite = (edgeBite * this.xSceneSize) / (this.zSceneSize * this.currGraphHeight);
        MeshMaker mm = new MeshMaker();
        
        for (int colNum = 0; colNum < numx; colNum++) //loop over columns
        {
            if ((colNum % 2) == 0)
            {
                front = 0f;
                back = 1f;
            }
            else
            {
                front = 0.001f;
                back = 1.001f;
            }
            topColor = this.NewMakeColor(DataManager.Mapping.TopColor, row, colNum);
            sideColor = this.NewMakeColor(DataManager.Mapping.SideColor, row, colNum);

            //Height
            if (colNum > 0)
            {
                prevX = thisX;
                prevZ = thisZ;
                thisX = nextX;
                thisZ = nextZ;
            }
            else
            {
                thisX = ((0.5f) - this.minCol) * this.xScale;
                thisZ = ((DataManager.Instance.HeightVar.Data[row][0] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
                prevX = thisX - this.xScale;
                prevZ = thisZ;
            }
            if (colNum < lastInd)
            {
                nextX = ((colNum + 1 + 0.5f) - this.minCol) * this.xScale;
                nextZ = ((DataManager.Instance.HeightVar.Data[row][colNum + 1] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
            }
            else
            {
                nextX = nextX + this.xScale;
            }
            leftZ = (prevZ + thisZ) / 2f;
            leftX = (prevX + thisX) / 2f;
            rightZ = (thisZ + nextZ) / 2f;
            rightX = (thisX + nextX) / 2f;

            mm.SetColor(topColor);
            if (this.bConnectX) // ribbon
            {
                mm.Verts(leftX, leftZ, front, 1, 0);
                mm.Verts(leftX, leftZ, back, 0, 0);
                mm.Verts(thisX, thisZ, front, 1, 1);
                mm.Verts(thisX, thisZ, back, 0, 1);
                mm.Verts(rightX, rightZ, front, 1, 0);
                mm.Verts(rightX, rightZ, back, 0, 0);
                mm.Tris(0, 1, 2, 2, 1, 3, 2, 3, 4, 5, 4, 3);
                // make bottom
                if (!this.bExtendZ)
                {
                    mm.Verts(leftX, leftZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, leftZ - slabZ, back, 0, 0);
                    mm.Verts(thisX, thisZ - slabZ, front, 1, 1);
                    mm.Verts(thisX, thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, rightZ - slabZ, front, 1, 0);
                    mm.Verts(rightX, rightZ - slabZ, back, 0, 0);
                    mm.Tris(2, 1, 0, 3, 1, 2, 4, 3, 2, 3, 4, 5);
                }
                // make sides
                mm.SetColor(sideColor);
                mm.Verts(leftX, leftZ, front, 0, 1);
                mm.Verts(leftX, leftZ, back, 1, 1);
                mm.Verts(leftX, this.bExtendZ ? 0f : leftZ - slabZ, front, 0, 0);
                mm.Verts(leftX, this.bExtendZ ? 0f : leftZ - slabZ, back, 1, 0);
                mm.Verts(thisX, thisZ, front, 0.5f, 1);
                mm.Verts(thisX, thisZ, back, 0.5f, 1);
                mm.Verts(thisX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0.5f, 0);
                mm.Verts(thisX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0.5f, 0);
                mm.Verts(rightX, rightZ, front, 0, 1);
                mm.Verts(rightX, rightZ, back, 1, 1);
                mm.Verts(rightX, this.bExtendZ ? 0f : rightZ - slabZ, front, 0, 0);
                mm.Verts(rightX, this.bExtendZ ? 0f : rightZ - slabZ, back, 0, 0);
                mm.Tris(0, 4, 6, 0, 6, 2, 1, 7, 5, 1, 3, 7);
                mm.Tris(4, 10, 6, 4, 8, 10, 5, 7, 11, 5, 11, 9);
            }
            else
            {
                // tile
                if (this.doingEdges) //Seems to mean draw a bevel
                {
                    edgeZ = thisZ - topBite;
                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(leftX + edgeBite, thisZ, front, 1, 1);
                    mm.Verts(leftX + edgeBite, thisZ, back, 0, 1);
                    mm.Verts(rightX - edgeBite, thisZ, front, 1, 1);
                    mm.Verts(rightX - edgeBite, thisZ, back, 0, 1);
                    // draw bevel
                    mm.Verts(leftX, edgeZ, front, 1, 0.9f);
                    mm.Verts(leftX, edgeZ, back, 0, 0.9f);
                    mm.Verts(rightX, edgeZ, front, 1, 0.9f);
                    mm.Verts(rightX, edgeZ, back, 0, 0.9f);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    mm.Tris(4, 2, 6, 2, 4, 0, 6, 3, 7, 3, 6, 2);
                    mm.Tris(7, 1, 5, 1, 7, 3, 5, 0, 4, 0, 5, 1);
                    // draw bottom
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Tris(2, 1, 0, 3, 1, 2);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, edgeZ, front, 0, 0.9f);
                    mm.Verts(rightX, edgeZ, front, 1, 0.9f);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(leftX, edgeZ, back, 1, 0.9f);
                    mm.Verts(rightX, edgeZ, back, 0, 0.9f);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
                }
                else
                {
                    edgeZ = thisZ;
                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(leftX, edgeZ, front, 1, 1);
                    mm.Verts(leftX, edgeZ, back, 0, 1);
                    mm.Verts(rightX, edgeZ, front, 1, 1);
                    mm.Verts(rightX, edgeZ, back, 0, 1);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw bottom
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(leftX, edgeZ, front, 0, 1);
                    mm.Verts(rightX, edgeZ, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(leftX, edgeZ, back, 1, 1);
                    mm.Verts(rightX, edgeZ, back, 0, 1);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
                }
            }
        }
        mm.Attach(amesh);

        // Set the mesh as the collider mesh for use in DataInspector
        // NOTE - this slows things down noticably on large data files (60% slower on 1000x1000 data set)
        // See notes in dev doc about MeshCollider Cooking Options (in short, use 'none' for much better speed - seems to work well)
        newRidge.transform.GetComponent<MeshCollider>().sharedMesh = amesh;

        this.xRidges[this.numRidges].AddRidge(newRidge, amesh, binindex, row);
        this.xRidges[this.numRidges++].AddLabel(newLabel);
    }


    public virtual void DatasetSelected(string newDB, bool newBConnectX, bool newBExtendZ, bool newBInterpolateY, int newTopColorChoice, int newSideColorChoice, float newGraphHeight)
    {
        int i = 0;
        int thisField = 0;
        //Debug.Log("DatasetSelected " + newDB);
        //Stauffer - NOTE that all these input params, from both calls to this method, are just
        // the same class props that they're assigned to. Useless.
        this.selTable = newDB;
        this.bConnectX = newBConnectX;
        this.bExtendZ = newBExtendZ;
        this.binInterleave = newBInterpolateY;
        this.topColorChoice = newTopColorChoice;
        this.sideColorChoice = newSideColorChoice;
        this.currGraphHeight = newGraphHeight;
        i = 0;
        while (i < this.numDB)
        {
            if (this.dbChoices[i] == newDB)
            {
                this.currDB = i;
            }
            ++i;
        }

        //Stauffer - this simply get the value range for row, col, bin numbers, and height values. Simple
        this.GetAxisExtentsOld();

        // Find the fields in this database
        //
        //Stauffer - this first step looks to be getting the names of *extra* fields, ie the optional int fields past
        // the required row, col, bin and height fields

        List<string> nameArray = new List<string>();

        Debug.LogError("Deprecated to be removed");
        /* Stauffer - removing Sqlite stuff to work on webGL build
        
        this.dbcmd.CommandText = ("PRAGMA table_info(" + this.selTable) + ");";
        this.reader = this.dbcmd.ExecuteReader();
        while (this.reader.Read())
        {
            string fname = this.reader.GetString(1);
            string fieldtype = this.reader.GetString(2);
            if (fieldtype != "INTEGER")
            {
                continue;
            }
            if (fname == "row")
            {
                continue;
            }
            if (fname == "col")
            {
                continue;
            }
            if (fname == "bin")
            {
                continue;
            }
            nameArray.Add(fname);
    

        }
        this.reader.Close();
        */

        //Stauffer - now it's setting up the field (variable) description array with bin, height, and any additional fields it found above
        thisField = 0;
        while (thisField < this.numFields)
        {
            this.allVariableDescs[thisField] = new VariableDesc(); // just to destroy old values
            ++thisField;
        }

        //Stauffer - the 2 required data fields (height & bin)
        this.allVariableDescs = new VariableDesc[2 + nameArray.Count];
        this.allVariableDescs[0] = new VariableDesc();
        this.allVariableDescs[0].SetAsFloat("height", this.minDataHeight, this.maxDataHeight);
        this.allVariableDescs[1] = new VariableDesc();
        this.allVariableDescs[1].SetAsInt("bin", this.minBin, this.maxBin);
        this.numFields = 2;

        //Stauffer - for each additional field beyond the required ones...
        // REMEMBER that a field/column here in the db format is a variable (i.e. colleciton of observations for a particular class/type/param)
        foreach (string fieldname in nameArray)
        {
            /* Stauffer - removing Sqlite stuff to work on webGL build

            this.dbcmd.CommandText = ((((("SELECT MIN(" + fieldname) + "), MAX(") + fieldname) + ") from ") + this.selTable) + ";";
            //Debug.Log("trying " + dbcmd.CommandText);
            this.reader = this.dbcmd.ExecuteReader();
            this.reader.Read();
            VariableDesc intField = new VariableDesc();

            //Set the field's name and int range
            intField.SetAsInt(fieldname as string, this.reader.GetInt32(0), this.reader.GetInt32(1));

            this.reader.Close();

            //Stauffer - look for table with color map for the optional int fields
            //If found, it gets stored in the variable description
            string infoTable = (("heatfield_" + this.selTable.Substring(5)) + "_") + fieldname;

            try
            {
                this.dbcmd.CommandText = ("SELECT value, r, g, b, name from " + infoTable) + ";";
                //Debug.Log(dbcmd.CommandText);
                this.reader = this.dbcmd.ExecuteReader();
                //Stauffer - each row in the heatfield_* table is functinally a colormap entry
                while (this.reader.Read())
                {
                    OneColor oneF = new OneColor();
                    oneF.r = this.reader.GetInt32(1) / 255f;
                    oneF.g = this.reader.GetInt32(2) / 255f;
                    oneF.b = this.reader.GetInt32(3) / 255f;
                    oneF.name = this.reader.GetString(4);
                    int value = this.reader.GetInt32(0);
                    intField.ColorMap[value] = oneF; //Stauffer - hashtable with key 'value' and value 'oneF, not an array or list
                    //Debug.Log("setting Fields of " + value + " to " + oneF.r + " " + oneF.g + " " + oneF.b + " " + oneF.name);
                }
                this.reader.Close();
            }
            catch
            {
                //no color map table found
            }
            this.allVariableDescs[this.numFields++] = intField;
            */
        }

        //Look for table with row labels and parse if found
        string rowtable = "heatrows_" + this.selTable.Substring(5);
          
        /* Stauffer - removing Sqlite stuff to work on webGL build
        this.dbcmd.CommandText = ("SELECT count(*) from " + rowtable) + ";";
        
        try
        {
            this.numRowLabels = UnityScript.Lang.UnityBuiltins.parseInt(this.dbcmd.ExecuteScalar() as string);
            this.dbcmd.CommandText = ("SELECT row, name from " + rowtable) + ";";
            this.rowLabels = new string[this.numRowLabels + 1];
            this.reader = this.dbcmd.ExecuteReader();
            while (this.reader.Read())
            {
                this.rowLabels[this.reader.GetInt32(0)] = this.reader.GetString(1);
            }
            this.reader.Close();
        }
        catch
        {
            this.numRowLabels = 0;
            this.rowLabels = new string[1];
        }
        */

        //Draw it!
        this.ShowDataOld();

        if (this.bScrollBin)
        {
            //Stauffer - show only ridges with this bin # (or all if currBin < 0)
            //Why is this done here? As a refresh-type action?
            this.VisBins(this.currBin);
        }

        this.dataChanged = false;
        this.wantRedraw = false;
        this.waitCount = 0;
        //Stauffer - has to do with clicking on graph and showing details
        this.pointedData.ready = false;
        this.ShowPointedData();
    }

    /// <summary>
    /// Set new graph height value.
    /// Does NOT require a redraw of the graph since it scales the ridges using transform scaling.
    /// See SetNewGraphheight to adjust using [0,1] value.
    /// </summary>
    /// <param name="newGraphHeight"></param>
    public virtual void GraphHeightSelected(float newGraphHeight)
    {
        this.currGraphHeight = newGraphHeight;
        this.ScaleRidges(this.currGraphHeight);
        this.ShowPointedData();
    }

    /// <summary>
    /// Stauffer added
    /// Let's us call this func from UI with a [0,1] fractional value
    /// </summary>
    /// <param name="frac"></param>
    public virtual void SetNewGraphHeight(float frac)
    {
        float newHeight = this.lowGraphHeightRange + (this.highGraphHeightRange - this.lowGraphHeightRange) * frac;
        GraphHeightSelected(newHeight);
    }

    public virtual void FOVSelected(float newFOV)
    {
        this.currFOV = newFOV;
        this.myCameraOld.fieldOfView = this.lowFOVRange + (this.highFOVRange - this.currFOV);
    }

    public virtual void LookSelected(int newLook)
    {
        switch (newLook)
        {
            case 0:
                Fly.NewRotation(0f, -90f);
                break;
            case 1:
                Fly.NewRotation(0f, 0f);
                break;
            case 2:
                Fly.NewRotation(180f, 0f);
                break;
            case 3:
                Fly.NewRotation(-90f, 0f);
                break;
            case 4:
                Fly.NewRotation(90f, 0f);
                break;
        }
    }

    public virtual void ZipSelected(int newZip)
    {
        float myX = 0.0f;
        float myY = 0.0f;
        float myZ = 0.0f;
        float hFOV = 0.0f;
        hFOV = Mathf.Atan((Screen.width * Mathf.Tan((this.myCameraOld.fieldOfView * Mathf.PI) / 360f)) / Screen.height);
        switch (newZip)
        {
            case 0:
                myY = this.xzySceneCorner.z + (this.ySceneSizeByBin / 2f);
                myX = this.xzySceneCorner.x + (this.xSceneSize / 2f);
                //Debug.Log("xzySceneCorner.y is " + xzySceneCorner.y);
                //Debug.Log("(zSceneSize * currGraphHeight) is " + (zSceneSize * currGraphHeight));
                //Debug.Log("myCameraOld.fieldOfView is " + myCameraOld.fieldOfView);
                //Debug.Log("Mathf.Tan(myCameraOld.fieldOfView / 2.0) is " + Mathf.Tan(myCameraOld.fieldOfView * Mathf.PI / 360.0));
                myZ = (this.xzySceneCorner.y + (this.zSceneSize * this.currGraphHeight)) + ((this.ySceneSizeByBin / 2f) / Mathf.Tan((this.myCameraOld.fieldOfView * Mathf.PI) / 360f));
                //Debug.Log("myZ is " + myZ);
                Fly.NewRotation(0f, -90f);
                break;
            case 1:
                myX = this.xzySceneCorner.x + (this.xSceneSize / 2f);
                myZ = this.xzySceneCorner.y + ((this.zSceneSize * this.currGraphHeight) / 2f);
                myY = this.xzySceneCorner.z - ((this.xSceneSize / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(0f, 0f);
                break;
            case 2:
                myX = this.xzySceneCorner.x + (this.xSceneSize / 2f);
                myZ = this.xzySceneCorner.y + ((this.zSceneSize * this.currGraphHeight) / 2f);
                myY = (this.xzySceneCorner.z + this.ySceneSizeByBin) + ((this.xSceneSize / 2f) / Mathf.Tan(hFOV));
                if ((this.numBins > 1) && !this.binInterleave)
                {
                    myY = myY + (this.ySceneSizeByBin * (this.numBins - 1));
                }
                Fly.NewRotation(180f, 0f);
                break;
            case 3:
                myY = this.xzySceneCorner.z + (this.ySceneSizeByBin / 2f);
                myZ = this.xzySceneCorner.y + ((this.zSceneSize * this.currGraphHeight) / 2f);
                myX = this.xzySceneCorner.x - ((this.ySceneSizeByBin / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(90f, 0f);
                break;
            case 4:
                myY = this.xzySceneCorner.z + (this.ySceneSizeByBin / 2f);
                myZ = this.xzySceneCorner.y + ((this.zSceneSize * this.currGraphHeight) / 2f);
                myX = (this.xzySceneCorner.x + this.xSceneSize) + ((this.ySceneSizeByBin / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(-90f, 0f);
                break;
        }
        this.myController.transform.position = new Vector3(myX, myZ, myY);
    }

    private StreamWriter vrout;
    private float vrmlScale;
    public virtual void DrawVRML()
    {
        int r = 0;
        this.vrmlScale = this.vrmlModelMM / (this.ySceneSizeFull > this.xSceneSize ? this.ySceneSizeFull : this.xSceneSize);
        this.vrout = File.CreateText("heat.wrl");
        this.vrout.WriteLine("#VRML V2.0 utf8");
        this.vrout.WriteLine("Group { children [");
        r = 0;
        while (r < this.numRidges)
        {
             //Debug.Log("Ridge " + r);
            this.WriteVRMLMesh(this.xRidges[r].myMesh, this.xRidges[r].trans, false);
            ++r;
        }
        //WriteVRMLMesh(baseCube.GetComponent(MeshFilter).mesh, baseCube.transform, true);
        this.vrout.WriteLine("]}"); // close children and Group
        this.vrout.Close();
        this.wantVRML = false;
        this.waitCount = 0;
    }

    private StreamWriter trout;
    public virtual void DrawTriangle()
    {
        int r = 0;
        Debug.Log("in DrawTriangle");
        this.vrmlScale = this.vrmlModelMM / (this.ySceneSizeFull > this.xSceneSize ? this.ySceneSizeFull : this.xSceneSize);
        this.trout = File.CreateText("triangles.txt");
        r = 0;
        while (r < this.numRidges)
        {
             //Debug.Log("Ridge " + r);
            this.WriteTriangleMesh(this.xRidges[r].myMesh, this.xRidges[r].trans, false);
            ++r;
        }
        this.trout.Close();
        this.wantTriangles = false;
        this.waitCount = 0;
    }

    public virtual void WriteTriangleMesh(Mesh amesh, Transform trans, bool makeColors)
    {
        Color[] colors = amesh.colors;
        Vector3[] vertices = amesh.vertices;
        int[] triangles = amesh.triangles;
        int numVerts = vertices.Length;
        int numTris = triangles.Length;
        int thisTri = 0;
        string cstring = null;
        thisTri = 0;
        while (thisTri < numTris)
        {
            int thisVertex = triangles[thisTri];
            Color thisColor = colors[thisVertex];
            cstring = ((((thisColor.r + " ") + thisColor.g) + " ") + thisColor.b) + " ";
            int corner = 0;
            while (corner < 3)
            {
                if (corner > 0)
                {
                    cstring = cstring + " ";
                }
                thisVertex = triangles[thisTri++];
                Vector3 apos = trans.TransformPoint(vertices[thisVertex]);
                apos = apos - new Vector3(this.xzySceneCorner.x, this.xzySceneCorner.y, this.xzySceneCorner.z);
                apos = apos * this.vrmlScale;
                cstring = cstring + ((((apos.x + " ") + apos.y) + " ") + apos.z);
                ++corner;
            }
            this.trout.WriteLine(cstring);
        }
    }

    public virtual void WriteVRMLMesh(Mesh amesh, Transform trans, bool makeColors)
    {
        int avert = 0;
        int thisp = 0;
        this.vrout.WriteLine("Transform {");
        this.vrout.WriteLine("children Shape {");
        this.vrout.WriteLine("geometry IndexedFaceSet {");
        Color[] colors = amesh.colors;
        Vector3[] vertices = amesh.vertices;
        int[] triangles = amesh.triangles;
        int numVerts = vertices.Length;
        int numTris = triangles.Length;
        //numVerts = 6;
        //numTris = 18;
        this.vrout.WriteLine("coord Coordinate {");
        string cstring = "point [";
        avert = 0;
        while (avert < numVerts)
        {
            Vector3 apos = trans.TransformPoint(vertices[avert]);
            apos = apos - new Vector3(this.xzySceneCorner.x, this.xzySceneCorner.y, this.xzySceneCorner.z);
            apos = apos * this.vrmlScale;
            if (avert > 0)
            {
                cstring = cstring + ", ";
            }
            cstring = cstring + ((((apos.x + " ") + apos.y) + " ") + apos.z);
            ++avert;
        }
        cstring = cstring + "]";
        this.vrout.WriteLine(cstring);
        this.vrout.WriteLine("}"); // closes Coordinate
        cstring = "coordIndex [";
        thisp = 0;
        while (thisp < numTris)
        {
            cstring = cstring + (" " + triangles[thisp++]);
            cstring = cstring + (" " + triangles[thisp++]);
            cstring = cstring + (" " + triangles[thisp++]);
            cstring = cstring + " -1";
        }
        cstring = cstring + "]";
        this.vrout.WriteLine(cstring);
        cstring = "color Color { color [";
        //Debug.Log("Number of vertices " + numVerts);
        avert = 0;
        while (avert < numVerts)
        {
             //Debug.Log("vertex " + avert);
            if (avert > 0)
            {
                cstring = cstring + ", ";
            }
            if (makeColors)
            {
                 //cstring += "0.5 0.5 0.5, 0.5 0.5 0.5, 0.5 0.5 0.5";
                cstring = cstring + "0.5 0.5 0.5";
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
        cstring = cstring + (colors[avert].r + " ");
                cstring = cstring + (colors[avert].g + " ");
                cstring = cstring + colors[avert].b;
            }
            ++avert;
        }
        cstring = cstring + ", 0 0 0";
        cstring = cstring + " ]}";
        this.vrout.WriteLine(cstring);
        this.vrout.WriteLine("}"); // closes IndexedFaceSet
        this.vrout.WriteLine("}"); // closes Shape
        this.vrout.WriteLine("}"); // closes Transform
    }

    public HeatVRML()
    {
        this.programVersion = "1.2";
        this.STYLEwin = 1;
        this.ZIPwin = 2;
        this.SLwin = 3;
        this.NUMwin = 4;
        this.isLayout = new bool[] {true, true, true, true};
        this.dsRect = new Rect(2, 2, 120, 100);
        this.styleRect = new Rect(130, 2, 120, 40);
        this.zipRect = new Rect(258, 2, 120, 40);
        this.slidersRect = new Rect(258, 2, 120, 40);
        this.pointedWindowRect = new Rect(0, 300, 120, 60);
        this.xrayWindowRect = new Rect(200, 200, 800, 800);
        this.helpWindowRect = new Rect(10, 10, 10, 10);
        this.windNames = new string[] {"Data Selection", "Chart Style", "Zip to Viewpoint", "Chart View"};
        this.xSceneSize = 400f;
        this.ySceneSizeByBin = 400f;
        this.ySceneSizeApproxMax = 2f * this.xSceneSize;
        this.zSceneSize = 200f;
        this.xzySceneCorner = new Vector3(0, 0, 0);
        this.dbPos = new Vector2(0, 0);
        this.currDB = -1;
        this.includeTriangles = true;
        this.bExtendZ = true;
        this.bShowLabels = true;
        //this.shaveCount = 3;
        this.minScrollSecs = 0.1f;
        this.choiceFOV = 1;
        this.choiceThick = 2;
        this.choiceSep = 3;
        this.choiceGap = 4;
        this.choiceBin = 5;
        this.scrollChoice = -1;
        this.lowGraphHeightRange = 0.002f;
        this.highGraphHeightRange = 1f;
        this.currGraphHeight = 0.5f;
        this.lowFOVRange = 20f;
        this.highFOVRange = 170f;
        this.lowDepthToWidthRatioRange = -4f;
        this.highDepthToWidthRatioRange = 4f;
        this.highSepRange = 4f;
        this.binSeparationFrac = 1.1f;
        this.highGapRange = 4f;
        this.rowGapFrac = 1f;
        this.pointedData = new PointedData();
        this.pixPerLine = 25;
        this.rowHeight = 25;
        this.colWidth = 100;
        this.lineWidth = 20;
        this.scrollHeight = 112;
        this.showHelp = false;
        this.allHidden = true; //stauffer add - don't show old gui
        this.oldHelpPage = 1;
        this.menuScrollPos = new Vector2(0f, 0f);
        this.vrmlModelMM = 200f;
        this.colLimit = 32000;
        this.doingEdges = true;
        this.bevelFraction = 0.05f;
    }

    static HeatVRML()
    {
        HeatVRML.dataPointMask = 1 << 8;
    }

}