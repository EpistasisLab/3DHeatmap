using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SMView;
using TMPro;

/// <summary>
/// Graph class. Has lots of state vars crammed in it. Needs more refactoring still.
/// As a MonoBehaviorSingleton, there's a single global instance that is accessible via Graph.Instance
/// </summary>
[System.Serializable]
public class Graph : MonoBehaviorSingleton<Graph>
{
    /*
	Portions of this file originally coded by Douglas P. Hill, Copyright (C) 2010  Dartmouth College

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

    /// <summary> Scene Object to conatain graph elements so they can be manipulated together </summary>
    public GameObject graphContainer;
    /// <summary> Scene object to hold all the ridge objects generated at runtime </summary>
    public GameObject runtimeRidgeContainer;
    /// <summary> Scene object to hold labels generated at runtime. Just to keep things tidy in hierarchy </summary>
    public GameObject runtimeLabelsContainer;
    /// <summary> Label object to use for instantiating row and column labels </summary>
    public GameObject protolabel;
    /// <summary> Scene object for the back wall in the scene. Need to hide it sometimes. </summary>
    public GameObject backWall;

    /// <summary> Lists of labels. Clunky to do like this, but keep it for now for simplicity. </summary>
    private List<Label> labelsRowRight;
    private List<Label> labelsRowLeft;
    private List<Label> labelsColumnBottom;
    private List<Label> labelsColumnTop;

    // scene related
    /// <summary> Stauffer - plotting area width in scene units. Row width. Seems hardcoded </summary>
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
    /// it's separation between groups of rows of each bin. If interleaved, this is separation between each row (different than rowGap, however).
    /// NOT USING</summary>
    public float binSeparation;
    /// <summary> Stauffer - full scene depth of each row, including gap beetween rows (and bin separation when interleaved) </summary>
    public float rowDepthFull;
    /// <summary> Stauffer - plotting area DEPTH in scene units, for single bin (or for all bins when interleaved) INCLUDING gap between bin groups </summary>
    public float sceneDepthByBinWithSep;

    // chart related
    /// <summary> Stauffer - as best I can tell, this flag controls interleaving of bins within the plot. i.e. if each bin is shown as separate group of rows, or interleaved by row </summary>
    public bool binInterleave;
    private bool bConnectX; //Draw ribbon. Flag.

    private GameObject protomesh;
    public GameObject Proto { get { return protomesh; } }
    private XRidge[] xRidges;
    //private int shaveCount; Stauffer - never used
    private int numRidges;
    /// <summary> Stauffer added. Minimum height of *mesh* used to make a ridge. Not minimum scene height - see MinGraphSceneHeight for that </summary>
    private float ridgeMeshMinHeight;
    /// <summary> Depth (unity z-dimension) of data representation, i.e. the depth of the block representing the data point. Does NOT include gap between rows. </summary>
    public float rowDepthDataOnly;
    /// <summary> Normalized scene width of each block (column of data). </summary>
    private float blockWidthNorm;
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
    /// <summary> Holdover from orig code that allowed binning of data. Will be replaced by new binning/clustering tools </summary>
    private int numBins;

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
        get { return SMV.I.GetValueFloat(SMVmapping.GraphHeightFrac); }
        set { SMV.I.SetValue(SMVmapping.GraphHeightFrac, value); }
    }
    public float bevelFraction;


    /// <summary> 
    /// Absolute minimum of graph height so bars/ridges don't get so short we can't see side colors.
    ///
    /// This is buggy, so don't use for now. See developer notes. For now, the height slider has a non-zero min value for its range.
    /// </summary>
    private float _minGraphSceneHeight;
    private float MinGraphSceneHeight
    {
        set { _minGraphSceneHeight = value; }
        get { return 0; }// _minGraphSceneHeight;  } //DataManager.I.Cols > 18 ? 0f : _minGraphSceneHeight; }
    }

    /// <summary> Stauffer - fractional amount of data row scene depth to apply to determining bin separation. </summary>
    private float binSeparationFrac;
    /// <summary> Stauffer - fractional value applied to rowDepthDataOnly to calculate gap between rows. Separate from binSeparation </summary>
    private float rowGapFrac;

//NOTE - these can probably be replaced by methods in DataManager
    private string[] rowLabels;
    private int numRowLabels;

    // Debugging
    private int colLimit;

    //Use this instead of Awake since this is a MonoBehaviorSingleton
    //void Awake()
    protected override void Initialize()
    {
        //Stauffer - move some things here that use SimpleModelView, because it
        // cannot be init'ed in Graph ctor
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
        this.protomesh = GameObject.Find("protomesh_SceneObj");
        if (this.protomesh == null)
            Debug.LogError("Failed to find object for protomesh ridge");

        //Stauffer
        //
        this.ridgeMeshMinHeight = 0.1f;

        labelsRowRight = new List<Label>();
        labelsRowLeft = new List<Label>();
        labelsColumnBottom = new List<Label>();
        labelsColumnTop = new List<Label>();

        ResetView();

        //Debug.Log("Application.persistentDataPath: " + Application.persistentDataPath);

        //Quick intro message with instructions
        UIManager.I.ShowIntroMessage();
    }

    public virtual void Update()
    {

    }


    /// <summary> Top-level command to redraw the graph for the current data. </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    public void Redraw(bool quiet = false)
    {
        //Start the draw in the next frame, see comments in coroutine
        StartCoroutine(RedrawCoroutine());

        //Refresh the UI
        UIManager.I.RefreshUI();

        //When doing UI prompts, this is the last one we do, so stop the whole process if we get here,
        // which also handles the case when user jumps ahead of the prompts to here.
        UIManager.I.StopAutoUIActionPrompts();
    }

    /// <summary> This coroutine simply lets us put up a message and then start the
    /// drawwing process in the next frame. Would be nice to multi-thread the drawing itself with a unity job. </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    /// <returns></returns>
    IEnumerator RedrawCoroutine(bool quiet = false)
    {
        int statusID = UIManager.I.StatusShow("Drawing...");
        yield return null;
        PrepareAndDrawData(quiet);

        //For any params that may have changed that need some action
        UpdateSceneDrawingParams();

        UIManager.I.StatusComplete(statusID);
    }

    /// <summary>
    /// Stauffer. New routine. Prep and draw data loaded in DataManager.
    /// Replacing functionality of DatasetSelected().
    /// Should generally not be called directly. See Redraw().
    /// </summary>
    /// <param name="quiet">Set to true for silent return when data not ready or error. Default is false.</param>
    private void PrepareAndDrawData(bool quiet = false)
    {
        // Do some verification of data, so we don't have to check things every time we access dataMgr
        // e.g. minimum variables are set (e.g. always expect a height var (or, actually maybe not??))
        string errorMsg;
        if (!DataManager.I.PrepareAndVerify(out errorMsg))
        {
            if (!quiet)
            {
                string msg = "Error with data prep and verification: \n\n" + errorMsg;
                Debug.LogError(msg);
                UIManager.I.ShowMessageDialog(msg);
            }
            return;
        }

        //Axis extents
        GetAxisExtents();

        //Setup row headers (row labels)
        //
        this.numRowLabels = DataManager.I.HeightVar.hasRowHeaders ? DataManager.I.HeightVar.numDataRows : 0;
        this.rowLabels = new string[this.numRowLabels + 1];
        //Copy
        for (int i = 0; i < this.numRowLabels; i++)
        {
            this.rowLabels[i] = DataManager.I.HeightVar.rowHeaders[i];
        }

        //Draw it!
        ShowData();

        ResetView();
    }



    public virtual void CalcSceneDimensions()
    {
        //Stauffer sceneWidth is set to fixed val (400) during init
        this.rowDepthDataOnly = this.sceneWidth / this.numCols;

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

    // Stauffer - NOTE this seems to work with the currently-loaded row data,
    //   by accessing the Graph class properties topVals[] and sideVals[]
    //Skipping 'bin' stuff for now
    public virtual Color MakeColor(DataManager.Mapping mapping, int row, int column)
    {
        bool isSide = mapping == DataManager.Mapping.SideColor;

        int colorTableID = DataManager.I.GetColorTableIdByMapping(mapping);

        float value = DataManager.I.GetValueByMapping(mapping, row, column, true);
        DataVariable var = DataManager.I.GetVariableByMapping(mapping);
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

    //Stauffer
    //Return the scene/plot center in scene units.
    public Vector3 GetPlotCenter()
    {
        //Debug.Log("sceneCorner: " + sceneCorner);
        return new Vector3(sceneCorner.x + sceneWidth / 2f, 0f, sceneCorner.z + sceneDepthFull / 2f);
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

        //Shader param for drawing edges on sides of blocks.
        //This is a mess and needs to be changed in shader to use a different method, but for
        // now set the value based on # of columns. Determined this equation by manually finding decent
        // setting for datasets at different sizes, for both vr and desktop views (although they could
        // probably each use their own setting).
        float m = numCols < 250 ? 0.00067f : 0.00067f - Mathf.Min(0.0004f, ((numCols - 250.0f) / 500.0f) * 0.0006f);
        float width = numCols * m + 0.01f;
        Shader.SetGlobalFloat("_EdgeShadeWidth", width );
        ///Debug.Log("UpdateSceneDrawingParams: m " + m + "  edge shade width: " + width);

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

        //Hide the rear wall in the scene if the data is extending past it
        backWall.SetActive(sceneDepthFull < (backWall.transform.position.z - 1));
    }


    public void ResetView()
    {
        //Move the graph back to default position in case
        // user moved it in VR
        ResetGraphPosition();
        
        //Desktop camera 
        CameraManager.I.ResetView();

        //VR player hmd
        VRManager.I.ResetPlayerPosition();
    }

    /// <summary> Get data axes extents (num rows, columns and height range) </summary>
    public void GetAxisExtents()
    {
        //In DatasetSelected, the ranges get set for the optional subsequent int columns.
        //
        DataVariable heightVar = DataManager.I.HeightVar;
        
        this.numRows = DataManager.I.Rows;
        this.minBin = 0; //Just always have 1 bin for now. Empirically, we want its number to be 0, otherwise a space for a phantom bin appears in render.
        this.maxBin = 0;
        this.minDataHeight = heightVar.MinValue;
        this.maxDataHeight = heightVar.MaxValue;
        this.numCols = DataManager.I.Cols;
        // debugging
        if (numCols > this.colLimit)
        {
            Debug.LogError("numCols > colLimit " + numCols + " > " + colLimit + " Limiting to colLimit");
            this.numCols = this.colLimit;
        }
        this.numBins = (this.maxBin - this.minBin) + 1;
        this.dataHeightRange = this.maxDataHeight - this.minDataHeight;
    }


    public virtual void ShowData()
    {
        if (!this.protomesh) // must be functioning only as user interface
        {
            return;
        }

        //Looks to be deleting old mesh/visualization
        if (this.numRidges > 0)
        {
            int iridge = 0;
            while (iridge < this.numRidges)
            {
                xRidges[iridge].Destroy();
                ++iridge;
            }
            DestroyLabels();
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
        DataVariable hVar = DataManager.I.HeightVar;

        //Build the ridges
        for (int row = 0; row < hVar.numDataRows; row++)
        {
            this.BuildRidge(row, this.minBin);//always one bin for now
        }
        //DebugRidge(this.xRidges[0]);

        //Labels
        GenerateRowLabels();
        GenerateColumnLabels();
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

    /// <summary> Move the whole graph </summary>
    public virtual void TranslateGraph(float xStep, float yStep, float zStep, float maxy /*constrain how high it can go*/)
    {
        Vector3 newPos = sceneCorner + new Vector3(xStep, yStep, zStep);
        SetGraphPosition(newPos, maxy);
    }

    public void ResetGraphPosition()
    {
        SetGraphPosition(Vector3.zero, 0);
    }

    /// <summary> Set the position of the graph relative to its front-left corner </summary>
    /// <param name="newPos"></param>
    /// <param name="maxy">Height constraint. Used in VR mode to keep graph below user's eye level</param>
    public virtual void SetGraphPosition( Vector3 newPos, float maxy)
    {
        if (xRidges == null)
            return;

        //Constrain yposition
        newPos.y = Mathf.Max(newPos.y, 0);
        newPos.y = Mathf.Min(newPos.y, maxy);

        //Update the scene corner state
        //NOTE - do we need this variable anymore now that we've got the ridges in graphContainer?
        sceneCorner = newPos;

        //Update the graphContainer position
        //It contains the ridges and labels so they move with it.
        graphContainer.transform.position = newPos;

        //Needs some updating
        UpdateSceneDrawingParams();
    }
    
    /// <summary>
    /// Scale the height of ridges as a fraction of max sceneHeight.
    /// Uses each mesh's transform.localScale so is quick </summary>
    /// <param name="frac"></param>
    public virtual void ScaleRidgeHeight(float frac)
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

    /// <summary> For the given data value (of data var assigned to height, a number or NaN), return the *unscaled* block height,
    /// i.e. the height of the mesh before any object scaling for the scene.
    /// NaN Values - pass them here as NaN and will be handled. </summary>
    public float GetBlockMeshHeight(float heightDataValue)
    {
        if (float.IsNaN(heightDataValue))
            return this.ridgeMeshMinHeight;

        return ((heightDataValue - this.minDataHeight) * this.dataHeightRangeScale) + this.ridgeMeshMinHeight;
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
        return GetBlockSceneHeight(DataManager.I.GetHeightValue(row, col, false));
    }

    /// <summary> Get the width of each block in scene units </summary>
    public float GetBlockSceneWidth()
    {
        return Graph.I.sceneWidth / Graph.I.numCols;
    }

    /// <summary> Return the z position of a row (0-based) </summary>
    /// <returns></returns>
    public float GetRowSceneZ( int row)
    {
        return row * this.rowDepthFull;
    }

    /// <summary> For a given row of data, build a single mesh containing all the blocks in the row </summary>
    /// <param name="row"></param>
    /// <param name="numx"></param>
    /// <param name="binindex"></param>
    public virtual void BuildRidge(int row, int binindex)
    {
        Color topColor = new Color(); // default(Color);
        Color sideColor = new Color(); //default(Color);
        /// <summary> Flag for whether current top data value is valid number or NaN/NoData </summary>
        float thisX = 0.0f;
        float thisY = 0.0f;
        float prevX = 0.0f;
        #if false
        float prevY = 0.0f;
        #endif
        float nextX = 0.0f;
        float nextY = 0.0f;
        float leftX = 0.0f;
        float rightX = 0.0f;
        float front = 0.0f;
        float back = 0.0f;
        float edgeY = 0.0f;

        float zoff = GetRowSceneZ(row);
        if (this.binInterleave)
        {
            zoff = zoff + (binindex * this.binSeparation);
        }
        else
        {
            zoff = zoff + (binindex * this.sceneDepthByBinWithSep);
        }

        //Stauffer - 'protomesh' is from protomesh scene object, which is a prefab. It's a private global instanced above.
        GameObject newRidge = UnityEngine.Object.Instantiate(this.protomesh, new Vector3(this.sceneCorner.x, this.sceneCorner.y, this.sceneCorner.z + zoff), Quaternion.identity);
        //Store the new object in container for easily moving it around, and tidyness
        newRidge.transform.SetParent(runtimeRidgeContainer.transform);
        newRidge.transform.localScale = new Vector3(this.sceneWidth, this.sceneHeight * this.currGraphHeightScale, this.rowDepthDataOnly);
        Mesh amesh = ((MeshFilter)newRidge.gameObject.GetComponent(typeof(MeshFilter))).mesh;
        this.xRidges[this.numRidges/*a class variable!*/] = new XRidge();

        //Helper class to make the mesh
        MeshMaker mm = new MeshMaker();

        //Loop over the columns and draw each block
        for (int colNum = 0; colNum < numCols; colNum++)
        {
            //Not really sure what this is doing. Presumably a
            // slight offset to avoid overlapping verts.
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
            topColor = this.MakeColor(DataManager.Mapping.TopColor, row, colNum);
            sideColor = this.MakeColor(DataManager.Mapping.SideColor, row, colNum);
            //Used to set vertex properties depending on whether data is a valid number or NaN
            bool topIsANumber = !DataManager.I.GetIsNanByMapping(DataManager.Mapping.TopColor, row, colNum);
            bool sideIsANumber = !DataManager.I.GetIsNanByMapping(DataManager.Mapping.SideColor, row, colNum);
            bool heightIsANumber = !DataManager.I.GetIsNanByMapping(DataManager.Mapping.Height, row, colNum);
            mm.SetIsANumber(heightIsANumber, topIsANumber, sideIsANumber);

            //Height & width
            //Assignment of these vars is more complicated than needed for a simple cuboid because orig code had more mesh-building options
            if (colNum > 0)
            {
                prevX = thisX;
                #if false
                prevY = thisY;
                #endif
                thisX = nextX;
                thisY = nextY;
            }
            else
            {   //column 0, get things init'ed
                thisX = 0.5f * this.blockWidthNorm;
                thisY = GetBlockMeshHeight(DataManager.I.GetHeightValue(row, 0, false /*return NaN if value is NaN*/));
                prevX = thisX - this.blockWidthNorm;
                #if false
                prevY = thisY;
                #endif
            }

            if (colNum < numCols - 1)
            {
                nextX = (colNum + 1 + 0.5f) * this.blockWidthNorm;
                nextY = GetBlockMeshHeight(DataManager.I.GetHeightValue(row, colNum + 1, false));
            }
            else
            {
                //last column
                nextX = nextX + this.blockWidthNorm;
            }

            //////
            //Draw a simple cube/cubiod

            edgeY = thisY;
            leftX = (prevX + thisX) / 2f;
            rightX = (thisX + nextX) / 2f;
            float slabY = 0.006f;

            // Stauffer - orig code here, UV's seem wrong on first few. Fixed.
            // This code doesn't draw right when using the orig material and shader - maybe cuz uv's were off?

            // draw top
            mm.SetColor(topColor);
            mm.Verts(false, false, leftX, edgeY, front, 0, 0); //Stauffer changed these uvs to what seems correct
            mm.Verts(false, false, leftX, edgeY, back, 0, 1);
            mm.Verts(false, false, rightX, edgeY, front, 1, 0);
            mm.Verts(false, false, rightX, edgeY, back, 1, 1);
            mm.Tris(0, 1, 2, 2, 1, 3);

            // draw bottom
            mm.Verts(false, true, leftX, 0, front, 1, 1);
            mm.Verts(false, true, leftX, 0, back, 0, 1);
            mm.Verts(false, true, rightX, 0, front, 1, 1);
            mm.Verts(false, true, rightX, 0, back, 0, 1);
            mm.Tris(0, 1, 2, 2, 1, 3);

            // draw sides
            mm.SetColor(sideColor);
            mm.Verts(true, true, leftX, 0, front, 0, 0);
            mm.Verts(true, true, rightX, 0, front, 1, 0);
            mm.Verts(true, false, leftX, edgeY, front, 0, 1);
            mm.Verts(true, false, rightX, edgeY, front, 1, 1);
            mm.Verts(true, true, leftX, 0, back, 1, 0);
            mm.Verts(true, true, rightX, 0, back, 0, 0);
            mm.Verts(true, false, leftX, edgeY, back, 1, 1);
            mm.Verts(true, false, rightX, edgeY, back, 0, 1);
            mm.Tris(0, 2, 1, 1, 2, 3);
            mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
            mm.Tris(1, 3, 5, 3, 7, 5);

            //Original code has the two other drawing options, but they haven't been in use
            // in this new code. But keep them around in case we want to add them back.
#if false
            float leftY = 0.0f;
            float rightY = 0.0f;
        
            //We'll only need these for the ribbon code
            leftY = (prevY + thisY) / 2f;
            rightY = (thisY + nextY) / 2f;

            float edgeBite = this.bevelFraction / numCols;
            // Note: this makes a 45 degree bevel at the curreent graph height, but it will be a different angle when height is changed.
            float topBite = (edgeBite * this.sceneWidth) / (this.sceneHeight * this.currGraphHeightScale);

            if (this.bConnectX) // ribbon
            {
                Debug.Log("GOT HERE bConnectX");

                mm.Verts(false, leftX, leftY, front, 1, 0);
                mm.Verts(false, leftX, leftY, back, 0, 0);
                mm.Verts(false, thisX, thisY, front, 1, 1);
                mm.Verts(false, thisX, thisY, back, 0, 1);
                mm.Verts(false, rightX, rightY, front, 1, 0);
                mm.Verts(false, rightX, rightY, back, 0, 0);
                mm.Tris(0, 1, 2, 2, 1, 3, 2, 3, 4, 5, 4, 3);
                // make bottom
                if (!this.bExtendZ)
                {
                    mm.Verts(false, leftX, leftY - slabY, front, 1, 0);
                    mm.Verts(false, leftX, leftY - slabY, back, 0, 0);
                    mm.Verts(false, thisX, thisY - slabY, front, 1, 1);
                    mm.Verts(false, thisX, thisY - slabY, back, 0, 1);
                    mm.Verts(false, rightX, rightY - slabY, front, 1, 0);
                    mm.Verts(false, rightX, rightY - slabY, back, 0, 0);
                    mm.Tris(2, 1, 0, 3, 1, 2, 4, 3, 2, 3, 4, 5);
                }
                // make sides
                mm.SetColor(sideColor);
                mm.Verts(true, leftX, leftY, front, 0, 1);
                mm.Verts(true, leftX, leftY, back, 1, 1);
                mm.Verts(true, leftX, this.bExtendZ ? 0f : leftY - slabY, front, 0, 0);
                mm.Verts(true, leftX, this.bExtendZ ? 0f : leftY - slabY, back, 1, 0);
                mm.Verts(true, thisX, thisY, front, 0.5f, 1);
                mm.Verts(true, thisX, thisY, back, 0.5f, 1);
                mm.Verts(true, thisX, this.bExtendZ ? 0f : thisY - slabY, front, 0.5f, 0);
                mm.Verts(true, thisX, this.bExtendZ ? 0f : thisY - slabY, back, 0.5f, 0);
                mm.Verts(true, rightX, rightY, front, 0, 1);
                mm.Verts(true, rightX, rightY, back, 1, 1);
                mm.Verts(true, rightX, this.bExtendZ ? 0f : rightY - slabY, front, 0, 0);
                mm.Verts(true, rightX, this.bExtendZ ? 0f : rightY - slabY, back, 0, 0);
                mm.Tris(0, 4, 6, 0, 6, 2, 1, 7, 5, 1, 3, 7);
                mm.Tris(4, 10, 6, 4, 8, 10, 5, 7, 11, 5, 11, 9);
            }
            else
            {
                // tile
                if (this.doingEdges) //Seems to mean draw a bevel
                {
                    Debug.Log("GOT HERE doingEdges");
                    //STAUFFER - NOTE - this is the orig default branch 
                    // I changed the UV's cuz they seemed wrong in orig code in some places
                    // and now with the orig material I can see some edges made by shading on a little bevel
                    edgeY = thisY - topBite;
                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(false, leftX + edgeBite, thisY, front, 0, 0); //0
                    mm.Verts(false, leftX + edgeBite, thisY, back, 0, 1);  //1
                    mm.Verts(false, rightX - edgeBite, thisY, front, 1, 0);//2
                    mm.Verts(false, rightX - edgeBite, thisY, back, 1, 1); //3
                    // draw bevel
                    // Stauffer - the bevel amount is tiny (topBite) at around 0.001
                    // but still makes a difference. If I remove it and use my unlit shader,
                    // I see black edges on most of edges but not all. 
                    mm.Verts(false, leftX, edgeY, front, 0, 0); //4
                    mm.Verts(false, leftX, edgeY, back, 0, 0.9f);  //5
                    mm.Verts(false, rightX, edgeY, front, 0.9f, 0);//6
                    mm.Verts(false, rightX, edgeY, back, 1, 0.9f); //7
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    mm.Tris(4, 2, 6, 2, 4, 0, 6, 3, 7, 3, 6, 2);
                    mm.Tris(7, 1, 5, 1, 7, 3, 5, 0, 4, 0, 5, 1);
                    // draw bottom
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisY - slabY, front, 0, 0);
                    mm.Verts(false, leftX, this.bExtendZ ? 0f : thisY - slabY, back, 0, 1);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisY - slabY, front, 1, 0);
                    mm.Verts(false, rightX, this.bExtendZ ? 0f : thisY - slabY, back, 1, 1);
                    mm.Tris(2, 1, 0, 3, 1, 2);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisY - slabY, front, 0, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisY - slabY, front, 1, 0);
                    mm.Verts(true, leftX, edgeY, front, 0, 0.9f);
                    mm.Verts(true, rightX, edgeY, front, 1, 0.9f);
                    mm.Verts(true, leftX, this.bExtendZ ? 0f : thisY - slabY, back, 1, 0);
                    mm.Verts(true, rightX, this.bExtendZ ? 0f : thisY - slabY, back, 0, 0);
                    mm.Verts(true, leftX, edgeY, back, 1, 0.9f);
                    mm.Verts(true, rightX, edgeY, back, 0, 0.9f);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);

                    /* - orig code - UV's seem wrong
                    // draw top
                    mm.SetColor(topColor);
                    mm.Verts(leftX + edgeBite, thisY, front, 1, 1);
                    mm.Verts(leftX + edgeBite, thisY, back, 0, 1);
                    mm.Verts(rightX - edgeBite, thisY, front, 1, 1);
                    mm.Verts(rightX - edgeBite, thisY, back, 0, 1);
                    // draw bevel
                    mm.Verts(leftX, edgeY, front, 1, 0.9f);
                    mm.Verts(leftX, edgeY, back, 0, 0.9f);
                    mm.Verts(rightX, edgeY, front, 1, 0.9f);
                    mm.Verts(rightX, edgeY, back, 0, 0.9f);
                    mm.Tris(0, 1, 2, 2, 1, 3);
                    mm.Tris(4, 2, 6, 2, 4, 0, 6, 3, 7, 3, 6, 2);
                    mm.Tris(7, 1, 5, 1, 7, 3, 5, 0, 4, 0, 5, 1);
                    // draw bottom
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisY - slabY, front, 1, 1);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisY - slabY, back, 0, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisY - slabY, front, 1, 1);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisY - slabY, back, 0, 1);
                    mm.Tris(2, 1, 0, 3, 1, 2);
                    // draw sides
                    mm.SetColor(sideColor);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisY - slabY, front, 0, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisY - slabY, front, 1, 0);
                    mm.Verts(leftX, edgeY, front, 0, 0.9f);
                    mm.Verts(rightX, edgeY, front, 1, 0.9f);
                    mm.Verts(leftX, this.bExtendZ ? 0f : thisY - slabY, back, 1, 0);
                    mm.Verts(rightX, this.bExtendZ ? 0f : thisY - slabY, back, 0, 0);
                    mm.Verts(leftX, edgeY, back, 1, 0.9f);
                    mm.Verts(rightX, edgeY, back, 0, 0.9f);
                    mm.Tris(0, 2, 1, 1, 2, 3);
                    mm.Tris(4, 5, 6, 5, 7, 6, 0, 4, 2, 2, 4, 6);
                    mm.Tris(1, 3, 5, 3, 7, 5);
                    */
                }
                else
                {
                    //Draw simple cube/block
                    //see code above, moved there
                }
            }
        }
