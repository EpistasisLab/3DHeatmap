using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary>
    /// An SVMview for a Text element. It's display-only, can't be edited, so simpler than others.
    /// It can accept any type that has ToString() method, and has decimal formatting option for floats.
    /// </summary>
    public class SMVViewText : SMVviewBase
    {

        /// <summary> Number of decimal places to show when showing a float value </summary>
        public int decimalPlaces = 2;

        /// <summary> A string prefix added to begin of whatever string is assigned to this </summary>
        public string prefix = "";

        /// <summary> A string postfix added to end of whatever string is assigned to this </summary>
        public string postfix = "";

        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<Text>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            smvtype = SMVtypeEnum.text;
            dataType = typeof(string);
        }

        public override object GetValueAsObject()
        {
            return ((Text)UIelement).text;
        }

        protected override void SetValueDerived(object val)
        {
            string txt;
            if (val.GetType() == typeof(float))
                txt = ((float)val).ToString("F" + decimalPlaces.ToString());
            else
                txt = val.ToString();
            ((Text)UIelement).text = prefix + txt + postfix;
        }
    }
}