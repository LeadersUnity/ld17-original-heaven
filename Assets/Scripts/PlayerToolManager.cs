using UnityEngine;
using UnityEngine.UI;

public class PlayerToolManager : MonoBehaviour
{
    public static PlayerToolManager Instance { get; private set; }

    public enum ToolMode
    {
        Bomb,
        Force,
        Annihilator,
    }

    [Header("現在のモード")]
    [SerializeField]
    private ToolMode currentMode = ToolMode.Bomb;

    [Header("モード別コントローラー")]
    [SerializeField]
    private BombPlacementController bombController;

    [SerializeField]
    private ForceApplicatorController forceController;

    [SerializeField]
    private AnnihilatorPlacementController annihilatorController;

    [Header("UIボタン")]
    [SerializeField]
    private Button bombModeButton;

    [SerializeField]
    private Button forceModeButton;

    [SerializeField]
    private Button annihilatorModeButton;

    [SerializeField]
    private Button activateAllButton;

    [Header("ボタンの色")]
    [SerializeField]
    private Color activeColor = Color.green;

    [SerializeField]
    private Color inactiveColor = Color.white;

    void Awake()
    {
        // シングルトンパターンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (bombController == null || forceController == null || annihilatorController == null)
        {
            Debug.LogError(
                "モードコントローラーがインスペクターで設定されていません！",
                gameObject
            );
        }
    }

    void Start()
    {
        bombModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Bomb));
        forceModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Force));
        annihilatorModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Annihilator));
        activateAllButton?.onClick.AddListener(ActivateCurrentTool);

        SwitchMode(currentMode);
    }

    void OnDestroy()
    {
        bombModeButton?.onClick.RemoveAllListeners();
        forceModeButton?.onClick.RemoveAllListeners();
        annihilatorModeButton?.onClick.RemoveAllListeners();
        activateAllButton?.onClick.RemoveAllListeners();
    }

    // 他のスクリプトが現在のモードを確認できるように公開
    public ToolMode GetCurrentMode()
    {
        return currentMode;
    }

    private void OnModeButtonPressed(ToolMode requestedMode)
    {
        if (currentMode == requestedMode)
        {
            switch (currentMode)
            {
                case ToolMode.Bomb:
                    bombController.SpawnNew();
                    break;
                case ToolMode.Annihilator:
                    annihilatorController.SpawnNew();
                    break;
            }
        }
        else
        {
            SwitchMode(requestedMode);
        }
    }

    private void SwitchMode(ToolMode newMode)
    {
        currentMode = newMode;

        // 各コントローラーの有効/無効を切り替える
        bombController.enabled = (newMode == ToolMode.Bomb);
        forceController.enabled = (newMode == ToolMode.Force);
        annihilatorController.enabled = (newMode == ToolMode.Annihilator);

        UpdateVisuals();
    }

    private void ActivateCurrentTool()
    {
        switch (currentMode)
        {
            case ToolMode.Bomb:
                bombController.ExplodeAllPlacedBombs();
                break;
            case ToolMode.Annihilator:
                annihilatorController.ActivateAll();
                break;
        }
    }

    private void UpdateVisuals()
    {
        if (bombModeButton == null || forceModeButton == null || annihilatorModeButton == null)
            return;

        bombModeButton.GetComponent<Image>().color =
            (currentMode == ToolMode.Bomb) ? activeColor : inactiveColor;
        forceModeButton.GetComponent<Image>().color =
            (currentMode == ToolMode.Force) ? activeColor : inactiveColor;
        annihilatorModeButton.GetComponent<Image>().color =
            (currentMode == ToolMode.Annihilator) ? activeColor : inactiveColor;

        activateAllButton.gameObject.SetActive(
            currentMode == ToolMode.Bomb || currentMode == ToolMode.Annihilator
        );
    }
}
