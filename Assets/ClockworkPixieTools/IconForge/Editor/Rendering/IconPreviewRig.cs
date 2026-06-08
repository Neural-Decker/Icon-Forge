using UnityEngine;

public class IconPreviewRig
{
    public GameObject Root;
    public GameObject PreviewObject;
    public Camera Camera;
    public Light MainLight;
    public Bounds ObjectBounds;

    public void Destroy()
    {
        if (Root != null)
        {
            Object.DestroyImmediate(Root);
        }
    }
}