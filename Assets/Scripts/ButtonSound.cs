using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    // ↓↓↓ この2行が非常に重要です！ ↓↓↓
    [Tooltip("このボタンがクリックされたときに再生する効果音")]
    [SerializeField]
    private AudioClip _soundEffect; // ← [SerializeField] がないと表示されません

    // ↑↑↑ ここまで ↑↑↑

    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        SoundManager.Instance.PlaySE(_soundEffect);
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(PlaySound);
        }
    }
}
