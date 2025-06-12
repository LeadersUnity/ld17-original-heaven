using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class GridSlicer : EditorWindow
{
    private GameObject targetObject;
    private Vector3Int gridCounts = new Vector3Int(3, 1, 3);
    private Material intersectionMaterial;
    private bool isSlicing = false;
    private string statusMessage = "";

    /// <summary>
    /// スライス処理中の一時的な破片情報を保持する内部クラス
    /// GameObjectとその正確なワールド境界をセットで管理する
    /// </summary>
    private class SliceablePiece
    {
        public GameObject GameObject { get; }
        public Bounds WorldBounds { get; }

        public SliceablePiece(GameObject gameObject)
        {
            GameObject = gameObject;
            WorldBounds = GetAccurateWorldBounds(gameObject);
        }

        /// <summary>
        /// メッシュの全頂点をワールド座標に変換し、正確なワールド境界を計算する。
        /// これにより、回転したオブジェクトでもタイトなバウンディングボックスが得られる。
        /// </summary>
        private static Bounds GetAccurateWorldBounds(GameObject go)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return new Bounds(go.transform.position, Vector3.zero);

            var mesh = mf.sharedMesh;
            var vertices = mesh.vertices;
            if (vertices.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);

            var matrix = go.transform.localToWorldMatrix;
            var min = matrix.MultiplyPoint3x4(vertices[0]);
            var max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                var p = matrix.MultiplyPoint3x4(vertices[i]);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }
    }

    [MenuItem("Tools/Grid Slicer")]
    public static void ShowWindow()
    {
        GetWindow<GridSlicer>("Grid Slicer");
    }

    void OnGUI()
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

        EditorGUI.BeginDisabledGroup(isSlicing);
        if (GUILayout.Button("Slice into Grid"))
        {
            if (targetObject == null)
            {
                Debug.LogError("Target Object is not assigned.");
                return;
            }
            isSlicing = true;
            statusMessage = "Slicing started...";
            Repaint();
            _ = SliceIntoGridAsync();
        }
        EditorGUI.EndDisabledGroup();

        if (isSlicing)
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    private async Task SliceIntoGridAsync()
    {
        var slicer = new Hanzzz.MeshSlicerFree.MeshSlicer();

        var sourceObject = Instantiate(targetObject);
        sourceObject.SetActive(true);
        targetObject.SetActive(false);

        var container = new GameObject(targetObject.name + "_Sliced_Pieces");
        container.transform.SetParent(targetObject.transform.parent, false);
        container.transform.position = targetObject.transform.position;
        container.transform.rotation = targetObject.transform.rotation;
        container.transform.localScale = targetObject.transform.localScale;

        var initialPiece = new SliceablePiece(sourceObject);
        var pieces = new List<SliceablePiece> { initialPiece };

        var totalBounds = initialPiece.WorldBounds;

        statusMessage = "Slicing along X-axis...";
        Repaint();
        pieces = await SlicePieceListOnAxis(
            slicer,
            pieces,
            container.transform.right,
            totalBounds,
            gridCounts.x
        );

        statusMessage = "Slicing along Y-axis...";
        Repaint();

        // Y軸がワールドのY軸に近すぎる場合、ごくわずかに傾けて数値的な問題を回避する
        Vector3 yAxis = container.transform.up;
        if (Mathf.Abs(Vector3.Dot(yAxis, Vector3.up)) > 0.999f)
        {
            yAxis = (yAxis + new Vector3(0.0001f, 0, 0.0001f)).normalized;
        }
        pieces = await SlicePieceListOnAxis(slicer, pieces, yAxis, totalBounds, gridCounts.y);

        statusMessage = "Slicing along Z-axis...";
        Repaint();
        pieces = await SlicePieceListOnAxis(
            slicer,
            pieces,
            container.transform.forward,
            totalBounds,
            gridCounts.z
        );

        statusMessage = "Finalizing pieces...";
        Repaint();
        foreach (var piece in pieces)
        {
            if (piece.GameObject == null)
                continue;
            var go = piece.GameObject;
            go.transform.SetParent(container.transform, true);

            var oldCollider = go.GetComponent<Collider>();
            if (oldCollider != null)
                DestroyImmediate(oldCollider);

            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.convex = true;

            if (go.GetComponent<Rigidbody>() == null)
                go.AddComponent<Rigidbody>();
        }

        isSlicing = false;
        statusMessage = $"Completed! Generated {pieces.Count} pieces.";
        Repaint();
        Debug.Log(statusMessage);
    }

    /// <summary>
    /// 指定された軸に沿って、ピースのリストを分割する。
    /// </summary>
    private async Task<List<SliceablePiece>> SlicePieceListOnAxis(
        Hanzzz.MeshSlicerFree.MeshSlicer slicer,
        List<SliceablePiece> inputPieces,
        Vector3 worldAxis,
        Bounds totalBounds,
        int finalPieceCount
    )
    {
        if (finalPieceCount <= 1)
            return inputPieces;

        var corners = GetBoundsCorners(totalBounds);

        float minDot = float.MaxValue;
        float maxDot = float.MinValue;
        foreach (var corner in corners)
        {
            float dot = Vector3.Dot(corner, worldAxis);
            minDot = Mathf.Min(minDot, dot);
            maxDot = Mathf.Max(maxDot, dot);
        }

        float sliceWidth = (maxDot - minDot) / finalPieceCount;
        var currentPieces = new List<SliceablePiece>(inputPieces);

        for (int i = 1; i < finalPieceCount; i++)
        {
            float sliceDot = minDot + sliceWidth * i;
            var plane = new Plane(worldAxis, sliceDot);
            var slicePlane = CreateSlicePlaneFromPlane(plane);

            var nextPieces = new List<SliceablePiece>();
            foreach (var piece in currentPieces)
            {
                if (!DoesBoundsIntersectPlane(piece.WorldBounds, plane))
                {
                    nextPieces.Add(piece);
                    continue;
                }

                var result = await slicer.SliceAsync(
                    piece.GameObject,
                    slicePlane,
                    intersectionMaterial
                );

                if (result.Item1 != null && result.Item2 != null)
                {
                    nextPieces.Add(new SliceablePiece(result.Item1));
                    nextPieces.Add(new SliceablePiece(result.Item2));
                }
                else
                {
                    nextPieces.Add(piece);
                }
            }
            currentPieces = nextPieces;
        }
        return currentPieces;
    }

    #region Helper Methods

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        var corners = new Vector3[8];
        var min = bounds.min;
        var max = bounds.max;
        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(min.x, min.y, max.z);
        corners[4] = new Vector3(max.x, max.y, min.z);
        corners[5] = new Vector3(min.x, max.y, max.z);
        corners[6] = new Vector3(max.x, min.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);
        return corners;
    }

    private (Vector3, Vector3, Vector3) CreateSlicePlaneFromPlane(Plane plane)
    {
        Vector3 p1 = -plane.normal * plane.distance;
        Vector3 refVector =
            (Mathf.Abs(Vector3.Dot(plane.normal, Vector3.up)) > 0.95f) ? Vector3.right : Vector3.up;
        Vector3 p2 = p1 + Vector3.Cross(plane.normal, refVector).normalized;
        Vector3 p3 = p1 + Vector3.Cross(plane.normal, p2 - p1).normalized;
        return (p1, p2, p3);
    }

    private bool DoesBoundsIntersectPlane(Bounds bounds, Plane plane)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        float r =
            extents.x * Mathf.Abs(plane.normal.x)
            + extents.y * Mathf.Abs(plane.normal.y)
            + extents.z * Mathf.Abs(plane.normal.z);
        float s = plane.GetDistanceToPoint(center);
        return Mathf.Abs(s) <= r;
    }

    #endregion
}
