using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    private GameObject visualMappingPanel;
    private VisualMappingUIHandler visualMappingUIHandler;

	// Use this for initialization
	void Start () {
        visualMappingPanel = GameObject.Find("VisualMappingPanel");
        if (visualMappingPanel == null)
            Debug.LogError("visualMappingPanel == null");
        visualMappingUIHandler = visualMappingPanel.GetComponent<VisualMappingUIHandler>();
        if (visualMappingUIHandler == null)
            Debug.LogError("visualMappingUIHandler == null");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Call this when data has been updated in some way the will need a UI refresh (i.e. DataVariables)
    /// </summary>
    public void DataUpdated()
    {
        visualMappingUIHandler.RefreshUI();
    }
}
