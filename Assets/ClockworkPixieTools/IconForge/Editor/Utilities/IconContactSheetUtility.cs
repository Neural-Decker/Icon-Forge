using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class IconContactSheetUtility
{
    private const int IconsPerRow = 10;
    private const int PreviewSize = 128;
    private const int Padding = 10;
    private const int FooterHeight = 0;

    public static void CreateContactSheet(
        List<string> exportedIconPaths,
        string outputFolder,
        int exportResolution)
    {
        if (exportedIconPaths == null || exportedIconPaths.Count == 0)
        {
            return;
        }

        int iconCount = exportedIconPaths.Count;
        int rows = Mathf.CeilToInt(iconCount / (float)IconsPerRow);

        int sheetWidth =
            IconsPerRow * PreviewSize +
            (IconsPerRow + 1) * Padding;

        int sheetHeight =
            rows * PreviewSize +
            (rows + 1) * Padding +
            FooterHeight;

        Texture2D sheet = new Texture2D(
            sheetWidth,
            sheetHeight,
            TextureFormat.RGBA32,
            false);

        Color backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);

        Color[] backgroundPixels = new Color[sheetWidth * sheetHeight];

        for (int i = 0; i < backgroundPixels.Length; i++)
        {
            backgroundPixels[i] = backgroundColor;
        }

        sheet.SetPixels(backgroundPixels);

        for (int i = 0; i < exportedIconPaths.Count; i++)
        {
            string iconPath = exportedIconPaths[i];

            if (!File.Exists(iconPath))
            {
                continue;
            }

            byte[] iconBytes = File.ReadAllBytes(iconPath);
            Texture2D icon = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!icon.LoadImage(iconBytes))
            {
                continue;
            }

            Texture2D scaledIcon = ScaleTexture(icon, PreviewSize, PreviewSize);

            int column = i % IconsPerRow;
            int row = i / IconsPerRow;

            int x = Padding + column * (PreviewSize + Padding);
            int y = FooterHeight
                + Padding
                + (rows - 1 - row) * (PreviewSize + Padding);

            sheet.SetPixels(x, y, PreviewSize, PreviewSize, scaledIcon.GetPixels());
        }

        sheet.Apply();

        string contactSheetPath = Path.Combine(
            outputFolder,
            "IconForge_ContactSheet.png");

        File.WriteAllBytes(contactSheetPath, sheet.EncodeToPNG());
    }

    private static Texture2D ScaleTexture(
        Texture2D source,
        int targetWidth,
        int targetHeight)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            targetWidth,
            targetHeight);

        Graphics.Blit(source, renderTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D result = new Texture2D(
            targetWidth,
            targetHeight,
            TextureFormat.RGBA32,
            false);

        result.ReadPixels(
            new Rect(0, 0, targetWidth, targetHeight),
            0,
            0);

        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return result;
    }
}