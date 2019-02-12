using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tools for selecting data from screen, mouse or touch.
/// Initially from
/// </summary>
public class PointerDataSelection : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    //	void Update () {

    //	}

    public bool SelectAtScreenPosition(Vector3 pointerPosition, out int row, out int col)
    {
        row = col = 0;

        //Stauffer - this code mostly taken from old HeatVRML.CapturePointedAt method that was unused.
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        ray.origin = Camera.main.transform.position;
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, HeatVRML.dataPointMask))
        {
            return false;
        }
        IdentifyRidge idScript = (IdentifyRidge) hit.transform.gameObject.GetComponent(typeof(IdentifyRidge));

        // Calculate column from x position of hit
        // It is possible for this to be one too high if it hits on the right side of a column, so check for this condition
   /*
        float floatCol = (((hit.point.x - HeatVRML.xzySceneCorner.x) * HeatVRML.numCols) / HeatVRML.xSceneSize) + HeatVRML.minCol;
        if (((floatCol - Mathf.Floor(floatCol)) < 0.1f) && (hit.normal.x > 0.95f))
        {
            floatCol = floatCol - 1f;
        }

        col = (int)Mathf.Floor(floatCol);
        row = idScript.row;
    */
        return true;
    }

}
