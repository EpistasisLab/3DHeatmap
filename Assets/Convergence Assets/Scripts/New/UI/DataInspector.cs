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
            //Hide any ui elements
            Hide();
            //Return empty data object, with isValid = false
            return new TriDataPoint();
        }

        TriDataPoint result = new TriDataPoint(row, col);

        ShowDataIndicator(result);
        ShowDataInspector(result);

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
    public virtual void ShowDataIndicator(TriDataPoint triData)
    {
        if ( ! triData.isValid )
            return;

        //Size a cube to the size of the selected column and flash it.

        float width = HeatVRML.Instance.xSceneSize / HeatVRML.Instance.numCols; //awkward. should have a method to retrieve this
        float height = HeatVRML.Instance.GetColumnSceneHeight(triData.heightValue);
        float depth = HeatVRML.Instance.rowDepthDataOnly;
        float extra = 1.02f; //to avoid artifacts from overlapping tris
        dataIndicator.transform.localScale = new Vector3(width, height, depth) * extra;

        Vector3 pos = new Vector3();
        pos.y = height / 2f + HeatVRML.Instance.xzySceneCorner.y;

        //Debug.Log("heightValue, minDataHeight, dataHeightRangeScale, zSceneSize, currGraphHeight, xzySceneCorner");
        //Debug.Log(triData.heightValue + ", " + HeatVRML.Instance.minDataHeight + ", " + HeatVRML.Instance.dataHeightRangeScale + ", " + HeatVRML.Instance.zSceneSize + ", " + HeatVRML.Instance.currGraphHeight + ", " + HeatVRML.Instance.xzySceneCorner);

        pos.x = ((((triData.col + 0.5f) - HeatVRML.Instance.minCol) * HeatVRML.Instance.xSceneSize) / HeatVRML.Instance.numCols) + HeatVRML.Instance.xzySceneCorner.x;

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
            color.a = phase;
            dataIndicator.GetComponent<Renderer>().material.color = color;
            yield return null;
        }
    }

    /// <summary> Show a simple text panel with the data point info </summary>
    /// <param name="triData"></param>
    public void ShowDataInspector(TriDataPoint triData)
    {
        string str = "";
        str += "row, col: " + triData.row + ", " + triData.col;
        if (triData.rowHeader != "")
            str += "\nrow header: " + triData.rowHeader;
        if (triData.colHeader != "")
            str += "\ncol header: " + triData.colHeader;
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
        //Debug.DrawRay(Vector3.zero, new Vector3(1, 1, 1)*1000f, Color.red);
    }

}
