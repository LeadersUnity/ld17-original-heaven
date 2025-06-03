using System.Collections.Generic;
using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    [Header("接着設定")]
    public float connectionRadius = 1.1f;
    public float breakForce = 1000f;
    public float breakTorque = 1000f;

    [Header("マテリアル設定")]
    public Material hardMaterial;
    public Material softMaterial;
    public float hardnessThreshold = 1500f;

    [Header("落下設定")]
    public float fallLimitY = -100f;

    [Header("デバッグ")]
    public bool showGizmos = true;

    private Rigidbody rb;
    private Renderer blockRenderer;
    private static HashSet<Rigidbody> processedRigidbodies = new HashSet<Rigidbody>();

    // --- ここから追加・変更 ---
    private bool hasBeenScored = false; // このブロックがスコア処理されたか
    private bool hasBeenCountedForTask1 = false; // このブロックがタスク1の破壊としてカウントされたか

    // --- ここまで追加・変更 ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        blockRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        if (rb == null)
            return;

        SetMaterialBasedOnHardness();

        if (processedRigidbodies.Contains(rb))
        {
            return;
        }
        ConnectToNeighbors();
        processedRigidbodies.Add(rb);

        // --- ここから追加 ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterBlock();
        }
        else
        {
            Debug.LogError("GameManager instance not found. Cannot register block.");
        }
        // --- ここまで追加 ---
    }

    void Update()
    {
        // Y座標がfallLimitYを下回ったら破壊としてカウントし、オブジェクトを破棄
        if (transform.position.y < fallLimitY)
        {
            // --- ここから追加・変更 ---
            if (!hasBeenCountedForTask1 && GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBlockDestroyed();
                hasBeenCountedForTask1 = true;
            }
            // スコアは地面/Roadへの接触時のみなので、ここではスコア処理しない
            Destroy(gameObject);
            // --- ここまで追加・変更 ---
        }
    }

    // --- ここから追加 ---
    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        // スコア処理 (一度だけ)
        if (!hasBeenScored)
        {
            if (contactTag == "Ground") // 「基本の地面」のタグを "Ground" と想定
            {
                GameManager.Instance.AddScore(100);
                hasBeenScored = true;
            }
            else if (contactTag == "Road") // 「Roadタグがついた道路」のタグは "Road"
            {
                GameManager.Instance.AddScore(-50);
                hasBeenScored = true;
            }
        }

        // タスク1: 破壊カウント (地面またはRoadに接触したら破壊とみなす。一度だけカウント)
        if (!hasBeenCountedForTask1 && (contactTag == "Ground" || contactTag == "Road"))
        {
            GameManager.Instance.NotifyBlockDestroyed();
            hasBeenCountedForTask1 = true;
        }

        // タスク2: Roadに接触したら失敗通知
        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }
    }

    // --- ここまで追加 ---

    void SetMaterialBasedOnHardness()
    {
        if (blockRenderer == null || hardMaterial == null || softMaterial == null)
        {
            if (blockRenderer != null && (hardMaterial == null || softMaterial == null))
            {
                Debug.LogWarning(
                    "hardMaterialまたはsoftMaterialが設定されていません。",
                    gameObject
                );
            }
            return;
        }

        if (breakForce >= hardnessThreshold)
        {
            blockRenderer.material = hardMaterial;
        }
        else
        {
            blockRenderer.material = softMaterial;
        }
    }

    void ConnectToNeighbors()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, connectionRadius);

        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider.gameObject == gameObject)
                continue;

            Rigidbody connectedRb = hitCollider.attachedRigidbody;

            if (connectedRb == null || processedRigidbodies.Contains(connectedRb))
                continue;

            bool alreadyConnected = false;
            FixedJoint[] existingJoints = GetComponents<FixedJoint>();
            foreach (FixedJoint joint in existingJoints)
            {
                if (joint.connectedBody == connectedRb)
                {
                    alreadyConnected = true;
                    break;
                }
            }

            if (!alreadyConnected)
            {
                FixedJoint[] otherExistingJoints = connectedRb.GetComponents<FixedJoint>();
                foreach (FixedJoint joint in otherExistingJoints)
                {
                    if (joint.connectedBody == rb)
                    {
                        alreadyConnected = true;
                        break;
                    }
                }
            }

            if (!alreadyConnected)
            {
                FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = connectedRb;

                BuildingBlock otherBlock = connectedRb.GetComponent<BuildingBlock>();
                if (otherBlock != null)
                {
                    joint.breakForce = Mathf.Min(this.breakForce, otherBlock.breakForce);
                    joint.breakTorque = Mathf.Min(this.breakTorque, otherBlock.breakTorque);
                }
                else
                {
                    joint.breakForce = this.breakForce;
                    joint.breakTorque = this.breakTorque;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (rb != null)
        {
            processedRigidbodies.Remove(rb);
        }
    }

    static BuildingBlock()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (
            state == UnityEditor.PlayModeStateChange.ExitingPlayMode
            || state == UnityEditor.PlayModeStateChange.ExitingEditMode
        )
        {
            processedRigidbodies.Clear();
        }
    }
#endif

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, connectionRadius);
        }
    }
}
