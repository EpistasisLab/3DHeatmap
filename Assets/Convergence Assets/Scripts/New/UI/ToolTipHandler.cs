using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles what's necessary for showing, placing, hiding tooltip window.
/// </summary>
public class ToolTipHandler : MonoBehaviour {

    private Text toolTipText;

    // Use this for initialization
    void Awake () {
        toolTipText = transform.Find("ToolTipText").GetComponent<Text>();
        if (toolTipText == null)
            Debug.LogError("toolTipText == null");
        //Hide to start
        gameObject.SetActive(false);
    }

    public void ToolTipShow(string tip, Transform txf)
    {
        gameObject.SetActive(true);
        //Debug.Log("tooltipshow: " + tip + " " + txf.name);
        //Get the corners of the object we want the tooltip next to
        Vector3[] corners = new Vector3[4];
        txf.GetComponent<RectTransform>().GetWorldCorners(corners);
        Vector3 position = corners[2]; //0th is lower-left, going round clockwise
        position.y = (position.y + corners[3].y) / 2f;

        position.x += transform.GetComponent<RectTransform>().rect.width / 2; //tooltip pivot is in center
        transform.position = position;
        toolTipText.text = tip;

        //Have to manually resize for some reason. Seems like an issue with elements that set their own
        // size, like this text element, and then higher-up elements that react to that size.
        //Interestingly, I can call this right here in same frame as changing the text, and it works,
        // whereas with MessageDialog I have to wait til next frame - maybe cuz I'm instantiating 
        // MessageDialog dynamically?
        ResizeBasedOnText();
    }

    public void ToolTipHide()
    {
        //Debug.Log("tooltiphide");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Manually resize the panel cuz it's not happening automatically
    /// when I change the text in ToolTipShow().
    /// </summary>
    private void ResizeBasedOnText()
    {
        Rect textRect = toolTipText.GetComponent<RectTransform>().rect;
        //Debug.Log("Resize: message obj rect: " + textRect.ToString());

        float height = textRect.height + 15f;
        float width = textRect.width + 30; 

        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
