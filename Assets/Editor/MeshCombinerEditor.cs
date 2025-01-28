using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MeshCombinerEditor : EditorWindow
{
    [MenuItem("Tools/Combine Meshes")]
    static void CombineMeshes()
    {
        // ユーザーに統合オプションを選択させる
        bool keepMaterials = EditorUtility.DisplayDialog(
            "Combine Meshes",
            "マテリアルを保持しますか？\n\n- 「はい」: マテリアルを維持\n- 「いいえ」: すべて統合",
            "はい (維持する)",
            "いいえ (統合する)"
        );

        bool keepRelativeTransform = EditorUtility.DisplayDialog(
            "Transform Handling",
            "選択オブジェクトの相対位置を保持しますか？\n\n- 「はい」: 選択オブジェクトを基準に統合（通常）\n- 「いいえ」: ワールド座標を適用",
            "はい (基準オブジェクトを維持)",
            "いいえ (ワールド座標適用)"
        );

        bool removeOriginalMeshes = EditorUtility.DisplayDialog(
            "Remove Original Meshes",
            "元のオブジェクトの Mesh と Renderer を削除しますか？\n\n- 「はい」: 削除（統合後に不要になる）\n- 「いいえ」: 残す",
            "はい (削除)",
            "いいえ (残す)"
        );

        // 1. 選択されているオブジェクトを取得
        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            Debug.LogError("No target object selected.");
            return;
        }

        Transform targetTransform = targetObject.transform;
        List<MeshFilter> meshFilterList = new List<MeshFilter>();
        MeshFilter targetMeshFilter = targetObject.GetComponent<MeshFilter>();
        if (targetMeshFilter != null && targetMeshFilter.sharedMesh != null)
        {
            meshFilterList.Add(targetMeshFilter);
        }
        meshFilterList.AddRange(targetObject.GetComponentsInChildren<MeshFilter>());

        MeshFilter[] meshFilters = meshFilterList.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Material[] materials = new Material[meshFilters.Length];

        // 2. メッシュを収集し、座標変換
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter.sharedMesh == null) continue;

            Transform meshTransform = meshFilter.transform;
            if (keepRelativeTransform)
            {
                combine[i].mesh = meshFilter.sharedMesh;
                combine[i].transform = targetTransform.worldToLocalMatrix * meshTransform.localToWorldMatrix;
            }
            else
            {
                combine[i].mesh = meshFilter.sharedMesh;
                combine[i].transform = meshTransform.localToWorldMatrix;
            }

            materials[i] = meshFilter.GetComponent<MeshRenderer>()?.sharedMaterial;
        }

        // 3. 新しいメッシュを作成
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine, !keepMaterials, true);

        string path = "Assets/CombinedMesh.asset";
        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();

        MeshFilter newMeshFilter = targetObject.GetComponent<MeshFilter>() ?? targetObject.AddComponent<MeshFilter>();
        newMeshFilter.sharedMesh = combinedMesh;
        MeshRenderer newMeshRenderer = targetObject.GetComponent<MeshRenderer>() ?? targetObject.AddComponent<MeshRenderer>();

        if (keepMaterials)
        {
            newMeshRenderer.sharedMaterials = materials;
        }
        else
        {
            newMeshRenderer.sharedMaterial = materials[0];
        }
        

        // 4. 元のオブジェクトの Mesh & Renderer を削除し、適切に処理
        if (removeOriginalMeshes)
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                GameObject obj = meshFilter.gameObject;
                if (obj == targetObject) continue; // 統合対象オブジェクトはスキップ

                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                DestroyImmediate(renderer);
                DestroyImmediate(meshFilter);

                // 他に Collider しかない場合、Collider を統合オブジェクトへ移動
                Collider[] colliders = obj.GetComponents<Collider>();
                Rigidbody rb = obj.GetComponent<Rigidbody>();

                bool onlyColliders = colliders.Length > 0 && (rb == null || rb.isKinematic);
                bool hasOtherComponents = obj.GetComponents<Component>().Length > colliders.Length + (rb ? 1 : 0) + 1; // Transform も含むので+1

                if (onlyColliders && !hasOtherComponents)
                {
                    foreach (Collider col in colliders)
                    {
                        Collider newCollider = targetObject.AddComponent(col.GetType()) as Collider;
                        if (newCollider != null)
                        {
                            EditorUtility.CopySerialized(col, newCollider);
                        }
                    }
                    DestroyImmediate(obj);
                }
                else if (colliders.Length == 0 && !hasOtherComponents)
                {
                    DestroyImmediate(obj); // 完全に不要なら削除
                }
            }
        }

        PrefabUtility.SaveAsPrefabAsset(targetObject, "Assets/YourPrefab.prefab");

        Debug.Log(
            (keepMaterials ? "Materials preserved." : "Materials merged.") +
            (keepRelativeTransform ? " Relative transform maintained." : " World position applied.") +
            (removeOriginalMeshes ? " Original meshes removed and colliders handled." : " Original meshes kept.")
        );
    }
}
