using UnityEngine;
using System.Collections;

public class ComputeManager : MonoBehaviour {

    RenderTexture velocity;
    RenderTexture resultVelocity;

    RenderTexture particles;
    RenderTexture resultParticles;

    RenderTexture pressure;
    RenderTexture resultPressure;

    RenderTexture divergence;

    public int iterations;
    public int w, h, d;

    public ComputeShader fillVolume;

    public ComputeShader clearFloatTex;

    public ComputeShader advect;
    public ComputeShader computeDivergence;
    public ComputeShader jacobi;
    public ComputeShader projectPressure;

    public PointCloud pointCloud;

	// Use this for initialization
	void Start () {
        velocity = CreateFloat4Texture();
        particles = CreateFloat4Texture();
        resultVelocity = CreateFloat4Texture();
        resultParticles = CreateFloat4Texture();

        pressure = CreateFloatTexture();
        resultPressure = CreateFloatTexture();
        divergence = CreateFloatTexture();

        fillVolume.SetTexture(0, "Velocity", velocity);
        fillVolume.SetTexture(0, "Particles", particles);
        SetShaderBounds(fillVolume);
        fillVolume.Dispatch(0, w / 8, h / 8, d / 8);
	}

    void Update ()
    {
        Advect();
        ComputePressure();
        ProjectPressure();

        pointCloud.SetFluidVolume(particles);
    }

    void Advect()
    {
        SetShaderBounds(advect);
        advect.SetFloat("timeStep", 0.1f);
        advect.SetTexture(0, "Velocity", velocity);
        advect.SetTexture(0, "Particles", particles);
        advect.SetTexture(0, "ResultVelocity", resultVelocity);
        advect.SetTexture(0, "ResultParticles", resultParticles);
        advect.Dispatch(0, w / 8, h / 8, d / 8);

        SwapTextures(ref velocity, ref resultVelocity);
        SwapTextures(ref particles, ref resultParticles);
    }

    void ComputePressure()
    {
        SetShaderBounds(computeDivergence);
        computeDivergence.SetTexture(0, "Divergence", divergence);
        computeDivergence.SetTexture(0, "Velocity", velocity);
        computeDivergence.Dispatch(0, w / 8, h / 8, d / 8);

        ClearFloatTex(pressure);
        SetShaderBounds(jacobi);
        jacobi.SetTexture(0, "Divergence", divergence);

        RenderTexture pI = pressure;
        RenderTexture pO = resultPressure;
        for (int i = 0; i < iterations; i++)
        {
            jacobi.SetTexture(0, "Pressure", pI);
            jacobi.SetTexture(0, "ResultPressure", pO);
            jacobi.Dispatch(0, w / 8, h / 8, d / 8);

            SwapTextures(ref pI, ref pO);
        }

        pressure = pI;
    }

    void ProjectPressure()
    {
        SetShaderBounds(projectPressure);
        projectPressure.SetTexture(0, "Velocity", velocity);
        projectPressure.SetTexture(0, "ResultVelocity", resultVelocity);
        projectPressure.SetTexture(0, "Pressure", pressure);
        projectPressure.Dispatch(0, w / 8, h / 8, d / 8);

        SwapTextures(ref velocity, ref resultVelocity);
    }

    RenderTexture CreateFloat4Texture()
    {
        RenderTexture tex = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tex.volumeDepth = d;
        tex.isVolume = true;
        tex.enableRandomWrite = true;
        tex.Create();

        return tex;
    }

    RenderTexture CreateFloatTexture()
    {
        RenderTexture tex = new RenderTexture(w, h, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        tex.volumeDepth = d;
        tex.isVolume = true;
        tex.enableRandomWrite = true;
        tex.Create();

        return tex;
    }

    void ClearFloatTex(RenderTexture tex)
    {
        clearFloatTex.SetTexture(0, "Texture", tex);
        clearFloatTex.Dispatch(0, tex.width / 8, tex.height / 8, tex.volumeDepth / 8);
    }

    void SetShaderBounds(ComputeShader shader)
    {
        shader.SetFloat("w", w);
        shader.SetFloat("h", h);
        shader.SetFloat("d", d);
    }

    void SwapTextures(ref RenderTexture t1, ref RenderTexture t2)
    {
        RenderTexture swap = t1;
        t1 = t2;
        t2 = swap;
    }

    void OnDestroy()
    {
        velocity.Release();
        particles.Release();
    }

}
