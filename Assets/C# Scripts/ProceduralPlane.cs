using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class ProceduralGridPlane : MonoBehaviour
{
    [Header("Grid Settings")]
    public int vertexCount = 10;
    public float planeSize = 10.0f;


    private void GeneratePlane()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        int vertCount = vertexCount * vertexCount;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[(vertexCount - 1) * (vertexCount - 1) * 6];

        float spacingX = planeSize / (vertexCount - 1);
        float spacingZ = planeSize / (vertexCount - 1);

        Vector3 originOffset = new Vector3(-planeSize / 2f, 0, -planeSize / 2f);

        // Generate vertices and uvs
        for (int z = 0; z < vertexCount; z++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int i = z * vertexCount + x;
                vertices[i] = new Vector3(x * spacingX, 0, z * spacingZ) + originOffset;
                uvs[i] = new Vector2((float)x / (vertexCount - 1), (float)z / (vertexCount - 1));
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

        // Set BoxCollider size to match plane
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(planeSize, 0.0001f, planeSize);
        boxCollider.center = Vector3.zero;
    }



    //Update when value changed ot first time created
    private void OnValidate()
    {
        if (vertexCount < 2)
        {
            Debug.LogWarning("Nee je kan geen niet minder dan 2 tris hebben twan");
            vertexCount = 2;
        }

        if (Application.isPlaying) return;

        GeneratePlane();
    }
    private void Reset()
    {
        if (Application.isPlaying) return;

        GeneratePlane();
    }
}
