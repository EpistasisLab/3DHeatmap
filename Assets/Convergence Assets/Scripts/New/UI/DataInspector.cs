using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tools for selecting data, initially from screen, mouse or touch.
/// Initially from
/// </summary>
public class DataInspector : MonoBehaviour {

    /// <summary> Unity layer assigned to data objects. For ray-casting. </summary>
    public int DataObjectLayer = 8;

    /// <summary> GameObject with cube that's a) shown to indicate selected data column; b) used for raycast collisions  </summary>
    public GameObject cubePrefab;
    private GameObject dataIndicatorCube;
    private GameObject blockIntersectionCube;
    private Collider blockIntersectionCollider;
    #pragma warning disable 414
    private RaycastHit blockHit;
    #pragma warning restore 414

    /// <summary> Simple display panel object with component SimpleTextPanelHandler </summary>
    public GameObject displayPanel;
    private SimpleTextPanelHandler displayPanelHandler;

    public float indicatorFlashFreq;
    /// <summary> Maximum alpha value used for the indicator when it's overlaid </summary>
    public float indicatorMaxAlpha;

    /// <summary> Show the ray we cast for data intersection for debugging </summary>
    public bool dbgShowRay;
    /// <summary> Flag to show some debugging info  </summary>
    public bool dbgOutput;

    /// <summary> Use these to generate a ray for debugging </summary>
    public Vector3 dbgRayOrigin;
    public Vector3 dbgRayDirex;
    private Ray dbgRay;

    /// <summary> True if the DataIndicator is currently being shown </summary>
    private bool isShowing;

	// Use this for initialization
	void Start ()
    {
        isShowing = false;
        displayPanelHandler = displayPanel.GetComponent<SimpleTextPanelHandler>();
        if (displayPanelHandler == null)
            Debug.LogError("displayPanelHandler == null");

        dataIndicatorCube = Instantiate(cubePrefab, transform);
        if(dataIndicatorCube == null)
            Debug.LogError("dataIndicatorCube == null");
        blockIntersectionCube = Instantiate(cubePrefab, transform);
        if (blockIntersectionCube == null)
            Debug.LogError("blockIntersectionCube == null");
        blockIntersectionCube.SetActive(true);
        blockIntersectionCube.GetComponent<MeshRenderer>().enabled = false;
        blockIntersectionCollider = blockIntersectionCube.GetComponent<Collider>();
        if (blockIntersectionCollider == null)
            Debug.LogError("blockIntersectionCollider == null");
        
    }

    // Update is called once per frame
    //	void Update () {

    //	}

    /// <summary>
    /// Get TriDataPoint data object from a screen position, and optionally show UI features for the selected point.
    /// Returned object will have isValid field set true if valid.
    /// </summary>
    /// <param name="pointerPosition">Screen position, expected from mouse or touch</param>
    /// <param name="showDataIndicator">True to highlight the data column that's being inspected</param>
    /// <param name="showDataInspector">True to show data point info in UI</param>
    /// <returns></returns>
    public TriDataPoint InspectDataAtScreenPosition(Vector3 pointerPosition, bool showDataIndicator, bool showDataInspector)
    {
        TriDataPoint triData = GetDataAtScreenPosition(pointerPosition);

        if (triData.isValid)
        {
            if (showDataIndicator)
                ShowDataIndicator(triData);
            else
                HideDataIndicator();
            if (showDataInspector)
                ShowDataInspector(triData);
            else
                HideDataInspector();
        }

        return triData;
    }

    /// <summary>
    /// Raycast from pointer position to data to get a data point.
    /// Check returned object 'isValid' property to test for success.
    /// </summary>
    /// <param name="pointerPosition"></param>
    /// <returns></returns>
    private TriDataPoint GetDataAtScreenPosition(Vector3 pointerPosition)
    {
        int row, col;

        if( !SelectAtScreenPositionUsingGridIntersection(pointerPosition, out row, out col) )
        {
            //Hide any ui elements
            Hide();
            //Return empty data object, with isValid = false
            return new TriDataPoint();
        }

        TriDataPoint result = new TriDataPoint(row, col);

        //This will hold the data for the data variables at [row,col]
        return result;
    }

    /// <summary>
    /// Determine which column we're pointing at by manually doing grid intersections. 
    /// Does not require meshes so is faster.
    /// </summary>
    /// <param name="pointerPosition">Screen position of pointer/mouse</param>
    /// <param name="rowOut">returns data row being pointed at</param>
    /// <param name="colOut">return data column </param>
    /// <returns>True if find an intersection, false if not</returns>
    private bool SelectAtScreenPositionUsingGridIntersection(Vector3 pointerPosition, out int rowOut, out int colOut)
    {
        //Debug.Log("---");
        rowOut = colOut = 0;

        //Hack during VR dev
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        return SelectWithRayUsingGridIntersection(ray, out rowOut, out colOut);
    }

