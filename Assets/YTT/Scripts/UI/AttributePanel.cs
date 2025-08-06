using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributePanel : MonoBehaviour
{
    [Header("属性文本显示")]
    [SerializeField] private TextMeshProUGUI wisdomText;
    [SerializeField] private TextMeshProUGUI hardworkingText;
    [SerializeField] private TextMeshProUGUI angerText;
    [SerializeField] private TextMeshProUGUI physiqueText;
    [SerializeField] private TextMeshProUGUI socialText;
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI charmText;
    [SerializeField] private TextMeshProUGUI combatText;

    [Header("面板设置")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        // 初始隐藏面板
        HidePanel();
    }

    private void Update()
    {
        // 检测按键切换面板显示状态
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("C键被按下");
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup组件未找到！请检查是否已添加CanvasGroup组件。");
            return;
        }

        //Debug.Log($"切换面板状态，当前alpha值: {canvasGroup.alpha}");
        if (canvasGroup.alpha > 0)
            HidePanel();
        else
            ShowPanel();
    }

    private void ShowPanel()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 显示面板时更新属性值
        UpdateAttributeDisplay();
    }

    private void HidePanel()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void UpdateAttributeDisplay()
    {
        // 更新Character属性
        if (Character.Instance != null)
        {
            physiqueText.text = "HP: " + Character.Instance.GetAttribute("physique");
            socialText.text = "Social: " + Character.Instance.GetAttribute("social");
            survivalText.text = "Survival: " + Character.Instance.GetAttribute("survival");
            intelligenceText.text = "Intelligence: " + Character.Instance.GetAttribute("intelligence");
            charmText.text = "Charm: " + Character.Instance.GetAttribute("charm");
            combatText.text = "Combat: " + Character.Instance.GetAttribute("combat");
        }

        // 更新PlayerManager属性
        var playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            wisdomText.text = "智慧: " + playerManager.GetStat("Wisdom");
            hardworkingText.text = "勤奋: " + playerManager.GetStat("Hardworking");
            angerText.text = "怒气: " + playerManager.GetStat("Anger");
        }
    }

    // 订阅属性变化事件
    private void OnEnable()
    {
        // 如果Character类添加了属性变化事件，可以在这里订阅
    }

    private void OnDisable()
    {
        // 取消订阅事件
    }
}
