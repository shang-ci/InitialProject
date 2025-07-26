using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// 需要物品的事件类
/// </summary>
public class Event_Item : EventBase
{
    [SerializeField] protected List<string> RequiredItems; // 需要的物品列表

    public override void Initialize(EventData eventData)
    {
        base.Initialize(eventData);

        RequiredItems = eventData.RequiredItems;
    }

    protected override void ExecutionEvent(EventData eventData)
    {
        isEventActive = true;

        if (RollTheDice_CharacterStat(eventData, successProbability) && Character.Instance.HasAllEquipByDefinitionNames(RequiredItems))
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true;
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        else
        {
            // 失败逻辑
            isSuccess_Event = false;
            Result_Story.text = eventData.FailedResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";
        }

        // 展开Three面板
        ExpandThree();

        //消耗掉所有卡槽中的卡牌
        foreach (var cardSlot in CardSlots)
        {
            var card = cardSlot.GetComponentInChildren<Card>();
            if (card != null)
            {
                Inventory_ZXH.Instance.Backpack.RemoveCard(card.cardData);
            }

        }
    }
}
