using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBaseClass : EventBase
{
    protected override void ExecutionEvent(EventData eventData)
    {
        isEventActive = true;

        if (RollTheDice_CharacterStat(eventData, successProbability))
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true; // 设置事件成功标志
            GameManager.Instance.RegisterChoice(eventData.SuccessEvent); // 注册成功事件
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        else
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            isSuccess_Event = false; // 设置事件失败标志
            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
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
