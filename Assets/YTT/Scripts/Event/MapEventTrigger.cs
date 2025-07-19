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
using Unity.Burst;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;

public class MapEventTrigger : MonoBehaviour
{
    public MapEvent mapEvent;
    public CardManager cardManager;
    public PlayerManager playerManager;
    public GameObject buttonObject;
    public GameManager gameManager;
    public Inventory inventory; // 背包系统

    void Start()
    {
        // 确保背包系统已初始化
        if (inventory == null)
        {
            // 尝试从当前对象获取
            inventory = GetComponent<Inventory>();
            
            // 如果仍然为空，尝试在场景中查找
            if (inventory == null)
            {
                inventory = FindObjectOfType<Inventory>();
                Debug.LogWarning("背包系统未直接绑定，已尝试在场景中查找");
            }
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        RefreshButton();
    }

    public void RefreshButton()
    {
        // ... 保持原有逻辑不变 ...
    }

    public void TriggerEvent()
    {
        if (DialogueManager.IsConversationActive) return;

        if (gameManager != null)
        {
            gameManager.RegisterEvent(this);
        }

        bool isWin = false;
        if (mapEvent.winCondition != null && playerManager != null)
        {
            int playerStatValue = playerManager.GetStat(mapEvent.winCondition.statToCheck);
            isWin = playerStatValue >= mapEvent.winCondition.minValueRequired;
        }

        // 设置对话变量
        DialogueLua.SetVariable("CurrentEventID", mapEvent.eventID);
        DialogueLua.SetVariable("WinConditionStat", mapEvent.winCondition.statName);
        DialogueLua.SetVariable("WinConditionValue", mapEvent.winCondition.minValueRequired);
        DialogueLua.SetVariable("EventResult", isWin ? "Win" : "Lose");

        if (isWin)
        {
            Debug.Log($"事件{mapEvent.title}胜利，玩家获得奖励！");
            ApplyOutcomeEffects(mapEvent.winOutcome);

            // 设置胜利奖励
            string rewardItem = mapEvent.winOutcome.itemRewards.Count > 0 ? 
                mapEvent.winOutcome.itemRewards[0].itemName : "无";
            DialogueLua.SetVariable("RewardItemName", rewardItem);
        }
        else
        {
            Debug.Log($"事件{mapEvent.title}失败，玩家得到惩罚！");
            ApplyOutcomeEffects(mapEvent.loseOutcome);
        }

        // 启动对话
        DialogueManager.StartConversation(mapEvent.conversationStartNode);
        RefreshButton();
    }

    private void ApplyOutcomeEffects(Outcome outcome)
    {
        // 1. 处理属性变化
        foreach (var effect in outcome.statEffects)
        {
            playerManager.AddStat(effect.statName, effect.valueChange);
        }

        // 2. 处理卡牌奖励
        if (outcome.cardRewards != null)
        {
            foreach (var card in outcome.cardRewards)
            {
                cardManager.AddCard(card);
            }
        }

        // 3. 处理物品奖励 - 关键修复部分
        if (outcome.itemRewards != null && outcome.itemRewards.Count > 0)
        {
            // 确保背包系统已初始化
            if (inventory == null)
            {
                Debug.LogError("背包系统未初始化，无法添加物品！");
                return;
            }

            foreach (var itemReward in outcome.itemRewards)
            {
                if (!string.IsNullOrEmpty(itemReward.itemName) && itemReward.quantity > 0)
                {
                    // 添加物品并记录日志
                    Debug.Log($"获得物品: {itemReward.itemName} x {itemReward.quantity}");
                    inventory.AddItem(itemReward.itemName, itemReward.quantity);
                    Debug.Log("物品已添加到背包");
                }
            }
        }

        // 4. 处理卡牌移除
        if (outcome.cardRemovals != null)
        {
            foreach (var card in outcome.cardRemovals)
            {
                cardManager.RemoveCard(card);
            }
        }
    }

    public void ResetEventForNewDay()
    {
        RefreshButton();
    }
}