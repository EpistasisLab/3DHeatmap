class XRidge extends System.Object
{
	var myMeshObject : GameObject;
	var myLabel : GameObject;
	var trans : Transform;
	var myMesh : Mesh;
	var vertexColors : float[];
	var myRenderer : Renderer;
	var myBin : int;
	var myRow : int;
	
	function AddRidge(aMeshObject : GameObject, passColors : float[], amesh : Mesh, bin : int, row : int)
	{
		myMeshObject = aMeshObject;
		trans = aMeshObject.transform;
		myRenderer = aMeshObject.GetComponent.<Renderer>();
		myMesh = amesh;
		vertexColors = passColors;
		myBin = bin;
		myRow = row;
	}
	
	function AddRidge(aMeshObject : GameObject, amesh : Mesh, bin : int, row : int)
	{
		myMeshObject = aMeshObject;
		trans = aMeshObject.transform;
		myRenderer = aMeshObject.GetComponent.<Renderer>();
		myMesh = amesh;
		myBin = bin;
		myRow = row;
	}
	
	function AddLabel(alabel : GameObject)
	{
		myLabel = alabel;
	}
	
	function PositionRidge(location : Vector3, extent : Vector3, numSlots : int, whichSlot : int)
	{
		var newWidth = extent.z / numSlots;
		var posOff = newWidth * whichSlot;
		trans.position = location + Vector3(0, 0, posOff);
		trans.localScale = extent;
		trans.localScale.z /= numSlots;
	}
	
	function NewHeight(nh : float)
	{
		trans.localScale.y = nh;
	}
	
	function Show(doShow : boolean)
	{
		myRenderer.enabled = doShow;
	}
	
	function AdvanceUV(newUV : Vector2, adv : Vector2)
	{
		var uv : Vector2[] = myMesh.uv;
		var i : int;
		for(i = 0; i < uv.length; ++i)
		{
			var myuv : Vector2 = newUV + (adv * i);
			uv[i] += myuv;
		}
		myMesh.uv = uv;
	}
	
	function ReleaseRidge()
	{
		Debug.Log("Destroying in ReleaseRidge");
//				gameObject.Destroy(myMeshObject.gameObject);
	}
}