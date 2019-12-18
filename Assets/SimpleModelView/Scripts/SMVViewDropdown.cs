using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SMView
{

    /// <summary>
    /// An SVMview for a dropdown element, stores and returns current selected dropdown 'value', i.e. the selected item index number, as int.
    /// </summary>
    public class SMVViewDropdown : SMVviewBase
    {
        protected override void InitDerived()
        {
            //Find the UI element within this components game object
            UIelement = transform.GetComponent<Dropdown>();
            if (UIelement == null)
                Debug.LogError("uiElement == null");

            //Add the change-listener from base class
            (((Dropdown)UIelement).onValueChanged).AddListener(delegate { OnValueChangedListener(); });

            smvtype = SMVtypeEnum.dropdown;
            dataType = typeof(int);
        }

        public override object GetValueAsObject()
        {
            //Just return as object, and validation method will handle the parsing
            return ((Dropdown)UIelement).value;
        }

        protected override void SetValueDerived(object val)
        {
            ((Dropdown)UIelement).value = (int)val;
            ((Dropdown)UIelement).RefreshShownValue();
        }

        /// <summary> Set the dropdown's options list using the SetSpecial method </summary>
        /// <param name="options">Pass eith List<Drodown.OptionData> or List<string></param>
        public override void SetSpecial(object options)
        {
            List<Dropdown.OptionData> listData = new List<Dropdown.OptionData>();
            List<string> listString = new List<string>();
            if(options.GetType() == listData.GetType())
            {
                ((Dropdown)UIelement).options = ((List<Dropdown.OptionData>)options);
            }
            else
            if (options.GetType() == listString.GetType())
            {
                ((Dropdown)UIelement).ClearOptions();
                ((Dropdown)UIelement).AddOptions((List<string>)options);
            }
            else
                Debug.LogError("SMVViewDropdown.SetOptions: type does not match: " + options.GetType().ToString());
        }
    }
}