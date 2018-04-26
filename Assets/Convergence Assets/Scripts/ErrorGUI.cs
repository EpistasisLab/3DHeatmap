using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class ErrorGUI : MonoBehaviour
{
    public static bool display;
    public static string[] messages;
    public static int messageCount;
    public static int maxMessages;
    public float messageHeight;
    public float messageWidth;
    public static void ShowError(string err)
    {
        if (ErrorGUI.messageCount >= ErrorGUI.maxMessages)
        {
            int i = 0;
            while (i < (ErrorGUI.maxMessages - 1))
            {
                ErrorGUI.messages[i] = ErrorGUI.messages[i + 1];
                ++i;
            }
            ErrorGUI.messageCount = ErrorGUI.maxMessages - 1;
        }
        ErrorGUI.messages[ErrorGUI.messageCount++] = err;
        ErrorGUI.display = true;
    }

    public static bool IsShowing()
    {
        return ErrorGUI.display;
    }

    public virtual void Start()
    {
        ErrorGUI.messages = new string[ErrorGUI.maxMessages];
    }

    public virtual void OnGUI()
    {
        if (!ErrorGUI.display)
        {
            return;
        }
        float totHeight = (ErrorGUI.messageCount * this.messageHeight) + this.messageHeight;
        GUILayout.Window(1000, new Rect((Screen.width - this.messageWidth) / 2f, (Screen.height - totHeight) / 2f, this.messageWidth, totHeight), this.DoError, "Message", new GUILayoutOption[] {});
    }

    public virtual void DoError(int windowID)
    {
        int i = 0;
        while (i < ErrorGUI.messageCount)
        {
            GUILayout.Label(ErrorGUI.messages[i], Const.bigRedLabel, new GUILayoutOption[] {});
            ++i;
        }
        if (GUILayout.Button("Dismiss", Const.bigToggle, new GUILayoutOption[] {}))
        {
            ErrorGUI.display = false;
            ErrorGUI.messageCount = 0;
        }
        GUI.DragWindow();
    }

    public ErrorGUI()
    {
        this.messageHeight = 24f;
        this.messageWidth = 200f;
    }

    static ErrorGUI()
    {
        ErrorGUI.maxMessages = 5;
    }

}