using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

public class MapEventTrigger : MonoBehaviour
{
    public MapEvent mapEvent;

    public void TriggerEvent()
    {
        if (DialogueManager.IsConversationActive) return;

        // 写入 DSU 变量供对话使用
        DialogueLua.SetVariable("CurrentEventID", mapEvent.eventID);
        DialogueLua.SetVariable("WinConditionStat", mapEvent.winCondition.statName);
        DialogueLua.SetVariable("WinConditionValue", mapEvent.winCondition.minValueRequired);

        // 清空旧的奖励/惩罚变量（可选，根据你的需求）

        // 写入胜利奖励属性
        foreach (var effect in mapEvent.winOutcome.statEffects)
        {
            DialogueLua.SetVariable($"Reward_{effect.statName}", effect.valueChange);
        }
        // 写入失败惩罚属性
        foreach (var effect in mapEvent.loseOutcome.statEffects)
        {
            DialogueLua.SetVariable($"Penalty_{effect.statName}", effect.valueChange);
        }

        // 奖励物品名
        DialogueLua.SetVariable("RewardItemName", mapEvent.winOutcome.itemRewards.Count > 0 ? mapEvent.winOutcome.itemRewards[0].itemName : "");

        // 启动 DSU 对话
        DialogueManager.StartConversation(mapEvent.conversationStartNode);
    }
}