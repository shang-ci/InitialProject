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
        
        // 添加对话状态变化监听
        StartCoroutine(CheckForPlayerResponses());
    }

    void OnDisable()
    {
        DialogueManager.instance.conversationStarted -= OnConversationStarted;
        DialogueManager.instance.conversationLinePrepared -= OnConversationLine;
        DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    private bool lastHadResponses = false; // 移到类级别

    void OnConversationStarted(Transform actor)
    {
        // 对话开始时重置状态
        lastHadResponses = false;
        Debug.Log("Conversation started - resetting response state");
    }

    // void Update()
    // {
    //     // 定期检查是否需要显示Player立绘
    //     if (DialogueManager.instance != null && DialogueManager.instance.isConversationActive)
    //     {
    //         CheckForPlayerResponse();
    //     }
    // }

    void OnConversationLine(Subtitle subtitle)
{
    string actorName = subtitle.speakerInfo.Name;
    string text = subtitle.formattedText.text;

    // NPC在左，Player在右
    bool isLeft = actorName != "Player";
    Debug.Log($"OnConversationLine: {actorName}, isLeft: {isLeft}");

    // 当新的对话行开始时，重置状态以便检测新的Player回答
    if (isLeft) // NPC说话时
    {
        lastHadResponses = false;
        Debug.Log("NPC speaking - resetting response state");
        
        // 确保Player立绘被隐藏
        if (rightPortrait != null)
        {
            rightPortrait.gameObject.SetActive(false);
        }
    }

    PortraitData data = GetPortraitData(actorName);
    Sprite portrait = data.GetEmotionPortraitByText(text);

    if (isLeft)
    {
        leftPortrait.sprite = portrait;
        leftPortrait.gameObject.SetActive(true);
        rightPortrait.gameObject.SetActive(false); // 隐藏右侧
        currentPortraitImage = leftPortrait;
    }
    else
    {
        rightPortrait.sprite = portrait;
        rightPortrait.gameObject.SetActive(true);
        leftPortrait.gameObject.SetActive(false); // 隐藏左侧
        currentPortraitImage = rightPortrait;
    }

    currentPortraitData = data;

    // 启动眨眼协程
    if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    blinkCoroutine = StartCoroutine(BlinkRoutine());
}

    // 添加一个方法来检查是否应该显示Player立绘
    // void CheckForPlayerResponse()
    // {
    //     // 检查当前对话状态
    //     if (DialogueManager.instance != null && DialogueManager.instance.conversationController != null)
    //     {
    //         var state = DialogueManager.instance.conversationController.currentState;
    //         if (state != null && state.pcResponses != null && state.pcResponses.Length > 0)
    //         {
    //             // 如果有Player回答选项，显示Player立绘
    //             OnPlayerResponseMenuOpen(state.pcResponses);
    //         }
    //     }
    // }

    public void OnPlayerResponseMenuOpen(Response[] responses)
    {
        // 当Player回答选项出现时，显示Player的立绘
        Debug.Log("PortraitManager.OnPlayerResponseMenuOpen called");
        
        PortraitData playerData = GetPortraitData("Player");
        if (playerData != null)
        {
            Debug.Log("Player PortraitData found, showing rightPortrait");
            // 使用默认表情
            Sprite playerPortrait = playerData.defaultPortrait;
            
            rightPortrait.sprite = playerPortrait;
            rightPortrait.gameObject.SetActive(true);
            leftPortrait.gameObject.SetActive(false); // 隐藏左侧NPC立绘
            
            currentPortraitImage = rightPortrait;
            currentPortraitData = playerData;
            
            // 启动眨眼协程
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
        else
        {
            Debug.LogWarning("PortraitManager: Player PortraitData not found!");
        }
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
        
        // 对话结束时也重置状态
        lastHadResponses = false;
        Debug.Log("Conversation ended - resetting response state");
    }

    PortraitData GetPortraitData(string actorName)
    {
        foreach (var data in portraitDatas)
        {
            Debug.Log($"Check PortraitData: {data.characterName}");
            if (data.characterName == actorName)
            {
                Debug.Log($"Matched PortraitData for: {actorName}");
                return data;
            }
        }
        Debug.LogWarning($"No PortraitData found for: {actorName}");
        return null;
    }

    // 检查Player回答的协程
    System.Collections.IEnumerator CheckForPlayerResponses()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // 每0.1秒检查一次
            
            if (DialogueManager.instance != null && DialogueManager.instance.isConversationActive)
            {
                var state = DialogueManager.instance.conversationController?.currentState;
                bool hasResponses = state?.pcResponses != null && state.pcResponses.Length > 0;
                
                // 只在状态变化时调用，并且确保当前没有正在显示的对话
                if (hasResponses && !lastHadResponses)
                {
                    // 计算延迟时间：基础延迟 + 根据对话长度调整
                    float baseDelay = 1.0f; // 基础延迟1秒
                    float textLengthDelay = 0f;
                    
                    // 如果有当前对话，根据文本长度调整延迟
                    if (state.subtitle != null && !string.IsNullOrEmpty(state.subtitle.formattedText.text))
                    {
                        int textLength = state.subtitle.formattedText.text.Length;
                        textLengthDelay = Mathf.Min(textLength * 0.05f, 2.0f); // 每字符0.05秒，最多2秒
                    }
                    
                    float totalDelay = baseDelay + textLengthDelay;
                    Debug.Log($"Calculated delay: {totalDelay}s (base: {baseDelay}s, text: {textLengthDelay}s)");
                    
                    yield return new WaitForSeconds(totalDelay);
                    
                    // 再次检查是否还有回答选项（防止对话状态变化）
                    var currentState = DialogueManager.instance.conversationController?.currentState;
                    bool stillHasResponses = currentState?.pcResponses != null && currentState.pcResponses.Length > 0;
                    
                    if (stillHasResponses)
                    {
                        Debug.Log("Player responses detected - showing Player portrait");
                        OnPlayerResponseMenuOpen(currentState.pcResponses);
                    }
                }
                
                // 如果之前有回答选项，现在没有了，说明Player已经选择了回答
                if (!hasResponses && lastHadResponses)
                {
                    Debug.Log("Player response selected - resetting state for next response");
                    lastHadResponses = false;
                }
                else
                {
                    lastHadResponses = hasResponses;
                }
            }
        }
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