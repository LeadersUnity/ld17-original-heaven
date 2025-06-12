using UnityEditor;
using UnityEngine;

public class ObjectSplitter : EditorWindow
{
    private GameObject targetObject;
    private int xDivisions = 2;
    private int yDivisions = 2;
    private int zDivisions = 2;

    // Unityエディタの "Edit" メニューに "Split Selected Object" を追加
    [MenuItem("Edit/Split Selected Object")]
    public static void ShowWindow()
    {
        // ウィンドウを表示
        GetWindow<ObjectSplitter>("Object Splitter");
    }

    // ウィンドウのGUIを描画
    void OnGUI()
    {
        GUILayout.Label("Object Splitting Settings", EditorStyles.boldLabel);

        // 分割したいオブジェクトを選択するフィールド
        targetObject = (GameObject)
            EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        // 分割数を入力するフィールド
        xDivisions = EditorGUILayout.IntField("X Divisions", xDivisions);
        yDivisions = EditorGUILayout.IntField("Y Divisions", yDivisions);
        zDivisions = EditorGUILayout.IntField("Z Divisions", zDivisions);

        // 分割数が1未満にならないように制御
        xDivisions = Mathf.Max(1, xDivisions);
        yDivisions = Mathf.Max(1, yDivisions);
        zDivisions = Mathf.Max(1, zDivisions);

        // 分割実行ボタン
        if (GUILayout.Button("Split Object"))
        {
            if (targetObject == null)
            {
                Debug.LogError("ターゲットとなるオブジェクトが選択されていません。");
                return;
            }

            Split();
        }
    }

    // オブジェクトが選択されたときにウィンドウのtargetObjectを更新
    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            targetObject = Selection.activeGameObject;
            Repaint(); // ウィンドウを再描画
        }
    }

    private void Split()
    {
        // 元オブジェクトのメッシュレンダラーとバウンディングボックスを取得
        MeshRenderer originalRenderer = targetObject.GetComponentInChildren<MeshRenderer>();
        if (originalRenderer == null)
        {
            Debug.LogError("ターゲットオブジェクトにMeshRendererが見つかりません。");
            return;
        }
        Bounds bounds = originalRenderer.bounds;

        // 分割後の1つあたりのサイズを計算
        Vector3 pieceSize = new Vector3(
            bounds.size.x / xDivisions,
            bounds.size.y / yDivisions,
            bounds.size.z / zDivisions
        );

        // 分割後のオブジェクトを格納する親オブジェクトを作成
        GameObject parentObject = new GameObject(targetObject.name + "_Pieces");
        parentObject.transform.position = targetObject.transform.position;
        parentObject.transform.rotation = targetObject.transform.rotation;
        parentObject.transform.localScale = targetObject.transform.localScale;

        // グリッド状にキューブを生成
        for (int x = 0; x < xDivisions; x++)
        {
            for (int y = 0; y < yDivisions; y++)
            {
                for (int z = 0; z < zDivisions; z++)
                {
                    // プリミティブのキューブを生成
                    GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    // 親オブジェクトの子にする
                    piece.transform.SetParent(parentObject.transform);

                    // 位置を計算して設定
                    float newX = bounds.min.x + pieceSize.x * (x + 0.5f);
                    float newY = bounds.min.y + pieceSize.y * (y + 0.5f);
                    float newZ = bounds.min.z + pieceSize.z * (z + 0.5f);
                    piece.transform.position = new Vector3(newX, newY, newZ);

                    // ローカルスケールをリセットし、サイズを設定
                    piece.transform.localScale = Vector3.one; // いったんリセット
                    piece.transform.localScale = new Vector3(
                        pieceSize.x / targetObject.transform.localScale.x,
                        pieceSize.y / targetObject.transform.localScale.y,
                        pieceSize.z / targetObject.transform.localScale.z
                    );

                    // 元オブジェクトのマテリアルを適用
                    piece.GetComponent<MeshRenderer>().material = originalRenderer.sharedMaterial;

                    // 物理演算のためにRigidbodyを追加
                    piece.AddComponent<Rigidbody>();
                }
            }
        }

        // 元のオブジェクトを非表示にする
        targetObject.SetActive(false);

        Debug.Log(
            targetObject.name
                + " を "
                + (xDivisions * yDivisions * zDivisions)
                + " 個のピースに分割しました。"
        );
    }
}
