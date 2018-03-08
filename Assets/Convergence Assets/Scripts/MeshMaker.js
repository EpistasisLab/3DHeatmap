class MeshMaker extends System.Object
{
	var currColor : Color;
	var numVerts : int = 0;
	var startVert : int = 0;
	var vert : Vector3[] = new Vector3[64000];
	var uv : Vector2[] = new Vector2[64000];
	var tri : int[] = new int[64000];
	var ok : boolean = true;
	var colors : Color[] = new Color[64000];
	var numTris : int = 0;
	var bumpStart : boolean = true;
	
	function Reset()
	{
		numVerts = 0;
		startVert = 0;
		numTris = 0;
		bumpStart = true;
		ok = true;
	}
	
	function SetColor(c : Color)
	{
		currColor = c;
	}
	
	function IsOK() : boolean
	{
		return ok;
	}
	
	function Verts(x : float, y : float, z: float, u : float, v : float)
	{
		if(bumpStart)
		{
			startVert = numVerts;
			bumpStart = false;
		}
		vert[numVerts] = Vector3(x, y, z);
		uv[numVerts] = Vector2(u, v);
		colors[numVerts] = currColor;
		if(numVerts >= 64000) ok = false;
		else ++numVerts;
	}
	
	function Tris(p1 : int, p2 : int, p3 : int)
	{
		bumpStart = true;
		if(numTris >= 63997)
		{
			ok = false;
			return;
		}
		tri[numTris++] = p1 + startVert;
		tri[numTris++] = p2 + startVert;
		tri[numTris++] = p3 + startVert;
	}
	
	function Tris(p1 : int, p2 : int, p3 : int, p4 : int, p5 : int, p6 : int)
	{
		bumpStart = true;
		if(numTris >= 63994)
		{
			ok = false;
			return;
		}
		tri[numTris++] = p1 + startVert;
		tri[numTris++] = p2 + startVert;
		tri[numTris++] = p3 + startVert;
		tri[numTris++] = p4 + startVert;
		tri[numTris++] = p5 + startVert;
		tri[numTris++] = p6 + startVert;
	}
	
	function Tris(p1 : int, p2 : int, p3 : int, p4 : int, p5 : int, p6 : int, p7 : int, p8 : int, p9 : int)
	{
		bumpStart = true;
		if(numTris >= 63991)
		{
			ok = false;
			return;
		}
		tri[numTris++] = p1 + startVert;
		tri[numTris++] = p2 + startVert;
		tri[numTris++] = p3 + startVert;
		tri[numTris++] = p4 + startVert;
		tri[numTris++] = p5 + startVert;
		tri[numTris++] = p6 + startVert;
		tri[numTris++] = p7 + startVert;
		tri[numTris++] = p8 + startVert;
		tri[numTris++] = p9 + startVert;
	}
	
	function Tris(p1 : int, p2 : int, p3 : int, p4 : int, p5 : int, p6 : int, p7 : int, p8 : int, p9 : int, p10 : int, p11 : int, p12 : int)
	{
		bumpStart = true;
		if(numTris >= 63991)
		{
			ok = false;
			return;
		}
		tri[numTris++] = p1 + startVert;
		tri[numTris++] = p2 + startVert;
		tri[numTris++] = p3 + startVert;
		tri[numTris++] = p4 + startVert;
		tri[numTris++] = p5 + startVert;
		tri[numTris++] = p6 + startVert;
		tri[numTris++] = p7 + startVert;
		tri[numTris++] = p8 + startVert;
		tri[numTris++] = p9 + startVert;
		tri[numTris++] = p10 + startVert;
		tri[numTris++] = p11 + startVert;
		tri[numTris++] = p12 + startVert;
	}
	
	function Attach(amesh : Mesh)
	{
		amesh.Clear();
		var newVertices : Vector3[] = new Vector3[numVerts];
		var newUV : Vector2[] = new Vector2[numVerts];
		var newColor : Color[] = new Color[numVerts];
		var newTris : int[] = new int[numTris];
		var i : int;
		for(i = 0; i < numVerts; ++i)
		{
			newVertices[i] = vert[i];
			newUV[i] = uv[i];
			newColor[i] = colors[i];
		}
		for(i = 0; i < numTris; ++i) newTris[i] = tri[i];
		
		amesh.vertices = newVertices;
		amesh.uv = newUV;
		amesh.triangles = newTris;
		amesh.colors = newColor;
		amesh.RecalculateNormals();
		amesh.RecalculateBounds();
		Reset();
	}
}