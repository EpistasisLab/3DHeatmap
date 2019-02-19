using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tools for selecting data from screen, mouse or touch.
/// Initially from
/// </summary>
public class PointerDataSelection : MonoBehaviour {

    /// <summary> Unity layer assigned to data objects. For ray-casting. </summary>
    public int DataObjectLayer = 8;

    /// <summary> GameObject with mesh that's shown to indicate selected data column </summary>
    public GameObject dataIndicator;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    //	void Update () {

    //	}

    /// <summary>
    /// Raycast from pointer position to data to select a data point.
    /// Check returned object 'isValid' property to test for success.
    /// </summary>
    /// <param name="pointerPosition"></param>
    /// <returns></returns>
    public TriDataPoint GetDataAtScreenPosition(Vector3 pointerPosition)
    {
        int row, col;
        if( ! SelectAtScreenPosition(pointerPosition, out row, out col) )
        {
            //Return empty data object, with isValid = false
            return new TriDataPoint();
        }

        TriDataPoint result = new TriDataPoint(row, col);

        ShowDataIndicator(result);

        //This will hold the data for the data variables at [row,col]
        return result;
    }

    public bool SelectAtScreenPosition(Vector3 pointerPosition, out int row, out int col)
    {
        row = col = 0;

        //Stauffer - this code mostly taken from old HeatVRML.CapturePointedAt method that was unused.
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        //Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 10, false);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << DataObjectLayer))
        {
            return false;
        }
        Debug.Log("Raycast gameObject name: " + hit.transform.gameObject.name);
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

    /// <summary>
    /// Show a graphic on top of a data column. Intended for showing which column was selected/clicked for inspection.
    /// Adapated from ShowPointedData() in original code. 
    /// Goal is to eventually have a better method that changes color or otherwise highlights the column.
    /// </summary>
    public virtual void ShowDataIndicator(TriDataPoint triData)
    {
        if ( ! triData.isValid )
            return;

        float signSize = 0.9f * Mathf.Min(HeatVRML.Instance.rowDepthDataOnly, HeatVRML.Instance.xSceneSize / HeatVRML.Instance.numCols);
        dataIndicator.transform.localScale = new Vector3(signSize, signSize, signSize);
        //Note - should make a method to get scene position from a data column/position
        //float sceney = (((((triData.heightValue - HeatVRML.Instance.minDataHeight) * HeatVRML.Instance.dataHeightRangeScale) + 0.1f) * HeatVRML.Instance.zSceneSize) * HeatVRML.Instance.currGraphHeight) + HeatVRML.Instance.xzySceneCorner.y;
        float sceney = HeatVRML.Instance.GetColumnSceneHeight(triData.heightValue);

        Debug.Log("heightValue, minDataHeight, dataHeightRangeScale, zSceneSize, currGraphHeight, xzySceneCorner");
        Debug.Log(triData.heightValue + ", " + HeatVRML.Instance.minDataHeight + ", " + HeatVRML.Instance.dataHeightRangeScale + ", " + HeatVRML.Instance.zSceneSize + ", " + HeatVRML.Instance.currGraphHeight + ", " + HeatVRML.Instance.xzySceneCorner);

        //Raise it up to rest on top of column
        sceney += signSize / 2;
        float scenex = ((((triData.col + 0.5f) - HeatVRML.Instance.minCol) * HeatVRML.Instance.xSceneSize) / HeatVRML.Instance.numCols) + HeatVRML.Instance.xzySceneCorner.x;
        //Remember orig developer switched y & z
        float yoff = triData.row * HeatVRML.Instance.rowDepthFull;
        if (HeatVRML.Instance.binInterleave)
        {
            yoff = yoff + (triData.bin * HeatVRML.Instance.binSeparation);
        }
        else
        {
            yoff = yoff + (triData.bin * HeatVRML.Instance.ySceneSizeByBinWithSep);
        }
        float scenez = (HeatVRML.Instance.xzySceneCorner.z + yoff) + (HeatVRML.Instance.rowDepthDataOnly / 2f);

        Debug.Log("signSize: " + signSize + " scenex, y, z: " + scenex + ", " + sceney + ", " + scenez);
        dataIndicator.transform.position = new Vector3(scenex, sceney, scenez);
        dataIndicator.GetComponent<Renderer>().enabled = true;
    }

    public void Update()
    {
        //Debug.DrawRay(Vector3.zero, new Vector3(1, 1, 1)*1000f, Color.red);
    }

}
