using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary>
    /// An SVMview for a Slider element
    /// </summary>
    public class SMVViewSlider : SMVviewBase
    {

        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<Slider>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            //Add the change-listener from base class
            (((Slider)UIelement).onValueChanged).AddListener(delegate { OnValueChangedListener(); });

            smvtype = SMVtypeEnum.slider;
            dataType = typeof(float);
        }

        public override object GetValueAsObject()
        {
            return ((Slider)UIelement).value;
        }

        protected override void SetValueDerived(object val)
        {
            ((Slider)UIelement).value = (float)val;
        }
    }
}