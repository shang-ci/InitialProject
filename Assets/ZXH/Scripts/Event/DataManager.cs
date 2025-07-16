using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 数据管理器，负责加载和存储所有事件数据
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // 使用字典存储所有事件，Key是EventID，Value是事件数据，我们可以知道事件的名字来查找ID然后再拿到数据
    // 通过ID查找事件
    private Dictionary<string, EventData> eventDatabase = new Dictionary<string, EventData>();
    private Dictionary<string, string> eventNameToIdMap = new Dictionary<string, string>();


    // CSV文件名，我们稍后会把它放在 Resources 文件夹下
    private const string EVENT_DATA_FILE_NAME = "EventData/EventData_ZXH";
    // 约定所有事件UI预制体存放的Resources子路径
    public const string EVENT_PREFAB_FOLDER_PATH = "EventPrefabs/";

    void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保在切换场景时数据管理器不被销毁
            LoadEventData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 从 Resources 文件夹加载并解析事件数据CSV文件
    /// </summary>
    private void LoadEventData()
    {
        // 从Resources文件夹加载文本文件
        TextAsset csvFile = Resources.Load<TextAsset>(EVENT_DATA_FILE_NAME);
        if (csvFile == null)
        {
            Debug.LogError($"Failed to load event data file: '{EVENT_DATA_FILE_NAME}.csv'. Make sure it's in a 'Resources' folder.");
            return;
        }

        // 按行分割文本内容，并跳过第一行（表头）
        //每一行的数据
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            //单个事件的每一个具体数据
            string[] values = lines[i].Split(','); // 简单的CSV解析

            // 注意：这种简单的Split方式如果您的文本内容（如Story）中包含逗号，会导致解析错误    
            // 生产环境中建议使用更健壮的CSV解析库，但对于原型，这已足够   

            if (values.Length > 0 && !string.IsNullOrEmpty(values[0]))
            {
                EventData newEvent = new EventData(values);
                if (!eventDatabase.ContainsKey(newEvent.EventID))
                {
                    eventDatabase.Add(newEvent.EventID, newEvent);

                    //填充名字到ID的映射字典
                    if (!eventNameToIdMap.ContainsKey(newEvent.EventName))
                    {
                        eventNameToIdMap.Add(newEvent.EventName, newEvent.EventID);
                    }
                    else
                    {
                        // 处理重名事件的警告
                        Debug.LogWarning($"Duplicate EventName found: '{newEvent.EventName}'. Searching by this name will only return the first loaded event with ID '{eventNameToIdMap[newEvent.EventName]}'.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Duplicate EventID found: {newEvent.EventID}. The later entry will be ignored.");
                }
            }
        }

        Debug.Log($"Successfully loaded {eventDatabase.Count} events into the database.");
    }


    #region 公共接口
    /// <summary>
    /// 根据ID获取事件数据
    /// </summary>
    /// <param name="eventID">事件的唯一ID</param>
    /// <returns>返回事件数据，如果未找到则返回null</returns>
    public EventData GetEventByID(string eventID)
    {
        if (eventDatabase.TryGetValue(eventID, out EventData eventData))
        {
            return eventData;
        }

        Debug.LogWarning($"Event with ID '{eventID}' not found in the database.");
        return null;
    }

    /// <summary>
    /// 根据事件名称获取事件数据
    /// </summary>
    public EventData GetEventByName(string eventName)
    {
        if (eventNameToIdMap.TryGetValue(eventName, out string eventID))
        {
            // 先通过名字找到ID，再通过ID获取完整数据
            return GetEventByID(eventID);
        }

        Debug.LogWarning($"Event with Name '{eventName}' not found in the database.");
        return null;
    }

    /// <summary>
    /// 加载并实例化一个事件的UI预制体
    /// </summary>
    /// <param name="eventData">要为其加载预制体的事件</param>
    /// <param name="parent">实例化的预制体要挂载的父节点</param>
    /// <returns>返回实例化的GameObject，如果失败则返回null</returns>
    public GameObject InstantiateEventPrefab(EventData eventData, Transform parent = null)
    {
        if (eventData == null || string.IsNullOrEmpty(eventData.EventPrefabName))
        {
            Debug.LogError("Cannot instantiate prefab: EventData is null or PrefabName is empty.");
            return null;
        }

        // 拼接完整的Resources路径
        string prefabPath = EVENT_PREFAB_FOLDER_PATH + eventData.EventPrefabName;

        // 从Resources加载预制体
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Failed to load event prefab from path: 'Resources/{prefabPath}'. Make sure the prefab exists and the name is correct.");
            return null;
        }

        // 实例化并设置父节点
        return Instantiate(prefab, parent);
    }
    #endregion
}
