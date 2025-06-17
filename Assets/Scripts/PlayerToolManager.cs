// PlayerToolManager.cs
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

    // --- ▼▼▼ ボタンの変数をpublicに変更 ▼▼▼ ---
    [Header("UIボタン")]
    public Button bombModeButton;
    public Button forceModeButton;
    public Button annihilatorModeButton;
    public Button activateAllButton;

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    [Header("ボタンの色")]
    [SerializeField]
    private Color activeColor = Color.green;

    [SerializeField]
    private Color inactiveColor = Color.white;

    // ... (以下、メソッド群は変更なし) ...
    void Awake()
    {
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
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStageData() != null)
        {
            SetupAbilityButtons(GameManager.Instance.GetCurrentStageData());
        }
        bombModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Bomb));
        forceModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Force));
        annihilatorModeButton?.onClick.AddListener(() => OnModeButtonPressed(ToolMode.Annihilator));
        activateAllButton?.onClick.AddListener(ActivateCurrentTool);
        if (!IsModeAllowed(currentMode))
        {
            if (bombModeButton.gameObject.activeSelf)
                SwitchMode(ToolMode.Bomb);
            else if (forceModeButton.gameObject.activeSelf)
                SwitchMode(ToolMode.Force);
            else if (annihilatorModeButton.gameObject.activeSelf)
                SwitchMode(ToolMode.Annihilator);
        }
        else
        {
            SwitchMode(currentMode);
        }
    }

    void OnDestroy()
    {
        bombModeButton?.onClick.RemoveAllListeners();
        forceModeButton?.onClick.RemoveAllListeners();
        annihilatorModeButton?.onClick.RemoveAllListeners();
        activateAllButton?.onClick.RemoveAllListeners();
    }

    public ToolMode GetCurrentMode()
    {
        return currentMode;
    }

    private void SetupAbilityButtons(StageData stageData)
    {
        bombModeButton?.gameObject.SetActive(stageData.allowBombs);
        forceModeButton?.gameObject.SetActive(stageData.allowForce);
        annihilatorModeButton?.gameObject.SetActive(stageData.allowAnnihilator);
    }

    private bool IsModeAllowed(ToolMode mode)
    {
        switch (mode)
        {
            case ToolMode.Bomb:
                return bombModeButton.gameObject.activeSelf;
            case ToolMode.Force:
                return forceModeButton.gameObject.activeSelf;
            case ToolMode.Annihilator:
                return annihilatorModeButton.gameObject.activeSelf;
            default:
                return false;
        }
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

    public void SwitchMode(ToolMode newMode)
    {
        currentMode = newMode;
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
