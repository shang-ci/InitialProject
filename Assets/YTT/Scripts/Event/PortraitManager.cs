using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

public class PortraitManager : MonoBehaviour
{
    public Image leftPortrait;
    public Image rightPortrait;
    public PortraitData[] portraitDatas;

    private Coroutine blinkCoroutine;
    private Image currentPortraitImage;
    private PortraitData currentPortraitData;

    void OnEnable()
    {
        DialogueManager.instance.conversationStarted += OnConversationStarted;
        DialogueManager.instance.conversationLinePrepared += OnConversationLine;
        DialogueManager.instance.conversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        DialogueManager.instance.conversationStarted -= OnConversationStarted;
        DialogueManager.instance.conversationLinePrepared -= OnConversationLine;
        DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    void OnConversationStarted(Transform actor)
    {
        // 可做初始化
    }

    void OnConversationLine(Subtitle subtitle)
    {
        string actorName = subtitle.speakerInfo.Name;
        string text = subtitle.formattedText.text;

        // 判断说话人站位（左/右），这里举例
        bool isLeft = actorName == "主角名"; // 你可以自定义规则

        // 查找立绘数据
        PortraitData data = GetPortraitData(actorName);

        // 关键词切换表情
        Sprite portrait = data.GetEmotionPortraitByText(text);

        if (isLeft)
        {
            leftPortrait.sprite = portrait;
            leftPortrait.gameObject.SetActive(true);
            rightPortrait.gameObject.SetActive(false);
            currentPortraitImage = leftPortrait;
        }
        else
        {
            rightPortrait.sprite = portrait;
            rightPortrait.gameObject.SetActive(true);
            leftPortrait.gameObject.SetActive(false);
            currentPortraitImage = rightPortrait;
        }

        currentPortraitData = data;

        // 启动眨眼协程
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    void OnConversationEnded(Transform actor)
    {
        leftPortrait.gameObject.SetActive(false);
        rightPortrait.gameObject.SetActive(false);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    PortraitData GetPortraitData(string actorName)
    {
        foreach (var data in portraitDatas)
            if (data.characterName == actorName) return data;
        return null;
    }

    // 眨眼协程
    System.Collections.IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f)); // 眨眼间隔
            if (currentPortraitImage != null && currentPortraitData != null)
            {
                var original = currentPortraitImage.sprite;
                currentPortraitImage.sprite = currentPortraitData.GetBlinkPortrait();
                yield return new WaitForSeconds(0.15f); // 眨眼持续时间
                currentPortraitImage.sprite = original;
            }
        }
    }
}