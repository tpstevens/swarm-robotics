using UnityEngine;
using UnityEngine.AI;

using System.Collections.Generic;

using Utilities;

public class ResourceFactory {

    private GameObject resourceHeader = null;
    private List<GameObject> resources;

    public ResourceFactory()
    {
        resources = new List<GameObject>();

        resourceHeader = GameObject.Find("Resources");
        if (resourceHeader == null)
        {
            Log.d(LogTag.MAIN, "Created Resources header object");
            resourceHeader = new GameObject("Resources");
            resourceHeader.transform.position = Vector3.zero;
            resourceHeader.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Create a patch of resources, all with NavMeshObstacles attached.
    /// </summary>
    /// <param name="number">The number of resources to create.</param>
    /// <param name="startId">The ID of the first resource in the patch.</param>
    /// <param name="center">The patch center.</param>
    /// <param name="radius">The patch radius.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="resourceColor">The resource color.</param>
    /// <param name="patchMaterial">The material used to create a patch indicator on the ground.</param>
    /// <returns>The ID of the next resource to be created.</returns>
    public uint createResourcePatch(uint patchId, uint number, uint startId, 
                                    Vector2 center, float radius, float sideLength, 
                                    Color resourceColor, Material patchMaterial = null)
    {
        Transform resourcePatchTransform = null;
        uint numResourcesRoot = (uint)Mathf.Sqrt(number);

        if (patchMaterial != null)
        {
            Rect quadRect = new Rect(center - new Vector2(radius + sideLength, radius + sideLength), new Vector2(2 * (radius + sideLength), 2 * (radius + sideLength)));
            GameObject resourcePatch = WorldspaceUIFactory.createQuad("Resource Patch " + patchId, quadRect, patchMaterial);
            resourcePatchTransform = resourcePatch.transform;
            resourcePatchTransform.SetParent(resourceHeader.transform);
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
    /// Create a patch of resources, all with NavMeshObstacles attached.
    /// </summary>
    /// <param name="number">The number of resources to create.</param>
    /// <param name="startId">The ID of the first resource in the patch.</param>
    /// <param name="center">The patch center.</param>
    /// <param name="radius">The patch radius.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="resourceMaterial">The resource material.</param>
    /// <param name="patchMaterial">The material used to create a patch indicator on the ground.</param>
    /// <returns>The ID of the next resource to be created.</returns>
    public uint createResourcePatch(uint patchId, uint number, uint startId,
                                    Vector2 center, float radius, float sideLength,
                                    Material resourceMaterial, Material patchMaterial = null)
    {
        Transform resourcePatchTransform = null;
        uint numResourcesRoot = (uint)Mathf.Sqrt(number);

        if (patchMaterial != null)
        {
            Rect quadRect = new Rect(center - new Vector2(radius + sideLength, radius + sideLength), new Vector2(2 * (radius + sideLength), 2 * (radius + sideLength)));
            GameObject resourcePatch = WorldspaceUIFactory.createQuad("Resource Patch " + patchId, quadRect, patchMaterial);
            resourcePatchTransform = resourcePatch.transform;
            resourcePatchTransform.SetParent(resourceHeader.transform);
        }

        if (numResourcesRoot == 1)
        {
            createResource(startId++, center, sideLength, resourceMaterial, resourcePatchTransform);
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
                    createResource(startId++, position, sideLength, resourceMaterial, resourcePatchTransform);
                }
            }
        }

        return startId;
    }

    public List<Vector2> getResourcePositions()
    {
        List<Vector2> positions = new List<Vector2>();

        if (resources.Count == 0)
        {
            Log.a("ResourceFactory", "no resources found");
        }

        foreach (GameObject r in resources)
        {
            positions.Add(new Vector2(r.transform.position.x, r.transform.position.z));
        }

        return positions;
    }

    /// <summary>
    /// Create a resource and attach a NavMeshObstacle to it.
    /// </summary>
    /// <param name="id">The resource ID (will appear in the name).</param>
    /// <param name="position">The 2D position of the resource.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="resourceColor">The resource color.</param>
    /// <param name="parent">The parent transform of the new resource (defaults to Resource Header).</param>
    /// <returns>The instantiated resource.</returns>
    public GameObject createResource(uint id, Vector2 position, float sideLength, Color color, Transform parent = null)
    {
        GameObject resource = GameObject.CreatePrimitive(PrimitiveType.Cube);
        resource.transform.name = "Resource " + id;
        resource.transform.tag = "Resource";
        resource.transform.localScale = new Vector3(sideLength, sideLength, sideLength);
        resource.transform.position = new Vector3(position.x, sideLength / 2.0f, position.y);
        resource.transform.SetParent(parent != null ? parent : resourceHeader.transform);
        resource.layer = ApplicationManager.LAYER_RESOURCES;

        Renderer renderer = resource.GetComponentInChildren<Renderer>();
        renderer.material.color = color;

        resource.AddComponent<NavMeshObstacle>();
        NavMeshObstacle obstacle = resource.GetComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.transform.localScale = resource.transform.localScale;

        resources.Add(resource);

        return resource;
    }

    /// <summary>
    /// Create a resource and attach a NavMeshObstacle to it.
    /// </summary>
    /// <param name="id">The resource ID (will appear in the name).</param>
    /// <param name="position">The 2D position of the resource.</param>
    /// <param name="sideLength">The resource size length.</param>
    /// <param name="material">The resource material.</param>
    /// <param name="parent">The parent transform of the new resource (defaults to Resource Header).</param>
    /// <returns>The instantiated resource.</returns>
    public GameObject createResource(uint id, Vector2 position, float sideLength, Material material, Transform parent = null)
    {
        GameObject resource = GameObject.CreatePrimitive(PrimitiveType.Cube);
        resource.transform.name = "Resource " + id;
        resource.transform.tag = "Resource";
        resource.transform.localScale = new Vector3(sideLength, sideLength, sideLength);
        resource.transform.position = new Vector3(position.x, sideLength / 2.0f, position.y);
        resource.transform.SetParent(parent != null ? parent : resourceHeader.transform);
        resource.layer = ApplicationManager.LAYER_RESOURCES;

        Renderer renderer = resource.GetComponentInChildren<Renderer>();
        renderer.material = material;

        resource.AddComponent<NavMeshObstacle>();
        NavMeshObstacle obstacle = resource.GetComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.transform.localScale = resource.transform.localScale;

        resources.Add(resource);

        return resource;
    }
}
