using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterEventManager : MonoBehaviour
{
    public static CharacterEventManager Instance { get; private set; }

    [Header("事件数据")]
    [Tooltip("这里可以引用DataManager中的所有事件数据，或在启动时从DataManager加载")]
    public List<EventData> allEventData = new List<EventData>();// 所有事件数据
    public List<EventBase> activeEventsList = new List<EventBase>();// 当前激活的事件实例


    [Header("运行时状态 (使用高效数据结构)")]
    public Dictionary<string, EventBase> activeEventsDic = new Dictionary<string, EventBase>();  //当前激活的事件实例DIC版本
    public HashSet<string> completedEventIDs = new HashSet<string>(); //已完成的事件ID集合

    [Header("UI容器")]
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

    private void Start()
    {

    }

    private void OnEnable()
    {
        Lua.RegisterFunction("CreateEventByID", this, GetType().GetMethod("CreateEventByID"));
    }

    private void OnDisable()
    {
        Lua.UnregisterFunction("CreateEventByID");
    }

    #region 辅助
    /// <summary>
    /// 检查一个事件当前是否处于激活状态
    /// </summary>
    public bool IsEventActive(string eventID)
    {
        return activeEventsDic.ContainsKey(eventID);
    }

    /// <summary>
    /// 检查一个事件是否已经被标记为已完成
    /// </summary>
    public bool IsEventCompleted(string eventID)
    {
        return completedEventIDs.Contains(eventID);
    }

    /// <summary>
    /// 获取所有激活事件的实例集合
    /// </summary>
    public ICollection<EventBase> GetActiveEvents()
    {
        return activeEventsDic.Values;
    }

    /// <summary>
    /// 获取所有尚未激活且尚未完成的“可用”事件定义
    /// </summary>
    public List<EventData> GetAvailableEvents()
    {
        return allEventData.Where(data =>
            !IsEventActive(data.EventID) && !IsEventCompleted(data.EventID)
        ).ToList();
    }

    /// <summary>
    /// 通过ID查找事件数据
    /// </summary>
    public EventData GetEventDataByID(string id)
    {
        return allEventData.Find(e => e.EventID == id);
    }

    /// <summary>
    /// 批量推进所有事件一天
    /// </summary>
    public void AddTimeToAllEvents()
    {
        foreach (var evt in activeEventsDic.Values)
        {
            evt.AddTime();
        }
    }
    #endregion

    #region 事件生命周期管理 
    /// <summary>
    /// [内部调用] 注册一个事件实例到激活字典中
    /// 建议此方法由 EventBase 的 Initialize 方法调用
    /// </summary>
    public void RegisterEvent(EventBase evt)
    {
        if (evt == null || evt.eventData == null) return;

        string id = evt.eventData.EventID;
        if (!activeEventsDic.ContainsKey(id))
        {
            activeEventsDic[id] = evt;
        }
        else
        {
            Debug.LogWarning($"试图注册一个已在激活列表中的事件: {id}");
        }
    }

    /// <summary>
    /// [内部调用] 从激活字典中注销一个事件实例
    /// </summary>
    public void UnregisterEvent(EventBase evt)
    {
        if (evt != null && evt.eventData != null)
        {
            activeEventsDic.Remove(evt.eventData.EventID);
        }
    }

    /// <summary>
    /// [内部调用] 将一个事件ID标记为已完成——当关闭事件时统一将事件标记为已完成
    /// </summary>
    public void MarkEventAsCompleted(string eventID)
    {
        if (!completedEventIDs.Contains(eventID))
        {
            completedEventIDs.Add(eventID);
        }
    }
    #endregion

    #region 创建、关闭事件
    /// <summary>
    /// 关闭一个事件，将其从激活列表移除，并标记为已完成。
    /// </summary>
    public void CloseEvent(EventBase evt)
    {
        if (evt == null) return;

        UnregisterEvent(evt);
        MarkEventAsCompleted(evt.eventData.EventID);// 当关闭事件时统一将事件标记为已完成

        // 调用事件自身的清理方法 (例如销毁GameObject)
        evt.CloseEvent();
    }

    /// <summary>
    /// 创建一个新的事件实例
    /// </summary>
    /// <param name="eventID"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public EventBase CreateEventByID(string eventID )
    {
        var eventData = DataManager.Instance.GetEventByID(eventID);
        if (eventData == null) return null;
        //判断该事件能否被创建
        if (CanBeCreated(eventData))
        {
            Debug.Log($"尝试创建事件: {eventID}");
        }
        else
        {
            Debug.LogWarning($"无法创建事件: {eventID}，可能是因为条件不满足");
            return null;
        }

        GameObject eventObj = DataManager.Instance.InstantiateEventPrefab(eventData, EventUIContainer);
        EventBase evt = eventObj.GetComponentInChildren<EventBase>();
        if (evt != null)
        {
            evt.Initialize(eventData);
            //遵循“创建者负责注册”的原则
            RegisterEvent(evt); 
        }
        return evt;
    }

    /// <summary>
    /// 判断一个事件当前是否满足所有被创建的条件
    /// </summary>
    /// <returns>true表示可以创建，false表示不行</returns>
    private bool CanBeCreated(EventData eventData)
    {
        if (eventData == null) return false;

        // 1. 通用检查：事件是否已激活
        //if (activeEventsList.ContainsKey(eventData.EventID)) return false;

        // 2. 通用检查：不可重复的事件是否已完成
        //if (!eventData.IsRepeatable && IsEventCompleted(eventData.EventID)) return false;

        // 3. 遍历并检查所有具体条件 (这段代码和上一版完全一样，但现在它处理的是从CSV动态创建的条件)
        foreach (var condition in eventData.Conditions)
        {
            if (!condition.IsMet())
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}
