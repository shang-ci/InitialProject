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

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        RefreshButton();
    }

    public void RefreshButton()
    {
        bool available = false;

        switch (mapEvent.triggerType)
        {
            case MapEvent.TriggerType.Always:
                available = true;
                break;
            case MapEvent.TriggerType.StatBased:
                int statsValue = DialogueLua.GetVariable(mapEvent.statToCheck).asInt;
                available = mapEvent.IsAvailable(0, statsValue);
                break;
            case MapEvent.TriggerType.DayBased:
                available = gameManager.currentDay >= mapEvent.triggerDay;
                break;
            case MapEvent.TriggerType.PrecedingEventCompleted:
                if (gameManager.HasEventBeenCompleted(mapEvent.precedingEventID))
                {
                    available = true;
                }
                break;
        }

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


    public void TriggerEvent()
    {
        if (DialogueManager.IsConversationActive) return;

        if (gameManager != null)
        {
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

        //注册事件为已触发
        if (gameManager != null)
            gameManager.RegisterEvent(this);

        //更新按钮状态
        RefreshButton();
    }

    private void ApplyOutcomeEffects(Outcome outcome)
    {
        foreach (var effect in outcome.statEffects)
        {
            playerManager.AddStat(effect.statName, effect.valueChange);
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


    public void ResetEventForNewDay()
    {
        RefreshButton();
    }
}