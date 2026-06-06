using UnityEngine;

public static class ObjectBoundsAnalyzer
{
    public static Bounds CalculateRendererBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("Icon Forge: Source object has no renderers. Using fallback bounds.");
            return new Bounds(Vector3.zero, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}
