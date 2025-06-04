using System.Collections;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("爆風に当たったときに吹っ飛ぶ力の強さ")]
    [SerializeField]
    private float explosivePower = 500f; // デフォルト値を明示するのも良い

    [Header("爆風の判定が実際に発生するまでの遅延時間")]
    [SerializeField]
    private float bombDelaySeconds = 0.1f;

    [Header("爆風の持続フレーム数")]
    [SerializeField]
    private int durationFrameCount = 1;

    [Header("エフェクト含めすべての再生が終了するまでの時間")]
    [SerializeField]
    private float stopSeconds = 2f;

    [SerializeField]
    private ParticleSystem effect;

    [SerializeField]
    private AudioSource sfx;

    [SerializeField]
    private SphereCollider bombCollider;

    private void Awake()
    {
        if (effect != null)
            effect.Stop();
        if (sfx != null)
            sfx.Stop();
        if (bombCollider != null)
            bombCollider.enabled = false;
    }

    public void Explode()
    {
        StartCoroutine(ExplodeCoroutine());
        StartCoroutine(StopCoroutine());

        if (effect != null)
            effect.Play();
        if (sfx != null)
            sfx.Play();
    }

    private IEnumerator ExplodeCoroutine()
    {
        float delay = Mathf.Max(0, bombDelaySeconds);
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay); // FixedUpdateベースからSecondsへ変更も検討可
        }

        if (bombCollider != null)
            bombCollider.enabled = true;

        for (var i = 0; i < durationFrameCount; i++)
        {
            yield return new WaitForFixedUpdate(); // 物理演算に影響を与えるためFixedUpdateが適切
        }

        if (bombCollider != null)
            bombCollider.enabled = false;
    }

    private IEnumerator StopCoroutine()
    {
        yield return new WaitForSeconds(stopSeconds);

        if (effect != null)
            effect.Stop();
        if (sfx != null)
            sfx.Stop();
        if (bombCollider != null)
            bombCollider.enabled = false;

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidBody = other.GetComponentInParent<Rigidbody>(); // GetComponentInParentが良い場合もある

        if (rigidBody == null)
            return;

        Vector3 direction = (other.transform.position - transform.position).normalized;
        rigidBody.AddForce(direction * explosivePower, ForceMode.Impulse);
    }
}
