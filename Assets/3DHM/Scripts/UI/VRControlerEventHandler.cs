using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary> 
/// Handle events from VRTK_ControllerEvents component and provide handedness info.
/// Needs a ref to a VRTK_ControllerEvents component.
/// Modified from VRTKExample_ControllerEventsDelegateListeners.cs
/// </summary>
public class VRControlerEventHandler : MonoBehaviour
{
    /// <summary> The VRTK component with event generators (what's the proper term?) we need to listen to </summary>
    public VRTK_ControllerEvents VRTKControllerEvents;

    /// <summary> Set to have all button events logged for debugging/testing </summary>
    public bool logAllEvents;

    /// <summary> Set this in the inspector based on which ControllerEvents gameobject this is attached to </summary>
    private VRManager.Hand hand = VRManager.Hand.undefined;

    /// <summary> Ref to the actual game object for the controller. Use this for getting pose </summary>
    private GameObject controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = null;
    }

    private void OnEnable()
    {
        //Attach listeners
        if (VRTKControllerEvents == null)
        {
            Debug.LogError("VRTK_ControllerEvents componenent not set. No controller events will be handled.");
            return;
        }

        Debug.Log("VRControlerEventHandler: attaching listeners");
        //Setup controller event listeners
        VRTKControllerEvents.TriggerPressed += DoTriggerPressed;
        VRTKControllerEvents.TriggerReleased += DoTriggerReleased;
        VRTKControllerEvents.TriggerTouchStart += DoTriggerTouchStart;
        VRTKControllerEvents.TriggerTouchEnd += DoTriggerTouchEnd;
        VRTKControllerEvents.TriggerHairlineStart += DoTriggerHairlineStart;
        VRTKControllerEvents.TriggerHairlineEnd += DoTriggerHairlineEnd;
        VRTKControllerEvents.TriggerClicked += DoTriggerClicked;
        VRTKControllerEvents.TriggerUnclicked += DoTriggerUnclicked;
        VRTKControllerEvents.TriggerAxisChanged += DoTriggerAxisChanged;
        VRTKControllerEvents.TriggerSenseAxisChanged += DoTriggerSenseAxisChanged;

        VRTKControllerEvents.GripPressed += DoGripPressed;
        VRTKControllerEvents.GripReleased += DoGripReleased;
        VRTKControllerEvents.GripTouchStart += DoGripTouchStart;
        VRTKControllerEvents.GripTouchEnd += DoGripTouchEnd;
        VRTKControllerEvents.GripHairlineStart += DoGripHairlineStart;
        VRTKControllerEvents.GripHairlineEnd += DoGripHairlineEnd;
        VRTKControllerEvents.GripClicked += DoGripClicked;
        VRTKControllerEvents.GripUnclicked += DoGripUnclicked;
        VRTKControllerEvents.GripAxisChanged += DoGripAxisChanged;

        VRTKControllerEvents.TouchpadPressed += DoTouchpadPressed;
        VRTKControllerEvents.TouchpadReleased += DoTouchpadReleased;
        VRTKControllerEvents.TouchpadTouchStart += DoTouchpadTouchStart;
        VRTKControllerEvents.TouchpadTouchEnd += DoTouchpadTouchEnd;
        VRTKControllerEvents.TouchpadAxisChanged += DoTouchpadAxisChanged;
        VRTKControllerEvents.TouchpadTwoPressed += DoTouchpadTwoPressed;
        VRTKControllerEvents.TouchpadTwoReleased += DoTouchpadTwoReleased;
        VRTKControllerEvents.TouchpadTwoTouchStart += DoTouchpadTwoTouchStart;
        VRTKControllerEvents.TouchpadTwoTouchEnd += DoTouchpadTwoTouchEnd;
        VRTKControllerEvents.TouchpadTwoAxisChanged += DoTouchpadTwoAxisChanged;
        VRTKControllerEvents.TouchpadSenseAxisChanged += DoTouchpadSenseAxisChanged;

        VRTKControllerEvents.ButtonOnePressed += DoButtonOnePressed;
        VRTKControllerEvents.ButtonOneReleased += DoButtonOneReleased;
        VRTKControllerEvents.ButtonOneTouchStart += DoButtonOneTouchStart;
        VRTKControllerEvents.ButtonOneTouchEnd += DoButtonOneTouchEnd;

        VRTKControllerEvents.ButtonTwoPressed += DoButtonTwoPressed;
        VRTKControllerEvents.ButtonTwoReleased += DoButtonTwoReleased;
        VRTKControllerEvents.ButtonTwoTouchStart += DoButtonTwoTouchStart;
        VRTKControllerEvents.ButtonTwoTouchEnd += DoButtonTwoTouchEnd;

        VRTKControllerEvents.StartMenuPressed += DoStartMenuPressed;
        VRTKControllerEvents.StartMenuReleased += DoStartMenuReleased;

        VRTKControllerEvents.ControllerEnabled += DoControllerEnabled;
        VRTKControllerEvents.ControllerDisabled += DoControllerDisabled;
        VRTKControllerEvents.ControllerIndexChanged += DoControllerIndexChanged;

        VRTKControllerEvents.MiddleFingerSenseAxisChanged += DoMiddleFingerSenseAxisChanged;
        VRTKControllerEvents.RingFingerSenseAxisChanged += DoRingFingerSenseAxisChanged;
        VRTKControllerEvents.PinkyFingerSenseAxisChanged += DoPinkyFingerSenseAxisChanged;
    }

    private void OnDisable()
    {
        if (VRTKControllerEvents == null)
            return;

        //Setup controller event listeners
        VRTKControllerEvents.TriggerPressed -= DoTriggerPressed;
        VRTKControllerEvents.TriggerReleased -= DoTriggerReleased;
        VRTKControllerEvents.TriggerTouchStart -= DoTriggerTouchStart;
        VRTKControllerEvents.TriggerTouchEnd -= DoTriggerTouchEnd;
        VRTKControllerEvents.TriggerHairlineStart -= DoTriggerHairlineStart;
        VRTKControllerEvents.TriggerHairlineEnd -= DoTriggerHairlineEnd;
        VRTKControllerEvents.TriggerClicked -= DoTriggerClicked;
        VRTKControllerEvents.TriggerUnclicked -= DoTriggerUnclicked;
        VRTKControllerEvents.TriggerAxisChanged -= DoTriggerAxisChanged;
        VRTKControllerEvents.TriggerSenseAxisChanged -= DoTriggerSenseAxisChanged;

        VRTKControllerEvents.GripPressed -= DoGripPressed;
        VRTKControllerEvents.GripReleased -= DoGripReleased;
        VRTKControllerEvents.GripTouchStart -= DoGripTouchStart;
        VRTKControllerEvents.GripTouchEnd -= DoGripTouchEnd;
        VRTKControllerEvents.GripHairlineStart -= DoGripHairlineStart;
        VRTKControllerEvents.GripHairlineEnd -= DoGripHairlineEnd;
        VRTKControllerEvents.GripClicked -= DoGripClicked;
        VRTKControllerEvents.GripUnclicked -= DoGripUnclicked;
        VRTKControllerEvents.GripAxisChanged -= DoGripAxisChanged;

        VRTKControllerEvents.TouchpadPressed -= DoTouchpadPressed;
        VRTKControllerEvents.TouchpadReleased -= DoTouchpadReleased;
        VRTKControllerEvents.TouchpadTouchStart -= DoTouchpadTouchStart;
        VRTKControllerEvents.TouchpadTouchEnd -= DoTouchpadTouchEnd;
        VRTKControllerEvents.TouchpadAxisChanged -= DoTouchpadAxisChanged;
        VRTKControllerEvents.TouchpadTwoPressed -= DoTouchpadTwoPressed;
        VRTKControllerEvents.TouchpadTwoReleased -= DoTouchpadTwoReleased;
        VRTKControllerEvents.TouchpadTwoTouchStart -= DoTouchpadTwoTouchStart;
        VRTKControllerEvents.TouchpadTwoTouchEnd -= DoTouchpadTwoTouchEnd;
        VRTKControllerEvents.TouchpadTwoAxisChanged -= DoTouchpadTwoAxisChanged;
        VRTKControllerEvents.TouchpadSenseAxisChanged -= DoTouchpadSenseAxisChanged;

        VRTKControllerEvents.ButtonOnePressed -= DoButtonOnePressed;
        VRTKControllerEvents.ButtonOneReleased -= DoButtonOneReleased;
        VRTKControllerEvents.ButtonOneTouchStart -= DoButtonOneTouchStart;
        VRTKControllerEvents.ButtonOneTouchEnd -= DoButtonOneTouchEnd;

        VRTKControllerEvents.ButtonTwoPressed -= DoButtonTwoPressed;
        VRTKControllerEvents.ButtonTwoReleased -= DoButtonTwoReleased;
        VRTKControllerEvents.ButtonTwoTouchStart -= DoButtonTwoTouchStart;
        VRTKControllerEvents.ButtonTwoTouchEnd -= DoButtonTwoTouchEnd;

        VRTKControllerEvents.StartMenuPressed -= DoStartMenuPressed;
        VRTKControllerEvents.StartMenuReleased -= DoStartMenuReleased;

        VRTKControllerEvents.ControllerEnabled -= DoControllerEnabled;
        VRTKControllerEvents.ControllerDisabled -= DoControllerDisabled;
        VRTKControllerEvents.ControllerIndexChanged -= DoControllerIndexChanged;

        VRTKControllerEvents.MiddleFingerSenseAxisChanged -= DoMiddleFingerSenseAxisChanged;
        VRTKControllerEvents.RingFingerSenseAxisChanged -= DoRingFingerSenseAxisChanged;
        VRTKControllerEvents.PinkyFingerSenseAxisChanged -= DoPinkyFingerSenseAxisChanged;
    }

    // Update is called once per frame
    void Update()
    {
        //Update transform
        if(controller != null)
            VRManager.I.OnControllerTransformChange(hand, controller.transform);
    }
    
    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        string debugString = "Hand " + hand.ToString() + ", controller on index '" + index + "' " + button + " has been " + action
                             + " with a pressure of " + e.buttonPressure + " / Primary Touchpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)" + " / Secondary Touchpad axis at: " + e.touchpadTwoAxis + " (" + e.touchpadTwoAngle + " degrees)";
        //VRTK_Logger.Info(debugString);
        Debug.Log(debugString);
    }

    //// Handlers
    //// These handle all VRTK-defined controller events

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "pressed", e);
        }
        VRManager.I.OnTriggerPressRelease(hand, true);
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "released", e);
        }
        VRManager.I.OnTriggerPressRelease(hand, false);
    }

    private void DoTriggerTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "touched", e);
        }
    }

    private void DoTriggerTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "untouched", e);
        }
    }

    private void DoTriggerHairlineStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "hairline start", e);
        }
    }

    private void DoTriggerHairlineEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "hairline end", e);
        }
    }

    private void DoTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "clicked", e);
        }
    }

    private void DoTriggerUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "unclicked", e);
        }
    }

    private void DoTriggerAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "axis changed", e);
        }
    }

    private void DoTriggerSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "sense axis changed", e);
        }
    }

    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "pressed", e);
        }
        VRManager.I.OnGripPressRelease(hand, true);
    }

    private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "released", e);
        }
        VRManager.I.OnGripPressRelease(hand, false);
    }

    private void DoGripTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "touched", e);
        }
    }

    private void DoGripTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "untouched", e);
        }
    }

    private void DoGripHairlineStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "hairline start", e);
        }
    }

    private void DoGripHairlineEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "hairline end", e);
        }
    }

    private void DoGripClicked(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "clicked", e);
        }
    }

    private void DoGripUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "unclicked", e);
        }
    }

    private void DoGripAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "axis changed", e);
        }
    }

    private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "pressed down", e);
        }
    }

    private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "released", e);
        }
    }

    private void DoTouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "touched", e);
        }
    }

    private void DoTouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "untouched", e);
        }
    }

    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "axis changed", e);
        }
    }

    private void DoTouchpadTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "pressed down", e);
        }
    }

    private void DoTouchpadTwoReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "released", e);
        }
    }

    private void DoTouchpadTwoTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "touched", e);
        }
    }

    private void DoTouchpadTwoTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "untouched", e);
        }
    }

    private void DoTouchpadTwoAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "axis changed", e);
        }
    }

    private void DoTouchpadSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "sense axis changed", e);
        }
    }

    private void DoButtonOnePressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "pressed down", e);
        }
    }

    private void DoButtonOneReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "released", e);
        }
    }

    private void DoButtonOneTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "touched", e);
        }
    }

    private void DoButtonOneTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "untouched", e);
        }
    }

    private void DoButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "pressed down", e);
        }
    }

    private void DoButtonTwoReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "released", e);
        }
    }

    private void DoButtonTwoTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "touched", e);
        }
    }

    private void DoButtonTwoTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "untouched", e);
        }
    }

    private void DoStartMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "pressed down", e);
        }
    }

    private void DoStartMenuReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "released", e);
        }
    }

    private void DoControllerEnabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "CONTROLLER STATE", "ENABLED", e);
        controller = e.controllerReference.actual;
        hand = e.controllerReference.hand == SDK_BaseController.ControllerHand.Left ? VRManager.Hand.left : e.controllerReference.hand == SDK_BaseController.ControllerHand.Right ? VRManager.Hand.right : VRManager.Hand.undefined;
    }

    private void DoControllerDisabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "CONTROLLER STATE", "DISABLED", e);
        controller = null;
        hand = VRManager.Hand.undefined;
    }

    private void DoControllerIndexChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "CONTROLLER STATE", "INDEX CHANGED", e);
    }

    private void DoMiddleFingerSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "MIDDLE FINGER", "sense axis changed", e);
        }
    }

    private void DoRingFingerSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "RING FINGER", "sense axis changed", e);
        }
    }

    private void DoPinkyFingerSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (logAllEvents)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "PINKY FINGER", "sense axis changed", e);
        }
    }
}
