/* 
7.3
定义一个类，包含事件应该有的字段
 */
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapEvent", menuName = "Events/Map Event")]
public class MapEvent : ScriptableObject
{
    [Header("基础信息")]
    public string eventID;
    public string title;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;

    public enum TriggerType { Always, StatBased, DayBased }
    public TriggerType triggerType;

    public int triggerDay;
    public int availableFromDay;
    public int availableUntilDay;

    public string statToCheck;
    public int requiredStatValue;

    [Header("行为信息")]
    public float durationHours;
    public string conversationStartNode;

    [Header("胜利条件")]
    public Condition winCondition;

    [Header("胜利结果")]
    public Outcome winOutcome;

    [Header("失败结果")]
    public Outcome loseOutcome;

    // 判断当前事件是否可用（在地图上显示）
    public bool IsAvailable(int currentDay, int currentStat)
    {
        if (currentDay < availableFromDay || currentDay > availableUntilDay)
            return false;

        switch (triggerType)
        {
            case TriggerType.Always:
                return true;
            case TriggerType.DayBased:
                return currentDay == triggerDay;
            case TriggerType.StatBased:
                return currentStat >= requiredStatValue;
            default:
                return false;
        }
    }
}

[System.Serializable]
public class Condition
{
    public string statToCheck;
    public int minValueRequired;
}

[System.Serializable]
public class Outcome
{
    public List<ItemReward> itemRewards = new();
    public List<StatEffect> statEffects = new();
}

[System.Serializable]
public class ItemReward
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public class StatEffect
{
    public string statName;
    public int valueChange;
}
