using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

/// <summary>
/// 存储单个事件所有信息的数据类
/// 它不继承 MonoBehaviour，是一个纯粹的 C# 数据容器
/// </summary>
[System.Serializable]
public class EventData
{
    public string EventID { get; private set; }
    public EventType EventType { get; private set; }
    public SelectType SelectType { get; private set; }
    public string EventName { get; private set; }
    public string EventPrefabName { get; private set; } // 存储Prefab在Resources下的路径
    public string Story { get; private set; }// 事件的故事文本
    public string Tips { get; private set; }// 事件的提示文本
    public List<string> RequiredAttributes { get; private set; } // 需要的属性，已解析为列表
    public List<string> RequiredItems { get; private set; } // 需要的物品
    public int RequiredCoin { get; private set; } //需要的金币
    public int DurationDays { get; private set; }// 事件持续的天数
    public int SuccessThreshold { get; private set; } // 成功的条件/阈值，投出的成功骰子需要大于这个数
    public string SuccessfulResults { get; private set; } // 成功的结果文本
    public string FailedResults { get; private set; } // 失败的结果文本

    public List<string> RewardItemIDs { get; private set; } // 奖励物品ID，已解析为列表
    public string SuccessEvent { get; private set; } // 成功的后续事件ID
    public string FailedEvent { get; private set; } // 失败的后续事件ID

    // 构造函数：负责将从CSV读取的原始字符串数据，解析并填充到类的属性中
    public EventData(string[] rawData)
    {
        try
        {
            EventID = rawData[0];

            // 使用 Enum.Parse 将字符串转换为枚举，true表示忽略大小写
            EventType = (EventType)Enum.Parse(typeof(EventType), rawData[1], true);
            SelectType = (SelectType)Enum.Parse(typeof(SelectType), rawData[2], true);

            EventName = rawData[3];
            EventPrefabName = rawData[4]; // 这里直接存储字符串路径
            Story = rawData[5];
            Tips = rawData[6];

            // 解析逗号分隔的字符串为列表
            // Trim()可以去除每个元素前后的空格，以防 "a, b" 这种情况
            RequiredAttributes = rawData[7].Split('、').Select(s => s.Trim()).ToList();
            RequiredItems = rawData[8].Split('、').Select(s  => s.Trim()).ToList();
            RequiredCoin = int .Parse(rawData[9]);

            DurationDays = int.Parse(rawData[10]);
            SuccessThreshold = int.Parse(rawData[11]);

            SuccessfulResults = rawData[12];
            FailedResults = rawData[13];

            RewardItemIDs = rawData[14].Split('、').Select(s => s.Trim()).ToList();
            SuccessEvent = rawData[15]; // 成功后续事件ID
            FailedEvent = rawData[16]; // 失败后续事件ID
        }
        catch (Exception e)
        {
            // 如果某一行数据格式错误，这能帮助我们快速定位问题
            Debug.LogError($"Error parsing event data for row with ID {rawData[0]}. Check your CSV format. Error: {e.Message}");
        }
    }
}