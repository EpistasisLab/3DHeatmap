using UnityEngine;
using System.Collections;

[System.Serializable]
public class MeshMaker : object
{
    public Color currColor;
    public int numVerts;
    public int startVert;
    public Vector3[] vert;
    public Vector2[] uv;
    public int[] tri;
    public bool ok;
    public Color[] colors;
    public int numTris;
    public bool bumpStart;
    public virtual void Reset()
    {
        this.numVerts = 0;
        this.startVert = 0;
        this.numTris = 0;
        this.bumpStart = true;
        this.ok = true;
    }

    public virtual void SetColor(Color c)
    {
        this.currColor = c;
    }

    public virtual bool IsOK()
    {
        return this.ok;
    }

    public virtual void Verts(float x, float y, float z, float u, float v)
    {
        if (this.bumpStart)
        {
            this.startVert = this.numVerts;
            this.bumpStart = false;
        }
        this.vert[this.numVerts] = new Vector3(x, y, z);
        this.uv[this.numVerts] = new Vector2(u, v);
        this.colors[this.numVerts] = this.currColor;
        if (this.numVerts >= 64000)
        {
            this.ok = false;
        }
        else
        {
            ++this.numVerts;
        }
    }

    public virtual void Tris(int p1, int p2, int p3)
    {
        this.bumpStart = true;
        if (this.numTris >= 63997)
        {
            this.ok = false;
            return;
        }
        this.tri[this.numTris++] = p1 + this.startVert;
        this.tri[this.numTris++] = p2 + this.startVert;
        this.tri[this.numTris++] = p3 + this.startVert;
    }

    public virtual void Tris(int p1, int p2, int p3, int p4, int p5, int p6)
    {
        this.bumpStart = true;
        if (this.numTris >= 63994)
        {
            this.ok = false;
            return;
        }
        this.tri[this.numTris++] = p1 + this.startVert;
        this.tri[this.numTris++] = p2 + this.startVert;
        this.tri[this.numTris++] = p3 + this.startVert;
        this.tri[this.numTris++] = p4 + this.startVert;
        this.tri[this.numTris++] = p5 + this.startVert;
        this.tri[this.numTris++] = p6 + this.startVert;
    }

    public virtual void Tris(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)
    {
        this.bumpStart = true;
        if (this.numTris >= 63991)
        {
            this.ok = false;
            return;
        }
        this.tri[this.numTris++] = p1 + this.startVert;
        this.tri[this.numTris++] = p2 + this.startVert;
        this.tri[this.numTris++] = p3 + this.startVert;
        this.tri[this.numTris++] = p4 + this.startVert;
        this.tri[this.numTris++] = p5 + this.startVert;
        this.tri[this.numTris++] = p6 + this.startVert;
        this.tri[this.numTris++] = p7 + this.startVert;
        this.tri[this.numTris++] = p8 + this.startVert;
        this.tri[this.numTris++] = p9 + this.startVert;
    }

    public virtual void Tris(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10, int p11, int p12)
    {
        this.bumpStart = true;
        if (this.numTris >= 63991)
        {
            this.ok = false;
            return;
        }
        this.tri[this.numTris++] = p1 + this.startVert;
        this.tri[this.numTris++] = p2 + this.startVert;
        this.tri[this.numTris++] = p3 + this.startVert;
        this.tri[this.numTris++] = p4 + this.startVert;
        this.tri[this.numTris++] = p5 + this.startVert;
        this.tri[this.numTris++] = p6 + this.startVert;
        this.tri[this.numTris++] = p7 + this.startVert;
        this.tri[this.numTris++] = p8 + this.startVert;
        this.tri[this.numTris++] = p9 + this.startVert;
        this.tri[this.numTris++] = p10 + this.startVert;
        this.tri[this.numTris++] = p11 + this.startVert;
        this.tri[this.numTris++] = p12 + this.startVert;
    }

    public virtual void Attach(Mesh amesh)
    {
        int i = 0;
        amesh.Clear();
        //Stauffer - This is inefficient! Why is this done? Cuz this.vert and others are a fixed size?
        //Should be able to do all this w/out this copying. Seems like a bit of a time waste and also
        // a memory waste for larger data sets.
        //BUT note, docs say you should use a separate Vector3 of verts and then assign it to amesh.vertices, so be sure to do that part that way.
        Vector3[] newVertices = new Vector3[this.numVerts];
        Vector2[] newUV = new Vector2[this.numVerts];
        Color[] newColor = new Color[this.numVerts];
        int[] newTris = new int[this.numTris];
        i = 0;
        while (i < this.numVerts)
        {
            newVertices[i] = this.vert[i];
            newUV[i] = this.uv[i];
            newColor[i] = this.colors[i];
            ++i;
        }
        i = 0;
        while (i < this.numTris)
        {
            newTris[i] = this.tri[i];
            ++i;
        }
        amesh.vertices = newVertices;
        amesh.uv = newUV;
        amesh.triangles = newTris;
        amesh.colors = newColor;
        amesh.RecalculateNormals();
        amesh.RecalculateBounds();

        //mark mesh as no longer readable to save memory, since we aren't using mesh collider for data inspection
        amesh.UploadMeshData(true);

        this.Reset();
    }

    public MeshMaker()
    {
        this.vert = new Vector3[64000];
        this.uv = new Vector2[64000];
        this.tri = new int[64000];
        this.ok = true;
        this.colors = new Color[64000];
        this.bumpStart = true;
    }

}