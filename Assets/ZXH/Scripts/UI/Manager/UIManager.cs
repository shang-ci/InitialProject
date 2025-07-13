using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("各UI引用")]
    public Backpack Backpack; // 背包UI

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

    private void Update()
    {
        // 切换背包UI的显示状态
        if (Input.GetKeyDown(KeyCode.M))
        {
            Backpack.gameObject.SetActive(!Backpack.gameObject.activeSelf);
        }
    }
}