#endif

        } //loop to draw each block in a row

        mm.Attach(amesh);

        xRidges[numRidges].AddRidge(newRidge, amesh, binindex, row);
        numRidges++;
    }

    private void AddLabel(Label alabel)
    {
        if (alabel.Type == Label.TypeE.row)
        {
            if (alabel.Side == Label.SideE.rightOrBottom)
                labelsRowRight.Add(alabel);
            else
                labelsRowLeft.Add(alabel);
        }
        else
        {
            if (alabel.Side == Label.SideE.rightOrBottom)
                labelsColumnBottom.Add(alabel);
            else
                labelsColumnTop.Add(alabel);
        }
    }

    private void DestroyLabels()
    {
        DestroyLabelList(labelsColumnBottom);
        DestroyLabelList(labelsColumnTop);
        DestroyLabelList(labelsRowLeft);
        DestroyLabelList(labelsRowRight);
    }

    private void DestroyLabelList(List<Label> list)
    {
        foreach (Label l in list)
            l.Destroy();
        list.Clear();
    }

    private void GenerateRowLabels()
    {
        for(int row = 0; row < numRows; row++)
        {
            float z = GetRowSceneZ(row);
            //Calc an extra amount to increase size of text box, since the characters don't fill the entire height of the text box.
            float extraZ = (rowDepthFull - rowDepthDataOnly) / 2;

            string labelTxt = row.ToString();
            if ((row <= numRowLabels))
                if (rowLabels[row] != null)
                    labelTxt = rowLabels[row];

            //An offset so label isn't right up on edge of graph
            float offset = GetBlockSceneWidth() / 2;

            //right side label
            Vector3 pos = new Vector3(this.sceneCorner.x + this.sceneWidth + offset, this.sceneCorner.y + 0.01f, this.sceneCorner.z + z - extraZ / 2);
            AddLabel(new Label(protolabel, runtimeLabelsContainer, Label.TypeE.row, Label.SideE.rightOrBottom, pos, rowDepthDataOnly + extraZ, labelTxt));

            //left side label
            pos.x = sceneCorner.x - offset;
            AddLabel(new Label(protolabel, runtimeLabelsContainer, Label.TypeE.row, Label.SideE.leftOrTop, pos, rowDepthDataOnly + extraZ, labelTxt));
        }
    }

    private void GenerateColumnLabels()
    {
        for(int col = 0; col < numCols; col++)
        {
            string labelTxt = col.ToString();
            if (DataManager.I.HeightVar.hasColumnHeaders) //need a general accessor to headers instead of going through HeightVar
                labelTxt = DataManager.I.HeightVar.columnHeaders[col];

            //An offset so label isn't right up on edge of graph
            float offset = rowDepthDataOnly / 2;

            //Bottom/near label
            Vector3 pos = new Vector3(sceneCorner.x + col * GetBlockSceneWidth(), sceneCorner.y + 0.01f, sceneCorner.z - offset);
            AddLabel(new Label(protolabel, runtimeLabelsContainer, Label.TypeE.column, Label.SideE.rightOrBottom, pos, GetBlockSceneWidth(), labelTxt));

            //Top/far label
            pos.z = sceneCorner.z + sceneDepthFull + offset;
            AddLabel(new Label(protolabel, runtimeLabelsContainer, Label.TypeE.column, Label.SideE.leftOrTop, pos, GetBlockSceneWidth(), labelTxt));

        }
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
            case SMVmapping.VRdesktopViewMode:
                VRManager.I.OnDesktopViewDropdown(SMV.I.GetValueInt(SMVmapping.VRdesktopViewMode));
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
        this.ScaleRidgeHeight(this.currGraphHeightScale);
    }

    public Graph()
    {
        //NOTE - stauffer - some inits are made now in Initialize()
        //But if I move everything here into Initialize(), then display/view
        // gets messed up

        this.sceneWidth = 20f; // 400f;
        this.sceneDepthByBin = sceneWidth; // 400f;
        this.sceneDepthMaxApprox = 2f * this.sceneWidth;
        this.sceneHeight = 5f; // 200f;

        //Keep this tiny, but > 0
        this.lowGraphHeightScaleRange = 0.001f;
        this.highGraphHeightScaleRange = 1f;
        //this.currGraphHeightScale = 0.5f; see Initialize()

        this.binSeparationFrac = 1.1f;
        this.rowGapFrac = 1f;
        this.colLimit = 32000;
        this.doingEdges = false; //If want to change this, see declaration of doingEdges
        this.bevelFraction = 0.05f;
    }
}