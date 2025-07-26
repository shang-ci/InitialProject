using PixelCrushers.DialogueSystem;
using UnityEngine;

public class DialogueLuaBridge : MonoBehaviour
{
    void Awake()
    {
        // 把 C# 方法注册到 Lua 环境
        Lua.RegisterFunction("RegisterChoiceInLua", this, 
            typeof(DialogueLuaBridge).GetMethod("RegisterChoice"));

        if (Character.Instance != null)
        {
            Lua.RegisterFunction("GetAttribute", Character.Instance, typeof(Character).GetMethod("GetAttribute"));
            Lua.RegisterFunction("AddToBaseAttribute", Character.Instance, typeof(Character).GetMethod("AddToBaseAttribute"));
        }

        if(QuestConditionChecker.Instance != null)
        {
            Debug.Log("QuestConditionChecker.Instance is not null, registering methods...");
            // 注册检查奈布哈尼拜访条件的方法
            Lua.RegisterFunction("CanTriggerNabuhaniVisit", QuestConditionChecker.Instance, typeof(QuestConditionChecker).GetMethod("CanTriggerNabuhaniVisit"));

            //创建事件方法
            Lua.RegisterFunction("CreateEventByID", CharacterEventManager.Instance, typeof(CharacterEventManager).GetMethod("CreateEventByID"));
        }
    }

    // 要暴露给 Lua 的方法
    public void RegisterChoice(string choiceID)
    {
        GameManager.Instance.RegisterChoice(choiceID);
    }
}