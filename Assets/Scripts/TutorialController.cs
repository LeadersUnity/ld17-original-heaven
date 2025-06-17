// TutorialController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [Header("制御対象")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerToolManager playerToolManager;

    [Header("UI要素")]
    [SerializeField]
    private GameObject tutorialPanel; // TutorialUIPanelを割り当てる

    [SerializeField]
    private Text messageText; // MessageTextを割り当てる

    void Start()
    {
        // チュートリアルシーケンスを開始
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // --- 0. 初期設定：全ての操作を無効化 ---
        playerController.canMove = false;
        playerController.canLook = false;
        if (playerToolManager != null)
        {
            // 全てのボタンを押せないようにする
            playerToolManager.bombModeButton.interactable = false;
            playerToolManager.forceModeButton.interactable = false;
            playerToolManager.annihilatorModeButton.interactable = false;
            playerToolManager.activateAllButton.interactable = false;

            // ★ご要望：開始時のモードをForceモードに強制設定
            playerToolManager.SwitchMode(PlayerToolManager.ToolMode.Force);
        }

        // --- 1. 移動操作の説明 ---
        playerController.canMove = true;
        yield return StartCoroutine(
            ShowMessageAndWait(
                "WASDキーで前後左右に移動できます\nSpaceキーで上昇、LeftShiftキーで下降します",
                6.0f
            )
        );

        playerController.canLook = true;
        // --- 2. 視点操作の説明 ---
        yield return StartCoroutine(ShowMessageAndWait("矢印キーで視点を動かせます", 6.0f));
        playerController.canLook = true;

        // --- 基本操作の確認完了 ---
        yield return StartCoroutine(ShowMessageAndWait("基本操作の確認は以上です", 4.0f));

        yield return StartCoroutine(
            ShowMessageAndWait("次に、アビリティの使い方を説明します", 4.0f)
        );

        // --- 3. Bombのチュートリアル ---
        yield return StartCoroutine(ShowMessageAndWait("次に、Bombの使い方を説明します", 4.0f));

        playerToolManager.bombModeButton.interactable = true;

        tutorialPanel.SetActive(true);
        messageText.text = "画面左のBombボタンを\nクリックしてください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.bombModeButton));
        tutorialPanel.SetActive(false);

        yield return StartCoroutine(
            ShowMessageAndWait(
                "Bombが選択され、ボタンが緑色になりました。\n選択中のアビリティは緑色で表示されます。",
                5.0f
            )
        );

        tutorialPanel.SetActive(true);
        messageText.text = "もう一度Bombボタンをクリックして、\n爆弾を生成してください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.bombModeButton));
        tutorialPanel.SetActive(false);

        yield return StartCoroutine(ShowMessageAndWait("爆弾が生成されました！", 2.0f));
        yield return StartCoroutine(
            ShowMessageAndWait("爆弾をドラッグして、\n好きな位置に配置してください", 10.0f)
        );

        playerToolManager.activateAllButton.interactable = true;

        tutorialPanel.SetActive(true);
        messageText.text = "右下のActivateボタンをクリックして\n爆弾を爆発させましょう";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.activateAllButton));
        playerToolManager.activateAllButton.interactable = false; // Activateボタンは一旦無効に
        tutorialPanel.SetActive(false);

        // --- 4. Forceのチュートリアル ---

        // Forceボタンだけを押せるようにする
        playerToolManager.forceModeButton.interactable = true;

        tutorialPanel.SetActive(true);
        messageText.text = "画面左のForceボタンをクリックしてください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.forceModeButton));
        tutorialPanel.SetActive(false);

        yield return StartCoroutine(ShowMessageAndWait("Forceモードが選択されました", 3.0f));
        yield return StartCoroutine(
            ShowMessageAndWait(
                "カーソルを建物に合わせてクリックすると、\nその方向に力を加えることができます。\n試しにいくつかブロックをクリックしてみましょう。",
                10.0f
            )
        );

        // --- 5. Annihilatorのチュートリアル ---
        yield return StartCoroutine(
            ShowMessageAndWait("最後に、Annihilatorの使い方を説明します", 4.0f)
        );

        playerToolManager.annihilatorModeButton.interactable = true;

        tutorialPanel.SetActive(true);
        messageText.text = "画面左のAnnihilatorボタンを\nクリックしてください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.annihilatorModeButton));
        tutorialPanel.SetActive(false);

        tutorialPanel.SetActive(true);
        messageText.text = "もう一度クリックして生成し、\n好きな位置に配置してください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.annihilatorModeButton));
        tutorialPanel.SetActive(false);
        yield return new WaitForSeconds(8.0f); // 配置する時間を少し待つ

        playerToolManager.activateAllButton.interactable = true;

        tutorialPanel.SetActive(true);
        messageText.text = "Activateボタンで起動すると、\n効果範囲内のブロックが全て消滅します";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.activateAllButton));
        tutorialPanel.SetActive(false);

        // --- 6. タスクとスコアの説明 ---
        yield return StartCoroutine(ShowMessageAndWait("基本的な操作説明は以上です", 4.0f));
        yield return StartCoroutine(
            ShowMessageAndWait("次に、ゲームの目的について説明します", 4.0f)
        );
        yield return StartCoroutine(
            ShowMessageAndWait(
                "画面右上には、このステージの\nクリア条件である「タスク」が表示されます",
                6.0f
            )
        );
        yield return StartCoroutine(
            ShowMessageAndWait(
                "タスクの横の〇は、\n達成すると緑色に変わります。\n（）の中には現在の進捗が表示されます。",
                8.0f
            )
        );
        yield return StartCoroutine(
            ShowMessageAndWait("全てのタスクを達成するとゲームクリアです！", 5.0f)
        );

        yield return StartCoroutine(ShowMessageAndWait("画面左上の数字は現在のスコアです", 5.0f));
        yield return StartCoroutine(
            ShowMessageAndWait("スコアは【加点要素】と【減点要素】\nによって変動します", 6.0f)
        );
        yield return StartCoroutine(
            ShowMessageAndWait(
                "【加点要素】\n・ブロックを地面に落とす\n・建物の結合部を破壊する",
                8.0f
            )
        );
        yield return StartCoroutine(
            ShowMessageAndWait(
                "【減点要素】\n・アイテム（Bombなど）を使用する\n・時間が経過する\n・ブロックを道路に落とす",
                8.0f
            )
        );
        yield return StartCoroutine(
            ShowMessageAndWait("高スコアを目指して、\n効率よく建物を破壊しましょう！", 5.0f)
        );

        // --- 7. チュートリアル完了 ---
        yield return StartCoroutine(
            ShowMessageAndWait("これでチュートリアルは完了です！\n自由にプレイしてください！", 5.0f)
        );

        // 全てのアビリティボタンを有効化してチュートリアルを終了
        playerToolManager.bombModeButton.interactable = true;
        playerToolManager.forceModeButton.interactable = true;
        playerToolManager.annihilatorModeButton.interactable = true;
        playerToolManager.activateAllButton.interactable = true;
    }

    private IEnumerator ShowMessageAndWait(string message, float waitTime)
    {
        tutorialPanel.SetActive(true);
        messageText.text = message;

        float timer = 0f;
        while (timer < waitTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        tutorialPanel.SetActive(false);
    }

    private IEnumerator WaitForButtonClick(Button targetButton)
    {
        bool isClicked = false;
        UnityEngine.Events.UnityAction onClickAction = () =>
        {
            isClicked = true;
        };
        targetButton.onClick.AddListener(onClickAction);

        while (!isClicked)
        {
            yield return null;
        }

        targetButton.onClick.RemoveListener(onClickAction);
    }
}
