using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用的物品管理系统——存数据
/// </summary>
public class Inventory_ZXH : MonoBehaviour
{
    public static Inventory_ZXH Instance;

    public Backpack Backpack;


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
}
