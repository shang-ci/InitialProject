using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameManager_ZXh : MonoBehaviour
{
    [Header("时间管理")]
    public int Time = 1;

    public static GameManager_ZXh Instance { get; private set; }
    public Transform EventUIContainer; // 事件UI容器

 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Invoke("TestEventLoading", 1f);
    }


    /// <summary>
    /// 测试事件加载功能
    /// </summary>
    void TestEventLoading()
    {
        // ͨ通过ID获取时间数据
        EventData testEvent = DataManager.Instance.GetEventByID("E1001");

        if (testEvent != null)
        {
            Debug.Log("--- Event E1001 Loaded Successfully! ---");
            Debug.Log($"时间名称: {testEvent.EventName}");
            Debug.Log($"事件类型: {testEvent.EventType}");
            Debug.Log($"需求属性: {string.Join(" & ", testEvent.RequiredAttributes)}");
            Debug.Log($"持续天数: {testEvent.DurationDays}");
            Debug.Log($"成功奖励物品: {string.Join(", ", testEvent.RewardItemIDs)}");
            Debug.Log($"Prefab路径: {testEvent.EventPrefabName}");
            Debug.Log($"成功结果: {testEvent.SuccessfulResults}");
            Debug.Log($"失败结果: {testEvent.FailedResults}");

            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager.Instance is null!");
                return;
            }

            GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
            if (eventUIInstance == null)
            {
                Debug.LogError($"Failed to instantiate event prefab for {testEvent.EventName}");
                return;
            }

            Debug.Log($"Event prefab instantiated: {eventUIInstance.name}");

            Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();

            if (eventUI == null)
            {
                Debug.LogError($"No Event_ZXH component found in {eventUIInstance.name} or its children");

                // 打印子对象信息用于调试
                foreach (Transform child in eventUIInstance.transform)
                {
                    Debug.Log($"Child: {child.name}");
                }

                return;
            }
            eventUI.Initialize(testEvent);
        }
    }

    /// <summary>
    /// 增加游戏时间
    /// </summary>
    public void AddTime()
    {
        Time++;
        Debug.Log($"Game Time increased to: {Time}");

        // 查找所有激活的 Event_ZXH 并调用 AddTime
        var eventPanels = GameObject.FindObjectsOfType<Event_ZXH>(true);
        foreach (var eventPanel in eventPanels)
        {
            // 只对激活的事件面板调用
            if (eventPanel.gameObject.activeInHierarchy)
            {
                eventPanel.AddTime();
            }
        }
    }
}
