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
    private GameObject tutorialPanel;

    [SerializeField]
    private Text messageText;

    // --- ▲▲▲ 追加ここまで ▲▲▲ ---

    void Start()
    {
        StartCoroutine(TutorialSequence());
    }

    // ... （TutorialSequenceや他のメソッドは変更なし） ...
    private IEnumerator TutorialSequence()
    {
        playerController.canMove = false;
        playerController.canLook = false;
        if (playerToolManager != null)
        {
            playerToolManager.bombModeButton.interactable = false;
            playerToolManager.forceModeButton.interactable = false;
            playerToolManager.annihilatorModeButton.interactable = false;
            playerToolManager.activateAllButton.interactable = false;
            playerToolManager.SwitchMode(PlayerToolManager.ToolMode.Force);
        }

        //基本操作
        playerController.canMove = true;
        yield return StartCoroutine(
            ShowMessageAndWait(
                "WASDキーで前後左右に移動できます\nSpaceキーで上昇\nLeftShiftキーで下降します",
                6.0f
            )
        );
        playerController.canLook = true;
        yield return StartCoroutine(ShowMessageAndWait("矢印キーで視点を動かせます", 6.0f));

        yield return StartCoroutine(ShowMessageAndWait("基本操作の確認は以上です", 6.0f));

        //アビリティ説明
        yield return StartCoroutine(
            ShowMessageAndWait("次に、アビリティの使い方を説明します", 6.0f)
        );
        yield return StartCoroutine(ShowMessageAndWait("まずは、Bombの使い方を説明します", 4.0f));
        playerToolManager.bombModeButton.interactable = true;
        tutorialPanel.SetActive(true);
        messageText.text = "画面左のBombボタンを\nクリックしてください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.bombModeButton));
        playerToolManager.bombModeButton.interactable = false;

        yield return StartCoroutine(
            ShowMessageAndWait(
                "Bombが選択され、ボタンが緑色になりました。\n選択中のアビリティは緑色で表示されます。",
                5.0f
            )
        );
        playerToolManager.bombModeButton.interactable = true;

        messageText.text = "もう一度Bombボタンをクリックして、\n爆弾を生成してください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.bombModeButton));

        yield return StartCoroutine(ShowMessageAndWait("爆弾が生成されました！", 6.0f));
        yield return StartCoroutine(
            ShowMessageAndWait("爆弾をドラッグして、\n好きな位置に配置してください", 10.0f)
        );
        playerToolManager.activateAllButton.interactable = true;
        messageText.text = "右下のActivateボタンをクリックして\n爆弾を爆発させましょう";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.activateAllButton));
        playerToolManager.activateAllButton.interactable = false;

        yield return StartCoroutine(ShowMessageAndWait("次にフォースモードの説明をします", 6.0f));
        messageText.text = "画面左のForceボタンをクリックしてください";
        playerToolManager.forceModeButton.interactable = true;

        yield return StartCoroutine(WaitForButtonClick(playerToolManager.forceModeButton));

        yield return StartCoroutine(ShowMessageAndWait("Forceモードが選択されました", 6.0f));

        yield return StartCoroutine(
            ShowMessageAndWait(
                "カーソルを建物に合わせてクリックすると、\nその方向に力を加えることができます。\n試しにいくつかブロックをクリックしてみましょう。",
                6.0f
            )
        );
        yield return StartCoroutine(ShowMessageAndWait("いい感じです！", 6.0f));

        yield return StartCoroutine(
            ShowMessageAndWait("最後に、Eliminaterの使い方を説明します", 6.0f)
        );
        playerToolManager.annihilatorModeButton.interactable = true;

        messageText.text = "画面左のEliminateボタンを\nクリックしてください";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.annihilatorModeButton));
        playerToolManager.activateAllButton.interactable = false;
        yield return StartCoroutine(
            ShowMessageAndWait("もう一度クリックして生成し、\n好きな位置に配置してください", 6.0f)
        );
        playerToolManager.activateAllButton.interactable = true;
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.annihilatorModeButton));

        yield return new WaitForSeconds(8.0f);

        messageText.text = "Activateボタンで起動すると、\n効果範囲内のブロックが全て消滅します";
        yield return StartCoroutine(WaitForButtonClick(playerToolManager.activateAllButton));

        yield return StartCoroutine(ShowMessageAndWait("基本的な操作説明は以上です", 6.0f));
        yield return StartCoroutine(
            ShowMessageAndWait("次に、ゲームの目的について説明します", 6.0f)
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
                6.0f
            )
        );
        yield return StartCoroutine(
            ShowMessageAndWait("全てのタスクを達成するとゲームクリアです！", 6.0f)
        );
        yield return StartCoroutine(ShowMessageAndWait("画面左上の数字は現在のスコアです", 6.0f));
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
            ShowMessageAndWait("高スコアを目指して、\n効率よく建物を破壊しましょう！", 6.0f)
        );
        yield return StartCoroutine(
            ShowMessageAndWait("これでチュートリアルは完了です！\n自由にプレイしてください！", 6.0f)
        );
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
