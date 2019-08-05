using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SMView;

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

    /// <summary> Object to conatain graph elements so they can be manipulated together </summary>
    public GameObject graphContainer;

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
    /// <summary>
    /// Stauffer - plotting area width in scene units. Row width. Seems hardcoded
    /// </summary>
    public float sceneWidth;
    /// <summary> Stauffer - full plotting area DEPTH in scene units, including possible multiple bins and bin increment
    private float sceneDepthFull;
    /// <summary> Stauffer - added this. Max size of full plot in Y scene units of (i.e. Unity Z) dim, i.e. depth when viewed from default camera positiona long unity z axis.
    /// Added this to constrain row depth when data has many more rows than columns. </summary>
    private float sceneDepthMaxApprox;
    /// <summary> Stauffer - plotting area DEPTH in scene units, for single bin (or for all bins when interleaved) </summary>
    public float sceneDepthByBin;
    /// <summary> Stauffer - plotting area HEIGHT in scene units. </summary>
    public float sceneHeight;
    /// <summary> Stauffer - starting corner of plot area in scene units. The left/front, 1st row/1st column. </summary>
    public Vector3 sceneCorner
    {
        get { return _sceneCorner; }
        set
        {
            //NOTE - made a setter so I could put a breakpoint here and see where the var's value
            // was getting changed inexplicably. However with the setter now, the value isn't getting
            // changed, so go figure.
            //Debug.Log("sceneCorner setter! " + value.ToString("F2"));
            _sceneCorner.Set(value.x, value.y, value.z);
        }
    }
    private Vector3 _sceneCorner;
    /// <summary> Stauffer - separtion in scene units between bins, whether bins are interleaved or not. So if not interleaved,
    /// it's separation between groups of rows of each bin. If interleaved, this is separation between each row (different than rowGap, however). </summary>
    public float binSeparation;
    /// <summary> Stauffer - full scene depth of each row, including gap beetween rows (and bin separation when interleaved) </summary>
    public float rowDepthFull;
    /// <summary> Stauffer - plotting area DEPTH in scene units, for single bin (or for all bins when interleaved) INCLUDING gap between bin groups </summary>
    public float sceneDepthByBinWithSep;

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
    public GameObject Proto { get { return proto; } }
    //private GameObject baseCube; Stauffer - seems unused
    private GameObject protolabel;
    private bool drawn;
    private XRidge[] xRidges;
    //private int shaveCount; Stauffer - never used
    private int numRidges;
    /// <summary> Stauffer added. Minimum height of *mesh* used to make a ridge. Not minimum scene height - see MinGraphSceneHeight for that </summary>
    private float ridgeMeshMinHeight;
    /// <summary> Depth (unity z-dimension) of data representation, i.e. the depth of the tower representing the data point. Does NOT include gap between rows. </summary>
    public float rowDepthDataOnly;
    /// <summary> Normalized scene width of each block (column of data). </summary>
    private float blockWidthNorm;
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
    public int numRows; //should give these a read-only accessor
    public int numCols;
    private int numBins;
    private int minMarker;
    private int maxMarker;
    private int numMarkers;
    private int currBin;
    private bool bScrollBin = false; //Stauffer - compiler says this val never changes from default of false, so set it to false explicitly
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
    /// <summary> The lowest height scaling factor allowed. Must be above 0. Actually ridge height gets scaled by sceneHeight </summary>
    private float lowGraphHeightScaleRange;
    /// <summary> The largest height scaling factor allowed. Actually ridge height gets scaled by sceneHeight </summary>
    private float highGraphHeightScaleRange;
    /// <summary> Current scaling of height of bar/ridges, within min/max of range, added to abs min height</summary>
    public float currGraphHeightScale;
    /// <summary> Stauffer added - The current graph display height as a fractional value, for use with UI </summary>
    public float CurrGraphHeightFrac
    {
        //Uses Simple Model View
        get { return SMV.Instance.GetValueFloat(SMVmapping.GraphHeightFrac); }
        set { SMV.Instance.SetValue(SMVmapping.GraphHeightFrac, value); }
    }
    public float bevelFraction;


    /// <summary> 
    /// Stauffer added - Absolute minimum of graph height so bars/ridges don't get so short we can't see side colors.
    ///
    /// NOTE - BUG WORKAROUND - the property returns 0 when the # of columns is > 18, because there's an odd bug when
    ///   # of columns > 18 that makes the minGraphSceneHeight not get added to the vertices in the shader.
    ///   I checked the mesh creation, and the vert heights (used in shader to determine which verts get min height added) look
    ///   the same for # of columns > 18 or not. Very weird.
    ///   This results in two symptoms:
    ///     - when currGraphHeightScale gets down close to 0, all buildings go flat and there's no min height to view the sides.
    ///     - when DataInspector draws the highlight box around the selected bar, it's far too high, espeically at lower height scales.
    ///   So this workaround is in here to avoid the second symptom, but can't address the first.
    /// </summary>
    private float _minGraphSceneHeight;
    private float MinGraphSceneHeight
    {
        set { _minGraphSceneHeight = value; }
        get { return DataManager.Instance.Cols > 18 ? 0f : _minGraphSceneHeight; }
    }

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
    private float highSepRange = 0f; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    /// <summary> Stauffer - fractional value applied to rowDepthDataOnly to calculate gap between rows. Separate from binSeparation </summary>
    private float rowGapFrac;
    private float lowGapRange = 0f; //Stauffer - compiler says this val never changes from default of 0, so set it to 0 explicitly
    private float highGapRange;

    private string[] rowLabels;
    private int numRowLabels;

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
    public bool includeTriangles;

    // Layout
    public int pixPerLine;
    public int rowHeight;
    //public int colWidth; stauffer - not used
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
    //public Camera myCameraOld; //Stauffer - add declare as type Camera
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
        //Stauffer - move some things here that use SimpleModelView, because it
        // cannot be init'ed in HeatVRML ctor
        //Also adding variables that I've added to keep them separate

        this.CurrGraphHeightFrac = 0.5f;
        this.MinGraphSceneHeight = 1.5f;

        //Uses a property now for debugging, so can't be in ctor
        this.sceneCorner = new Vector3(0, 0, 0);
    }

    //
    public virtual void Start()
    {
        
        // Find the prototype mesh object, if present
        this.proto = GameObject.Find("protomesh_SceneObj");
        if (this.proto == null)
            Debug.LogError("Failed to find object for proto ridge");

        //this.baseCube = GameObject.Find("basecube");
        //this.MakeUnitCube(this.baseCube);
        this.protolabel = GameObject.Find("protolabel");

        /* Stauffer - removing Sqlite stuff to work on webGL build
        this.connection = new SqliteConnection(this.connStrn);
        this.connection.Open();
        this.dbcmd = this.connection.CreateCommand();
        */
        //this.myCameraOld = GameObject.FindWithTag("MainCamera").GetComponent("Camera") as Camera;
        //Stauffer - change this to my new camera for now while new camera controls are implemented
        //this.myCameraOld = GameObject.Find("Camera").GetComponent<Camera>() as Camera;
        //this.currFOV = (this.lowFOVRange + this.highFOVRange) - this.myCameraOld.fieldOfView; // hokey, but we want currFOV to increase as fieldOfView decreases
        this.myController = GameObject.Find("FPC");
        this.allVariableDescs = new VariableDesc[2];
        this.allVariableDescs[0] = new VariableDesc();
        this.allVariableDescs[0].SetAsFloat("height", 0f, 10f);
        this.allVariableDescs[1] = new VariableDesc();
        this.allVariableDescs[1].SetAsInt("bin", 0, 2);
        this.xray = new Texture2D(Screen.width / 2, Screen.height / 2);
        //SpreadBalls(16, 10.0);

        //Stauffer
        //
        this.ridgeMeshMinHeight = 0.1f;
        //Quick intro message with instructions
        UIManager.Instance.ShowIntroMessage();
    }

    public virtual void Update()
    {

    }

    /// <summary>
    /// Stauffer added for testing
    /// </summary>
    public virtual void TranslateRidges(float x, float y, float z, float maxy)
    {
        if (xRidges == null)
            return;

        //Constrain yposition
        float newy = sceneCorner.y + y;
        if (newy < 0 || newy > maxy)
            y = 0;

        //NOTE - should put these in graphContainer and move all together
        foreach (XRidge xr in this.xRidges)
        {
            xr.Translate(x, y, z);
        }

        //Update the scene corner state
        //NOTE - should maybe fold this into graphContainer
        sceneCorner = sceneCorner + new Vector3(x, y, z);

        //Update the graphContainer position
        graphContainer.transform.position = graphContainer.transform.position + new Vector3(x, y, z);

        //Needs some updating
        UpdateSceneDrawingParams();
    }

    public virtual void ScaleRidges(float frac)
    {
        int i = 0;
        float newSize = frac * this.sceneHeight;
        i = 0;
        while (i < this.numRidges)
        {
            this.xRidges[i].NewHeight(newSize);
            ++i;
        }
    }

    public virtual void CalcSceneDimensions()//	baseCube.transform.position = Vector3(sceneCorner.x - 1.0, sceneCorner.y - (cubeHeight * 0.9), sceneCorner.z - 1.0);
    {
        //Stauffer sceneWidth is set to fixed val (400) during init
        this.rowDepthDataOnly = (this.sceneWidth * Mathf.Pow(2, this.currDepthToWidthRatioExp)) / this.numCols;

        //Update this in case sceneWidth has changed
        this.sceneDepthMaxApprox = 4f * this.sceneWidth;

        //Constrain row depth to not exceed max total depth of plot. This is necessary for data that has many more
        // rows than columns
        if (this.numRows > (int)(this.numCols * 1.25f))
        {
            this.rowDepthDataOnly = this.sceneDepthMaxApprox / (this.numRows * (1f + this.rowGapFrac));
        }
        //Hack in here that row depth shouldn't be < some proportion of block width
        rowDepthDataOnly = Mathf.Max(rowDepthDataOnly, 0.6f * GetBlockSceneWidth());

        this.binSeparation = this.rowDepthDataOnly * this.binSeparationFrac;
        //Stauffer - this flag seems to control interleaving rows by bin or not
        if (this.binInterleave)
        {
            //Show bins interleaved
            this.rowDepthFull = (this.binSeparation * (this.numBins - 1)) + ((1f + this.rowGapFrac) * this.rowDepthDataOnly);
            this.sceneDepthByBinWithSep = (this.rowDepthFull * this.numRows) - (this.rowGapFrac * this.rowDepthDataOnly);
            this.sceneDepthByBin = this.sceneDepthByBinWithSep;
            this.sceneDepthFull = this.sceneDepthByBin;
        }
        else
        {
            //Show bins as separate groups of rows
            this.rowDepthFull = (1f + this.rowGapFrac) * this.rowDepthDataOnly;
            //sceneDepthByBinWithSep = (rowDepthFull * numRows) + (binSeparationFrac * tokenWidth);
            //sceneDepthByBin = rowDepthFull * numRows - rowGapFrac;
            //sceneDepthFull = (sceneDepthByBinWithSep * numBins) - (binSeparationFrac * tokenWidth);
            this.sceneDepthByBin = this.rowDepthDataOnly * (this.numRows + ((this.numRows - 1) * this.rowGapFrac));
            this.sceneDepthByBinWithSep = this.sceneDepthByBin + this.binSeparation;
            this.sceneDepthFull = (this.sceneDepthByBin * this.numBins) + ((this.numBins - 1) * this.binSeparation);
        }
        //float cubeHeight = this.sceneWidth * 0.05f;
    }

    //Stauffer - seems to determine if a bevel is drawn at top of block
    // Made it a getter/setter so I could put a breakpoint on it to see when it changes, cuz
    // when I set it to false in the ctor below, it was getting set back to true somewhere that
    // I couldn't see in the code. But weirdly, when I made this getter/setter and put a breakpoint
    // on 'set', it didn't change back to true.
    public bool doingEdges
    {
        get;
        set;
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

        int colorTableID = DataManager.Instance.GetColorTableIdByMapping(mapping);

        float value = DataManager.Instance.GetValueByMapping(mapping, row, column, true);
        DataVariable var = DataManager.Instance.GetVariableByMapping(mapping);
        float inv = (value - var.MinValue) / var.Range;

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
                retColor = this.GrayScale(inv, isSide);
                Debug.LogWarning("Unmatched color table ID: " + colorTableID);
                break;
        }
        return retColor;
    }



