using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // 非同期処理のために追加
// 正しい名前空間を指定
using Hanzzz.MeshSlicerFree;
using UnityEditor;
using UnityEngine;

public class GridSlicer : EditorWindow
{
    private GameObject targetObject;
    private Vector3Int gridCounts = new Vector3Int(3, 10, 3);
    private Material intersectionMaterial;
    private bool isSlicing = false;

    [MenuItem("Tools/Grid Slicer")]
    public static void ShowWindow()
    {
        GetWindow<GridSlicer>("Grid Slicer");
    }

    async void OnGUI()
    {
        GUILayout.Label("Grid Slicing Settings", EditorStyles.boldLabel);

        targetObject = (GameObject)
            EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
        gridCounts = EditorGUILayout.Vector3IntField("Grid Counts (X, Y, Z)", gridCounts);
        intersectionMaterial = (Material)
            EditorGUILayout.ObjectField(
                "Intersection Material",
                intersectionMaterial,
                typeof(Material),
                false
            );

        // 処理中はボタンを無効化
        EditorGUI.BeginDisabledGroup(isSlicing);
        if (GUILayout.Button("Slice into Grid"))
        {
            if (targetObject == null)
            {
                Debug.LogError("Target Object is not assigned.");
                return;
            }

            // 非同期でスライス処理を実行
            isSlicing = true;
            await SliceIntoGridAsync();
            isSlicing = false;
            Repaint(); // ウィンドウを再描画してボタンを有効に戻す
        }
        EditorGUI.EndDisabledGroup();

        if (isSlicing)
        {
            EditorGUILayout.HelpBox("Slicing in progress... Please wait.", MessageType.Info);
        }
    }

    private async Task SliceIntoGridAsync()
    {
        MeshSlicer meshSlicer = new MeshSlicer();

        // 元オブジェクトを非表示にし、複製を処理対象にする
        targetObject.SetActive(false);
        GameObject processingObject = Instantiate(targetObject);

        var initialBounds = processingObject.GetComponent<MeshRenderer>().bounds;

        // 最初の破片リスト
        List<GameObject> pieces = new List<GameObject> { processingObject };

        // 各軸で順番にスライス処理
        pieces = await SliceAxis(meshSlicer, pieces, Vector3.right, gridCounts.x, initialBounds);
        pieces = await SliceAxis(meshSlicer, pieces, Vector3.up, gridCounts.y, initialBounds);
        pieces = await SliceAxis(meshSlicer, pieces, Vector3.forward, gridCounts.z, initialBounds);

        // 最終的に生成された破片をコンテナにまとめる
        GameObject container = new GameObject(targetObject.name + "_Sliced_Container");
        foreach (var piece in pieces)
        {
            piece.transform.SetParent(container.transform);

            // 物理演算のためにコンポーネントを追加
            if (piece.GetComponent<Rigidbody>() == null)
            {
                piece.AddComponent<Rigidbody>();
            }

            var meshCollider = piece.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = piece.AddComponent<MeshCollider>();
            }
            // ★★★ここでConvexを有効化し、警告を回避します★★★
            meshCollider.convex = true;
        }

        Debug.Log($"Slicing complete! Generated {pieces.Count} pieces.");
    }

    private async Task<List<GameObject>> SliceAxis(
        MeshSlicer slicer,
        List<GameObject> inputPieces,
        Vector3 axis,
        int cuts,
        Bounds initialBounds
    )
    {
        if (cuts <= 1)
            return inputPieces;

        List<GameObject> outputPieces = new List<GameObject>();
        float totalSize = Vector3.Dot(initialBounds.size, axis);
        float sliceWidth = totalSize / cuts;

        foreach (var piece in inputPieces)
        {
            var currentPieceForSlicing = new List<GameObject> { piece };
            for (int i = 1; i < cuts; i++)
            {
                var nextPieces = new List<GameObject>();
                foreach (var currentPiece in currentPieceForSlicing)
                {
                    Vector3 slicePoint = initialBounds.min + axis * (sliceWidth * i);
                    Vector3 p1 = slicePoint;
                    Vector3 p2 = slicePoint + (axis == Vector3.up ? Vector3.right : Vector3.up);
                    Vector3 p3 =
                        slicePoint
                        + (
                            axis == Vector3.right
                                ? Vector3.forward
                                : (axis == Vector3.forward ? Vector3.right : Vector3.forward)
                        );
                    var slicePlane = (p1, p2, p3);

                    (GameObject, GameObject) result = await slicer.SliceAsync(
                        currentPiece,
                        slicePlane,
                        intersectionMaterial
                    );

                    if (result != (null, null))
                    {
                        nextPieces.Add(result.Item1);
                        nextPieces.Add(result.Item2);
                        DestroyImmediate(currentPiece);
                    }
                    else
                    {
                        nextPieces.Add(currentPiece);
                    }
                }
                currentPieceForSlicing = nextPieces;
            }
            outputPieces.AddRange(currentPieceForSlicing);
        }

        return outputPieces;
    }
}
