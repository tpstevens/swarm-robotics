using UnityEngine;
using UnityEngine.AI;

using Utilities;

public class ResourceFactory {

    public static GameObject ResourceHeader = null;

    /// <summary>
    /// Create a patch of resources, all with NavMeshObstacles attached.
    /// </summary>
    /// <param name="number">The number of resources to create.</param>
    /// <param name="startId">The ID of the first resource in the patch.</param>
    /// <param name="center">The patch center.</param>
    /// <param name="radius">The patch radius.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="color">The resource color.</param>
    /// <returns>The ID of the next resource to be created.</returns>
    public static uint createResourcePatch(uint patchId, uint number, uint startId, Vector2 center, float radius, float sideLength, Color resourceColor, Material patchMaterial = null)
    {
        ensureResourceHeaderExists();

        Transform resourcePatchTransform = null;
        uint numResourcesRoot = (uint)Mathf.Sqrt(number);

        if (patchMaterial != null)
        {
            Rect quadRect = new Rect(center - new Vector2(radius + sideLength, radius + sideLength), new Vector2(2 * (radius + sideLength), 2 * (radius + sideLength)));
            GameObject resourcePatch = WorldspaceUIFactory.createQuad("Resource Patch " + patchId, quadRect, patchMaterial);
            resourcePatchTransform = resourcePatch.transform;
            resourcePatchTransform.SetParent(ResourceHeader.transform);
        }

        if (numResourcesRoot == 1)
        {
            createResource(startId++, center, sideLength, resourceColor, resourcePatchTransform);
        }
        else
        {
            float resourceSpacing = radius * 2.0f / (numResourcesRoot - 1);
            for (uint i = 0; i < numResourcesRoot; ++i)
            {
                for (uint j = 0; j < numResourcesRoot; ++j)
                {
                    float x = center.x - radius + i * resourceSpacing;
                    float z = center.y - radius + j * resourceSpacing;
                    Vector2 position = new Vector3(x, z);
                    createResource(startId++, position, sideLength, resourceColor, resourcePatchTransform);
                }
            }
        }

        return startId;
    }

    /// <summary>
    /// Create a resource and attach a NavMeshObstacle to it.
    /// </summary>
    /// <param name="id">The resource ID (will appear in the name).</param>
    /// <param name="position">The 2D position of the resource.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="color">The resource color.</param>
    /// <returns></returns>
    private static GameObject createResource(uint id, Vector2 position, float sideLength, Color color, Transform parent = null)
    {
        ensureResourceHeaderExists();

        GameObject resource = GameObject.CreatePrimitive(PrimitiveType.Cube);
        resource.transform.name = "Resource " + id;
        resource.transform.tag = "Resource";
        resource.transform.localScale = new Vector3(sideLength, sideLength, sideLength);
        resource.transform.position = new Vector3(position.x, sideLength / 2.0f, position.y);
        resource.transform.SetParent(parent != null ? parent : ResourceHeader.transform);

        Renderer renderer = resource.GetComponentInChildren<Renderer>();
        renderer.material.color = color;

        resource.AddComponent<NavMeshObstacle>();
        NavMeshObstacle obstacle = resource.GetComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.transform.localScale = resource.transform.localScale;

        return resource;
    }

    /// <summary>
    /// Ensure that the Resources header exists in the scene, creating it if necessary.
    /// </summary>
    private static void ensureResourceHeaderExists()
    {
        if (ResourceHeader == null)
        {
            ResourceHeader = GameObject.Find("Resources");
            if (ResourceHeader == null)
            {
                Log.d(LogTag.MAIN, "Created Resources header object");
                ResourceHeader = new GameObject("Resources");
                ResourceHeader.transform.position = Vector3.zero;
                ResourceHeader.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            }
        }
    }
}
