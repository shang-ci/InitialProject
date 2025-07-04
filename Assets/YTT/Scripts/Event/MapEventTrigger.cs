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
        DialogueLua.SetVariable("WisdomRequirement", mapEvent.winCondition.minValueRequired);
        
        foreach (var effect in mapEvent.winOutcome.statEffects)
        {
            DialogueLua.SetVariable("WisdomReward", effect.valueChange);
        }
        foreach (var effect in mapEvent.loseOutcome.statEffects)
        {
            DialogueLua.SetVariable("WisdomPenalty", effect.valueChange);
        }

        DialogueLua.SetVariable("RewardItemName", mapEvent.winOutcome.itemRewards.Count > 0 ? mapEvent.winOutcome.itemRewards[0].itemName : "");

        // 启动 DSU 对话
        DialogueManager.StartConversation(mapEvent.conversationStartNode);
    }
}