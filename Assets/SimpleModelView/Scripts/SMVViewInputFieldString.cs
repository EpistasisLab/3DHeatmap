using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary>
    /// An SVMview for an InputField element used for strings.
    /// </summary>
    public class SMVViewInputFieldString : SMVviewBase
    {
        public enum UpdateMode { Continuous, EndEdit };

        /// <summary>
        /// When update events should be sent - either continuously on every change to the input,
        ///  or only when done (i.e. hit return or input focus is lost).
        ///  Must be set before running.
        /// </summary>
        public UpdateMode updateMode = UpdateMode.EndEdit;

        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<InputField>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            //Add the change-listener from base class
            if (updateMode == UpdateMode.Continuous)
                (((InputField)UIelement).onValueChanged).AddListener(delegate { OnValueChangedListener(); });
            else
                (((InputField)UIelement).onEndEdit).AddListener(delegate { OnValueChangedListener(); });

            smvtype = SMVtypeEnum.inputFieldString;
            dataType = typeof(string);
        }

        public override object GetValueAsObject()
        {
            return ((InputField)UIelement).text;
        }

        protected override void SetValueDerived(object val)
        {
            //Change all input to string
            ((InputField)UIelement).text = val.ToString();
        }
    }
}