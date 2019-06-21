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

    /// <summary> GameObject with mesh that's shown to indicate selected data column </summary>
    public GameObject dataIndicator;

    /// <summary> Simple display panel object with component SimpleTextPanelHandler </summary>
    public GameObject displayPanel;
    private SimpleTextPanelHandler displayPanelHandler;

    public float indicatorFlashFreq;
    /// <summary> Maximum alpha value used for the indicator when it's overlaid </summary>
    public float indicatorMaxAlpha;

    /// <summary> Show the ray we cast for data intersection for debugging </summary>
    public bool dbgShowRay;
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
    /// Use the ridge mesh collider to determine column that's being pointed at.
    /// </summary>
    /// <returns>True if find an intersection, false if not</returns>
    private bool SelectAtScreenPositionUsingCollider(Vector3 pointerPosition, out int row, out int col)
    {
        row = col = 0;

        //Hack during VR dev
        if (Camera.main == null)
            return false;

        //Stauffer - this code mostly taken from old HeatVRML.CapturePointedAt method that was unused. 
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << DataObjectLayer))
        {
            return false;
        }
        //Debug.Log("Raycast gameObject name: " + hit.transform.gameObject.name);
        IdentifyRidge idScript = (IdentifyRidge) hit.transform.gameObject.GetComponent(typeof(IdentifyRidge));

        // Calculate column from x position of hit
        // It is possible for this to be one too high if it hits on the right side of a column, so check for this condition
   
        float floatCol = (((hit.point.x - HeatVRML.Instance.xzySceneCorner.x) * HeatVRML.Instance.numCols) / HeatVRML.Instance.xSceneSize) + HeatVRML.Instance.minCol;
        if (((floatCol - Mathf.Floor(floatCol)) < 0.1f) && (hit.normal.x > 0.95f))
        {
            floatCol = floatCol - 1f;
        }

        col = (int)Mathf.Floor(floatCol);
        row = idScript.row;
    
        return true;
    }


    /// <summary> Simple struct used by SelectAtScreenPositionUsingGridIntersection </summary>
    struct Intersection
    {
        public int row;
        public int col;
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
        Debug.Log("---");
        rowOut = colOut = 0;

        //Hack during VR dev
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        //Debug.Log("ray origin, direction: " + ray.origin.ToString("f3") + " " + ray.direction.ToString("f3"));
        if (dbgShowRay)
        {
            dbgRay = ray;
        }

        //Determine equation of the 2D line that the ray makes relative to xz-plane, where z = mx + b
        //m = slope (z/x)
        //b = intercept
        float m = ray.direction.z / Mathf.Max(ray.direction.x, 0.00001f);
        //Intercept, relative to graph centered at 0,0, and normalized to [0,1] for graph area
        float bz = (ray.origin.z - HeatVRML.Instance.xzySceneCorner.z) / HeatVRML.Instance.rowDepthFull;
        float bx = (ray.origin.x - HeatVRML.Instance.xzySceneCorner.x) / HeatVRML.Instance.GetColumnSceneWidth();
        float b = bz - m * bx; //b = z - mx

        //List of data cell positions that are crossed by the line and are intersecting the ray
        List<Intersection> isx = new List<Intersection>();

        //Step through columns, including the right-side of the last column.
        //We operate in unit widths and depths.
        //We check the z intersection at the left side of each column (and right side of last column),
        // then compare between neighboring columns, and use that to know the particular data point(s)
        // we've intersected. If from one column edge to the next the z value crosses more than one row,
        // we track that and add more than one data point to the list.
        //
        for ( int c = 0; c < HeatVRML.Instance.numCols; c++)
        {
            //For the left of this column, calc the z intersection point, i.e. the row
            float z = m * c + b;
            //Convert it to row number by taking into account the spacing between rows
            float depthRatio = (HeatVRML.Instance.rowDepthFull / HeatVRML.Instance.rowDepthDataOnly);
            int r = Mathf.FloorToInt(  z / depthRatio );

            //If it's < 0, we're not on the scene grid yet, or we've gone past it
            if (r < 0 || r >= HeatVRML.Instance.numRows )
                continue;

            //rows at which ray passes through left and right edges of column
            int r0 = r;
            float z1 = m * (c + 1) + b; //intesection at right edge of column
            int r1 = Mathf.Min( Mathf.FloorToInt( z1 / depthRatio), HeatVRML.Instance.numRows-1 ); //don't let it go out of bounds
            //Cycle over all the rows we cross between this column's left and right edges
            for (int rr = r0; rr <= r1; rr++)
            {
                Debug.Log("examining col,row: " + c + ", " + rr);
                //Find where in the column's cell it intersects (i.e. which sides it enters and leaves),
                //and calc ray height at those points, and store the min of those.
                //
                float minIxHeight = float.MaxValue;
                float xix, zix;
                //Intersections with bottom and top of cell
                // x = (z - b) / m
                xix = (rr - b) / m; //need to handle m == 0
                if (xix >= c && xix <= c + 1)
                    minIxHeight = Mathf.Min(minIxHeight, CalcRayScreenHeight(ray, xix, rr));
                xix = (rr+1 - b) / m;
                if (xix >= c && xix <= c + 1)
                    minIxHeight = Mathf.Min(minIxHeight, CalcRayScreenHeight(ray, xix, rr));
                //Intersections with sides of cell
                zix = m * c + b; //left
                if(zix >= rr && zix <= rr+1)
                    minIxHeight = Mathf.Min(minIxHeight, CalcRayScreenHeight(ray, c, zix));
                zix = m * (c+1) + b; //right
                if (zix >= rr && zix <= rr + 1)
                    minIxHeight = Mathf.Min(minIxHeight, CalcRayScreenHeight(ray, c, zix));

                //Get scene-height of the column at this position
                float h = HeatVRML.Instance.GetColumnSceneHeightByPosition(rr, c);

                //If the minimum ray intersection height is <= the height of the column
                // at this point, we've got a hit.
                if( minIxHeight <= h)
                {
                    isx.Add(new Intersection { col = c, row = rr });
                    Debug.Log("found intersection at col, row: " + c + ", " + rr);
                }
                //Debug.Log("minIxHeight, h: " + minIxHeight + ", " + h);
            }
        }

        //Did we find anything?
        if (isx.Count == 0)
            return false;

        //Now that we've gone all through the columns and found all the intersections with
        // the ray, choose the appropriate one based on direction of ray
        rowOut = isx[0].row;
        colOut = isx[0].col;

        return true;
    }

    /// <summary> Calc screen-unit height of a ray at normalized x and z positions. Helper function. </summary>
    private float CalcRayScreenHeight(Ray ray, float xNorm, float zNorm)
    {
        //Calc distance in xz-plane from eye/ray to cell intersection point
        Vector2 ix = new Vector2(xNorm * HeatVRML.Instance.GetColumnSceneWidth(), zNorm * HeatVRML.Instance.rowDepthFull);
        float xzDist = (ix - new Vector2(ray.origin.x, ray.origin.z)).magnitude;
        //Project distance xzDist along the ray to get height at the cell intersection point
        return (ray.origin + xzDist * ray.direction).y;
    }

    /// <summary> Hide any UI elements managed by this component </summary>
    public void Hide()
    {
        HideDataIndicator();
        HideDataInspector();
    }

    /// <summary>
    /// Show a graphic on top of a data column. Intended for showing which column was selected/clicked for inspection.
    /// Adapated from ShowPointedData() in original code. 
    /// </summary>
    private void ShowDataIndicator(TriDataPoint triData)
    {
        if ( ! triData.isValid )
            return;

        //Size a cube to the size of the selected column and flash it.

        float width = HeatVRML.Instance.GetColumnSceneWidth();
        float height = HeatVRML.Instance.GetColumnSceneHeight(triData.heightValue);
        float depth = HeatVRML.Instance.rowDepthDataOnly;
        float extra = 1.02f; //to avoid artifacts from overlapping tris
        dataIndicator.transform.localScale = new Vector3(width, height, depth) * extra;

        Vector3 pos = new Vector3();
        pos.y = height / 2f + HeatVRML.Instance.xzySceneCorner.y;

        //Debug.Log("heightValue, minDataHeight, dataHeightRangeScale, zSceneSize, currGraphHeightScale, xzySceneCorner");
        //Debug.Log(triData.heightValue + ", " + HeatVRML.Instance.minDataHeight + ", " + HeatVRML.Instance.dataHeightRangeScale + ", " + HeatVRML.Instance.zSceneSize + ", " + HeatVRML.Instance.currGraphHeightScale + ", " + HeatVRML.Instance.xzySceneCorner);

        pos.x = ((((triData.col + 0.5f) - HeatVRML.Instance.minCol) * HeatVRML.Instance.xSceneSize) / HeatVRML.Instance.numCols) + HeatVRML.Instance.xzySceneCorner.x;

        //Remember orig developer switched y & z - I really should rename these!
        float yoff = triData.row * HeatVRML.Instance.rowDepthFull;
        if (HeatVRML.Instance.binInterleave)
        {
            yoff = yoff + (triData.bin * HeatVRML.Instance.binSeparation);
        }
        else
        {
            yoff = yoff + (triData.bin * HeatVRML.Instance.ySceneSizeByBinWithSep);
        }
        pos.z = (HeatVRML.Instance.xzySceneCorner.z + yoff) + (HeatVRML.Instance.rowDepthDataOnly / 2f);

        dataIndicator.transform.position = pos;
        dataIndicator.SetActive(true);

        isShowing = true;
        StartCoroutine(DataIndicatorAnimate());
    }

    public void HideDataIndicator()
    {
        isShowing = false;
        dataIndicator.SetActive(false);
    }

    IEnumerator DataIndicatorAnimate()
    {
        while (isShowing)
        {
            float phase = Mathf.Sin(Mathf.PI * 2f * Time.time * indicatorFlashFreq );
            Color color = dataIndicator.GetComponent<Renderer>().material.color;
            color.a = phase * indicatorMaxAlpha; //note, if try this: (phase + 1 )/2;  then when alpha goes to 0, the underlying mesh is not drawn/seen
            dataIndicator.GetComponent<Renderer>().material.color = color;
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
        if (dbgShowRay)
        {
            Debug.DrawRay(dbgRay.origin, dbgRay.direction * 10000f, Color.red);
            Vector3 o = dbgRay.origin;
            o.y = HeatVRML.Instance.xzySceneCorner.y;
            Vector3 d = dbgRay.direction;
            d.y = 0;
            Debug.DrawRay(o, d * 10000f, Color.yellow);
            //Debug.DrawRay(new Vector3(0, 100, 0), new Vector3(0, 0, 1) * 10000f, Color.yellow);
        }
    }

}
