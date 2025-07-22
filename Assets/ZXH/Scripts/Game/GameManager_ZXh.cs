using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager_ZXh : MonoBehaviour
{
    [Header("时间管理")]
    public int Time = 1;

    public static GameManager_ZXh Instance { get; private set; }
    public Transform EventUIContainer; // 事件UI容器

    [Header("骰子")]
    [SerializeField] private float successProbability = 1f;//成功概率
    [SerializeField] private int t = 0;//成功次数
    [SerializeField] private bool isSuccess;

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

        Time++;
        Debug.Log($"Game Time increased to: {Time}");
    }


    #region 事件
    /// <summary>
    /// 掷骰子，判断事件是否成功
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="successProbability"></param>
    /// <returns></returns>
    public bool RollTheDice(EventData eventData, float successProbability)
    {
        Debug.Log($"Event_ZXH: 掷骰子，成功概率为 {successProbability * 100}%");

        int diceSum = 3;//GetAllValueTextSum();//骰子个数
        int threshold = eventData.SuccessThreshold;//成功阈值

        t = 0;//成功次数

        successProbability = Mathf.Clamp01(successProbability);

        for (int i = 1; i <= diceSum; i++)
        {
            // 掷骰子
            float rand = Random.value; // [0,1)
            bool isSuccess = rand < successProbability;
            if (isSuccess)
            {
                t++;
            }
        }

        if (t >= threshold)
        {
            isSuccess = true;
        }
        else
        {
            isSuccess = false;
        }

        return isSuccess;
    }
    #endregion
}
