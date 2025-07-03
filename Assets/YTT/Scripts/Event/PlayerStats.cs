using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public Dictionary<string, int> stats = new Dictionary<string, int>();

    void Start()
    {
        stats["Charm"] = 40;
        stats["Wealth"] = 100;
    }

    public int GetStat(string statName)
    {
        return stats.ContainsKey(statName) ? stats[statName] : 0;
    }

    public void AddStat(string statName, int amount)
    {
        if (stats.ContainsKey(statName))
            stats[statName] += amount;
    }
}
