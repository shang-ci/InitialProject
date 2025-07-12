/*
7.8
将玩家属性管理器 PlayerManager 提取出来
将玩家属性存储在字典中，方便管理和扩展
7.10 PlayerStatUI
制作一个属性版
实时更新玩家的属性值
从dialogue system中实时获取各属性的数值
7.12
将PlayerStatUI脚本内容（实时获取属性值打印在屏幕上）合并到该脚本
*/
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    // 玩家属性字典，key为属性名，value为属性值
    private Dictionary<string, int> stats = new Dictionary<string, int>();
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI hardworkingText;
    public TextMeshProUGUI angerText;

    void Update()
    {
        int wisdom = DialogueLua.GetVariable("Wisdom").asInt;
        int hardworking = DialogueLua.GetVariable("Hardworking").asInt;
        int anger = DialogueLua.GetVariable("Anger").asInt;

        //更新stats字典
        stats["Wisdom"] = wisdom;
        stats["Hardworking"] = hardworking;
        stats["Anger"] = anger;

        //更新UI显示
        wisdomText.text = $"Wisdom: {wisdom}";
        hardworkingText.text = $"Hardworking: {hardworking}";
        angerText.text = $"Anger: {anger}";
    }

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
    }
}