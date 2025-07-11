// /*
// 7.11
// YTT作出改动
// */
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int currentDay = 1; // ��Ϸʱ�䣬��ʼֵΪ1

    public static GameManager Instance { get; private set; }
    public Transform EventUIContainer; // 事件UI容器
    public TextMeshProUGUI DayText; //UI时间文本
    public Button nextDayButton; // 下一天按钮
    public List<MapEventTrigger> allEventTrigger; //所有事件触发器
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

        // �ȴ�һ֡ȷ��DataManager��Awake�Ѿ�ִ�����
        Invoke("TestEventLoading", 1f);
    }

    public void RegisterEvent(MapEventTrigger trigger)
    {
        triggeredEvents.Add(trigger);
    }

    public void NextDay()
    {
        Debug.Log($"第{currentDay}天结束，属性值：{FindObjectOfType<PlayerManager>()?.GetAllStatsString()}");
        currentDay++;
        UpdateDayUI();
        ResetEvents();
        triggeredEvents.Clear();
    }

    void UpdateActiveEventsTime()
    {
        var eventPanels = new List<Event_ZXH>();
        var allEventComponents = GameObject.FindObjectsOfType<Event_ZXH>();
        foreach (var component in allEventComponents)
        {
            eventPanels.Add(component);
        }
        foreach (var eventPanel in eventPanels)
        {
            if (eventPanel.gameObject.activeInHierarchy)
            {
                eventPanel.AddTime();
            }
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

    //功能添加，测试是否会报错
    /// <summary>
    /// ������Ϸʱ�䣬ÿ�ε�������1��
    /// </summary>
    // public void AddTime()
    // {
    //     Time++;
    //     Debug.Log($"Game Time increased to: {Time}");

    //     // �������м���� Event_ZXH ������ AddTime������д�¼���������
    //     var eventPanels = GameObject.FindObjectsOfType<Event_ZXH>(true);
    //     foreach (var eventPanel in eventPanels)
    //     {
    //         // ֻ�Լ�����¼�������
    //         if (eventPanel.gameObject.activeInHierarchy)
    //         {
    //             eventPanel.AddTime();
    //         }
    //     }
    // }

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

            // ��һ�����Ǹ������·��ȥ���ز�ʵ����UIԤ������
            GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
            Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();
            eventUI.Initialize(testEvent);
        }
    }
}





// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     public int Time = 1; // ��Ϸʱ�䣬��ʼֵΪ1

//     public static GameManager Instance { get; private set; }
//     public Transform EventUIContainer; // �¼�UI����

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); 
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         // �ȴ�һ֡ȷ��DataManager��Awake�Ѿ�ִ�����
//         Invoke("TestEventLoading", 1f);
//     }


//     /// <summary>
//     /// ������Ϸʱ�䣬ÿ�ε�������1��
//     /// </summary>
//     public void AddTime()
//     {
//         Time++;
//         Debug.Log($"Game Time increased to: {Time}");

//         // �������м���� Event_ZXH ������ AddTime������д�¼���������
//         var eventPanels = GameObject.FindObjectsOfType<Event_ZXH>(true);
//         foreach (var eventPanel in eventPanels)
//         {
//             // ֻ�Լ�����¼�������
//             if (eventPanel.gameObject.activeInHierarchy)
//             {
//                 eventPanel.AddTime();
//             }
//         }
//     }

//         void TestEventLoading()
//     {
//         // ͨ��ID��ȡ�������õ��¼�
//         EventData testEvent = DataManager.Instance.GetEventByID("E1001");

//         if (testEvent != null)
//         {
//             Debug.Log("--- Event E1001 Loaded Successfully! ---");
//             Debug.Log($"�¼�����: {testEvent.EventName}");
//             Debug.Log($"�¼�����: {testEvent.EventType}");
//             Debug.Log($"������: {string.Join(" & ", testEvent.RequiredAttributes)}");
//             Debug.Log($"��������: {testEvent.DurationDays}");
//             Debug.Log($"�ɹ�������Ʒ: {string.Join(", ", testEvent.RewardItemIDs)}");
//             Debug.Log($"Prefab·��: {testEvent.EventPrefabName}");
//             Debug.Log($"�ɹ����: {testEvent.SuccessfulResults}");
//             Debug.Log($"ʧ�ܽ��: {testEvent.FailedResults}");

//             // ��һ�����Ǹ������·��ȥ���ز�ʵ����UIԤ������
//             GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
//             Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();
//             eventUI.Initialize(testEvent);
//         }
//     }
// }