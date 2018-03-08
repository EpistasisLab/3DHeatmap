class OneField extends System.Object
{
	var r : float;
	var g : float;
	var b : float;
	var name : String;
}

class FieldData extends System.Object
{
	var fieldName : String;
	var isFloat : boolean;
	var lowInt : int;
	var highInt : int;
	var lowFloat : float;
	var highFloat : float;
	var range : float;
	var Fields : Hashtable = new Hashtable();
	
	function SetFloat(name : String, low : float, high : float)
	{
		fieldName = name;
		isFloat = true;
		lowFloat = low;
		highFloat = high;
		range = high - low;
	}
	
	function SetInt(name : String, low : int, high : int)
	{
		fieldName = name;
		isFloat = false;
		lowInt = low;
		highInt = high;
		range = high - low;
	}
}