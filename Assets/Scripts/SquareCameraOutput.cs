using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCameraOutput : MonoBehaviour
{
    Camera TargetCamera;
    int LastWidth;
    int LastHeight;

    private void Start()
    {
        TargetCamera = GetComponent<Camera>();
        UpdateCameraRect();
        LastWidth = Display.main.renderingWidth;
        LastHeight = Display.main.renderingHeight;
    }

    private void Update()
    {
        if (LastWidth != Display.main.renderingWidth || LastHeight != Display.main.renderingHeight)
        {
            UpdateCameraRect();
            LastWidth = Display.main.renderingWidth;
            LastHeight = Display.main.renderingHeight;
        }
    }

    private void UpdateCameraRect()
    {
        float heightToWidthFactor = (float)Display.main.renderingHeight / (float)Display.main.renderingWidth;
        TargetCamera.rect = new Rect(1 - heightToWidthFactor, 0, heightToWidthFactor, 1);
    }
}
