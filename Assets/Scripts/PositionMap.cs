using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PositionMap : MonoBehaviour
{
    public static Camera UnwrapCamera;
    public Material UnwrapMaterial;

    public RawImage UIDisplay;
    public bool QueueUnwrap;

    [HideInInspector]
    public RenderTexture UnwrapTexture;
    protected Renderer ThisRenderer;
    private const int Size = 512;

    private void Awake()
    {
        if (!UnwrapCamera)
        {
            UnwrapCamera = GameObject.Find("UnwrapCamera").GetComponent<Camera>();
        }
        UnwrapTexture = new RenderTexture(Size, Size, 0);
        ThisRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if(QueueUnwrap)
        {
            Unwrap();
            QueueUnwrap = false;
        }  
    }

    public void Unwrap()
    {
        int oldLayer = gameObject.layer;
        Material[] oldMaterials = ThisRenderer.materials;
        gameObject.layer = 31;

        UnwrapMaterial.SetVector("_Scale", new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z));
        UnwrapMaterial.SetFloat("_Size", 32);
        Material[] unwraps = new Material[oldMaterials.Length];
        for(int i = 0; i < unwraps.Length; i++)
        {
            unwraps[i] = UnwrapMaterial;
        }
        ThisRenderer.materials = unwraps;
        UnwrapCamera.targetTexture = UnwrapTexture;
        UnwrapCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 100, transform.position.z);

        CommandBuffer cb = new CommandBuffer();
        cb.SetRenderTarget(UnwrapTexture);
        cb.DrawRenderer(ThisRenderer, UnwrapMaterial);
        cb.SetViewProjectionMatrices(UnwrapCamera.worldToCameraMatrix, UnwrapCamera.projectionMatrix);
        cb.Blit(BuiltinRenderTextureType.CameraTarget, UnwrapTexture);
        UnwrapCamera.AddCommandBuffer(CameraEvent.AfterSkybox, cb);
        Graphics.ExecuteCommandBuffer(cb);
        //UnwrapCamera.Render();


        ThisRenderer.materials = oldMaterials;
        gameObject.layer = oldLayer;
        if (UIDisplay)
        {
            UIDisplay.texture = UnwrapTexture;
        }
    }
}
