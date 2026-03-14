//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;

//public class MeshCombineTool
//{
//    [MenuItem("Tools/Combine Mesh (Keep Materials)")]
//    static void CombineMeshes()
//    {
//        if (Selection.activeGameObject == null)
//        {
//            Debug.LogError("请先选择一个父物体");
//            return;
//        }

//        GameObject root = Selection.activeGameObject;

//        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();

//        Dictionary<Material, List<CombineInstance>> combineDict = new Dictionary<Material, List<CombineInstance>>();

//        foreach (MeshFilter mf in meshFilters)
//        {
//            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
//            if (mr == null) continue;

//            for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
//            {
//                Material mat = mr.sharedMaterials[i];

//                if (!combineDict.ContainsKey(mat))
//                {
//                    combineDict[mat] = new List<CombineInstance>();
//                }

//                CombineInstance ci = new CombineInstance();
//                ci.mesh = mf.sharedMesh;
//                ci.subMeshIndex = i;
//                ci.transform = mf.transform.localToWorldMatrix;

//                combineDict[mat].Add(ci);
//            }
//        }

//        List<Mesh> meshes = new List<Mesh>();
//        List<Material> materials = new List<Material>();

//        foreach (var pair in combineDict)
//        {
//            Mesh mesh = new Mesh();
//            mesh.CombineMeshes(pair.Value.ToArray(), true, true);

//            meshes.Add(mesh);
//            materials.Add(pair.Key);
//        }

//        CombineInstance[] finalCombine = new CombineInstance[meshes.Count];

//        for (int i = 0; i < meshes.Count; i++)
//        {
//            finalCombine[i].mesh = meshes[i];
//            finalCombine[i].subMeshIndex = 0;
//            finalCombine[i].transform = Matrix4x4.identity;
//        }

//        Mesh finalMesh = new Mesh();
//        finalMesh.CombineMeshes(finalCombine, false, false);

//        GameObject combinedObject = new GameObject("CombinedMesh");

//        MeshFilter mfFinal = combinedObject.AddComponent<MeshFilter>();
//        mfFinal.sharedMesh = finalMesh;

//        MeshRenderer mrFinal = combinedObject.AddComponent<MeshRenderer>();
//        mrFinal.sharedMaterials = materials.ToArray();

//        Debug.Log("Mesh合并完成（材质保留）");
//    }
//}
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class MeshCombineTool
{
    [MenuItem("Tools/Combine Mesh (Keep Materials + Collider)")]
    static void CombineMeshes()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("请先选择一个父物体");
            return;
        }

        GameObject root = Selection.activeGameObject;

        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();

        Dictionary<Material, List<CombineInstance>> combineDict =
            new Dictionary<Material, List<CombineInstance>>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
            {
                if (i >= mr.sharedMaterials.Length) continue;

                Material mat = mr.sharedMaterials[i];

                if (!combineDict.ContainsKey(mat))
                {
                    combineDict.Add(mat, new List<CombineInstance>());
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.subMeshIndex = i;
                ci.transform = mf.transform.localToWorldMatrix;

                combineDict[mat].Add(ci);
            }
        }

        List<Mesh> meshes = new List<Mesh>();
        List<Material> materials = new List<Material>();

        foreach (var pair in combineDict)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;

            mesh.CombineMeshes(pair.Value.ToArray(), true, true);

            meshes.Add(mesh);
            materials.Add(pair.Key);
        }

        CombineInstance[] finalCombine = new CombineInstance[meshes.Count];

        for (int i = 0; i < meshes.Count; i++)
        {
            finalCombine[i].mesh = meshes[i];
            finalCombine[i].subMeshIndex = 0;
            finalCombine[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = IndexFormat.UInt32;

        finalMesh.CombineMeshes(finalCombine, false, false);

        GameObject combinedObject = new GameObject(root.name + "_Combined");

        combinedObject.transform.position = root.transform.position;
        combinedObject.transform.rotation = root.transform.rotation;
        combinedObject.transform.localScale = root.transform.localScale;

        MeshFilter mfFinal = combinedObject.AddComponent<MeshFilter>();
        mfFinal.sharedMesh = finalMesh;

        MeshRenderer mrFinal = combinedObject.AddComponent<MeshRenderer>();
        mrFinal.sharedMaterials = materials.ToArray();

        // 自动添加 MeshCollider
        MeshCollider mc = combinedObject.AddComponent<MeshCollider>();
        mc.sharedMesh = finalMesh;
        mc.convex = false;

        // 关闭原物体
        root.SetActive(false);

        Debug.Log("Mesh 合并完成！");
        Debug.Log("合并 Mesh 数量: " + meshFilters.Length);
        Debug.Log("最终 SubMesh 数量: " + materials.Count);
    }
}