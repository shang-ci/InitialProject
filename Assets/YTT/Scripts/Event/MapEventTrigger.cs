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

public class MapEventTrigger : MonoBehaviour
{
    public MapEvent mapEvent;
    public CardManager cardManager;
    public PlayerManager playerManager;
    public GameObject buttonObject;
    public GameManager gameManager;
    public Inventory inventory;

    void Start()
    {
        if (gameManager == null)
        {
            //获得游戏管理器的单例实例
            gameManager = GameManager.Instance;
        }

        //游戏开始执行时便刷新事件按钮的显示状态
        RefreshButton();
    }

    //刷新事件按钮的显示状态，根据触发条件判断按钮是否可见
    public void RefreshButton()
    {
        int statValue = 0;
        if (playerManager != null && !string.IsNullOrEmpty(mapEvent.statToCheck))
        {
            statValue = playerManager.GetStat(mapEvent.statToCheck);
        }

        bool available = mapEvent.IsAvailable(
            gameManager.currentDay,
            statValue,
            gameManager.HasCompletedEventSuccessfully
        );
        //Debug.Log($"Event {mapEvent.eventID} is {available}");

        buttonObject.SetActive(available);

        if (available && gameManager.HasEventBeenCompleted(mapEvent.eventID))
        {
            buttonObject.GetComponent<Button>().interactable = false;
        }
        else
        {
            buttonObject.GetComponent<Button>().interactable = true;
        }
    }

    //触发事件的逻辑，玩家点击事件按钮时执行
    public void TriggerEvent()
    {
        //若当前有对话在进行，则不执行事件
        if (DialogueManager.IsConversationActive) return;

        if (gameManager != null)
        {
            //将该事件注册为已触发
            gameManager.RegisterEvent(this);
            //Debug.Log($"Event : {mapEvent.eventID} has been registered as completed");
        }

        //判定胜利
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
        DialogueLua.SetVariable("EventResult", isWin ? "Win" : "Lose");

        if (isWin)
        {
            Debug.Log($"事件{mapEvent.title}胜利，玩家获得奖励！");
            ApplyOutcomeEffects(mapEvent.winOutcome);

            //设置胜利奖励
            DialogueLua.SetVariable("RewardItemName", mapEvent.winOutcome.itemRewards.Count > 0 ?
            mapEvent.winOutcome.itemRewards[0].itemName : "");
        }
        else
        {
            Debug.Log($"事件{mapEvent.title}失败，玩家得到惩罚！");
            ApplyOutcomeEffects(mapEvent.loseOutcome);
        }

        //启动对话
        DialogueManager.StartConversation(mapEvent.conversationStartNode);

        

        //更新按钮状态
        RefreshButton();
    }

    private void ApplyOutcomeEffects(Outcome outcome)
    {
        foreach (var effect in outcome.statEffects)
        {
            playerManager.AddStat(effect.statName, effect.valueChange);

            foreach (var itemReward in outcome.itemRewards)
            {
                inventory.AddItemToInventory(itemReward.itemName, itemReward.quantity);
                Debug.Log($"map event trigger里面获得物品 {itemReward.itemName}");
            }
        }

        if (outcome.cardRewards != null)
        {
            foreach (var card in outcome.cardRewards)
            {
                cardManager.AddCard(card);
            }
        }

        if (outcome.cardRemovals != null)
        {
            foreach (var card in outcome.cardRemovals)
            {
                cardManager.RemoveCard(card);
            }
        }
    }

    //新的一天重置事件，并更新按钮状态
    public void ResetEventForNewDay()
    {
        RefreshButton();
    }
}