using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class Event_Coin : EventBase
{
    [Header("事件属性")]
    [SerializeField] private int requiredCoin; // 需要的金币数量
    [SerializeField] private string currencyName = "Bronze"; // 需要的金币类型名称
    [SerializeField] private TMP_InputField coinInput;// 输入
    [SerializeField] private int amount; // 玩家消耗的金币数量


    private void OnEnable()
    {
        coinInput.onValueChanged.AddListener(OnCoinInputChanged);
    }

    private void OnDestroy()
    {
        coinInput.onValueChanged.RemoveListener(OnCoinInputChanged);
    }


    public override void Initialize(EventData eventData)
    {
        base.Initialize(eventData);

        requiredCoin = eventData.RequiredCoin; 
    }


    protected override void ExecutionEvent(EventData eventData)
    {
        isEventActive = true;

        //属性和金币都过关
        if (RollTheDice_CharacterStat(eventData, successProbability) && Character.Instance.SpendCurrency(currencyName, amount))
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true;
            GameManager.Instance.RegisterChoice(eventData.SuccessEvent); // 注册成功事件
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        //属性过关但金币不满足要求
        else if (RollTheDice_CharacterStat(eventData, successProbability))
        {
            // 成功但没有满足金币要求
            Result_Story.text = eventData.FailedResults + "骰子成功，但没有满足金币要求";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            isSuccess_Event = false;
        }
        //金币满足但属性不满足
        else if (Character.Instance.SpendCurrency(currencyName, amount))
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "金币满足，但骰子不满足要求";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            isSuccess_Event = false;
        }
        //都不满足
        else
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "骰子和金币都不满足";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            isSuccess_Event = false;
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

    /// <summary>
    /// 更新金币输入框的值
    /// </summary>
    /// <param name="value"></param>
    private void OnCoinInputChanged(string value)
    {
        if (int.TryParse(value, out int v))
        {
            amount = v;
        }
        else
        {
            amount = 0;
        }
        Debug.Log($"输入框变更，amount = {amount}");
    }
}
