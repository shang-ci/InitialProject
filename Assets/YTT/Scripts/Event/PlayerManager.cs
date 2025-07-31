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
    // 玩家属性字典
    private Dictionary<string, int> stats = new Dictionary<string, int>();
    private Dictionary<string, bool> boolStats = new Dictionary<string, bool>(); // 新增bool类型字典

    // UI引用
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI hardworkingText;
    public TextMeshProUGUI angerText;

    void Start()
    {
        // 初始化所有属性
        InitializeStats();
    }

    void Update()
    {
        // 更新int类型属性
        int wisdom = DialogueLua.GetVariable("Wisdom").asInt;
        int hardworking = DialogueLua.GetVariable("Hardworking").asInt;
        int anger = DialogueLua.GetVariable("Anger").asInt;

        stats["Wisdom"] = wisdom;
        stats["Hardworking"] = hardworking;
        stats["Anger"] = anger;

        // 更新bool类型属性
        boolStats["IsZhunaAlive"] = DialogueLua.GetVariable("IsZhunaAlive").asBool;
        boolStats["IsZhunaFree"] = DialogueLua.GetVariable("IsZhunaFree").asBool;
        boolStats["IsXiamaAlive"] = DialogueLua.GetVariable("IsXiamaAlive").asBool;
        boolStats["IsXiamaFree"] = DialogueLua.GetVariable("IsXiamaFree").asBool;
        boolStats["IsJialilaAlive"] = DialogueLua.GetVariable("IsJialilaAlive").asBool;
        boolStats["IsJialilaFree"] = DialogueLua.GetVariable("IsJialilaFree").asBool;

        // 更新UI显示
        wisdomText.text = $"Wisdom: {wisdom}";
        hardworkingText.text = $"Hardworking: {hardworking}";
        angerText.text = $"Anger: {anger}";
    }

    // 初始化所有属性
    private void InitializeStats()
    {
        var variables = DialogueManager.masterDatabase.variables;
        foreach (var variable in variables)
        {
            // 初始化int类型属性
            if (variable.Type == FieldType.Number)
            {
                stats[variable.Name] = DialogueLua.GetVariable(variable.Name).asInt;
            }
            // 初始化bool类型属性
            else if (variable.Type == FieldType.Boolean)
            {
                boolStats[variable.Name] = DialogueLua.GetVariable(variable.Name).asBool;
            }
        }
        
        // 确保所有bool变量都有初始值
        EnsureBoolVariable("IsZhunaAlive", false);
        EnsureBoolVariable("IsZhunaFree", false);
        EnsureBoolVariable("IsXiamaAlive", false);
        EnsureBoolVariable("IsXiamaFree", false);
        EnsureBoolVariable("IsJialilaAlive", false);
        EnsureBoolVariable("IsJialilaFree", false);
    }

    // 确保bool变量存在并有默认值
    private void EnsureBoolVariable(string varName, bool defaultValue)
    {
        if (!boolStats.ContainsKey(varName))
        {
            boolStats[varName] = defaultValue;
            DialogueLua.SetVariable(varName, defaultValue);
        }
    }

    // 获取int属性值
    public int GetStat(string statName)
    {
        return stats.ContainsKey(statName) ? stats[statName] : 0;
    }

    // 获取bool属性值
    public bool GetBoolStat(string statName)
    {
        return boolStats.ContainsKey(statName) ? boolStats[statName] : false;
    }

    // 增加或减少int属性值
    public void AddStat(string statName, int valueChange)
    {
        if (!stats.ContainsKey(statName))
            stats[statName] = 0;
        
        stats[statName] += valueChange;
        DialogueLua.SetVariable(statName, stats[statName]);
    }

    // 设置bool属性值
    public void SetBoolStat(string statName, bool value)
    {
        boolStats[statName] = value;
        DialogueLua.SetVariable(statName, value);
    }
}
// using System.Collections.Generic;
// using UnityEngine;
// using PixelCrushers.DialogueSystem;
// using TMPro;

// public class PlayerManager : MonoBehaviour
// {
//     // 玩家属性字典，key为属性名，value为属性值
//     private Dictionary<string, int> stats = new Dictionary<string, int>();
    
//     public TextMeshProUGUI wisdomText;
//     public TextMeshProUGUI hardworkingText;
//     public TextMeshProUGUI angerText;

//     void Update()
//     {
//         int wisdom = DialogueLua.GetVariable("Wisdom").asInt;
//         int hardworking = DialogueLua.GetVariable("Hardworking").asInt;
//         int anger = DialogueLua.GetVariable("Anger").asInt;

//         //更新stats字典
//         stats["Wisdom"] = wisdom;
//         stats["Hardworking"] = hardworking;
//         stats["Anger"] = anger;

//         //更新UI显示
//         wisdomText.text = $"Wisdom: {wisdom}";
//         hardworkingText.text = $"Hardworking: {hardworking}";
//         angerText.text = $"Anger: {anger}";
//     }

//     // 初始化属性,自动同步所有 DSU 变量
//     public void Start()
//     {
//         var variables = DialogueManager.masterDatabase.variables;
//         foreach (var variable in variables)
//         {
//             // 只同步数字类型属性
//             if (variable.Type == PixelCrushers.DialogueSystem.FieldType.Number)
//             {
//                 stats[variable.Name] = DialogueLua.GetVariable(variable.Name).asInt;
//             }
//         }
//     }

//     // 获取属性值
//     public int GetStat(string statName)
//     {
//         if (stats.ContainsKey(statName))
//             return stats[statName];
//         else
//             return 0;
//     }

//     // 增加或减少属性值
//     public void AddStat(string statName, int valueChange)
//     {
//         if (!stats.ContainsKey(statName))
//             stats[statName] = 0;
//         stats[statName] += valueChange;

//         DialogueLua.SetVariable(statName, stats[statName]);
//     }
// }