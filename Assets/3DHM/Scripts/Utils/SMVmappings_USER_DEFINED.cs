using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMView
{

    /// <summary>
    /// This is a part of SMV (Simple Model View) utility.
    /// List of mappings that determine which UI items are mapped to which model fields.
    /// Add a new one when you have a new model/control field that you want to map to a view.
    /// 
    /// *** NOTE - if you change the order of these in any way, make sure you update selection 
    /// in the editor for any SMVView components you've already assigned in your project.
    /// 
    /// *** NOTE - you may want to pull this file out of the SimpleModelView package folder, so that if you 
    /// install a new version of SimpleModelView, it won't overwrite your changes.
    /// 
    /// </summary>
    public enum SMVmapping { undefined/*always include this*/, GraphHeightFrac, VRdesktopViewMode };


}