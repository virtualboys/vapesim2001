using UnityEngine;
using System.Collections;

public class RenderingController : MonoBehaviour {

    public FluidRenderer fluidRenderer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPreRender()
    {
        fluidRenderer.PreRender();
    }

    public void OnPostRender()
    {
        fluidRenderer.Render();
    }
}
