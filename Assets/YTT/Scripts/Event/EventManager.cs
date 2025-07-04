using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public List<MapEvent> allEvents;
    public Transform eventButtonParent;
    public GameObject eventButtonPrefab;

    public int currentDay = 1;
    public PlayerStats playerStats; // 玩家属性管理器

    void Start()
    {
        RefreshEvents();
    }
    public void RefreshEvents()
    {
        foreach (Transform child in eventButtonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var mapEvent in allEvents)
        {
            int statValue = playerStats.GetStat(mapEvent.statToCheck);

            if (mapEvent.IsAvailable(currentDay, statValue))
            {
                var btn = Instantiate(eventButtonPrefab, eventButtonParent);
                btn.GetComponent<MapEventTrigger>().mapEvent = mapEvent;
                // 设置图标、标题等 UI
            }
        }
    }
}
