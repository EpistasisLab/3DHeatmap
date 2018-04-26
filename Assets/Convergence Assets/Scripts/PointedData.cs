using UnityEngine;
using System.Collections;

[System.Serializable]
public class PointedData : object
{
    public bool ready;
    public Vector3 position;
    public int row;
    public int bin;
    public int col;
    public float height;
}