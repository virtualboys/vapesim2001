using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FluidRenderer : MonoBehaviour {

    public float scale;

    public ComputeManager computeManager;

    private Mesh mesh;
    private Material mat;

    private RenderTexture rayTex;

	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(scale * computeManager.w, scale * computeManager.h, scale * computeManager.d);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        //mr.enabled = false;
        mat = mr.material;
        mesh = GetComponent<MeshFilter>().mesh;
        SetTexCoords();

        rayTex = new RenderTexture(Screen.width, Screen.height, 24);
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void PreRender()
    {
        //RenderTexture.active = rayTex;
        Camera.main.targetTexture = rayTex;
        mat.SetTexture("_FluidDensity", computeManager.GetFluidDensity());
    }

    public void Render()
    {
        //Camera.main.clearFlags = CameraClearFlags.Nothing;
        //Graphics.SetRenderTarget(rayTex);
        //Camera.main.targetTexture = rayTex;
        //RenderTexture.active = rayTex;

        //mat.SetPass(0);
        //GL.PushMatrix();
        //GL.LoadIdentity();
        ////GL.MultMatrix(Camera.main.cameraToWorldMatrix);
        //Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        //GL.PopMatrix();

        //mat.SetPass(1);
        //Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);

        //RenderTexture.active = null;
        //Camera.main.targetTexture = null;
        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rayTex);

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Graphics.Blit(rayTex, null as RenderTexture);
    }

    void SetTexCoords()
    {
        List<Vector4> texCoords = new List<Vector4>(mesh.vertexCount);
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 v = mesh.vertices[i];
            Vector4 coord = new Vector4(v.x, v.y, v.z, 0) + new Vector4(.5f, .5f, .5f, 0);
            texCoords.Add(coord);
        }

        mesh.SetUVs(0, texCoords);
    }

    Mesh GenerateBoundary ()
    {
        float w = scale * computeManager.w;
        float h = scale * computeManager.h;
        float d = scale * computeManager.d;

        int i = 0;
        Vector3[] verts = new Vector3[8];
        verts[i++] = new Vector3(-w / 2, -h / 2, -d / 2);
        verts[i++] = new Vector3(-w / 2, -h / 2, d / 2);
        verts[i++] = new Vector3(-w / 2, h / 2, -d / 2);
        verts[i++] = new Vector3(-w / 2, h / 2, d / 2);
        verts[i++] = new Vector3(w / 2, -h / 2, -d / 2);
        verts[i++] = new Vector3(w / 2, -h / 2, d / 2);
        verts[i++] = new Vector3(w / 2, h / 2, -d / 2);
        verts[i++] = new Vector3(w / 2, h / 2, d / 2);

        i = 0;
        int[] inds = new int[12 * 3];

        // left
        inds[i++] = 2; inds[i++] = 0; inds[i++] = 1;
        inds[i++] = 2; inds[i++] = 1; inds[i++] = 3;

        // right 
        inds[i++] = 5; inds[i++] = 4; inds[i++] = 6;
        inds[i++] = 5; inds[i++] = 6; inds[i++] = 7;

        // top
        inds[i++] = 3; inds[i++] = 6; inds[i++] = 2;
        inds[i++] = 7; inds[i++] = 6; inds[i++] = 3;

        // bottom 
        inds[i++] = 0; inds[i++] = 4; inds[i++] = 1;
        inds[i++] = 1; inds[i++] = 4; inds[i++] = 5;

        // back
        inds[i++] = 1; inds[i++] = 5; inds[i++] = 7;
        inds[i++] = 7; inds[i++] = 3; inds[i++] = 1;

        // front 
        inds[i++] = 6; inds[i++] = 0; inds[i++] = 2;
        inds[i++] = 4; inds[i++] = 0; inds[i++] = 6;

        List<Vector3> v = new List<Vector3>(verts);

        Mesh mesh = new Mesh();
        mesh.SetVertices(v);
        mesh.SetIndices(inds, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        return mesh;
    }
}
