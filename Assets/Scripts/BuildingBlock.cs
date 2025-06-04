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

    private bool hasBeenScored = false;
    private bool hasBeenCountedForTask1 = false;

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

        // GameManagerのInitializeGameでブロック数をカウントするため、ここでの個別登録は不要。
        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.RegisterBlock(); // この行を削除またはコメントアウト
        // }
        // else
        // {
        //     Debug.LogError("GameManager instance not found. Cannot register block.", gameObject);
        // }
        if (GameManager.Instance == null) // GameManagerの存在チェックは残しても良い
        {
            Debug.LogError(
                "GameManager instance not found during BuildingBlock.Start(). Game logic might be affected.",
                gameObject
            );
        }
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
        {
            if (!hasBeenCountedForTask1 && GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBlockDestroyed();
                hasBeenCountedForTask1 = true;
            }
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        if (!hasBeenScored)
        {
            if (contactTag == "Ground")
            {
                GameManager.Instance.AddScore(100);
                hasBeenScored = true;
            }
            else if (contactTag == "Road")
            {
                GameManager.Instance.AddScore(-50);
                hasBeenScored = true;
            }
        }

        if (!hasBeenCountedForTask1 && (contactTag == "Ground" || contactTag == "Road"))
        {
            GameManager.Instance.NotifyBlockDestroyed();
            hasBeenCountedForTask1 = true;
        }

        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }
    }

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

        blockRenderer.material = (breakForce >= hardnessThreshold) ? hardMaterial : softMaterial;
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
            foreach (FixedJoint joint in GetComponents<FixedJoint>())
            {
                if (joint.connectedBody == connectedRb)
                {
                    alreadyConnected = true;
                    break;
                }
            }
            if (alreadyConnected)
                continue;

            foreach (FixedJoint joint in connectedRb.GetComponents<FixedJoint>())
            {
                if (joint.connectedBody == rb)
                {
                    alreadyConnected = true;
                    break;
                }
            }
            if (alreadyConnected)
                continue;

            FixedJoint newJoint = gameObject.AddComponent<FixedJoint>();
            newJoint.connectedBody = connectedRb;

            BuildingBlock otherBlock = connectedRb.GetComponent<BuildingBlock>();
            if (otherBlock != null)
            {
                newJoint.breakForce = Mathf.Min(this.breakForce, otherBlock.breakForce);
                newJoint.breakTorque = Mathf.Min(this.breakTorque, otherBlock.breakTorque);
            }
            else
            {
                newJoint.breakForce = this.breakForce;
                newJoint.breakTorque = this.breakTorque;
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
