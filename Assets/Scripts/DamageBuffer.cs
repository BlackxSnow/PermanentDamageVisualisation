using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Utility;
using UnityAsync;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine.Profiling;

public class DamageBuffer : MonoBehaviour
{
    const int MaxDequeueCount = 128;
    const int MapSize = 1024;
    public struct Hit
    {
        public bool IsLine;
        public Vector3 Position;

        //These Vectors only apply to lines
        public Vector3 LineEndPosition;
        public Vector3 ABDirection; //Direction from Position to LineEndPosition projected onto Position's face
        public Vector3 BADirection; //Direction from LineEndPosition to Position projected onto LineEndPosition's face

        public float Size;
        public float Falloff;
        public float Strength;
    }
    public Material DamageMaterial;
    public Material UnwrapMaterial;
    public Material DilateMaterial;
    public Camera ProjectionCamera;

    protected ComputeBuffer HitBuffer;
    protected Queue<Hit> HitQueue = new Queue<Hit>();
    
    protected Renderer ThisRenderer;
    protected CommandBuffer DamageCBuffer;
    protected CommandBuffer UnwrapCBuffer;
    protected Mesh ThisMesh;

    protected RenderTexture Damage;
    protected RenderTexture UVPositionMap;
    protected RenderTexture UndilatedUVPositionMap;

    public RawImage DebugImage;
    public bool DisplayOnImage;
    private bool IsUVUnwrapped;

    private void Awake()
    {
        ThisRenderer = GetComponent<Renderer>();
        ThisMesh = GetComponent<MeshFilter>().mesh;

        DamageMaterial = new Material(DamageMaterial);
        UnwrapMaterial = new Material(UnwrapMaterial);
        DilateMaterial = new Material(DilateMaterial);

        Damage = new RenderTexture(MapSize, MapSize, 0, RenderTextureFormat.ARGBHalf, 0);
        UVPositionMap = new RenderTexture(MapSize, MapSize, 0, RenderTextureFormat.ARGBHalf, 0);
        Damage.antiAliasing = 1;
        UVPositionMap.antiAliasing = 1;
        Damage.name = $"_RWDamage {gameObject.name}";
        UVPositionMap.name = $"_UVPosMap {gameObject.name}";
        Damage.Create();
        Graphics.SetRenderTarget(Damage);
        GL.Clear(true, true, new Color(0.5f,0,0,0));
        UVPositionMap.Create();
        UndilatedUVPositionMap = new RenderTexture(UVPositionMap);
        UndilatedUVPositionMap.Create();

        if (DisplayOnImage)
        {
            DebugImage.texture = UVPositionMap; 
        }

        BuildUnwrapCBuffer();
        ThisRenderer.material.SetTexture("_DamageMap", Damage);

    }

    private void BuildUnwrapCBuffer()
    {
        UnwrapCBuffer = new CommandBuffer();
        UnwrapCBuffer.name = $"{gameObject.name} Unwrap";
        UnwrapCBuffer.BeginSample($"Unwrap {gameObject.name}");
        UnwrapCBuffer.SetRenderTarget(UndilatedUVPositionMap);
        UnwrapCBuffer.ClearRenderTarget(true, true, Color.black);
        UnwrapCBuffer.DrawMesh(ThisMesh, Matrix4x4.identity, UnwrapMaterial, 0, -1);
        UnwrapCBuffer.EndSample($"Unwrap {gameObject.name}");
    }

    private void Update()
    {
        RenderDamageMap();
    }

    private void RenderUVPositionMap()
    {
        UnwrapMaterial.SetMatrix("_ProjectionVPMatrix", GL.GetGPUProjectionMatrix(ProjectionCamera.projectionMatrix, true) * ProjectionCamera.worldToCameraMatrix);
        UnwrapMaterial.SetFloat("_MaxObjectSize", ThisRenderer.bounds.extents.Max() * 2f);
        DilateMaterial.SetFloat("_TextureSize", MapSize);
        Graphics.ExecuteCommandBuffer(UnwrapCBuffer);
        Profiler.BeginSample($"Dilate {gameObject.name}");
        Graphics.Blit(UndilatedUVPositionMap, UVPositionMap, DilateMaterial);
        Profiler.EndSample();
        UndilatedUVPositionMap.Release();
    }

    private int GetLineCount(Hit[] hits)
    {
        int lineCount = 0;
        foreach (Hit hit in hits)
        {
            if (hit.IsLine)
            {
                lineCount++;
            }
        }
        return lineCount;
    }

