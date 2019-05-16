using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMView
{

    /// <summary> Class functions somehwat like a controller in MVC paradigm, but for a single value.
    /// It holds the value/state for a model-view mapping, and manages
    ///  the one or more views that are assigned to it. </summary>
    public class SMVcontrol
    {

        /// <summary> The mapping that this value is assigned to </summary>
        private SMVmapping mapping;
        public SMVmapping Mapping { get { return mapping; } }

        /// <summary> The SMVviews that are associated with the mapping </summary>
        List<SMVviewBase> views;

        /// <summary> Get the number of views mapped to this control </summary>
        public int Count
        {
            get { return views.Count; }
        }

        /// <summary> The data type this link uses. i.e. the data type used for setting and returned by getting value.
        /// All views that this link contains, i.e. all view that use the same mapping, must use same data type as well,
        /// or be able to handle conversion from any type (e.g. read-only Text element).</summary>
        private System.Type dataType;
        public System.Type DataType { get { return dataType; } }

        /// <summary> The current value </summary>
        private object value;

        /// <summary>
        /// The event from the main SMV instance, for invoking an event from this class.
        /// </summary>
        private SMView.OnUpdateEvent onUpdateEvent;

        /// <summary> Flags to track if we've warned about get or set value attempts that
        /// fail because there are no views mapped. </summary>
        private bool haveWarnedForGetValueNoMapping = false;
        private bool haveWarnedForSetValueNoMapping = false;

        // ctor
        public SMVcontrol()
        {
            views = new List<SMVviewBase>();
            this.mapping = SMVmapping.undefined;
            value = null;
        }

        /// <summary> Initialize a new instance with the passed mapping. 
        /// **NOTE** This will reset value to default. </summary>
        public void Init(SMVmapping mapping, SMView.OnUpdateEvent updateEvent)
        {
            this.mapping = mapping;
            SetupMappings();
            //if(mapping != SMVmapping.undefined)
            //    SetValue(SMV.Instance.GetDefault(DataType));
            value = SMV.Instance.GetDefault(DataType);
            onUpdateEvent = updateEvent;
        }

        /// <summary>
        /// Search for SMVView* components that are assigned to the same
        /// mapping as this control, and set them up. Does not change the
        /// current state value, so can be called when UI changes but you want
        /// to preserve value.
        /// </summary>
        public void SetupMappings()
        {
            views = new List<SMVviewBase>();
            bool typeIsSet = false;
            bool foundText = false;

            SMVviewBase[] allViews = Resources.FindObjectsOfTypeAll<SMVviewBase>();
            foreach (SMVviewBase view in allViews)
            {

                if (view.mapping != mapping)
                    continue;

                if (view.mapping == SMVmapping.undefined)
                {
                    Debug.LogError("Tried to assign a view of undefined type, with instance ID " + view.GetInstanceID() + " and parent " + view.gameObject.name + ". Make sure view has mapping assigned in the editor. Skipping.");
                    continue;
                }

                //We've got a match
                view.Init(this);

                //Special handling for noneditable text elements, which are always string, even when used to show numeric values.
                //Keep track and if no non-Text elements are found, then control type will be string.
                //Ugly.
                if (view.IsUneditableText)
                {
                    foundText = true;
                }
                else
                {
                    if (!typeIsSet)
                    {
                        dataType = view.DataType;
                        typeIsSet = true;
                    }
                    //If a 2nd or later mapping is of a different type, show an error and skip it,
                    // EXCEPT for Text type (noted and skipped above) which is just for display so works with any type
                    if (view.DataType != dataType)
                    {
                        Debug.LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + ": tried adding view with type of " + view.DataType.Name + ", but doesn't match this control's type of " + dataType.Name + ", for view " + view.SMVType.ToString() + " from game object " + view.UIelement.transform.parent.name + ". Skipping.");
                        continue;
                    }
                }
                //Add this view to the list
                views.Add(view);
            }
            //special handling when there's only Text elements
            if (!typeIsSet && foundText)
                dataType = typeof(string);

            //Warning if we didn't find any views for this control
            if (Count == 0 && Mapping != SMVmapping.undefined)
            {
                Debug.LogWarning(System.Reflection.MethodBase.GetCurrentMethod().Name + ": no views found for this control with mapping " + Mapping);
                return;
            }
        }

        /// <summary>
        /// Compare the passed object value with the control's value.
        /// Return true/false for equal/not-equal
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool IsEqual(object val)
        {
            //NOTE - must be better way to handle variable typing. Should switch to generics?
            if (DataType == typeof(float))
            {
                return (float)value == (float)val;
            }
            if (DataType == typeof(int))
            {
                return (int)value == (int)val;
            }
            if (DataType == typeof(string))
            {
                //Convert val to string so we can take in any type for
                // text UI elements that are read-only.
                return (string)value == val.ToString();
            }
            if (DataType == typeof(bool))
            {
                return (bool)value == (bool)val;
            }
            Debug.LogError("IsEqual: val type '" + val.GetType().Name + "' not matched for control date type of '" + DataType.Name + "'");
            return false;
        }

        /// <summary>
        /// Set the value for this control.
        /// Gets called both from logic side and from UI/view side.
        /// Updates any mapped views, and then calls the main update handler,
        ///  BUT only if value has changed.
        /// </summary>
        /// <param name="val"></param>
        public void SetValue(object val)
        {
            if (Count == 0)
            {
                if (!haveWarnedForSetValueNoMapping)
                    Debug.LogWarning(System.Reflection.MethodBase.GetCurrentMethod().Name + ": tried setting value but no views mapped for this control with mapping " + Mapping + ". You will not receive further warnings.");
                haveWarnedForSetValueNoMapping = true;
                return;
            }

            //Is the passed value different than what's already set?
            //We want to avoid calling the UI update and update handler unnecessarily
            if (IsEqual(val))
                return;


            if (val.GetType() != this.DataType)
            {
                if (this.DataType == typeof(string))
                {
                    //For string types, like uneditable text elements, we'll take
                    // any type and just turn it into a string in the UI.
                    // But store as a string here.
                    value = val.ToString();
                }
                else
                {
                    Debug.LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + ": input with value " + val.ToString() + ", and type of " + val.GetType().Name + ", doesn't match this control's type of " + DataType.Name + ", for mapping " + mapping.ToString() + ". Skipping setting of value.");
                    return;
                }
            }
            else
            {
                //Store the actual value
                value = val;
            }

            //Update each UI element
            foreach (SMVviewBase view in views)
            {
                //Pass the orig input val (i.e. not converted to a string), so that
                // uneditable text elements can receive numeric types and do formatting during conversion
                view.SetValue(val);
            }

            //Invoke event for the optional update handler
            onUpdateEvent.Invoke(mapping);
        }

        /// <summary> Return the current value as generic object type </summary>
        public object GetValueAsObject()
        {
            return value;
        }

        private bool CheckForMapping()
        {
            if (Count == 0)
            {
                if (!haveWarnedForGetValueNoMapping)
                    Debug.LogWarning(System.Reflection.MethodBase.GetCurrentMethod().Name + ": tried getting value no views mapped for this control. You won't be warned again.");
                haveWarnedForGetValueNoMapping = true;
                return false;
            }
            return true;
        }

        public float GetValueFloat()
        {
            if (CheckForMapping())
            {
                if (ValidateDataType(typeof(float)))
                {
                    return (float)value;
                }

            }
            return (float)SMV.Instance.GetDefault(DataType);
        }

        public int GetValueInt()
        {
            if (CheckForMapping())
            {
                if (ValidateDataType(typeof(int)))
                {
                    return (int)value;
                }
            }

            return (int)SMV.Instance.GetDefault(DataType);
        }

        public string GetValueString()
        {
            if (CheckForMapping())
            {
                if (ValidateDataType(typeof(string)))
                {
                    return (string)value;
                }
            }

            return (string)SMV.Instance.GetDefault(DataType);
        }

        public bool GetValueBool()
        {
            if (CheckForMapping())
            {
                if (ValidateDataType(typeof(bool)))
                {
                    return (bool)value;
                }
            }

            return (bool)SMV.Instance.GetDefault(DataType);
        }

        private bool ValidateDataType(System.Type typeRequested)
        {
            if (typeRequested != this.DataType)
            {
                Debug.LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + ": passed type " + typeRequested.Name + " doesn't match this link's type of " + DataType.Name + ". Returning default value.");
                return false;
            }
            return true;
        }

        public void DebugDump()
        {
            Debug.Log("--------- control dump: mapping: " + this.mapping.ToString() + " dataType: " + this.DataType + " and views: ");
            if (Count == 0)
            {
                Debug.Log("No views mapped");
                return;
            }

            foreach (SMVviewBase view in views)
            {
                Debug.Log("Mapping  " + view.mapping.ToString() + " mapped to SMVtype " + view.SMVType.ToString() + ", with Instance ID " + view.GetInstanceID() + ", with behavior: " + view.UIelement.GetType() + " in gameobject: " + view.gameObject.name + "\n");
            }
        }
    }
}