/*
7.11
YTT作出改动
*/
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;

public class GameManager : MonoBehaviour
{
    public int currentDay = 1;
    public static GameManager Instance { get; private set; }
    public Transform EventUIContainer; // 事件UI容器
    public TextMeshProUGUI DayText; //UI时间文本
    public Button nextDayButton; // 下一天按钮
    public List<MapEventTrigger> allEventTrigger; //所有事件触发器

    //triggeredEvents 记录当天已触发的事件
    private HashSet<MapEventTrigger> triggeredEvents = new HashSet<MapEventTrigger>();
    //wonTriggeredEvents 记录当前胜利了的已触发事件
    private HashSet<MapEventTrigger> wonTriggeredEvents = new HashSet<MapEventTrigger>();
    private Dictionary<string, int> choiceBasedEvents = new Dictionary<string, int>();

    [Header("事件数据")]
    public Event_ZXH eventUI;
    private Dictionary<string, int> eventFirstAvailableDay = new Dictionary<string, int>();

    [Header("事件参数")]
    [SerializeField] private float successProbability = 1f;//成功概率
    [SerializeField] private int numberOfSuccesses = 0;//成功次数
    [SerializeField] private bool isSuccess;

    [Header("事件容器")]
    public Transform EventUIContainer_Card; // 卡牌事件UI容器

    [Header("库存")]
    public Inventory Inventory; // 背包系统引用

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


    public int GetFirstAvailableDay(string eventID)
    {
        if (eventFirstAvailableDay.TryGetValue(eventID, out int day))
            return day;
        return -1;
    }

    public void SetFirstAvailableDay(string eventID, int day)
    {
        if (!eventFirstAvailableDay.ContainsKey(eventID))
            eventFirstAvailableDay[eventID] = day;
    }

    #region ZXH_CardEvent
    /// <summary>
    /// 测试事件加载功能
    /// </summary>
    void TestEventLoading()
    {
        //通过ID获取时间数据
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

            GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer_Card);
            if (eventUIInstance == null)
            {
                Debug.LogError($"Failed to instantiate event prefab for {testEvent.EventName}");
                return;
            }

            Debug.Log($"Event prefab instantiated: {eventUIInstance.name}");

            eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();

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
    /// 事件通知_所有事件拿到时间
    /// </summary>
    private void EventTake()
    {
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

    /// <summary>
    /// 掷骰子，返回是否成功,successProbability=0.5表示50%概率成功
    /// </summary>
    public bool RollTheDice(EventData eventData, float successProbability)
    {
        Debug.Log($"Event_ZXH: 掷骰子，成功概率为 {successProbability * 100}%");
        int diceSum = GetAllDiceCount(eventData);//骰子个数
        int threshold = eventData.SuccessThreshold;//成功阈值

        numberOfSuccesses = 0;//成功次数

        successProbability = Mathf.Clamp01(successProbability);

        for (int i = 1; i <= diceSum; i++)
        {
            // 掷骰子
            float rand = Random.value; // [0,1)
            bool isSuccess = rand < successProbability;
            if (isSuccess)
            {
                numberOfSuccesses++;
            }
        }

        if (numberOfSuccesses >= threshold)
        {
            isSuccess = true;
        }
        else
        {
            isSuccess = false;
        }

        return isSuccess;
    }

    /// <summary>
    /// 获得需要的所有属性的总和——可以用来产生骰子的个数
    /// </summary>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    public int GetAllDiceCount(EventData eventData)
    {
        int sum = 0;
        foreach (var attribute in eventData.RequiredAttributes)
        {
            int attributeValue = Character.Instance.GetAttribute(attribute);
            sum += attributeValue;
        }
        return sum;
    }

    #endregion


    //注册事件触发
    public void RegisterEvent(MapEventTrigger trigger)
    {
        triggeredEvents.Add(trigger);
        //Debug.Log($"事件{trigger.mapEvent.eventID}已触发");
        MapEvent mapEvent = trigger.mapEvent;
        Condition winCondition = mapEvent.winCondition;
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        int currentStatValue = 0;

        if (playerManager != null)
        {
            currentStatValue = playerManager.GetStat(winCondition.statToCheck);
        }
        if (currentStatValue >= winCondition.minValueRequired)
        {
            wonTriggeredEvents.Add(trigger);
            //Debug.Log($"事件{trigger.mapEvent.eventID}已成功");
        }

        //！！！重要：刷新按钮状态后即可开启后续事件！！！
        RefreshButton();
    }

    public void NextDay()
    {
        currentDay++;
        UpdateDayUI();

        EventTake();

        //重置事件
        ResetEvents();

        //清空已触发的事件集合
        triggeredEvents.Clear();
        //清空胜利了的已触发事件集合
        wonTriggeredEvents.Clear();
        //清空基于对话选项选择的事件集合
        //choiceBasedEvents.Clear();

        //更新按钮状态
        RefreshButton();
    }


    //刷新所有的事件按钮状态
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

    //新一天重置所有事件状态
    void ResetEvents()
    {
        foreach (var trigger in allEventTrigger)
        {
            trigger.ResetEventForNewDay();
        }
    }

    //检测事件是否触发
    public bool HasEventBeenCompleted(string eventID)
    {
        return triggeredEvents.Any(trigger => trigger.mapEvent.eventID == eventID);
    }

    //检测触发了的事件是否胜利
    public bool HasCompletedEventSuccessfully(string eventID)
    {
        return wonTriggeredEvents.Any(trigger => trigger.mapEvent.eventID == eventID);
    }

    public void RegisterChoice(string eventID)
    {
        if (!choiceBasedEvents.ContainsKey(eventID))
        {
            choiceBasedEvents.Add(eventID, currentDay);
            //Debug.Log($"事件 {eventID} 已在第 {currentDay} 天被选择");
        }
        else
        {
            //Debug.Log($"事件 {eventID} 已被选择过");
        }

        //！！！重要：刷新按钮状态后即可开启后续事件！！！(针对于状态先true后false的情况)
        RefreshButton();
    }

    public bool HasMadeChoice(string eventID, int activeAfterDays = 0)
    {
        if (choiceBasedEvents.TryGetValue(eventID, out int choiceDay))
        {
            bool isActiveDayResult = currentDay == choiceDay + activeAfterDays;
            //Debug.Log($"事件 {eventID} 在第 {choiceDay} 天被选择，当前第 {currentDay} 天，结果: {isActiveDayResult}");
            return isActiveDayResult;
        }

        //Debug.Log($"事件 {eventID} 未被选择");
        return false;
    }
}
