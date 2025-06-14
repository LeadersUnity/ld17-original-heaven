using UnityEngine;
using UnityEngine.EventSystems;

public class ForceApplicatorController : MonoBehaviour
{
    [Header("フォース設定")]
    [SerializeField]
    private float forceMagnitude = 500f;

    [Header("エフェクト設定")]
    [SerializeField]
    private ParticleSystem forceEffectPrefab;

    [Header("サウンド設定")]
    [SerializeField]
    private AudioClip forceSound;

    private Camera mainCamera;
    private ParticleSystem currentForceEffectInstance;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 【重要】現在のモードがForceモードでなければ、何も処理しない
        if (
            PlayerToolManager.Instance == null
            || PlayerToolManager.Instance.GetCurrentMode() != PlayerToolManager.ToolMode.Force
        )
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ApplyForceAndStartEffect();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopForceEffect();
        }
    }

    private void ApplyForceAndStartEffect()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyForceUsed();
            }

            if (SoundManager.Instance != null && forceSound != null)
            {
                SoundManager.Instance.PlaySE(forceSound);
            }

            Rigidbody targetRb = hit.collider.GetComponentInParent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.AddForceAtPosition(
                    ray.direction * forceMagnitude,
                    hit.point,
                    ForceMode.Impulse
                );
            }

            if (forceEffectPrefab != null)
            {
                StopForceEffect();
                currentForceEffectInstance = Instantiate(
                    forceEffectPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
                currentForceEffectInstance.Play();
            }
        }
    }

    private void StopForceEffect()
    {
        if (currentForceEffectInstance != null)
        {
            currentForceEffectInstance.Stop();
            Destroy(currentForceEffectInstance.gameObject, 2f);
            currentForceEffectInstance = null;
        }
    }
}
