using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("爆風に当たったときに吹っ飛ぶ力の強さ")]
    [SerializeField]
    private float explosivePower;

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
        var delayCount = Mathf.Max(0, bombDelaySeconds);
        while (delayCount > 0)
        {
            yield return new WaitForFixedUpdate();
            delayCount -= Time.fixedDeltaTime;
        }

        if (bombCollider != null)
            bombCollider.enabled = true;

        for (var i = 0; i < durationFrameCount; i++)
        {
            yield return new WaitForFixedUpdate();
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
        var rigidBody = other.GetComponentInParent<Rigidbody>();

        if (rigidBody == null)
            return;

        var direction = (other.transform.position - transform.position).normalized;

        rigidBody.AddForce(direction * explosivePower, ForceMode.Impulse);
    }
}
