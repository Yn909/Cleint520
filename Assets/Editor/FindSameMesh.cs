using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindSameMesh
{
    [MenuItem("Tools/Find Objects Using Same Mesh")]
    static void FindObjects()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("请先选一个物体");
            return;
        }

        MeshFilter selected = Selection.activeGameObject.GetComponent<MeshFilter>();

        if (selected == null)
        {
            Debug.LogError("选中的物体没有MeshFilter");
            return;
        }

        Mesh targetMesh = selected.sharedMesh;

        MeshFilter[] all = GameObject.FindObjectsOfType<MeshFilter>();

        List<GameObject> results = new List<GameObject>();

        foreach (MeshFilter mf in all)
        {
            if (mf.sharedMesh == targetMesh)
            {
                results.Add(mf.gameObject);
            }
        }

        Selection.objects = results.ToArray();

        Debug.Log("找到相同Mesh数量: " + results.Count);
    }
}