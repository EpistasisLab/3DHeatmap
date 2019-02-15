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
        if( SelectAtScreenPosition(pointerPosition, out row, out col) == false)
        {
            //Return empty data object, with isValid = false
            return new TriDataPoint();
        }

        //This will hold the data for the data variables at [row,col]
        return new TriDataPoint(row, col);
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

    public void Update()
    {
        //Debug.DrawRay(Vector3.zero, new Vector3(1, 1, 1)*1000f, Color.red);
    }

}
