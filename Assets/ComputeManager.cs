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

    RenderTexture boundaryNormals;

    public int iterations;
    public int w, h, d;

    public float impulseSize;
    public float impulseMag;
    public Vector4 impulseLoc;
    public Vector4 impulseDir;
    public Vector4 splatColor;

    public ComputeShader fillVolume;
    public ComputeShader initBoundaries;

    public ComputeShader clearFloatTex;

    public ComputeShader advect;
    public ComputeShader gaussian;
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
        boundaryNormals = CreateFloat4Texture();

        pressure = CreateFloatTexture();
        resultPressure = CreateFloatTexture();
        divergence = CreateFloatTexture();

        fillVolume.SetTexture(0, "Velocity", velocity);
        fillVolume.SetTexture(0, "Particles", particles);
        SetShaderBounds(fillVolume);
        fillVolume.Dispatch(0, w / 8, h / 8, d / 8);

        initBoundaries.SetTexture(0, "BoundaryNormals", boundaryNormals);
        SetShaderBounds(initBoundaries);
        initBoundaries.Dispatch(0, w / 8, h / 8, d / 8);

        ApplyExternalForces();
        ApplyExternalForces();
        ApplyExternalForces();
	}

    void Update ()
    {
        Advect();
        //ApplyExternalForces();
        ComputePressure();
        ProjectPressure();

        //pointCloud.SetFluidVolume(particles);
    }

    void Advect()
    {
        SetShaderBounds(advect);
        advect.SetFloat("timeStep", 0.01f);
        advect.SetTexture(0, "Velocity", velocity);
        advect.SetTexture(0, "Particles", particles);
        advect.SetTexture(0, "ResultVelocity", resultVelocity);
        advect.SetTexture(0, "ResultParticles", resultParticles);
        advect.Dispatch(0, w / 8, h / 8, d / 8);

        SwapTextures(ref velocity, ref resultVelocity);
        SwapTextures(ref particles, ref resultParticles);
    }

    void ApplyExternalForces()
    {
        SetShaderBounds(gaussian);
        gaussian.SetFloat("splatSize", impulseSize);
        gaussian.SetVector("splatPos", impulseLoc);
        gaussian.SetVector("splatColor", splatColor);
        gaussian.SetVector("impulse", impulseMag * impulseDir.normalized);

        gaussian.SetTexture(0, "Velocity", velocity);
        gaussian.SetTexture(0, "Particles", particles);
        gaussian.SetTexture(0, "ResultVelocity", resultVelocity);
        gaussian.SetTexture(0, "ResultParticles", resultParticles);
        gaussian.SetTexture(0, "BoundaryNormals", boundaryNormals);
        gaussian.Dispatch(0, w / 8, h / 8, d / 8);

        SwapTextures(ref velocity, ref resultVelocity);
        SwapTextures(ref particles, ref resultParticles);
    }

    void ComputePressure()
    {
        SetShaderBounds(computeDivergence);
        computeDivergence.SetTexture(0, "Divergence", divergence);
        computeDivergence.SetTexture(0, "Velocity", velocity);
        computeDivergence.SetTexture(0, "BoundaryNormals", boundaryNormals);
        computeDivergence.Dispatch(0, w / 8, h / 8, d / 8);

        ClearFloatTex(pressure);
        SetShaderBounds(jacobi);
        jacobi.SetTexture(0, "Divergence", divergence);
        jacobi.SetTexture(0, "BoundaryNormals", boundaryNormals);

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
        projectPressure.SetTexture(0, "BoundaryNormals", boundaryNormals);
        projectPressure.Dispatch(0, w / 8, h / 8, d / 8);

        SwapTextures(ref velocity, ref resultVelocity);
    }

    public RenderTexture GetFluidDensity()
    {
        return particles;
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