    private Vector4[] BuildPositionArray(Hit[] hits, int lineCount)
    {
        int arraySize = hits.Length + lineCount;

        //Build the array
        Vector4[] positionArray = new Vector4[arraySize];
        int posIndex = 0;
        foreach(Hit hit in hits)
        {
            positionArray[posIndex] = hit.Position;

            if(hit.IsLine)
            {
                positionArray[posIndex + 1] = hit.LineEndPosition;
            }

            posIndex += hit.IsLine ? 2 : 1;
        }

        return positionArray;
    }


    private Vector4[] BuildDataArray(Hit[] hits)
    {
        Vector4[] result = new Vector4[hits.Length];
        for(int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector4(hits[i].Size, hits[i].Falloff, hits[i].Strength, hits[i].IsLine ? 1 : 0);
        }
        return result;
    }

    /// <summary>
    /// Build Vector4 array of line directions in pairs of A->B, B->A. Line directions should be already projected onto planes.
    /// </summary>
    /// <param name="hits"></param>
    /// <param name="lineCount"></param>
    /// <returns></returns>
    private Vector4[] BuildDirectionArray(Hit[] hits, int lineCount)
    {
        Vector4[] directions = new Vector4[lineCount * 2];
        int i = 0;
        foreach(Hit hit in hits)
        {
            directions[i * 2] = hit.ABDirection;
            directions[i * 2 + 1] = hit.BADirection;
            i++;
        }
        return directions;
    }

    private void RenderDamageMap()
    {
        if (!IsUVUnwrapped)
        {
            IsUVUnwrapped = true;
            RenderUVPositionMap();
        }
        if (HitQueue.Count > 0)
        {
            //Hit Data
            Hit[] hits = HitQueue.DequeueMany(Mathf.Min(MaxDequeueCount, HitQueue.Count));

            int lineCount = GetLineCount(hits);

            DamageMaterial.SetInt("_BufferSize", hits.Length);
            DamageMaterial.SetVectorArray("_HitPositions", BuildPositionArray(hits, lineCount));
            DamageMaterial.SetVectorArray("_HitData", BuildDataArray(hits));
            DamageMaterial.SetVectorArray("_LineDirections", BuildDirectionArray(hits, lineCount));

            DamageMaterial.SetFloat("_MaxObjectSize", ThisRenderer.bounds.extents.Max() * 2f);
            DamageMaterial.SetInt("_ComputeDamage", 1);
            DamageMaterial.SetTexture("_UVPositionMap", UVPositionMap);
        }
        else
        {
            DamageMaterial.SetInt("_ComputeDamage", 0);
        }

        Profiler.BeginSample($"Apply Damage to {gameObject.name}");
        RenderTexture tempDamage = RenderTexture.GetTemporary(MapSize, MapSize, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1);
        Graphics.Blit(Damage, tempDamage);
        DamageMaterial.SetTexture("_OldDamage", tempDamage);
        Graphics.Blit(tempDamage, Damage, DamageMaterial);
        RenderTexture.ReleaseTemporary(tempDamage);
        Profiler.EndSample();
    }

    public void RegisterHit(Vector3 position, float size, float falloff, float strength)
    {
        HitQueue.Enqueue(new Hit()
        {
            Position = transform.worldToLocalMatrix.MultiplyPoint(position),
            Size = size,
            Falloff = falloff,
            Strength = strength
        });
    }

    /// <summary>
    /// Registers a hit between two points to draw a line
    /// </summary>
    /// <param name="a">The first position in object space</param>
    /// <param name="b">The second position in object space</param>
    /// <param name="abProjection">The rejection of the vector a->b from the normal of a's hit surface</param>
    /// <param name="baProjection">The rejection of the vector b->a from the normal of b's hit surface</param>
    /// <param name="size"></param>
    /// <param name="falloff"></param>
    /// <param name="strength"></param>
    public void RegisterLineHit(Vector3 a, Vector3 b, Vector3 abProjection, Vector3 baProjection, float size, float falloff, float strength)
    {
        HitQueue.Enqueue(new Hit()
        {
            IsLine = true,
            Position = a,
            LineEndPosition = b,
            ABDirection = abProjection,
            BADirection = baProjection,
            Size = size,
            Falloff = falloff,
            Strength = strength
        });

        //Debug.Log($"From: {a*100}, to: {b*100} @ {Time.frameCount}");
    }
}
