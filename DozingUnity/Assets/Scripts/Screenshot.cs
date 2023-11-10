using System;
using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour {
    // Width of screenshot
    public int Width;
    // Height of screenshot
    public int Height;
    // Camera to use for screenshot
    public Camera Camera;
    // The path to save the screenshot to
    private static readonly string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TakeTransparentScreenshot();
        }
    }

    public void TakeTransparentScreenshot() {
        RenderTexture backupTargetTexture = Camera.targetTexture;
        CameraClearFlags backupClearFlags = Camera.clearFlags;
        RenderTexture backupRenderTexture = RenderTexture.active;

        Texture2D baseTexture = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
        RenderTexture renderTexture = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.ARGB32);

        RenderTexture.active = renderTexture;
        Camera.targetTexture = renderTexture;
        Camera.clearFlags = CameraClearFlags.SolidColor;

        Camera.backgroundColor = Color.clear;
        Camera.Render();
        baseTexture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        baseTexture.Apply();

        byte[] pngShot = ImageConversion.EncodeToPNG(baseTexture);
        File.WriteAllBytes($"{SavePath}/screenshot.png", pngShot);

        Camera.clearFlags = backupClearFlags;
        Camera.targetTexture = backupTargetTexture;
        RenderTexture.active = backupRenderTexture;
        RenderTexture.ReleaseTemporary(renderTexture);
        Destroy(baseTexture);
    }
}
