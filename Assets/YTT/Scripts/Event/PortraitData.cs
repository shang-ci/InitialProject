using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PortraitData", menuName = "ProtraitCreation/Portrait Data")]
public class PortraitData : ScriptableObject
{
    public string characterName; // 角色名，需与Dialogue System的Actor Name一致

    [Header("立绘")]
    public Sprite defaultPortrait; // 默认立绘
    public Sprite blinkPortrait;   // 眨眼立绘

    [Header("表情立绘")]
    public List<EmotionPortrait> emotions = new List<EmotionPortrait>();

    // 根据台词内容返回对应表情
    public Sprite GetEmotionPortraitByText(string text)
    {
        foreach (var emo in emotions)
        {
            foreach (var keyword in emo.keywords)
            {
                if (!string.IsNullOrEmpty(keyword) && text.Contains(keyword))
                    return emo.portrait;
            }
        }
        return defaultPortrait;
    }

    // 获取眨眼立绘
    public Sprite GetBlinkPortrait() => blinkPortrait != null ? blinkPortrait : defaultPortrait;
}

[System.Serializable]
public class EmotionPortrait
{
    public string emotionName;      // 表情名
    public Sprite portrait;         // 对应立绘
    public List<string> keywords;   // 触发该表情的关键词
}