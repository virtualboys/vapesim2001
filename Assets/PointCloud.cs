using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PointCloud : MonoBehaviour
{
    public int gridDimensions;
    public float size;

    public GameObject pointPrefab;

    private List<Material> pointMats;

    private int numVerts;
    private float dVert;

    // Use this for initialization
    void Start()
    {
        dVert = size / gridDimensions;
        numVerts = gridDimensions * gridDimensions * gridDimensions;

        pointMats = new List<Material>();
        CreateMesh();
    }

    void CreateMesh()
    {

        for (int x = 0; x < gridDimensions; x++)
        {
            for (int y = 0; y < gridDimensions; y++)
            {
                for (int z = 0; z < gridDimensions; z++)
                {
                    int ind = x * gridDimensions * gridDimensions + y * gridDimensions + z;
                    Vector3 pos = new Vector3(x * dVert - size / 2, y * dVert - size / 2, z * dVert - size / 2);
                    CreatePoint(pos);
                }
            }
        }
    }

    void CreatePoint(Vector3 pos)
    {
        GameObject p = (GameObject)Instantiate(pointPrefab, pos, Quaternion.identity);
        Material m = p.GetComponent<MeshRenderer>().material;

        Vector3 coord = (pos + new Vector3(size / 2, size / 2, size / 2)) / size;
        m.SetFloat("_Size", size);
        m.SetFloat("_CoordX", coord.x);
        m.SetFloat("_CoordY", coord.y);
        m.SetFloat("_CoordZ", coord.z);

        pointMats.Add(m);
    }

    public void SetFluidVolume(RenderTexture tex)
    {
        foreach (Material m in pointMats)
        {
            m.SetTexture("_PointCloud", tex);
        }
    }

    void Update()
    {
        
    }
}