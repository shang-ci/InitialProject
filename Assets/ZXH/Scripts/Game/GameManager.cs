using UnityEngine;

public class GameManager : MonoBehaviour
{
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

            // 下一步就是根据这个路径去加载并实例化UI预制体了
            GameObject eventUIInstance = DataManager.Instance.InstantiateEventPrefab(testEvent, EventUIContainer);
            Event_ZXH eventUI = eventUIInstance.GetComponentInChildren<Event_ZXH>();
            eventUI.Initialize(testEvent);
        }
    }
}
