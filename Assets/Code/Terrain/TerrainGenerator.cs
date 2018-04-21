using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    /// <summary>
    /// Size of terrrain chunk meshes.
    /// </summary>
    private const int size = 256;

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

    private float Perlin(int x, int y, Dictionary<int, float> weightings, int size)
    {
        return weightings.Aggregate(
            0f,
            (acc, w) => acc + Mathf.PerlinNoise((float)x / size * w.Key, (float)y / size * w.Key) * w.Value
        );
    }

    [ContextMenu("Update mesh")]
    private void UpdateMesh()
    {
        var weightings = new Dictionary<int, float>()
        { 
            { 4, 100f },
            { 8, 50f },
            { 16, 20f },
            { 32, 10f },
            { 64, 5f },

        };

        var mesh = MeshFilter.mesh = new Mesh();

        mesh.name = "TerrainChunk";

        mesh.vertices = Enumerable.Range(0, size)
            .SelectMany(i => Enumerable.Range(0, size)
                //.Select(j => new Vector3(i, Random.Range(0, 1) * 10, j) // TODO: take y from perlin noise
                .Select(j => new Vector3(i, Perlin(i, j, weightings, size), j) // TODO: take y from perlin noise
            ))
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
}
