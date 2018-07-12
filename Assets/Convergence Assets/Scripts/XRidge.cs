using UnityEngine;
using System.Collections;

[System.Serializable]
///<summary>
///Stauffer: a 'ridge' is a mesh and assoc'ed objects that visualizes a full row of data
/// </summary>
public class XRidge : object
{
    public GameObject myMeshObject;
    public GameObject myLabel;
    public Transform trans;
    public Mesh myMesh;
    public float[] vertexColors;
    public Renderer myRenderer;
    public int myBin;
    public int myRow;
    /* Stauffer - unused 
    public virtual void AddRidge(GameObject aMeshObject, float[] passColors, Mesh amesh, int bin, int row)
    {
        this.myMeshObject = aMeshObject;
        this.trans = aMeshObject.transform;
        this.myRenderer = aMeshObject.GetComponent<Renderer>();
        this.myMesh = amesh;
        this.vertexColors = passColors;
        this.myBin = bin;
        this.myRow = row;
    }*/

    public virtual void AddRidge(GameObject aMeshObject, Mesh amesh, int bin, int row)
    {
        this.myMeshObject = aMeshObject;
        this.trans = aMeshObject.transform;
        this.myRenderer = aMeshObject.GetComponent<Renderer>();
        this.myMesh = amesh;
        this.myBin = bin;
        this.myRow = row;
    }

    public virtual void AddLabel(GameObject alabel)
    {
        this.myLabel = alabel;
    }

    /* Stauffer - unused 
    public virtual void PositionRidge(Vector3 location, Vector3 extent, int numSlots, int whichSlot)
    {
        float newWidth = extent.z / numSlots;
        float posOff = newWidth * whichSlot;
        this.trans.position = location + new Vector3(0, 0, posOff);
        this.trans.localScale = extent;

        {
            float _57 = this.trans.localScale.z / numSlots;
            Vector3 _58 = this.trans.localScale;
            _58.z = _57;
            this.trans.localScale = _58;
        }
    }*/

    public virtual void NewHeight(float nh)
    {

        {
            float _59 = nh;
            Vector3 _60 = this.trans.localScale;
            _60.y = _59;
            this.trans.localScale = _60;
        }
    }

    //Stauffer - used (at least in one way) to show/hide ridges based on their bin number, and what bins are chosen for viewing
    public virtual void Show(bool doShow)
    {
        this.myRenderer.enabled = doShow;
    }

    //Stauffer - seems unused
    public virtual void AdvanceUV(Vector2 newUV, Vector2 adv)
    {
        int i = 0;
        Vector2[] uv = this.myMesh.uv;
        i = 0;
        while (i < uv.Length)
        {
            Vector2 myuv = newUV + (adv * i);
            uv[i] = uv[i] + myuv;
            ++i;
        }
        this.myMesh.uv = uv;
    }

    //Stauffer - seems unused
    public virtual void ReleaseRidge()
    {
        Debug.Log("Destroying in ReleaseRidge");
        //				gameObject.Destroy(myMeshObject.gameObject);
    }

}