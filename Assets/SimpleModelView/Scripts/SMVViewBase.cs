using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SMView
{

    /// <summary>
    /// Abstract base class for SMVView* components. Provides common interface and method implementations.
    /// Attach as component to UI elements that are to be mapped using SMV.
    /// </summary>
    public abstract class SMVviewBase : MonoBehaviour
    {
        /// <summary> Specific type of SMVView that this instance is </summary>
        public enum SMVtypeEnum { undefined, toggle, slider, text, inputFieldString, inputFieldFloat, inputFieldInt, textMeshProWorldSpace, textMeshProUGUI, dropdown };

        /// <summary> Internal tracking of type of view/UI element. </summary>
        protected SMVtypeEnum smvtype;
        public SMVtypeEnum SMVType { get { return smvtype; } }

        /// <summary> The data type this view uses. i.e. the data type used for setting and returned by getting value</summary>
        protected System.Type dataType;
        public System.Type DataType { get { return dataType; } }

        /// <summary> True for views that are textual in nature (InputField), editable, and are holding numeric values/types. </summary>
        public bool IsEditableTextualNumeric { get { return SMVType == SMVtypeEnum.inputFieldFloat || SMVType == SMVtypeEnum.inputFieldInt; } }

        /// <summary> True for text elements that are for display only. They can accept any type that has ToString() method. </summary>
        public bool IsUneditableText { get { return SMVType == SMVtypeEnum.text || SMVType == SMVtypeEnum.textMeshProWorldSpace || SMVType == SMVtypeEnum.textMeshProUGUI; } }

        /// <summary> The mapping for this view </summary>
        public SMVmapping mapping;

        /// <summary> The UI element we're controlling/reading for this view element. 
        /// Gets assigned automatically during init. </summary>
        private UIBehaviour uiElement;
        public UIBehaviour UIelement { protected set { uiElement = value; } get { return uiElement; } }

        /// <summary> The SMVcontrol that this view belongs too.
        /// Gets assigned automatically during init. </summary>
        protected SMVcontrol parent = null;

        /// <summary> Flag to track if this instance has been initialized </summary>
        bool hasBeenInited;

        void Awake()
        {
            hasBeenInited = false;
        }

        // Use this for initialization
        void Start()
        {

        }

        /// <summary> Init this view </summary>
        /// <param name="parent">Provide ref to the SMVcontrol that's managing this view </param>
        public void Init(SMVcontrol parent)
        {
            //Always set this in case it can change if controls are reloaded for a scene
            this.parent = parent;
            //Only init once!
            if (hasBeenInited)
                return;
            //Init the derived class
            InitDerived();
            hasBeenInited = true;
        }

        /// <summary> Set up everything that's specific to this SMVview type.
        /// This is called during init of the main SMV object. </summary>
        protected abstract void InitDerived();

        //testing
        void PrintVal(object o)
        {
            Debug.Log("PrintVal type: " + o.GetType().Name + " value: " + o);
        }

        /// <summary>
        /// Single setvalue method takes an object and does type-checking.
        /// If the passed value is the same as the current value of the view element,
        ///  nothing happens - this is done to avoid possible re-trigger of change-events (if that even happens with setting value directly, I don't know).
        /// </summary>
        /// <param name="val">param of type that matches the view you're mapped to</param>
        public void SetValue(object val)
        {
            if (!ValidateAndParse(ref val))
                return;

            //If it's a new value, then update the view. We do this check
            // to avoid updating when not needed, in case the UI event triggers some event
            // in response that leads to a loop (really should know from the logic if this is possible, but leave it for now to be safe).
            if (val != GetValueAsObject())
                SetValueDerived(val);
        }

        /// <summary> Set value for the derived class. It will know how to handle the particulars for the UI element type it manages. </summary>
        protected abstract void SetValueDerived(object val);

        /// <summary>
        /// Pass an object containing ui-specific special item.
        /// Overridden method will check for proper type and convert, and ignore if not proper.
        /// e.g. for Dropdown UI, this is list of items to populate the dropdown
        /// </summary>
        public virtual void SetSpecial(object obj)
        {
            //Be default, do nothing.
        }

        /// <summary>
        /// Check the passed value to see that its data type matches that of the view's data type.
        /// For textual-class views, if we get a string for a non-string textual-class view, attempt to parse it into the appropriate
        /// type. On success, change val to the parsed value.
        /// </summary>
        /// <param name="inputVal">The object to validate for type. May hold new object on return for textual-class views</param>
        /// <returns>True on success. False if non-matching types or string could not be parsed to appropriate type for textual-class views.</returns>
        private bool ValidateAndParse(ref object inputVal)
        {
            //If it's a display-only Text element, no need to validate since it's display-only, i.e. non-editable.
            //Any input will just be turned to string
            if (IsUneditableText)
                return true;

            //If it's an editable text element that holds a numeric value, we have to validate the entry
            // is a valid number.
            if (IsEditableTextualNumeric)
            {
                //Textual views (InputField types) can be set up to have numeric
                // data types. Make sure the type matches here, and if not and it's 
                // a string, try to parse it into proper type.
                if (inputVal.GetType() == this.DataType)
                    return true;

                //If we're passed a string, see if it can be parsed into the right type, and
                // if so then return the string parsed into new type
                if (inputVal.GetType() == typeof(string))
                {
                    if (DataType == typeof(float))
                    {
                        float newVal;
                        if (float.TryParse(inputVal.ToString(), out newVal))
                        {
                            inputVal = newVal;
                            return true;
                        }
                    }
                    else
                    if (DataType == typeof(int))
                    {
                        int newVal;
                        if (int.TryParse(inputVal.ToString(), out newVal))
                        {
                            inputVal = newVal;
                            return true;
                        }
                    }
                    //else...
                    //Parsing/conversion failed
                    //Fall through to below and report the error
                }
            }

            if (inputVal.GetType() != this.DataType)
            {
                Debug.LogWarning(System.Reflection.MethodBase.GetCurrentMethod().Name + ": input with value " + inputVal.ToString() + ", and type of " + inputVal.GetType().Name + ", doesn't match or can't be converted to the view's type of " + DataType.Name + " for view " + SMVType.ToString() + " in game object " + UIelement.gameObject.name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieve the value from UI element as a generic object. We use object type to have a single method for this.
        /// Derived classes must implement.
        /// </summary>
        public abstract object GetValueAsObject();

        /// <summary> Listener for all derived classes when value changes via UI.
        /// It validates the value and updates the parent SMVcontrol if validation succeeds </summary>
        public void OnValueChangedListener()
        {
            //Debug.Log("+ + + SMVViewBase OnValueChangedListener called within view type " + SMVType.ToString() + " and mapping " + mapping.ToString() + " - Current control value before change is " + parent.GetValueAsObject().ToString());
            object val = GetValueAsObject();
            if (ValidateAndParse(ref val))
            {
                //Value is valid, and if it's a string it has been parsed into numeric type if appropriate
                //Update the parent and it will update any other views with same mapping
                parent.SetValue(val);
            }
            else
            {
                //It failed, likely because it's a string type and didn't parse into the proper
                // numeric type, so revert the state value in SMVcontrol parent
                SetValue(parent.GetValueAsObject());
            }
        }

    }

 }