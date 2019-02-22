using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles what's necessary for showing, placing, hiding a simple text display window.
/// </summary>
public class SimpleTextPanelHandler : MonoBehaviour {

    /// <summary> Text UI element child of the game object this component is a part of </summary>
    public Text textElement;

    // Use this for initialization
    void Awake () {
        if (textElement == null)
            Debug.LogError("textElement == null");
        //Hide to start
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the text panel by a given transform. Caller must also call Hide() when done.
    /// Idea is to be able to have the panel pop-up by a desired UI or other scene element (think tool-tip).
    /// </summary>
    /// <param name="text"></param>
    /// <param name="txf">The transform of the gameObject we want the text placed next to</param>
    public void ShowNearTransform(string text, Transform txf)
    {
        //Debug.Log("ShowNearTransform: " + tip + " " + txf.name);
        //Get the corners of the object we want the text next to
        Vector3[] corners = new Vector3[4];
        txf.GetComponent<RectTransform>().GetWorldCorners(corners);
        Vector3 position = corners[2]; //0th is lower-left, going round clockwise
        position.y = (position.y + corners[3].y) / 2f;

        position.x += transform.GetComponent<RectTransform>().rect.width / 2; //tooltip pivot is in center
        transform.position = position;

        Show(text);
    }


    public void ShowInLowerLeft(string text)
    {
        //Call this first cuz it calcs width and height of the panel' rectTxf
        Show(text);
        Debug.LogWarning("ShowInLowerLeft not implemented fully");
        //NOTE this isn't working like I expected. Placement is dependent on 
        // anchors, and setting 'inset' param doesn't behave like I'd expect
        /*
        RectTransform rt = transform.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 1f, rt.rect.width);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 1f, rt.rect.height);
        */
    }

    public void ShowInUpperRight(string text)
    {
        //Call this first cuz it calcs width and height of the panel' rectTxf
        Show(text);

        RectTransform rt = transform.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 1f, rt.rect.width);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1f, rt.rect.height);
    }

    /// <summary> Show at current position. Use other routines and they will set position and then call this </summary>
    /// <param name="text"></param>
    public void Show(string text)
    {
        textElement.text = text;
        gameObject.SetActive(true);

        //Have to manually resize for some reason. Seems like an issue with elements that set their own
        // size, like this text element, and then higher-up elements that react to that size.
        //Interestingly, I can call this right here in same frame as changing the text, and it works,
        // whereas with MessageDialog I have to wait til next frame - maybe cuz I'm instantiating 
        // MessageDialog dynamically?
        ResizeBasedOnText();
    }

    public void Hide()
    {
        //Debug.Log("hide");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Manually resize the panel cuz it's not happening automatically
    /// when I change the text in Show().
    /// </summary>
    private void ResizeBasedOnText()
    {
        Rect textRect = textElement.GetComponent<RectTransform>().rect;
        //Debug.Log("Resize: message obj rect: " + textRect.ToString());

        float height = textRect.height + 11f;
        float width = textRect.width + 20f; 

        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
