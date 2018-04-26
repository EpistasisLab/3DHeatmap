using System.Collections;

[System.Serializable]
public class OneField : object
{
    public float r;
    public float g;
    public float b;
    public string name;
}
[System.Serializable]
public class FieldData : object
{
    public string fieldName;
    public bool isFloat;
    public int lowInt;
    public int highInt;
    public float lowFloat;
    public float highFloat;
    public float range;
    public Hashtable Fields; //Stauffer - will need type here for 'value' in C#. I think it's type OneField from above.
    public virtual void SetFloat(string name, float low, float high)
    {
        this.fieldName = name;
        this.isFloat = true;
        this.lowFloat = low;
        this.highFloat = high;
        this.range = high - low;
    }

    public virtual void SetInt(string name, int low, int high)
    {
        this.fieldName = name;
        this.isFloat = false;
        this.lowInt = low;
        this.highInt = high;
        this.range = high - low;
    }

    public FieldData()
    {
        this.Fields = new Hashtable();
    }

}