using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary> A row or column label to display on the graph. Holds a game obj with TextMeshPro component, and options for type of lable </summary>
public class Label {

    public enum TypeE { undefined, row, column};
    public enum SideE { undefined, rightOrBottom, leftOrTop};

    private TypeE type;
    private SideE side;

    public TypeE Type { get { return type; } }
    public SideE Side { get { return side; } }

    /// <summary> The game obj holding the text mesh pro component </summary>
    private GameObject gameObj;
    /// <summary> The TextMeshPro component </summary>
    private TextMeshPro textMesh;

    public string Text
    {
        set { if (textMesh == null) return; textMesh.text = value; }
        get { if (textMesh == null) return ""; return textMesh.text; }
    }

    public Label()
    {
        InitEmpty();
    }

    private void InitEmpty()
    {
        type = TypeE.undefined;
        side = SideE.undefined;
        textMesh = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="protoLabel"></param>
    /// <param name="hierarchyContainer"></param>
    /// <param name="newType"></param>
    /// <param name="newSide"></param>
    /// <param name="borderPos">Position of border of graph along which the label should be aligned. </param>
    /// <param name="textHeight"></param>
    /// <param name="text"></param>
    public Label( GameObject protoLabel, GameObject hierarchyContainer, Label.TypeE newType, Label.SideE newSide, Vector3 borderPos, float textHeight, string text)
    {
        Quaternion rotation = newType == TypeE.row ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(90, 90, 0);

        gameObj = Object.Instantiate(protoLabel, borderPos, rotation);

        textMesh = gameObj.GetComponent<TextMeshPro>();
        if(textMesh == null)
        {
            Debug.LogError("Failed to get TextMeshPro for new Label");
            Object.Destroy(gameObj);
            return;
        }

        //Add the label to the graph container object for easier manipulation and a cleaner scene hierarchy
        gameObj.transform.SetParent(hierarchyContainer.transform);

        Text = text;
        type = newType;
        side = newSide;

        //For left-side labels, the position that gets passed in is left edge of graph, so we have to account for
        // the width of the text box
        if (type == TypeE.row && side == SideE.leftOrTop)
        {
            Vector3 newPos = gameObj.transform.position;
            newPos.x = gameObj.transform.position.x - textMesh.rectTransform.rect.width;
            gameObj.transform.position = newPos;
        }

        //Set the alignment based on right/left, top/bottom
        textMesh.alignment = side == SideE.rightOrBottom ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;

        //Set the height of the text box (i.e. the 2D box in which the region is drawn, regardless of its world orientation)
        SetTextHeight(textHeight);

    }

    public void SetTextHeight(float th)
    {
        RectTransform rt = textMesh.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, th);
        rt.ForceUpdateRectTransforms();
    }

    public void Destroy()
    {
        Object.Destroy(textMesh);
        InitEmpty();
    }
}
