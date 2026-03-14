using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshStatistics
{
    [MenuItem("Tools/Scan Scene Mesh Usage")]
    static void Scan()
    {
        MeshFilter[] filters = GameObject.FindObjectsOfType<MeshFilter>();
        Dictionary<Mesh, int> meshCount = new Dictionary<Mesh, int>();

        foreach (MeshFilter mf in filters)
        {
            Mesh mesh = mf.sharedMesh;

            if (mesh == null) continue;

            if (meshCount.ContainsKey(mesh))
                meshCount[mesh]++;
            else
                meshCount.Add(mesh, 1);
        }

        foreach (var pair in meshCount)
        {
            Debug.Log(pair.Key.name + "  使用数量: " + pair.Value);
        }
    }
}