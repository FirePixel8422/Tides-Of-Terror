using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(MeshCollider))]
public class MeshColliderCombiner : MonoBehaviour
{
    private void Reset()
    {
        Mesh combinedMesh = CombineMesh();
    }


    private Mesh CombineMesh()
    {
        MeshCollider coll = GetComponent<MeshCollider>();
        Mesh colliderMesh = new Mesh();

        MeshFilter[] childMeshRenderers = GetComponentsInChildren<MeshFilter>();


        CombineInstance[] combiner = new CombineInstance[childMeshRenderers.Length];

        for (int i = 0; i < childMeshRenderers.Length; i++)
        {
            combiner[i].mesh = childMeshRenderers[i].sharedMesh;
            combiner[i].transform = transform.worldToLocalMatrix;
        }


        colliderMesh.CombineMeshes(combiner);
        colliderMesh.RecalculateBounds();

        coll.sharedMesh = colliderMesh;

        return colliderMesh;
    }



    void SaveMeshAsAsset(Mesh mesh, string assetName)
    {
#if UNITY_EDITOR
        string path = $"Assets/{assetName}.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"Mesh saved as asset at: {path}");
#endif
    }
}
