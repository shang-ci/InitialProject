// /*
// 7.11
// YTT作出改动
// */
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public int currentDay = 1;

    public static GameManager Instance { get; private set; }
    public Transform EventUIContainer; // 事件UI容器
    public TextMeshProUGUI DayText; //UI时间文本
    public Button nextDayButton; // 下一天按钮
    public List<MapEventTrigger> allEventTrigger; //所有事件触发器
    public string preEvent;
    public string afterEvent;

    //triggeredEvents 用于记录当天已触发的事件
    private HashSet<MapEventTrigger> triggeredEvents = new HashSet<MapEventTrigger>();

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
        UpdateDayUI();
        nextDayButton.onClick.AddListener(NextDay);
        ResetEvents();

        nextDayButton.gameObject.SetActive(true);

        Invoke("TestEventLoading", 1f);
    }

    public void RegisterEvent(MapEventTrigger trigger)
    {
        triggeredEvents.Add(trigger);
    }

    public void CompleteEvent(MapEventTrigger trigger)
    {
        GameManager.Instance.RegisterEvent(trigger);
        Debug.Log($"事件 {trigger.mapEvent.eventID} 已完成并加入到已触发的事件列表！");
    }

    public bool HasEventOccurred(string eventID)
    {
        return triggeredEvents.Any(trigger => trigger.mapEvent.eventID == eventID);
    }

    public void NextDay()
    {
        //Debug.Log($"第{currentDay}天结束，属性值：{FindObjectOfType<PlayerManager>()?.GetAllStatsString()}");
        currentDay++;
        UpdateDayUI();

        //重置事件
        ResetEvents();

        //清空已触发的事件集合
        triggeredEvents.Clear();

        // 检查是否有跨天事件完成（例如：学校事件完成，办公室事件按钮显示）
        CheckEventForNextDay();

        //更新按钮状态
        RefreshButton();
    }

    private void CheckEventForNextDay()
    {
        if (HasEventBeenCompleted(preEvent))
        {
            var newEvent = allEventTrigger.FirstOrDefault(trigger => trigger.mapEvent.eventID == afterEvent);
            if (newEvent != null)
            {
                newEvent.buttonObject.SetActive(true);
                Debug.Log($"{currentDay} 事件{preEvent}已完成，触发新事件：{afterEvent}");
            }
            else
            {
                Debug.Log($"{currentDay} ，无法触发新事件");
            }
        }
        else
        {
            Debug.Log($"{currentDay} 事件{preEvent}未完成");
        }
    }

    private void RefreshButton()
    {
        foreach (var eventTrigger in allEventTrigger)
        {
            eventTrigger.RefreshButton();
        }
    }

    void UpdateDayUI()
    {
        DayText.text = $"Day : {currentDay}";
    }

    void ResetEvents()
    {
        foreach (var trigger in allEventTrigger)
        {
            trigger.ResetEventForNewDay();
        }
    }

    // void TestEventLoading()
    // {
    //     // ͨ通过ID获取时间数据
    //     EventData testEvent = DataManager.Instance.GetEventByID("E1001");

    //     if (testEvent != null)
    //     {
    //         Debug.Log("--- Event E1001 Loaded Successfully! ---");
    //         Debug.Log($"时间名称: {testEvent.EventName}");
    //         Debug.Log($"事件类型: {testEvent.EventType}");
    //         Debug.Log($"需求属性: {string.Join(" & ", testEvent.RequiredAttributes)}");
    //         Debug.Log($"持续天数: {testEvent.DurationDays}");
    //         Debug.Log($"成功奖励物品: {string.Join(", ", testEvent.RewardItemIDs)}");
    //         Debug.Log($"Prefab路径: {testEvent.EventPrefabName}");
    //         Debug.Log($"成功结果: {testEvent.SuccessfulResults}");
    //         Debug.Log($"失败结果: {testEvent.FailedResults}");

    //         if (DataManager.Instance == null)
    //         {
    //             Debug.LogError("DataManager.Instance is null!");
    //             return;
    //         }

    //         GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
    //         if (eventUIInstance == null)
    //         {
    //             Debug.LogError($"Failed to instantiate event prefab for {testEvent.EventName}");
    //             return;
    //         }

    //         Debug.Log($"Event prefab instantiated: {eventUIInstance.name}");

    //         Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();

    //         if (eventUI == null)
    //         {
    //             Debug.LogError($"No Event_ZXH component found in {eventUIInstance.name} or its children");

    //             // 打印子对象信息用于调试
    //             foreach (Transform child in eventUIInstance.transform)
    //             {
    //                 Debug.Log($"Child: {child.name}");
    //             }

    //             return;
    //         }
    //         eventUI.Initialize(testEvent);
    //     }
    // }

    public bool HasEventBeenCompleted(string eventID)
    {
        return triggeredEvents.Any(trigger => trigger.mapEvent.eventID == eventID);
    }
}