    /// <summary>
    /// Debugging method. Use dbgRay direction to select block
    /// </summary>
    /// <returns></returns>
    public void DbgSelectWithDebugRay()
    {
        Debug.Log("====== Select with dbgRay ");
        int row = 0;
        int col = 0;
        Ray ray = new Ray(dbgRayOrigin, dbgRayDirex);
        SelectWithRayUsingGridIntersection(ray, out row, out col);
    }

    /// <summary> Working method so we can easily send it controlled rays for debugging </summary>
    private bool SelectWithRayUsingGridIntersection(Ray ray, out int rowOut, out int colOut)
    {
        rowOut = colOut = 0;
        string dbgStr = "";

        if (dbgOutput)
            dbgStr = "ray origin, direction: " + ray.origin.ToString("f3") + " " + ray.direction.ToString("f3") + "\n";
        if (dbgShowRay)
        {
            dbgRay = ray;
        }

        //Find the intersection with ground/terrain.
        RaycastHit terrainHit;
        int terrainHitIntX;
        int terrainHitIntZ;
        if ( ! Terrain.activeTerrain.GetComponent<Collider>().Raycast(ray, out terrainHit, 100000f))
        {
            //If it doesn't intersect, we still might be pointing flat or upwards and hitting something,
            // so set point to some max values based on ray direction, so the tests below will work 
            terrainHitIntX = ray.direction.x >= 0 ? int.MaxValue : int.MinValue;
            terrainHitIntZ = ray.direction.z >= 0 ? int.MaxValue : int.MinValue;
        }
        else
        {
            terrainHitIntX = Mathf.FloorToInt((terrainHit.point.x - HeatVRML.Instance.xzySceneCorner.x) / HeatVRML.Instance.GetColumnSceneWidth());
            terrainHitIntZ = Mathf.FloorToInt((terrainHit.point.z - HeatVRML.Instance.xzySceneCorner.z) / HeatVRML.Instance.rowDepthFull);
        }
        if (dbgOutput)
            dbgStr += "terrainHit, HitInt X Z: " + terrainHit.point.ToString("F1") + " " + terrainHitIntX + " " + terrainHitIntZ + "\n";

        //Determine equation of the 2D line that the ray makes in the xz-plane, where z = mx + b
        //m = slope (z/x)
        //b = intercept
        //
        //First calc the slope ratio, since our grid cells in scene are not square  - because each ridge (row of blocks)
        // can have separator space between them.
        float slopeRatio = (HeatVRML.Instance.rowDepthFull / HeatVRML.Instance.rowDepthDataOnly);
        //The slope, taking slopeRatio into account. So this should now be in normalized unit for a regularized data grid.
        float m = ray.direction.x != 0 ? ray.direction.z / ray.direction.x / slopeRatio : float.MaxValue;
        //A point on the line in normalized grid space. Using the ray origin
        float pz = (ray.origin.z - HeatVRML.Instance.xzySceneCorner.z) / HeatVRML.Instance.rowDepthFull;
        float px = (ray.origin.x - HeatVRML.Instance.xzySceneCorner.x) / HeatVRML.Instance.GetColumnSceneWidth();
        //Intercept, relative to graph front-left corner at 0,0
        float b = pz - m * px;

        if (dbgOutput)
            dbgStr += "m, px, pz, b, sceneCorner: " + m + ", " + px + ", " + pz + ", " + b + ", " + HeatVRML.Instance.xzySceneCorner + "\n";

        //If we're pointing away from the data, skeedaddle
        if (px >= HeatVRML.Instance.numCols)
            if ( ray.direction.x >= 0)
                return false;
        if (px < 0)
            if (ray.direction.x <= 0)
                return false;
        if (pz >= HeatVRML.Instance.numRows)
            if (ray.direction.z >= 0)
                return false;
        if (pz < 0)
            if (ray.direction.z <= 0)
                return false;
        
        //Step through columns, including the right-side of the last column.
        //We operate in unit widths and depths.
        //We check the z intersection at the left side of each column (and right side of last column),
        // then compare between neighboring columns, and use that to know the particular data point(s)
        // we've intersected. If from one column edge to the next the z value crosses more than one row,
        // we track that and add more than one data point to the list.
        //
        //Take into account whether we start with ray origin over/within the data, and ray direction
        //We've already made sure above that we're pointing at the data.
        int cStart;
        int cEnd;
        if ( ray.direction.x >= 0)
        {
            cStart = Mathf.Max(0, Mathf.FloorToInt(px));
            //Only go as far as the intersection of ray with terrain/ground (make sure to adjust this for VR if terrain/ground doesn't end up moving with graph)
            cEnd = Mathf.Min(terrainHitIntX, HeatVRML.Instance.numCols-1);
        }
        else
        {
            cStart = Mathf.Max(0, terrainHitIntX);
            cEnd = Mathf.Min( Mathf.FloorToInt(px), HeatVRML.Instance.numCols-1);
        }

        bool foundAnIntersection = false;
        for ( int c = cStart; c <= cEnd; c++)
        {
            //For the left side of this column, calc the z intersection point, i.e. the row
            float z = m * c + b;
            //the rows at which ray passes through left and right edges of column
            //The integer row number. FloorToInt yields largest int <= z, so for negs, gets more negative
            int rLeft = Mathf.FloorToInt( z );
            float z1 = m * (c + 1) + b; //intesection at right edge of column
            int rRight = Mathf.FloorToInt(z1);
            if (dbgOutput)
                dbgStr += "col " + c + ", raw rLeft rRight " + rLeft + ", " + rRight + "\n";
            //If left side row < 0 or > numRows, we're not on the scene grid yet, or we've gone past it, so just do right edge if it's in bounds.
            if (rLeft < 0 )
            {
                if (rRight < 0)
                    continue;
                rLeft = 0;
            }
            if (rLeft >= HeatVRML.Instance.numRows)
            {
                if (rRight >= HeatVRML.Instance.numRows)
                    continue;
                rLeft = HeatVRML.Instance.numRows - 1;
            }

            //Now that we now rLeft is in bounds, check rRight. Don't just assign rLeft to it,
            // because we have to be able to traverse multiple rows when right side is out of bounds.
            if (rRight < 0 || rRight >= HeatVRML.Instance.numRows)
               rRight = Mathf.Max(0, Mathf.Min(rRight, HeatVRML.Instance.numRows-1));

            //If ray origin is within/over the data, only search rows that are under the ray,
            // and if ray intersection with plane is within data area, only search that far.
            if (ray.direction.z >= 0)
            {
                rLeft = Mathf.Max(rLeft, Mathf.FloorToInt(pz));
                rLeft = Mathf.Min(rLeft, terrainHitIntZ);
                rRight = Mathf.Max(rRight, Mathf.FloorToInt(pz));
                rRight = Mathf.Min(rRight, terrainHitIntZ);
            }
            else
            {
                rLeft = Mathf.Min(rLeft, Mathf.FloorToInt(pz));
                rLeft = Mathf.Max(rLeft, terrainHitIntZ);
                rRight = Mathf.Min(rRight, Mathf.FloorToInt(pz));
                rRight = Mathf.Max(rRight, terrainHitIntZ);
            }

            if (dbgOutput)
                dbgStr += "col " + c + ", checking rows: " + rLeft + " - " + rRight + "\n";

            //Cycle over all the in-bounds rows we cross between this column's left and right edges
            //Direction of iteration over rows depends on whether starting point is above or below grid.
            int step = rLeft <= rRight ? 1 : -1;
            int end = rRight + step;
            for (int rr = rLeft; rr != end; rr+=step)
            {
                if (dbgOutput)
                    dbgStr += "   examining col,row: " + c + ", " + rr + "\n";

                //Scale and move the cube for raycasted intersection testing.
                PrepareBlockOverlay(blockIntersectionCube, rr, c, 0/*bin - just always do 0, for now at least*/);
                if( blockIntersectionCollider.Raycast(ray, out blockHit, 100000f))
                {
                    rowOut = rr;
                    colOut = c;
                    //For rays going left-to-right, we just return the first intersection
                    if (ray.direction.x >= 0)
                        return true;
                    //Otherwise for right-to-left rays we return the last one we find.
                    //NOTE if it gets too slow to do all these intersection tests with 
                    // huge data sets, we could reverse the order of columns traversal above
                    // for right-to-left rays, and then pick the first one.
                    foundAnIntersection = true;
                }
            }
        }

        if(dbgOutput)
            Debug.Log(dbgStr);

        return foundAnIntersection;
    }


