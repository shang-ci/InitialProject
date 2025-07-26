using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager_InZXHScene : MonoBehaviour
{
    public static GameManager_InZXHScene Instance;

    public int currentDay = 1;


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


    #region ZXH_CardEvent

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
        // 查找所有激活的 Event_Item 并调用 AddTime
        var eventPanels = GameObject.FindObjectsOfType<Event_Item>(true);
        foreach (var eventPanel in eventPanels)
        {
            // 只对激活的事件面板调用
            if (eventPanel.gameObject.activeInHierarchy)
            {
                eventPanel.AddTime();
            }
        }
    }

    #endregion
}
