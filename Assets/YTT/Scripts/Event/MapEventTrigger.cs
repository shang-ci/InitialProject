/*
7.4
RefreshButton()方法根据事件类型和条件，决定地图上的事件按钮是否显示
TriggerEvent() 方法在玩家点击事件按钮时执行：
判定事件胜利，进行奖惩，
*/
using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine.UI;

public class MapEventTrigger : MonoBehaviour
{
    public MapEvent mapEvent;
    public CardManager cardManager;
    public PlayerManager playerManager;
    public GameObject buttonObject;
    public string unlockVariableName = ""; // 解锁后续事件的变量名
    public int unlockValue = 1;            // 解锁时赋的值
    private bool triggeredToday = false;
    public GameManager gameManager;

    // void Start()
    // {
    //     RefreshButton();
    // }

    // public void RefreshButton()
    // {
    //     int statValue = 0;

    //     if (mapEvent.triggerType == MapEvent.TriggerType.StatBased)
    //     {
    //         statValue = DialogueLua.GetVariable(mapEvent.statToCheck).asInt;
    //     }
    //     bool available = mapEvent.IsAvailable(0, statValue);
    //     buttonObject.SetActive(available);
    // }

    public void TriggerEvent()
    {
        if (triggeredToday) return; // 已触发则不再响应
        triggeredToday = true;

        if (DialogueManager.IsConversationActive) return;

        // 判定胜利
        bool isWin = false;
        if (mapEvent.winCondition != null && playerManager != null)
        {
            int playerStatValue = playerManager.GetStat(mapEvent.winCondition.statToCheck);
            isWin = playerStatValue >= mapEvent.winCondition.minValueRequired;
        }

        // 写入 DSU 变量供对话使用
        DialogueLua.SetVariable("CurrentEventID", mapEvent.eventID);
        DialogueLua.SetVariable("WinConditionStat", mapEvent.winCondition.statName);
        DialogueLua.SetVariable("WinConditionValue", mapEvent.winCondition.minValueRequired);

        if (isWin)
        {
            Debug.Log($"事件胜利：{mapEvent.title}，玩家获得奖励！"); // 添加胜利提示
            // 写入胜利奖励属性
            foreach (var effect in mapEvent.winOutcome.statEffects)
            {
                // 给玩家加成属性
                playerManager.AddStat(effect.statName, effect.valueChange);
                Debug.Log($"属性 {effect.statName} 变更为 {effect.valueChange}");
            }
            // 处理胜利获得卡牌
            if (mapEvent.winOutcome.cardRewards != null)
            {
                foreach (var card in mapEvent.winOutcome.cardRewards)
                {
                    cardManager.AddCard(card);
                }
            }
            // 奖励物品名
            DialogueLua.SetVariable("RewardItemName", mapEvent.winOutcome.itemRewards.Count > 0 ? mapEvent.winOutcome.itemRewards[0].itemName : "");
            // 触发解锁变量，用于解锁后续事件
            if (!string.IsNullOrEmpty(unlockVariableName))
            {
                DialogueLua.SetVariable(unlockVariableName, unlockValue);
            }
        }
        else
        {
            Debug.Log($"事件失败：{mapEvent.title}，玩家接受惩罚！"); // 添加胜利提示
            // 写入失败惩罚属性
            foreach (var effect in mapEvent.loseOutcome.statEffects)
            {
                DialogueLua.SetVariable($"Penalty_{effect.statName}", effect.valueChange);
                // 给玩家减成属性
                playerManager.AddStat(effect.statName, effect.valueChange);
            }
            // 处理失败失去卡牌
            if (mapEvent.loseOutcome.cardRemovals != null)
            {
                foreach (var card in mapEvent.loseOutcome.cardRemovals)
                {
                    cardManager.RemoveCard(card);
                }
            }
            // 失败物品名（如有需要可写入）
        }

        // 启动 DSU 对话
        DialogueManager.StartConversation(mapEvent.conversationStartNode);

        // if (gameManager != null)
        //     gameManager.registerEvent(this);
        // buttonObject.GetComponent<Button>().interactable = false; // 禁用按钮，防止重复触发
    }

    public void ResetEventForNewDay()
    {
        triggeredToday = false;
        buttonObject.GetComponent<Button>().interactable = true; // 重置按钮状态
    }
}