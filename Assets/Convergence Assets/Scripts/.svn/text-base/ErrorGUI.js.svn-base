static var display : boolean = false;
static var messages : String[];
static var messageCount : int = 0;
static var maxMessages : int = 5;
var messageHeight : float = 24.0;
var messageWidth : float = 200.0;

static function ShowError(err : String)
{
	if(messageCount >= maxMessages)
	{
		for(var i = 0; i < maxMessages - 1; ++i)
		{
			messages[i] = messages[i + 1];
		}
		messageCount = maxMessages - 1;
	}
	messages[messageCount++] = err;
	display = true;
}

static function IsShowing()
{
	return display;
}

function Start()
{
	messages = new String[maxMessages];
}

function OnGUI()
{
	if(!display) return;
	totHeight = (messageCount * messageHeight) + messageHeight;
	GUILayout.Window(1000, Rect((Screen.width - messageWidth) / 2.0, (Screen.height - totHeight) / 2.0, messageWidth, totHeight), DoError, "Message");
}

function DoError(windowID : int)
{
	for(var i : int = 0; i < messageCount; ++i)
	{
		GUILayout.Label(messages[i], Const.bigRedLabel);
	}
	if(GUILayout.Button("Dismiss", Const.bigToggle))
	{
		display = false;
		messageCount = 0;
	}
	GUI.DragWindow();
}