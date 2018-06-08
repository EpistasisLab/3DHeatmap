using System.Collections;
using System.Collections.Generic;

[System.Serializable]
//Stauffer - a single color within a field's optional custom color map
// Renamed from OneField
public class OneColor : object
{
    public float r;
    public float g;
    public float b;
    public string name;
}

//Stauffer - this is really a *description* of a _variable_, i.e. a dependent variable that's being visualized, and not a single database value field, i.e. not a particular value for a given record/field(column) pair.
// So, I'll rename it VariableDesc. The original use of 'Field' was probalby from the fact that each variable (height, bin, and optional int columns)
// is fully listed in a single column/field. But it's confusing terminology. 
[System.Serializable]
public class VariableDesc : object
{
    public string name;
    public bool isFloat;
    public int lowInt;
    public int highInt;
    public float lowFloat;
    public float highFloat;
    public float range;
    public Hashtable ColorMap; //Stauffer - renamed from 'Fields'. Stores value/color info for discreate data coloring. hashtable takes any types

    public virtual void SetAsFloat(string name, float low, float high)
    {
        this.name = name;
        this.isFloat = true;
        this.lowFloat = low;
        this.highFloat = high;
        this.range = high - low;
    }

    public virtual void SetAsInt(string name, int low, int high)
    {
        this.name = name;
        this.isFloat = false;
        this.lowInt = low;
        this.highInt = high;
        this.range = high - low;
    }

    public VariableDesc()
    {
        this.ColorMap = new Hashtable();
    }

}