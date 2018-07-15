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

    /// <param name="preferredWidth">Optionally change the width of the message box.
    /// Do this if you know you want it wider than its prefab setting. 0 means no change.</param>
    public void ShowMessage(string msg, float preferredWdith = 0f, bool leftAlign = false)
    {
        GameObject messageObj = transform.Find("Message").gameObject;
        if (preferredWdith > 0f)
            messageObj.GetComponent<LayoutElement>().preferredWidth = preferredWdith;
        messageObj.GetComponent<Text>().text = msg;
        //Put the UI input focus onto the OK button so we can just hit enter to dismiss
        EventSystem.current.SetSelectedGameObject(transform.Find("Button").gameObject);
        //Default alignment is middle
        if(leftAlign)
            messageObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
    }

    public void OnClickOK()
    {
        Destroy(gameObject);
    }
}