#if false
    old code to remove
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
#endif

    //Color Maps
    public virtual Color GreenRed(float inv, bool isSide)
    {
        float green = 0.0f;
        float red = 0.0f;
        float trans = 1;// isSide ? 0.7f : 0.9f;
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
        float trans = 1;// isSide ? 0.7f : 0.9f;
        return new Color(1f - inv, 1f - inv, inv, trans);
    }

    public virtual Color Spectrum(float inv, bool isSide)
    {
        float trans = 1;// isSide ? 0.7f : 0.9f;
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
        return new Color(aColor.r, aColor.g, aColor.b, 1);// isSide ? 0.7f : 0.9f);
    }

    public virtual Color GrayScale(float inv, bool isSide)
    {
        return new Color(inv, inv, inv, 1);// isSide ? 0.7f : 0.9f);
    }

    public virtual Color ConstantColor(float inv, bool isSide)
    {
        return new Color(0.5f, 0.5f, 0.5f, 1);// isSide ? 0.7f : 0.9f);
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

        this.CalcSceneDimensions(); // Stauffer - new values for newThick, newSep, newGap get used here

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
                yoff = yoff + (this.xRidges[i].myBin * this.sceneDepthByBinWithSep);
            }
            thisz = this.sceneCorner.z + yoff;

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
                float _49 = (this.sceneCorner.z + yoff) + (this.rowDepthDataOnly * 0.1f);
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
        //float cubeHeight = this.sceneWidth * 0.05f;
        //baseCube.transform.localScale = Vector3(sceneWidth + 2.0, cubeHeight, sceneDepthFull + 2.0);
        //baseCube.transform.position = Vector3(sceneCorner.x - 1.0, sceneCorner.y - (cubeHeight * 0.9), sceneCorner.z - 1.0);
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
        //Debug.Log("sceneCorner: " + sceneCorner);
        return new Vector3(sceneCorner.x + sceneWidth / 2f, 0f, sceneCorner.z + sceneDepthFull / 2f);
    }

    /// <summary> This coroutine simply lets us put up a message and then start the
    /// drawwing process in the next frame. Would be nice to multi-thread the drawing itself with a unity job. </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    /// <returns></returns>
    IEnumerator RedrawCoroutine(bool quiet = false)
    {
        int statusID = UIManager.Instance.StatusShow("Drawing...");
        yield return null;
        NewPrepareAndDrawData(quiet);
        //For any params that may have changed that need some action
        UpdateSceneDrawingParams();

        UIManager.Instance.StatusComplete(statusID);
    }

    /// <summary>
    /// Refresh for some drawing params that may have changed.
    /// Call this AFTER scene dimensions have been updated
    /// </summary>
    private void UpdateSceneDrawingParams()
    {
        //We'll want to put these into a more self-contained class and method at some point,
        // but for now just use this.
        //
        //Set shader params used for drawing at a minimum height. We use a shader so that
        // we can use a simple txf scaling to change height while viewing and avoid
        // redrawing the meshes - i.e. for speed.
        Shader.SetGlobalFloat("_gMinimumHeight", this.MinGraphSceneHeight);
        Shader.SetGlobalFloat("_gSceneCornerY", this.sceneCorner.y);

        //Scale and move the floor of the graph
        //Need to do this after scene dimensions (sceneWidth and sceneDepthFull) are calc'ed. Would be better to move out of here, though
        Transform floor = graphContainer.transform.Find("GraphFloor");
        if (floor == null)
            Debug.LogError("GraphFloor == null");
        else
        {
            //Floor object is a cube because I was getting weird over-scaling with a plane
            floor.localScale = new Vector3(sceneWidth - 0.01f, 0.0001f, sceneDepthFull - 0.01f);
            floor.localPosition = new Vector3(sceneWidth / 2 + 0.01f, 0, sceneDepthFull / 2 + 0.01f);
        }
    }

    /// <summary> Start redrawing the graph for the current data. </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    public void Redraw(bool quiet = false)
    {
        //Start the draw in the next frame, see comments in coroutine
        StartCoroutine(RedrawCoroutine());

        //Refresh the UI
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
        if (!DataManager.Instance.PrepareAndVerify(out errorMsg))
        {
            if (!quiet)
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

        //Setup row headers (row labels)
        //
        this.numRowLabels = DataManager.Instance.HeightVar.hasRowHeaders ? DataManager.Instance.HeightVar.numDataRows : 0;
        this.rowLabels = new string[this.numRowLabels + 1];
        //Copy
        for (int i = 0; i < this.numRowLabels; i++)
        {
            this.rowLabels[i] = DataManager.Instance.HeightVar.rowHeaders[i];
        }

        //Draw it!
        this.NewShowData();

        ResetView();

        /* Stauffer - this code is never reached because bScrollBin never changes from its default val of false
        if (this.bScrollBin)
        {
            //Stauffer - show only ridges with this bin # (or all if currBin < 0)
            //Why is this done here? As a refresh-type action?
            this.VisBins(this.currBin);
        }
        */

        this.waitCount = 0;
    }

    public void ResetView()
    {
        //Desktop camera 
        CameraManager.Instance.ResetView();
        //VR player hmd
        VRManager.Instance.ResetPlayerPosition();
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
        this.maxRow = DataManager.Instance.Rows - 1;
        this.minCol = 0;
        this.maxCol = DataManager.Instance.Cols - 1;
        this.minBin = 0; //Just always have 1 bin for now. Empirically, we want its number to be 0, otherwise a space for a phantom bin appears in render.
        this.maxBin = 0;
        this.minDataHeight = heightVar.MinValue;
        this.maxDataHeight = heightVar.MaxValue;
        this.numRows = (this.maxRow - this.minRow) + 1;
        // debugging
        if (this.maxCol > this.colLimit)
        {
            Debug.LogError("maxCol > colLimit " + maxCol + " > " + colLimit + " Limiting to colLimit");
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
        this.CalcSceneDimensions();

        this.blockWidthNorm = 1f / this.numCols;
        this.dataHeightRangeScale = 1f / (this.maxDataHeight - this.minDataHeight);
        this.xRidges = new XRidge[this.numRows * this.numBins];

        //Stauffer - commented out these lines, the vars are unused. What are they for originally?
        //string extra1 = this.topColorChoice > 1 ? ", " + this.allVariableDescs[this.topColorChoice].name : ", 0";
        //string extra2 = this.sideColorChoice > 1 ? ", " + this.allVariableDescs[this.sideColorChoice].name : ", 0";

        //For each row, setup data and draw a ridge
        DataVariable hVar = DataManager.Instance.HeightVar;

        //Build the ridges
        for (int row = 0; row < hVar.numDataRows; row++)
        {
            //NOTE - these are class properties, that then get used in BuildRidge
            this.heightVals = hVar.Data[row];
            this.NewBuildRidge(row, this.numCols, this.minBin);//always one bin for now
        }
        //DebugRidge(this.xRidges[0]);
    }

    private void DebugRidge(XRidge ridge)
    {
        string s = "--- Debug Ridge ---\n";
        s += " pos: " + ridge.trans.position + "\n";
        s += " scl: " + ridge.trans.localScale + "\n";
        for (int i = 0; i < 16; i++)
        {
            s += ridge.myMesh.vertices[i].ToString("F3") + "\n";
        }
        Debug.Log(s);
    }

    /// <summary> For the given data value (of data var assigned to height, a number or NaN), return the *unscaled* block height,
    /// i.e. the height of the mesh before any object scaling for the scene.
    /// NaN Values - pass them here as NaN and will be handled. </summary>
    public float GetBlockMeshHeight(float heightValue)
    {
        if (float.IsNaN(heightValue))
            return this.ridgeMeshMinHeight;

        return ((heightValue - this.minDataHeight) * this.dataHeightRangeScale) + this.ridgeMeshMinHeight;
    }
    /// <summary> For the given data value, return the *scaled* block height, i.e. the height of the mesh WITH scene scaling and minimum scene height
    /// NOT the world y position of top of block, but the height of the block itself, independent of its bottom y position.</summary>
    public float GetBlockSceneHeight(float heightValue)
    {
        return (GetBlockMeshHeight(heightValue) * this.sceneHeight * this.currGraphHeightScale) + this.MinGraphSceneHeight;
    }

    /// <summary> Get the height for the block in scene units at a particular data position (row, column).
    /// NOT the world y position of top of block, but the height of the block itself, independent of its bottom y position.</summary>
    /// <returns>Will return minimum height if data not set, is NaN, or if row or col is out of range</returns>
    public float GetBlockSceneHeightByPosition(int row, int col)
    {
        return GetBlockSceneHeight(DataManager.Instance.GetHeightValue(row, col, false));
    }

    /// <summary> Get the width of each block in scene units </summary>
    public float GetBlockSceneWidth()
    {
        return HeatVRML.Instance.sceneWidth / HeatVRML.Instance.numCols;
    }

    public virtual void NewBuildRidge(int row, int numx /*== num of columns*/, int binindex)
    {
        Color topColor = new Color(); // default(Color);
        Color sideColor = new Color(); //default(Color);
        /// <summary> Flag for whether current top data value is valid number or NaN/NoData </summary>
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
            yoff = yoff + (binindex * this.sceneDepthByBinWithSep);
        }
        //Stauffer - 'proto' is from protomesh scene object. It's a private global instanced above.
        GameObject newRidge = UnityEngine.Object.Instantiate(this.proto, new Vector3(this.sceneCorner.x, this.sceneCorner.y, this.sceneCorner.z + yoff), Quaternion.identity);
        //Stauffer these vals used to set localScale are calc'ed in CalcSceneDimensions, I believe.
        //NOTE z is used in variable names for up, i.e. y in unity.
        newRidge.transform.localScale = new Vector3(this.sceneWidth, this.sceneHeight * this.currGraphHeightScale, this.rowDepthDataOnly);
        Mesh amesh = ((MeshFilter)newRidge.gameObject.GetComponent(typeof(MeshFilter))).mesh;
        this.xRidges[this.numRidges/*a class variable!*/] = new XRidge();

        //Row labels
        GameObject newLabel = UnityEngine.Object.Instantiate(this.protolabel, new Vector3(this.sceneCorner.x + this.sceneWidth, this.sceneCorner.y + 1f, (this.sceneCorner.z + yoff) + (this.rowDepthDataOnly * 0.1f)), this.protolabel.transform.rotation);
        //Add the label to the graph container object for easier manipulation
        newLabel.transform.SetParent(graphContainer.transform.Find("Labels"));

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
        //float minZ = 0.1f; never used
        int lastInd = numx - 1;
        float slabZ = 0.006f;
        float edgeBite = this.bevelFraction / this.numCols;
        // Note: this makes a 45 degree bevel at the curreent graph height, but it will be a different angle when height is changed.
        float topBite = (edgeBite * this.sceneWidth) / (this.sceneHeight * this.currGraphHeightScale);
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

            //For the values at this row/col, set up state
            topColor = this.NewMakeColor(DataManager.Mapping.TopColor, row, colNum);
            sideColor = this.NewMakeColor(DataManager.Mapping.SideColor, row, colNum);
            //Used to set vert properties depending on whether data is a valid number or NaN
            bool topIsANumber = ! DataManager.Instance.GetIsNanByMapping(DataManager.Mapping.TopColor, row, colNum);
            bool sideIsANumber = !DataManager.Instance.GetIsNanByMapping(DataManager.Mapping.SideColor, row, colNum);
            bool heightIsANumber = !DataManager.Instance.GetIsNanByMapping(DataManager.Mapping.Height, row, colNum);
            mm.SetIsANumber(heightIsANumber, topIsANumber, sideIsANumber);

            //Height
            if (colNum > 0)
            {
                prevX = thisX;
                prevZ = thisZ;
                thisX = nextX;
                thisZ = nextZ;
            }
            else
            {   //column 0
                thisX = ((0.5f) - this.minCol) * this.blockWidthNorm;
                //thisZ = ((DataManager.Instance.HeightVar.Data[row][0] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
                thisZ = GetBlockMeshHeight(DataManager.Instance.GetHeightValue(row, 0, false)); //send NaN if value is NaN
                prevX = thisX - this.blockWidthNorm;
                prevZ = thisZ;
            }
            if (colNum < lastInd)
            {
                nextX = ((colNum + 1 + 0.5f) - this.minCol) * this.blockWidthNorm;
                //nextZ = ((DataManager.Instance.HeightVar.Data[row][colNum + 1] - this.minDataHeight) * this.dataHeightRangeScale) + minZ;
                nextZ = GetBlockMeshHeight(DataManager.Instance.GetHeightValue(row, colNum + 1, false));
            }
            else
            {
                nextX = nextX + this.blockWidthNorm;
            }

            leftZ = (prevZ + thisZ) / 2f;
            leftX = (prevX + thisX) / 2f;
            rightZ = (thisZ + nextZ) / 2f;
            rightX = (thisX + nextX) / 2f;

            mm.SetColor(topColor);
            if (this.bConnectX) // ribbon
            {
                mm.Verts(false, leftX, leftZ, front, 1, 0);
                mm.Verts(false, leftX, leftZ, back, 0, 0);
                mm.Verts(false, thisX, thisZ, front, 1, 1);
                mm.Verts(false, thisX, thisZ, back, 0, 1);
                mm.Verts(false, rightX, rightZ, front, 1, 0);
                mm.Verts(false, rightX, rightZ, back, 0, 0);
                mm.Tris(0, 1, 2, 2, 1, 3, 2, 3, 4, 5, 4, 3);
                // make bottom
                if (!this.bExtendZ)
                {
                    mm.Verts(false, leftX, leftZ - slabZ, front, 1, 0);
                    mm.Verts(false, leftX, leftZ - slabZ, back, 0, 0);
                    mm.Verts(false, thisX, thisZ - slabZ, front, 1, 1);
                    mm.Verts(false, thisX, thisZ - slabZ, back, 0, 1);
                    mm.Verts(false, rightX, rightZ - slabZ, front, 1, 0);
                    mm.Verts(false, rightX, rightZ - slabZ, back, 0, 0);
                    mm.Tris(2, 1, 0, 3, 1, 2, 4, 3, 2, 3, 4, 5);
                }
                // make sides
                mm.SetColor(sideColor);
                mm.Verts(true, leftX, leftZ, front, 0, 1);
                mm.Verts(true, leftX, leftZ, back, 1, 1);
                mm.Verts(true, leftX, this.bExtendZ ? 0f : leftZ - slabZ, front, 0, 0);
                mm.Verts(true, leftX, this.bExtendZ ? 0f : leftZ - slabZ, back, 1, 0);
                mm.Verts(true, thisX, thisZ, front, 0.5f, 1);
                mm.Verts(true, thisX, thisZ, back, 0.5f, 1);
                mm.Verts(true, thisX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0.5f, 0);
                mm.Verts(true, thisX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0.5f, 0);
                mm.Verts(true, rightX, rightZ, front, 0, 1);
                mm.Verts(true, rightX, rightZ, back, 1, 1);
                mm.Verts(true, rightX, this.bExtendZ ? 0f : rightZ - slabZ, front, 0, 0);
                mm.Verts(true, rightX, this.bExtendZ ? 0f : rightZ - slabZ, back, 0, 0);
                mm.Tris(0, 4, 6, 0, 6, 2, 1, 7, 5, 1, 3, 7);
                mm.Tris(4, 10, 6, 4, 8, 10, 5, 7, 11, 5, 11, 9);
            }
            else
            {
                // tile
                if (this.doingEdges) //Seems to mean draw a bevel
                {
                    //STAUFFER - NOTE - this is the orig default branch 
                    // I changed the UV's cuz they seemed wrong in orig code in some places
                    // and now with the orig material I can see some edges made by shading on a little bevel
                    edgeZ = thisZ - topBite;
                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(false, leftX + edgeBite, thisZ, front, 0, 0); //0
                    mm.Verts(false, leftX + edgeBite, thisZ, back, 0, 1);  //1
                    mm.Verts(false, rightX - edgeBite, thisZ, front, 1, 0);//2
                    mm.Verts(false, rightX - edgeBite, thisZ, back, 1, 1); //3
                    // draw bevel
                    // Stauffer - the bevel amount is tiny (topBite) at around 0.001
                    // but still makes a difference. If I remove it and use my unlit shader,
                    // I see black edges on most of edges but not all. 
                    mm.Verts(false, leftX, edgeZ, front, 0, 0); //4
                    mm.Verts(false, leftX, edgeZ, back, 0, 0.9f);  //5
                    mm.Verts(false, rightX, edgeZ, front, 0.9f, 0);//6
                    mm.Verts(false, rightX, edgeZ, back, 1, 0.9f); //7
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    mm.Tris(4, 2, 6, 2, 4, 0, 6, 3, 7, 3, 6, 2);
                    mm.Tris(7, 1, 5, 1, 7, 3, 5, 0, 4, 0, 5, 1);
                    // draw bottom
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 1);
                    mm.Tris(2, 1, 0, 3, 1, 2);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(true, leftX, edgeZ, front, 0, 0.9f);
                    mm.Verts(true, rightX, edgeZ, front, 1, 0.9f);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(true, leftX, edgeZ, back, 1, 0.9f);
                    mm.Verts(true, rightX, edgeZ, back, 0, 0.9f);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);

                    /* - orig code - UV's seem wrong
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
                    */
                }
                else
                {
                    edgeZ = thisZ;

                    // Stauffer - orig code here, UV's seem wrong on first few. Fixed.
                    // When force doingEdges false and we get here, this code doesn't draw right when
                    // using the orig material and shader - maybe cuz uv's were off?

                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(false, leftX, edgeZ, front, 0, 0); //Stauffer changed these uvs to what seems correct
                    mm.Verts(false, leftX, edgeZ, back, 0, 1);
                    mm.Verts(false, rightX, edgeZ, front, 1, 0);
                    mm.Verts(false, rightX, edgeZ, back, 1, 1);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw bottom
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 1);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 1);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisZ - slabZ, front, 0, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisZ - slabZ, front, 1, 0);
                    mm.Verts(true, leftX, edgeZ, front, 0, 1);
                    mm.Verts(true, rightX, edgeZ, front, 1, 1);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisZ - slabZ, back, 1, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisZ - slabZ, back, 0, 0);
                    mm.Verts(true, leftX, edgeZ, back, 1, 1);
                    mm.Verts(true, rightX, edgeZ, back, 0, 1);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
                }
            }
        }
        mm.Attach(amesh);

        this.xRidges[this.numRidges].AddRidge(newRidge, amesh, binindex, row);
        this.xRidges[this.numRidges++].AddLabel(newLabel);
    }


    /// <summary>
    /// Event handler for the SMV Simple Model View system.
    /// This handles events that are generated when an SMV-mapped state is
    /// changed, either from UI or from code, so it acts like a method that
    /// you normally would call from both your UI event handler and from
    /// your code when a particular value is changed.
    /// </summary>
    /// <param name="mapping"></param>
    public void SMV_OnUpdateEvent(SMVmapping mapping)
    {
        switch (mapping)
        {
            case SMVmapping.GraphHeightFrac:
                UpdateGraphHeight();
                break;
            default:
                Debug.LogError("Unrecognized SMVmapping in event handler: " + mapping.ToString());
                break;
        }
    }

    /// <summary>
    /// Stauffer added
    /// Let's us call this func from UI with a [0,1] fractional value.
    /// Used internally from SimpleModelView callback.
    /// </summary>
    /// <param name="frac"></param>
    private void UpdateGraphHeight()
    {
        //Square the height-scaling-fraction so we get more sensitivity in the bottom of the range.
        //This is important for getting a good height scale in larger data sets where ridges need to be short to see an overview.
        this.currGraphHeightScale = this.lowGraphHeightScaleRange + (this.highGraphHeightScaleRange - this.lowGraphHeightScaleRange) * (CurrGraphHeightFrac * CurrGraphHeightFrac);
        this.ScaleRidges(this.currGraphHeightScale);
    }

    /// <summary>
    /// Set new graph height value.
    /// Does NOT require a redraw of the graph since it scales the ridges using transform scaling.
    /// See new method UpdateGraphHeight to adjust using [0,1] value.
    /// </summary>
    /// <param name="newGraphHeight">proportional between lowGraphHeightScaleRange and highGraphHeightScaleRange</param>
    public virtual void GraphHeightSelected_OLD(float newGraphHeight)
    {
        this.currGraphHeightScale = newGraphHeight;
        this.ScaleRidges(this.currGraphHeightScale);
    }

    public virtual void FOVSelected(float newFOV)
    {
        this.currFOV = newFOV;
        //this.myCameraOld.fieldOfView = this.lowFOVRange + (this.highFOVRange - this.currFOV);
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
        //hFOV = Mathf.Atan((Screen.width * Mathf.Tan((this.myCameraOld.fieldOfView * Mathf.PI) / 360f)) / Screen.height);
        switch (newZip)
        {
            case 0:
                myY = this.sceneCorner.z + (this.sceneDepthByBin / 2f);
                myX = this.sceneCorner.x + (this.sceneWidth / 2f);
                //Debug.Log("sceneCorner.y is " + sceneCorner.y);
                //Debug.Log("(sceneHeight * currGraphHeightScale) is " + (sceneHeight * currGraphHeightScale));
                //Debug.Log("myCameraOld.fieldOfView is " + myCameraOld.fieldOfView);
                //Debug.Log("Mathf.Tan(myCameraOld.fieldOfView / 2.0) is " + Mathf.Tan(myCameraOld.fieldOfView * Mathf.PI / 360.0));
                //myZ = (this.sceneCorner.y + (this.sceneHeight * this.currGraphHeightScale)) + ((this.sceneDepthByBin / 2f) / Mathf.Tan((this.myCameraOld.fieldOfView * Mathf.PI) / 360f));
                //Debug.Log("myZ is " + myZ);
                Fly.NewRotation(0f, -90f);
                break;
            case 1:
                myX = this.sceneCorner.x + (this.sceneWidth / 2f);
                myZ = this.sceneCorner.y + ((this.sceneHeight * this.currGraphHeightScale) / 2f);
                myY = this.sceneCorner.z - ((this.sceneWidth / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(0f, 0f);
                break;
            case 2:
                myX = this.sceneCorner.x + (this.sceneWidth / 2f);
                myZ = this.sceneCorner.y + ((this.sceneHeight * this.currGraphHeightScale) / 2f);
                myY = (this.sceneCorner.z + this.sceneDepthByBin) + ((this.sceneWidth / 2f) / Mathf.Tan(hFOV));
                if ((this.numBins > 1) && !this.binInterleave)
                {
                    myY = myY + (this.sceneDepthByBin * (this.numBins - 1));
                }
                Fly.NewRotation(180f, 0f);
                break;
            case 3:
                myY = this.sceneCorner.z + (this.sceneDepthByBin / 2f);
                myZ = this.sceneCorner.y + ((this.sceneHeight * this.currGraphHeightScale) / 2f);
                myX = this.sceneCorner.x - ((this.sceneDepthByBin / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(90f, 0f);
                break;
            case 4:
                myY = this.sceneCorner.z + (this.sceneDepthByBin / 2f);
                myZ = this.sceneCorner.y + ((this.sceneHeight * this.currGraphHeightScale) / 2f);
                myX = (this.sceneCorner.x + this.sceneWidth) + ((this.sceneDepthByBin / 2f) / Mathf.Tan(hFOV));
                Fly.NewRotation(-90f, 0f);
                break;
        }
        this.myController.transform.position = new Vector3(myX, myZ, myY);
    }

    public HeatVRML()
    {
        //NOTE - stauffer - some inits are made now in Initialize()
        //But if I move everything here into Initialize(), then display/view
        // gets messed up

        this.programVersion = "0.1.0";
        this.sceneWidth = 20f; // 400f;
        this.sceneDepthByBin = sceneWidth; // 400f;
        this.sceneDepthMaxApprox = 2f * this.sceneWidth;
        this.sceneHeight = 5f; // 200f;
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
        //Keep this tiny, but > 0
        this.lowGraphHeightScaleRange = 0.001f;
        this.highGraphHeightScaleRange = 1f;
        //this.currGraphHeightScale = 0.5f; see Initialize()
        this.lowFOVRange = 20f;
        this.highFOVRange = 170f;
        this.lowDepthToWidthRatioRange = -4f;
        this.highDepthToWidthRatioRange = 4f;
        this.highSepRange = 4f;
        this.binSeparationFrac = 1.1f;
        this.highGapRange = 4f;
        this.rowGapFrac = 1f;
        this.pixPerLine = 25;
        this.rowHeight = 25;
        //this.colWidth = 100; stauffer - not used
        this.lineWidth = 20;
        this.scrollHeight = 112;
        this.showHelp = false;
        this.oldHelpPage = 1;
        this.menuScrollPos = new Vector2(0f, 0f);
        this.vrmlModelMM = 200f;
        this.colLimit = 32000;
        this.doingEdges = false; //If want to change this, see declaration of doingEdges
        this.bevelFraction = 0.05f;
    }
}