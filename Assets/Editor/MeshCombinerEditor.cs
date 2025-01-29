using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MeshCombinerEditor : EditorWindow
{
    private bool keepSubMeshesSeparate = true;
    private bool keepRelativeTransform = true;
    private bool removeOriginalMeshes = false;

    [MenuItem("Tools/Combine Meshes")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombinerEditor>("Mesh Combiner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Combiner Settings", EditorStyles.boldLabel);

        keepSubMeshesSeparate = EditorGUILayout.Toggle("マテリアルの保持/マテリアル毎にサブメッシュを統合", keepSubMeshesSeparate);
        keepRelativeTransform = EditorGUILayout.Toggle("選択オブジェクトの相対位置を保持", keepRelativeTransform);
        removeOriginalMeshes = EditorGUILayout.Toggle("元のメッシュを削除", removeOriginalMeshes);
        EditorGUILayout.Toggle("ダミーわんわん", true);

        if (GUILayout.Button("Combine Meshes"))
        {
            CombineMeshes();
        }
    }
    private void CombineMeshes()
    {
        // 1. 選択されているオブジェクトを取得
        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            Debug.LogError("No target object selected.");
            return;
        }

        Transform targetTransform = targetObject.transform;
        List<MeshFilter> meshFilterList = new List<MeshFilter>();
        
        meshFilterList.AddRange(targetObject.GetComponentsInChildren<MeshFilter>());

        MeshFilter[] meshFilters = meshFilterList.ToArray();
        List<CombineInstance> combines = new List<CombineInstance>();
        List<Material> materials = new List<Material>();    // マテリアルのリスト。マテリアル毎にサブメッシュを統合する設定なら一意にする
        Dictionary<Material, List<int>> materialToSubmeshIndices = new Dictionary<Material, List<int>>();   // マテリアル毎のサブメッシュ統合用　マテリアルごとに紐づくサブメッシュのリスト


        // 2. メッシュを収集し、座標変換
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter.sharedMesh == null) continue;

            MeshRenderer meshRenderer = null;
            if(meshFilter.TryGetComponent<MeshRenderer>(out meshRenderer))
            {
                // マテリアル登録
                Material[] objMaterials = meshRenderer.sharedMaterials;

                Mesh mesh = meshFilter.sharedMesh;
                for (int j = 0; j < mesh.subMeshCount; j++)
                {
                    Material mat = objMaterials[j];

                    // 既存のマテリアルリストにこのマテリアルがあるか確認
                    if (!materials.Contains(mat) || keepSubMeshesSeparate)
                    {
                        materials.Add(mat);
                        materialToSubmeshIndices[mat] = new List<int>();
                    }

                    Transform meshTransform = meshFilter.transform;
                    CombineInstance combine = default;
                    if (keepRelativeTransform)
                    {
                        // CombineInstanceを作成
                        combine = new CombineInstance
                        {
                            mesh = mesh,
                            subMeshIndex = j,
                            transform = targetTransform.worldToLocalMatrix * meshTransform.localToWorldMatrix
                        };
                    }
                    else
                    {
                        // CombineInstanceを作成
                        combine = new CombineInstance
                        {
                            mesh = mesh,
                            subMeshIndex = j,
                            transform = meshTransform.localToWorldMatrix
                        };
                    }

                    combines.Add(combine);
                    materialToSubmeshIndices[mat].Add(combines.Count - 1);
                }
            }
        }

        // 3. 新しいメッシュを作成
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combines.ToArray(), false, true);   // このクラスの核。メッシュ結合
        // // 結合されたメッシュの最適化？
        // combinedMesh.RecalculateNormals();  // 法線の再計算
        // combinedMesh.RecalculateBounds();   // 境界ボックスの再計算

        // --- ここからサブメッシュ統合処理 ---
        if (!keepSubMeshesSeparate) // 統合処理を適用する場合
        {
            List<int[]> newSubmeshTriangles = new List<int[]>();

            foreach (var kvp in materialToSubmeshIndices)
            {
                List<int> mergedTriangles = new List<int>();

                foreach (int index in kvp.Value)
                {
                    mergedTriangles.AddRange(combinedMesh.GetTriangles(index));
                }

                newSubmeshTriangles.Add(mergedTriangles.ToArray());
            }

            // 新しいサブメッシュ情報を設定
            combinedMesh.subMeshCount = newSubmeshTriangles.Count;
            for (int i = 0; i < newSubmeshTriangles.Count; i++)
            {
                combinedMesh.SetTriangles(newSubmeshTriangles[i], i);
            }

            // マテリアルリストを更新
            materials = new List<Material>(materialToSubmeshIndices.Keys);
        }

        string path = "Assets/CombinedMesh.asset";
        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();

        MeshFilter newMeshFilter = null;
        if( !targetObject.TryGetComponent<MeshFilter>(out newMeshFilter) )
        {
            newMeshFilter = targetObject.AddComponent<MeshFilter>();
        }
            
        newMeshFilter.sharedMesh = combinedMesh;

        MeshRenderer newMeshRenderer = null;
        if( !targetObject.TryGetComponent<MeshRenderer>(out newMeshRenderer) )
        {
            newMeshRenderer = targetObject.AddComponent<MeshRenderer>();
        }

        newMeshRenderer.sharedMaterials = materials.ToArray();

        

        // 4. 元のオブジェクトの Mesh & Renderer を削除し、適切に処理
        if (removeOriginalMeshes)
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if(meshFilter == null) continue;
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

            // 空っぽになったゲームオブジェクトがあれば消す
            Transform[] childlen = targetObject.GetComponentsInChildren<Transform>();
            foreach (Transform child in childlen)
            {
                if( child == null || !child.gameObject ) continue;
                if(child == targetObject.transform) continue;
                GameObject obj = child.gameObject;

                if(obj.GetComponents<Component>().Length <= 1)
                    DestroyImmediate(obj);
            }
        }

        // PrefabUtility.SaveAsPrefabAsset(targetObject, "Assets/YourPrefab.prefab");

        Debug.Log(
            (keepSubMeshesSeparate ? "Materials preserved." : "Materials merged.") +
            (keepRelativeTransform ? " Relative transform maintained." : " World position applied.") +
            (removeOriginalMeshes ? " Original meshes removed and colliders handled." : " Original meshes kept.")
        );
    }
}