using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Very simple message dialog class. Instantiate it from prefab each time you want to show a message.
/// OK button destorys itself.
/// Size automatically to fit text.
/// Call ShowMessage() with a string and that's it.
/// </summary>
public class MessageDialog : MonoBehaviour {

    int updateCount = 0;

    /// <param name="preferredWidth">Optionally change the width of the message box.
    /// Do this if you know you want it wider than its prefab setting. 0 means no change.</param>
    public void ShowMessage(string msg, float preferredWdith = 0f, bool leftAlign = false)
    {
        GameObject messageObj = transform.Find("Scroll View/Viewport/Content/Message").gameObject; //transform.Find("Message").gameObject;
        if (preferredWdith > 0f)
            messageObj.GetComponent<LayoutElement>().preferredWidth = preferredWdith;
        messageObj.GetComponent<Text>().text = msg;

        //NOTE - tried to force the UI to update its state so I can get size of message element, but neither
        // of these calls do anything. See hack in Update().
        //transform.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        //messageObj.transform.GetComponent<RectTransform>().ForceUpdateRectTransforms();

        //Put the UI input focus onto the OK button so we can just hit enter to dismiss
        EventSystem.current.SetSelectedGameObject(transform.Find("Button").gameObject);
        //Default alignment is middle
        if(leftAlign)
            messageObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //Does NOT work to simply call this here.
        //Have to do the hack in Update().
        //Interestingly in ToolTipHandler, it does work to simply
        // do resize from this equivalent part of the code/process.
        //
        //** NOTE maybe try this:
        //  LayoutRebuilder.ForceRebuildLayoutImmediate
        //
        //ResizeBasedOnText();
    }

    /// <summary>
    /// Resize the dialog (by changing size of ScrollView element) in response to the text element
    /// size.
    /// Advantage of doing it this way is that we can set a maximum height (or width too if we 
    /// don't want to stick with preferred width of text element), and otherwise unity UI doesn't
    /// have a way to set max height.
    /// This is a hack of a workaround. The text element should drive the size of the whole GUI, but:
    /// 1) If I place a vert layout group on the ScrollView, it screws everything up. So we let the
    /// ScrollView's content element follow the text element size, then manually set the ScrollView size based on
    /// the reported text element size after it's done its own content size fitting.
    /// 2) The text elements size isn't updated until the frame after the UI element has been created,
    /// so have to resort to the hack below in which we wait til 2nd frame in Update() and then do the resize. Yikes.
    /// </summary>
    private void ResizeBasedOnText()
    {
        Rect textRect = transform.Find("Scroll View/Viewport/Content/Message").GetComponent<RectTransform>().rect;
        //Debug.Log("Resize: message obj rect: " + textRect.ToString());

        //Resize if we're getting close to filling the screen
        float height = Mathf.Min(Screen.height * 0.85f, textRect.height) + 12;
        float width = textRect.width + 30; //A min width is set in inspector, so we should be fine in that regard. Add a little fudge otherwise sometimes get a horiz. scrollbar when we shouldn't

        RectTransform rectTransform = transform.Find("Scroll View").GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

    }

    public void OnClickOK()
    {
        //Tell main UI handler we're done. Awkward. Should do via an event?
        GameObject.Find("UIManager").GetComponent<UIManager>().OnMessageDialogDone();

        Destroy(gameObject);
    }

    private void Update()
    {
        //The first time we come in here it seems to be in same frame as when we
        // instantiate the dialog above, so we can't get the text element dimensions.
        //So the second time in here, we act.
        if (updateCount == 1)
        {
            ResizeBasedOnText();
        }

        updateCount ++;
    }
}
