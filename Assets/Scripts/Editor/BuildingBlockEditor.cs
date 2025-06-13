using System.Collections.Generic;
using UnityEditor; // Editorスクリプトには必須
using UnityEngine;

/// <summary>
/// BuildingBlockの接続をエディタ上で行うためのカスタムエディタスクリプト。
/// </summary>
public class BuildingBlockEditor
{
    // Unityの上部メニューに "Tools/Building Destructor/Connect All Blocks" を追加
    [MenuItem("Tools/Building Destructor/Connect All Blocks")]
    private static void ConnectAllBlocksInScene()
    {
        // シーン内のBuildingBlockをすべて検索
        BuildingBlock[] allBlocks = Object.FindObjectsOfType<BuildingBlock>();
        if (allBlocks.Length == 0)
        {
            Debug.LogWarning("No BuildingBlocks found in the scene.");
            return;
        }

        // まず既存のジョイントをすべて削除して初期化する
        ClearAllJoints(allBlocks);

        // 接続処理（アプローチ1とほぼ同じロジック）
        var blockDictionary = new Dictionary<Rigidbody, BuildingBlock>();
        foreach (var block in allBlocks)
        {
            Rigidbody rb = block.GetComponent<Rigidbody>();
            if (rb != null && !blockDictionary.ContainsKey(rb))
            {
                blockDictionary.Add(rb, block);
            }
        }

        int jointCount = 0;
        foreach (var block in allBlocks)
        {
            Rigidbody currentRb = block.GetComponent<Rigidbody>();
            Collider[] colliders = Physics.OverlapSphere(
                block.transform.position,
                block.connectionRadius
            );

            foreach (var hitCollider in colliders)
            {
                Rigidbody connectedRb = hitCollider.attachedRigidbody;
                if (connectedRb == null || connectedRb == currentRb)
                    continue;

                if (blockDictionary.ContainsKey(connectedRb))
                {
                    bool alreadyConnected = false;
                    foreach (var joint in block.GetComponents<FixedJoint>())
                    {
                        if (joint.connectedBody == connectedRb)
                        {
                            alreadyConnected = true;
                            break;
                        }
                    }

                    // エディタスクリプトでは、相手側からのチェックも念のため入れておくとより安全
                    foreach (var joint in connectedRb.GetComponents<FixedJoint>())
                    {
                        if (joint.connectedBody == currentRb)
                        {
                            alreadyConnected = true;
                            break;
                        }
                    }

                    if (!alreadyConnected)
                    {
                        BuildingBlock otherBlock = blockDictionary[connectedRb];
                        FixedJoint newJoint = block.gameObject.AddComponent<FixedJoint>();
                        newJoint.connectedBody = connectedRb;
                        newJoint.breakForce = Mathf.Min(block.breakForce, otherBlock.breakForce);
                        newJoint.breakTorque = Mathf.Min(block.breakTorque, otherBlock.breakTorque);

                        // 変更をシーンに保存するようマークする
                        EditorUtility.SetDirty(block);
                        jointCount++;
                    }
                }
            }
        }

        Debug.Log(
            $"Successfully connected {allBlocks.Length} blocks with {jointCount} new joints. Remember to save your scene!"
        );
    }

    // 既存のジョイントをクリアするためのメニュー項目
    [MenuItem("Tools/Building Destructor/Clear All Joints")]
    private static void ClearAllJointsMenu()
    {
        BuildingBlock[] allBlocks = Object.FindObjectsOfType<BuildingBlock>();
        ClearAllJoints(allBlocks);
        Debug.Log("Cleared all FixedJoints from BuildingBlocks. Remember to save your scene!");
    }

    private static void ClearAllJoints(BuildingBlock[] blocks)
    {
        foreach (var block in blocks)
        {
            FixedJoint[] joints = block.GetComponents<FixedJoint>();
            foreach (var joint in joints)
            {
                // エディタ上でのコンポーネント削除はDestroyImmediateを使う
                Object.DestroyImmediate(joint);
            }
            EditorUtility.SetDirty(block);
        }
    }
}
