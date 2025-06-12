// ファイル名: CubeGeneratorEditor.cs
// プロジェクトの "Editor" フォルダ内に配置してください。

using UnityEditor;
using UnityEngine;

public class CubeGeneratorEditor : EditorWindow
{
    private GameObject targetObject; // 分割の元となるオブジェクト
    private Vector3Int divisions = new Vector3Int(10, 20, 10); // X, Y, Z軸の分割数
    private GameObject cubePrefab; // 生成するキューブのプレハブ

    // メニューに項目を追加
    [MenuItem("Tools/Generate Cubes From Selection")]
    public static void ShowWindow()
    {
        GetWindow<CubeGeneratorEditor>("Cube Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cube Generation Settings", EditorStyles.boldLabel);

        // UIで設定項目を表示
        targetObject = (GameObject)
            EditorGUILayout.ObjectField(
                "Target Object",
                Selection.activeGameObject,
                typeof(GameObject),
                true
            );
        divisions = EditorGUILayout.Vector3IntField("Divisions (X, Y, Z)", divisions);
        cubePrefab = (GameObject)
            EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false);

        // ボタンが押されたら生成処理を実行
        if (GUILayout.Button("Generate"))
        {
            if (targetObject == null)
            {
                Debug.LogError("Target Object is not selected.");
                return;
            }
            if (cubePrefab == null)
            {
                Debug.LogError("Cube Prefab is not assigned.");
                return;
            }

            GenerateCubes();
        }
    }

    private void GenerateCubes()
    {
        // 元のオブジェクトのメッシュレンダラーとバウンディングボックスを取得
        MeshRenderer originalRenderer = targetObject.GetComponentInChildren<MeshRenderer>();
        if (originalRenderer == null)
        {
            Debug.LogError("Target object does not have a MeshRenderer component.");
            return;
        }
        Bounds bounds = originalRenderer.bounds;

        // キューブ1つあたりのサイズを計算
        Vector3 cubeSize = new Vector3(
            bounds.size.x / divisions.x,
            bounds.size.y / divisions.y,
            bounds.size.z / divisions.z
        );

        // 生成したキューブをまとめるための親オブジェクトを作成
        GameObject container = new GameObject(targetObject.name + "_Fractured");
        container.transform.position = targetObject.transform.position;
        container.transform.rotation = targetObject.transform.rotation;
        container.transform.localScale = targetObject.transform.localScale;

        // バウンディングボックス内をループしてキューブを生成
        for (int x = 0; x < divisions.x; x++)
        {
            for (int y = 0; y < divisions.y; y++)
            {
                for (int z = 0; z < divisions.z; z++)
                {
                    // キューブの生成位置を計算
                    Vector3 position =
                        bounds.min
                        + new Vector3(
                            cubeSize.x * (x + 0.5f),
                            cubeSize.y * (y + 0.5f),
                            cubeSize.z * (z + 0.5f)
                        );

                    // プレハブからキューブをインスタンス化
                    GameObject cube = (GameObject)
                        PrefabUtility.InstantiatePrefab(cubePrefab, container.transform);
                    cube.transform.position = position;
                    cube.transform.localScale = cubeSize;

                    // 物理演算のためにコンポーネントを追加
                    if (cube.GetComponent<Rigidbody>() == null)
                    {
                        cube.AddComponent<Rigidbody>();
                    }
                    if (cube.GetComponent<MeshCollider>() == null)
                    {
                        var collider = cube.AddComponent<MeshCollider>();
                        collider.convex = true; // Rigidbodyと衝突するためにはConvexを有効にする
                    }
                }
            }
        }

        // 元のオブジェクトを非表示にする
        targetObject.SetActive(false);

        Debug.Log("Cube generation complete!");
    }
}
