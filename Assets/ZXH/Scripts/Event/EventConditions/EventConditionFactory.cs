using UnityEngine;

public static class EventConditionFactory
{
    public static EventTriggerConditionBase CreateEventTriggerCondition(string[] parts)
    {
        if (parts == null || parts.Length == 0) return null;

        string type = parts[0];

        try
        {
            switch (type)
            {
                case "Attribute":
                    return new AttributeCondition { AttributeName = parts[1], RequiredValue = float.Parse(parts[2]) };
                case "Item":
                    return new ItemCondition { RequiredItemID = parts[1], RequiredCount = int.Parse(parts[2]) };
                case "Time":
                    return new TimeCondition { RequiredDay = int.Parse(parts[1]) };
                case "EventCompleted":
                    return new EventCompletedCondition { RequiredEventID = parts[1] };
                case "Random":
                    return new RandomCondition{ Chance = float.Parse(parts[1]) };
                case "Direct":
                    return new DirectCondition();
                case "AllDay":
                    return new AllDayCondition();
                default:
                    Debug.LogWarning($"未知的事件条件类型: '{type}'");
                    return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建事件条件失败: 类型='{type}', 参数='{string.Join(",", parts)}'. 错误: {e.Message}");
            return null;
        }
    }
}