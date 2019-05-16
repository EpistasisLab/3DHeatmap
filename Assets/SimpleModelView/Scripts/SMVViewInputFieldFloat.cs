using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary>
    /// An SVMview for an InputField element used for float values.
    /// </summary>
    public class SMVViewInputFieldFloat : SMVviewBase
    {
        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<InputField>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            //Add the change-listener from base class
            //Always use the EndEdit event so we don't end up with validation errors while typing
            (((InputField)UIelement).onEndEdit).AddListener(delegate { OnValueChangedListener(); });

            smvtype = SMVtypeEnum.inputFieldFloat;
            dataType = typeof(float);
        }

        public override object GetValueAsObject()
        {
            //Just return the string object, and validation method will handle the parsing
            return ((InputField)UIelement).text;
        }

        protected override void SetValueDerived(object val)
        {
            ((InputField)UIelement).text = val.ToString();
        }
    }

}