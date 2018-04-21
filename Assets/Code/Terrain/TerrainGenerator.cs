using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Generates terrain on the attached MeshFilter
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    // Enum so that it can be easily shown in the inspector.
    enum TerrainResolution
    {
        Resolution_129,
        Resolution_65,
        Resolution_33,
        Resolution_17,
        Resolution_9
    }

    [SerializeField]
    private TerrainResolution meshResolution = TerrainResolution.Resolution_129;

    private TerrainResolution cachedMeshResolution;

    /// <summary>
    /// The X position of this chunk in the world grid.
    /// </summary>
    [SerializeField]
    private int posX;

    private int cachedPosX;

    /// <summary>
    /// The Y position of this chunk in the world grid.
    /// </summary>
    [SerializeField]
    private int posY;

    private int cachedPosY;

    /// <summary>
    /// Size of terrrain chunk meshes.
    /// </summary>
    private int Size { get { return TerrainResolutionToSize(meshResolution); } }

    [SerializeField]
    private Vector3 scale = Vector3.one;

    private Vector3 cachedScale;

    private bool dirty = true;

    [Serializable]
    struct Weighting : IEquatable<Weighting>
    {
        public int Level;
        public float Weight;

        public bool Equals(Weighting other)
        {
            return Level == other.Level && Weight == other.Weight;
        }
    }

    [SerializeField]
    private Weighting[] weightings = new[]
    { 
        new Weighting { Level = 4, Weight = 200f },
        new Weighting { Level = 8, Weight = 50f },
        new Weighting { Level = 16, Weight = 20f },
        new Weighting { Level = 32, Weight = 5f },
        new Weighting { Level = 64, Weight = 3f }, 
    };

    private Weighting[] cachedWeightings;


    private MeshFilter meshFilter;

    private MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                Assert.IsNotNull(meshFilter);
            }
            return meshFilter;
        }
    }

    private MeshCollider meshCollider;

    private MeshCollider MeshCollider
    {
        get
        {
            if (meshCollider == null)
            {
                meshCollider = GetComponentInChildren<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = InitColliderObject();
                }
            }
            return meshCollider;
        }
    }

    private MeshCollider InitColliderObject()
    {
        var colliderObject = new GameObject
        {
            name = "Collider",
        };
        colliderObject.transform.parent = transform;

        // Move the collider object up to ensure that it doesn't intersect with the visual
        // mesh.
        colliderObject.transform.position += Vector3.up * 1.2f;

        var collider = colliderObject.AddComponent<MeshCollider>();
        return collider;
    }

    /// <summary>
    /// Ensure the game object is set up correctly.
    /// </summary>
    private void SetupGameObject()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.isStatic = true;
    }

    private static float Perlin(float x, float y, IEnumerable<Weighting> weightings)
    {
        return weightings.Aggregate(
            0f,
            (acc, w) => acc + Mathf.PerlinNoise(x * w.Level, y * w.Level) * w.Weight
        );
    }

    private static Vector3[] GenerateVerts(
        int size, 
        int posX, 
        int posY, 
        IEnumerable<Weighting> weightings,
        Vector3 scale
    )
    {
        return Enumerable.Range(0, size)
            .SelectMany(i => Enumerable.Range(0, size)
                .Select(j => new Vector3(
                    i * scale.x / (size - 1),
                    scale.y * Perlin((float)i / (size - 1) + posX, (float)j / (size - 1) + posY, weightings),
                    j * scale.z / (size - 1)
                ))
            )
            .ToArray();
    }

    private static int[] GenerateTris(
        int size,
        int posX,
        int posY,
        IEnumerable<Weighting> weightings,
        Vector3 scale
    )
    {
        return Enumerable.Range(0, size - 1)
            .SelectMany(i => Enumerable.Range(0, size - 1)
                .SelectMany(j => new[] {
                    i * size + j,
                    i * size + j + 1,
                    i * size + j + size,

                    i * size + j + 1,
                    i * size + j + size + 1,
                    i * size + j + size
                })
            )
            .ToArray();
    }

    [ContextMenu("Update mesh")]
    private void UpdateMesh()
    {
        var mesh = MeshFilter.mesh = new Mesh
        {
            name = "TerrainChunk",
            vertices = GenerateVerts(Size, posX, posY, weightings, scale),
            triangles = GenerateTris(Size, posX, posY, weightings, scale),
            uv = Enumerable.Range(0, Size)
                .SelectMany(i => Enumerable.Range(0, Size)
                    .Select(j => new Vector2((float)i / Size, (float)j / Size)
                ))
                .ToArray()
        };

        mesh.RecalculateNormals();

        UpdateCollider();
    }

    private void UpdateCollider()
    {
        var mesh = new Mesh
        {
            name = "TerrainCollider",
            vertices = GenerateVerts(17, posX, posY, weightings, scale),
            triangles = GenerateTris(17, posX, posY, weightings, scale)
        };

        MeshCollider.sharedMesh = mesh;
    }


    private static int TerrainResolutionToSize(TerrainResolution res)
    {
        switch (res)
        {
            case TerrainResolution.Resolution_9:
                return 9;

            case TerrainResolution.Resolution_17:
                return 17;

            case TerrainResolution.Resolution_33:
                return 33;

            case TerrainResolution.Resolution_65:
                return 65;

            case TerrainResolution.Resolution_129:
                return 129;

            default:
                throw new ArgumentOutOfRangeException();
        }
    
    }

    private void Update()
    {
        // Check dirty
        if (scale != cachedScale)
        {
            cachedScale = scale;
            dirty = true;
        }

        if (meshResolution != cachedMeshResolution)
        {
            cachedMeshResolution = meshResolution;
            dirty = true;
        }

        if (weightings != null && cachedWeightings != null && weightings.Length == cachedWeightings.Length)
        {
            for (var i = 0; i < weightings.Length; i++)
            {
                if (!weightings[i].Equals(cachedWeightings[i]))
                {
                    dirty = true;
                    cachedWeightings = (Weighting[])weightings.Clone();
                    break;
                }
            }
        }
        else
        {
            dirty = true;
            cachedWeightings = (Weighting[])weightings.Clone();
        }

        if (posX != cachedPosX)
        {
            dirty = true;
            cachedPosX = posX;
        }

        if (posY != cachedPosY)
        {
            dirty = true;
            cachedPosY = posY;
        }

        if (dirty)
        {
            UpdateMesh();
            dirty = false;
        }
    }
}
