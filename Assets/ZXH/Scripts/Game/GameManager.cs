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
    public Event_Item eventUI;
    private Dictionary<string, int> eventFirstAvailableDay = new Dictionary<string, int>();



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

        //初始化创建事件
        CharacterEventManager.Instance.TriggerNewEvents(); // 通知事件管理器触发新事件
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

        CharacterEventManager.Instance.ProcessActiveEventsOnNewDay(); //处理所有激活事件的逻辑——该标记、注销的都被处理
        CharacterEventManager.Instance.TriggerNewEvents(); // 通知事件管理器触发新事件——先让所有事件增加时间，再触发新事件

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
            Debug.Log($"事件 {eventID} 已在第 {currentDay} 天被选择");
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
