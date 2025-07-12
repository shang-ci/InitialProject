/*
7.8
将玩家属性管理器 PlayerManager 提取出来
将玩家属性存储在字典中，方便管理和扩展
*/
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class PlayerManager : MonoBehaviour
{
    // 玩家属性字典，key为属性名，value为属性值
    private Dictionary<string, int> stats = new Dictionary<string, int>();

    // 初始化属性,自动同步所有 DSU 变量
    public void Start()
    {
        var variables = DialogueManager.masterDatabase.variables;
        foreach (var variable in variables)
        {
            // 只同步数字类型属性
            if (variable.Type == PixelCrushers.DialogueSystem.FieldType.Number)
            {
                stats[variable.Name] = DialogueLua.GetVariable(variable.Name).asInt;
            }
        }
    }

    // 获取属性值
    public int GetStat(string statName)
    {
        if (stats.ContainsKey(statName))
            return stats[statName];
        else
            return 0;
    }

    // 增加或减少属性值
    public void AddStat(string statName, int valueChange)
    {
        if (!stats.ContainsKey(statName))
            stats[statName] = 0;
        stats[statName] += valueChange;

        DialogueLua.SetVariable(statName, stats[statName]);
        //Debug.Log($"Stat {statName} changed by {valueChange}, new value: {stats[statName]}");
    }

    // 可选：设置属性值
    public void SetStat(string statName, int value)
    {
        stats[statName] = value;

        DialogueLua.SetVariable(statName, value);
    }

    public string GetAllStatsString()
    {
        string result = "";
        foreach (var stat in stats)
        {
            result += $"{stat.Key} : {stat.Value},";
        }

        if (result.Length > 2)
            result = result.Substring(0, result.Length - 2);
            return result;
    }
}