using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralSpherePlane : MonoBehaviour
{
    [Header("Sphere Settings")]
    public int vertexCount = 10;
    public float sphereDiameter = 10.0f;

    private void GenerateSpherePlane()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        int vertCount = vertexCount * vertexCount;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[(vertexCount - 1) * (vertexCount - 1) * 6];

        float radius = sphereDiameter * 0.5f;

        // Generate vertices and uvs
        for (int z = 0; z < vertexCount; z++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int i = z * vertexCount + x;

                // Normalized grid position from 0-1
                float u = (float)x / (vertexCount - 1);
                float v = (float)z / (vertexCount - 1);

                // Convert to spherical coordinates
                float theta = u * Mathf.PI * 2.0f;  // around Y axis
                float phi = v * Mathf.PI;           // from top (0) to bottom (PI)

                // Convert spherical to cartesian
                float sinPhi = Mathf.Sin(phi);
                Vector3 pos = new Vector3(
                    Mathf.Cos(theta) * sinPhi,
                    Mathf.Cos(phi),
                    Mathf.Sin(theta) * sinPhi
                ) * radius;

                vertices[i] = pos;
                uvs[i] = new Vector2(u, v);
            }
        }

        // Generate triangles
        int t = 0;
        for (int z = 0; z < vertexCount - 1; z++)
        {
            for (int x = 0; x < vertexCount - 1; x++)
            {
                int i = z * vertexCount + x;

                // Triangle 1
                triangles[t++] = i;
                triangles[t++] = i + vertexCount;
                triangles[t++] = i + 1;

                // Triangle 2
                triangles[t++] = i + 1;
                triangles[t++] = i + vertexCount;
                triangles[t++] = i + vertexCount + 1;
            }
        }

        // Assign mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        GetComponent<BoxCollider>().size = new Vector3(sphereDiameter, 0.001f, sphereDiameter);
    }



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
}
