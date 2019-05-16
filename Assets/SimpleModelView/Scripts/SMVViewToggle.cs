using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary> An SVMview for a toggle, a checkbox. Stores a bool </summary>
    public class SMVViewToggle : SMVviewBase
    {
        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<Toggle>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            //Add the change-listener from base class
            (((Toggle)UIelement).onValueChanged).AddListener(delegate { OnValueChangedListener(); });

            smvtype = SMVtypeEnum.toggle;
            dataType = typeof(bool);
        }

        public override object GetValueAsObject()
        {
            //Just return the string object, and validation method will handle the parsing
            return ((Toggle)UIelement).isOn;
        }

        protected override void SetValueDerived(object val)
        {
            ((Toggle)UIelement).isOn = (bool)val;
        }
    }
}