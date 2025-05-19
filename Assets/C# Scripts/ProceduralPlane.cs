using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralSpherePlane : MonoBehaviour
{
    [Header("Sphere Settings")]
    [SerializeField] private int vertexCount = 10;
    [SerializeField] private float sphereDiameter = 10.0f;




    private void GenerateSpherePlane()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        int vertCount = vertexCount * vertexCount;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        List<int> triangles = new List<int>();

        float halfSize = sphereDiameter * 0.5f;
        float step = sphereDiameter / (vertexCount - 1);
        float radius = sphereDiameter * 0.5f;

        // Generate vertices and uvs
        for (int z = 0; z < vertexCount; z++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int i = z * vertexCount + x;

                float px = -halfSize + x * step;
                float pz = -halfSize + z * step;
                float distSq = px * px + pz * pz;

                // If within sphere boundary, project onto sphere surface
                float py = distSq <= radius * radius
                    ? Mathf.Sqrt(radius * radius - distSq)
                    : 0.0f;

                vertices[i] = new Vector3(px, py, pz);
                uvs[i] = new Vector2((float)x / (vertexCount - 1), (float)z / (vertexCount - 1));
            }
        }

        // Generate triangles — only if all four corners are inside the sphere
        for (int z = 0; z < vertexCount - 1; z++)
        {
            for (int x = 0; x < vertexCount - 1; x++)
            {
                int i0 = z * vertexCount + x;
                int i1 = i0 + 1;
                int i2 = i0 + vertexCount;
                int i3 = i2 + 1;

                // Check if all four corners are inside the sphere
                if (vertices[i0].y > 0.0f && vertices[i1].y > 0.0f &&
                    vertices[i2].y > 0.0f && vertices[i3].y > 0.0f)
                {
                    // Triangle 1
                    triangles.Add(i0);
                    triangles.Add(i2);
                    triangles.Add(i1);

                    // Triangle 2
                    triangles.Add(i1);
                    triangles.Add(i2);
                    triangles.Add(i3);
                }
            }
        }

        // Assign mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter filter = GetComponent<MeshFilter>();
#pragma warning disable CS0618
        filter.sharedMesh = mesh;
#pragma warning restore CS0618

        GetComponent<BoxCollider>().size = new Vector3(sphereDiameter, 0.001f, sphereDiameter);
    }



#if UNITY_EDITOR

    // Update when value changed or first time created
    private void OnValidate()
    {
        if (vertexCount < 2)
        {
            Debug.LogWarning("Minimum vertex count is 2, Twan.");
            vertexCount = 2;
        }

        if (Application.isPlaying) return;

        GenerateSpherePlane();
    }

    private void Reset()
    {
        if (Application.isPlaying) return;

        GenerateSpherePlane();
    }

#endif
}