    /// <summary> Hide any UI elements managed by this component </summary>
    public void Hide()
    {
        HideDataIndicator();
        HideDataInspector();
    }

    /// <summary>
    /// Take a game object for a cube and scale and position it based on data vals so it can overlay the block in the scene.
    /// Used for both of the cubes used for the block visual overlay and the block interection test.
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="dataHeightValue"></param>
    /// <param name="dataRow"></param>
    /// <param name="dataCol"></param>
    /// <param name="dataBin"></param>
    /// <param name="extraScale">Small scale factor that helps with visual overlay not causing visual artifacts.</param>
    private void PrepareBlockOverlay(GameObject cube, int dataRow, int dataCol, int dataBin, float extraScale = 1f)
    {
        //Size a cube to the size of the selected column and flash it.
        float width = HeatVRML.Instance.GetColumnSceneWidth();
        float height = HeatVRML.Instance.GetColumnSceneHeightByPosition(dataRow, dataCol);
        float depth = HeatVRML.Instance.rowDepthDataOnly;
        cube.transform.localScale = new Vector3(width, height, depth) * extraScale;

        //Debug.Log("heightValue, minDataHeight, dataHeightRangeScale, zSceneSize, currGraphHeightScale, xzySceneCorner");
        //Debug.Log(triData.heightValue + ", " + HeatVRML.Instance.minDataHeight + ", " + HeatVRML.Instance.dataHeightRangeScale + ", " + HeatVRML.Instance.zSceneSize + ", " + HeatVRML.Instance.currGraphHeightScale + ", " + HeatVRML.Instance.xzySceneCorner);
        Vector3 pos = new Vector3()
        {
            y = height / 2f + HeatVRML.Instance.xzySceneCorner.y,
            x = ((((dataCol + 0.5f) - HeatVRML.Instance.minCol) * HeatVRML.Instance.xSceneSize) / HeatVRML.Instance.numCols) + HeatVRML.Instance.xzySceneCorner.x
        };

        //Remember orig developer switched y & z - I really should rename these!
        float yoff = dataRow * HeatVRML.Instance.rowDepthFull;
        if (HeatVRML.Instance.binInterleave)
        {
            yoff = yoff + (dataBin * HeatVRML.Instance.binSeparation);
        }
        else
        {
            yoff = yoff + (dataBin * HeatVRML.Instance.ySceneSizeByBinWithSep);
        }
        pos.z = (HeatVRML.Instance.xzySceneCorner.z + yoff) + (HeatVRML.Instance.rowDepthDataOnly / 2f);

        cube.transform.position = pos;
    }

