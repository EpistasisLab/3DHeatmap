using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


//
//Simple Model View(SMV) is a Unity package meant to be a very simple implementation of a Model View-type system for simpliflying UI plumbing.
//
//It creates a simple paradigm for:
//- synchronizing a state value and multiple UI elements
//- getting update events when a state value is changed either via UI element or via code.
//
//See the README in the top level of the distribution for details
//

namespace SMView
{

[Serializable]
public class OnUpdateEvent : UnityEvent<SMVmapping> { }

/// <summary>
/// This is the main class, a singleton. It provides the interface for users.
/// Add an instance of this component to your scene and it will initialize on program start in the Initialize() method.
/// </summary>
public class SMV : SMView.MonoBehaviorSingleton<SMV> {

    /// <summary> OPTIONAL - use the editor to assign a listener within your own code to handle
    ///  updates to an SMVcontrol state/value, either from logic or UI/view side of things. </summary>
    public OnUpdateEvent onUpdateEvent;

    /// <summary> Flag to output some debug info, like the control and view-mapping info after initilization  </summary>
    public bool doDebugLogging = false;

    /// <summary>
    /// Array of SMVcontrol, each of which holds a value/state and the view(s) to which it's mapped.
    /// There will always be one SMVcontrol for each SMVmapping member.
    /// </summary>
    private SMVcontrol[] controls;

	// in a MonoBehaviorSingleton object, use this for initialization instead of Awake 
	protected override void Initialize () {
        SetupForScene(true);
	}

    /// <summary>
    ///  Initialize. Go through the scene and find all SMVviewBase components
    ///  and assign them to their respective SMVcontrol objects.
    ///  Can be called as needed to reload all the controls and mappings if you're making 
    ///  runtime changes to the UI or loading a new scene with different UI objects.
    ///  Pass true to set up a new list and new controls - typically this will only
    ///  be done once per scene. Otherwise when passing false, controls and
    ///  their values are preserved, but scene is still searched to find UI changes.  
    ///  </summary>
    public void SetupForScene(bool initialize = false)
    {

        if( initialize)
            controls = new SMVcontrol[Enum.GetNames(typeof(SMVmapping)).Length];

        for(int i = 0; i < controls.Length; i++)
        {
            if (initialize)
            {
                controls[i] = new SMVcontrol();
                controls[i].Init((SMVmapping)i, onUpdateEvent);
            }
            else
                controls[i].SetupMappings();
        }
        
        if(doDebugLogging)
            DebugDump();
    }

    /// <summary>
    /// Assign a value for the mapping. 
    /// User must pass value of appropriate type for the mapping. 
    /// If value is the wrong type, an error gets logged and state and view are not changed.
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="val"></param>
    public void SetValue(SMVmapping mapping, object val)
    {
        controls[(int)mapping].SetValue(val);
    }

    /// <summary>
    /// Pass an object containing ui-specific special item.
    /// Each view will check for proper type and convert, and ignore if not proper.
    /// e.g. for Dropdown UI, this is list of items to populate the dropdown
    /// </summary>
    /// <param name="options"></param>
    public void SetSpecial(SMVmapping mapping, object obj)
    {
        controls[(int)mapping].SetSpecial(obj);
    }

    /// <summary>
    /// Get the state value for the passed mapping.
    /// </summary>
    /// <returns></returns>
    public float GetValueFloat(SMVmapping mapping)
    {
        return controls[(int)mapping].GetValueFloat();
    }
    public int GetValueInt(SMVmapping mapping)
    {
        return controls[(int)mapping].GetValueInt();
    }
    public string GetValueString(SMVmapping mapping)
    {
        return controls[(int)mapping].GetValueString();
    }
    public bool GetValueBool(SMVmapping mapping)
    {
        return controls[(int)mapping].GetValueBool();
    }

    /// <summary>
    /// Get an object of type with default value.
    /// Returns null for non-value-types
    /// </summary>
    public object GetDefault(Type type)
    {
        if (type == null)
            return null;

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public void DebugDump()
    {
        Debug.Log("====== SMV version " + SMVversion.String + " == SMVcontrols dump ======");
        foreach (SMVcontrol control in controls)
        {
            control.DebugDump();
        }
    }
}

}//namespace
