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
    private int size = 256;

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

    /// <summary>
    /// Ensure the game object is set up correctly.
    /// </summary>
    private void SetupGameObject()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.isStatic = true;
    }

    private float Perlin(float x, float y, IEnumerable<Weighting> weightings)
    {
        return weightings.Aggregate(
            0f,
            (acc, w) => acc + Mathf.PerlinNoise(x * w.Level, y * w.Level) * w.Weight
        );
    }

    [ContextMenu("Update mesh")]
    private void UpdateMesh()
    {
        var mesh = MeshFilter.mesh = new Mesh();

        mesh.name = "TerrainChunk";

        mesh.vertices = Enumerable.Range(0, size)
            .SelectMany(i => Enumerable.Range(0, size)
                .Select(j => new Vector3(
                    i * scale.x / (size - 1),
                    scale.y * Perlin((float)i / (size - 1) + posX, (float)j / (size - 1) + posY, weightings),
                    j * scale.z / (size - 1)
                ))
            )
            .ToArray();

        mesh.triangles = Enumerable.Range(0, size - 1)
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

        mesh.uv = Enumerable.Range(0, size)
            .SelectMany(i => Enumerable.Range(0, size)
                .Select(j => new Vector2((float)i / size, (float)j / size)
            ))
            .ToArray();

        mesh.RecalculateNormals();
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

            switch (meshResolution)
            {
                case TerrainResolution.Resolution_9:
                    size = 9;
                    break;

                case TerrainResolution.Resolution_17:
                    size = 17;
                    break;

                case TerrainResolution.Resolution_33:
                    size = 33;
                    break;

                case TerrainResolution.Resolution_65:
                    size = 65;
                    break;

                case TerrainResolution.Resolution_129:
                    size = 129;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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