    /// <summary>
    /// Show a graphic on top of a data column. Intended for showing which column was selected/clicked for inspection.
    /// Adapated from ShowPointedData() in original code. 
    /// </summary>
    private void ShowDataIndicator(TriDataPoint triData)
    {
        if ( ! triData.isValid )
            return;

        PrepareBlockOverlay(dataIndicatorCube, triData.row, triData.col, triData.bin, 1.02f);

        dataIndicatorCube.SetActive(true);

        isShowing = true;
        StartCoroutine(DataIndicatorAnimate());
    }

    public void HideDataIndicator()
    {
        isShowing = false;
        dataIndicatorCube.SetActive(false);
    }

    IEnumerator DataIndicatorAnimate()
    {
        while (isShowing)
        {
            float phase = Mathf.Sin(Mathf.PI * 2f * Time.time * indicatorFlashFreq );
            Color color = dataIndicatorCube.GetComponent<Renderer>().material.color;
            color.a = phase * indicatorMaxAlpha; //note, if try this: (phase + 1 )/2;  then when alpha goes to 0, the underlying mesh is not drawn/seen
            dataIndicatorCube.GetComponent<Renderer>().material.color = color;
            yield return null;
        }
    }

    /// <summary> Show a simple text panel with the data point info </summary>
    /// <param name="triData"></param>
    public void ShowDataInspector(TriDataPoint triData)
    {
        string str = "";
        str += "row, col #: " + triData.row + ", " + triData.col;
        if (triData.rowHeader != "")
            str += "\nrow: " + triData.rowHeader;
        if (triData.colHeader != "")
            str += "\ncol : " + triData.colHeader;
        str += "\n\n" + triData.heightValue.ToString("F3") + " - " + triData.heightLabel +
            "\n" + triData.topValue.ToString("F3") + " - " + triData.topLabel +
            "\n" + triData.sideValue.ToString("F3") + " - " + triData.sideLabel;

        displayPanelHandler.Show(str);
    }

    public void HideDataInspector()
    {
        displayPanelHandler.Hide();
    }

    public void Update()
    {
        if (dbgShowRay && dbgRay.direction.magnitude > 0)
        {
            RaycastHit hit;
            float dist = 10000f;
            if (Terrain.activeTerrain.GetComponent<Collider>().Raycast(dbgRay, out hit, 10000f))
                dist = hit.distance;
            Debug.DrawRay(dbgRay.origin, dbgRay.direction * dist, Color.red);
            Vector3 o = dbgRay.origin;
            o.y = HeatVRML.Instance.xzySceneCorner.y;
            Vector3 d = dbgRay.direction;
            d.y = 0;
            Debug.DrawRay(o, d * 10000f, Color.yellow);
            //Debug.DrawRay(new Vector3(0, 100, 0), new Vector3(0, 0, 1) * 10000f, Color.yellow);
        }
    }

}
