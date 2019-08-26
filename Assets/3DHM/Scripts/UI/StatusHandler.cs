using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the status window.
/// Allows multiple messages to come in, and displays
/// the most recent one until it's told the message is
/// no longer needed. Then will look for the next most recent
/// that's still active and display that.
/// </summary>
public class StatusHandler : MonoBehaviour {

    private Text statusText;
    private int nextMessageID = 0;
    //Keep track of what messages are active.
    private Dictionary<int, string> messageHistory;

    // Use this for initialization
    void Awake()
    {
        statusText = transform.GetComponentInChildren<Text>();
        if (statusText == null)
            Debug.LogError("statusText == null");
        messageHistory = new Dictionary<int, string>();
        //Hide to start
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Post a new message to the status window. Will overwrite a current message
    /// if there is one. 
    /// Calling method should store the returned messageID and pass it to StatusComplete when you want the
    /// message gone. 
    /// When a message is completed by StatusComplete, the class will look for the most
    /// recent still-active message, if any, that was hidden by the recently completed
    /// message, and show if found. 
    /// Thus, it allows for nested messages.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="txfToShowNear"></param>
    /// <returns></returns>

    public int StatusShow(string message, Transform txfToShowNear = null)
    {
        gameObject.SetActive(true);

        //Get the corners of the object we want the windows next to
        //May never use this - copied from tooltip code
        if(txfToShowNear != null)
        {
            Vector3[] corners = new Vector3[4];
            txfToShowNear.GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 position = corners[2]; //0th is lower-left, going round clockwise
            position.y = (position.y + corners[3].y) / 2f;

            position.x += transform.GetComponent<RectTransform>().rect.width / 2; //tooltip pivot is in center
            transform.position = position;
        }

        statusText.text = message;

        //Have to manually resize for some reason. Seems like an issue with elements that set their own
        // size, like this text element, and then higher-up elements that react to that size.
        //Interestingly, I can call this right here in same frame as changing the text, and it works,
        // whereas with MessageDialog I have to wait til next frame - maybe cuz I'm instantiating 
        // MessageDialog dynamically?
        ResizeBasedOnText();

        int id = nextMessageID;
        messageHistory.Add(id, message);
        nextMessageID++;
        return id;
    }

    public void StatusComplete(int messageID)
    {
        //Debug.Log("tooltiphide");
        if (messageHistory.ContainsKey(messageID))
        {
            messageHistory.Remove(messageID);
            //Get the most recent remaining message and display it
            if (messageHistory.Count > 0)
            {
                //Order of key/values in dictionary is not guaranteed
                // to be same as order in which they were added.
                //So, search for key with max value, and that will be
                // the most recently added.
                int max = 0;
                foreach( int k in messageHistory.Keys)
                    if (k > max)
                        max = k;
                string prevMsg = "retrieval failed";
                if( messageHistory.TryGetValue(max, out prevMsg) == false)
                {
                    Debug.LogError("Dictionary retrieval failed");
                }
                statusText.text = prevMsg;
                ResizeBasedOnText();
            }
        }
        else
            Debug.LogWarning("messageID not found in dictionary: " + messageID);

        if(messageHistory.Count == 0)
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Manually resize the panel cuz it's not happening automatically
    /// when I change the text in StatusShow().
    /// </summary>
    private void ResizeBasedOnText()
    {
        Rect textRect = statusText.GetComponent<RectTransform>().rect;
        //Debug.Log("Resize: message obj rect: " + textRect.ToString());

        float height = textRect.height + 15f;
        float width = textRect.width + 30;

        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
