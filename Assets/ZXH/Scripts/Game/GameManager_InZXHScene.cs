using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager_InZXHScene : MonoBehaviour
{
    public static GameManager_InZXHScene Instance;

    public int currentDay = 1;

    [Header("事件数据")]
    public Event_ZXH eventUI;//保留当前事件的数据——等会专门有个事件管理器管理所有的事件
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
        Invoke("TestEventLoading", 1f);
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

    public void AddTime()
    {
        CharacterEventManager.Instance.AddTimeToAllEvents(); // 通知事件管理器所有事件增加时间
        currentDay++;
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
}
