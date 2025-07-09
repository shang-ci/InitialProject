using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int Time = 1; // 游戏时间，初始值为1

    public static GameManager Instance { get; private set; }
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
        // 等待一帧确保DataManager的Awake已经执行完毕
        Invoke("TestEventLoading", 1f);
    }


    /// <summary>
    /// 增加游戏时间，每次调用增加1天
    /// </summary>
    public void AddTime()
    {
        Time++;
        Debug.Log($"Game Time increased to: {Time}");

        // 查找所有激活的 Event_ZXH 并调用 AddTime——不写事件管理器了
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

        void TestEventLoading()
    {
        // 通过ID获取我们配置的事件
        EventData testEvent = DataManager.Instance.GetEventByID("E1001");

        if (testEvent != null)
        {
            Debug.Log("--- Event E1001 Loaded Successfully! ---");
            Debug.Log($"事件名称: {testEvent.EventName}");
            Debug.Log($"事件类型: {testEvent.EventType}");
            Debug.Log($"剧情: {testEvent.Story}");
            Debug.Log($"所需属性: {string.Join(" & ", testEvent.RequiredAttributes)}");
            Debug.Log($"持续天数: {testEvent.DurationDays}");
            Debug.Log($"成功后奖励物品: {string.Join(", ", testEvent.RewardItemIDs)}");
            Debug.Log($"Prefab路径: {testEvent.EventPrefabName}");
            Debug.Log($"成功结果: {testEvent.SuccessfulResults}");
            Debug.Log($"失败结果: {testEvent.FailedResults}");

            // 下一步就是根据这个路径去加载并实例化UI预制体了
            GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
            Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();
            eventUI.Initialize(testEvent);
        }
    }
}
