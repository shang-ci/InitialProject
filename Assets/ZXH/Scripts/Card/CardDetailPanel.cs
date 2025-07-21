using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDetailPanel : MonoBehaviour
{
    public static CardDetailPanel Instance { get; private set; }

    [Header("CardDetailPanel引用")]
    public GameObject cardDetailPanel;
    public TextMeshProUGUI cardNameText;//
    public TextMeshProUGUI idText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI attributesText;
    public Image artworkImage;


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // 只有面板激活时才响应
        if (cardDetailPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }
    }


    public void Show(CardData data, Vector3 position)
    {
        if (data == null) return;

        cardDetailPanel.SetActive(true);

        cardNameText.text = $"名称: {data.cardName}";
        idText.text = $"ID: {data.id}";
        //typeText.text = $"类型: {data.cardType}";
        descriptionText.text = $"描述: {data.description}";
        attributesText.text = GetAttributesString(data.GetAttributes());
        if (artworkImage != null && data.artwork != null)
            artworkImage.sprite = data.artwork;

        // 设置面板位置（可根据UI需求调整锚点/偏移）
        transform.position = position;
    }

    public void Hide()
    {
        cardDetailPanel.SetActive(false);
    }

    private string GetAttributesString(Attributes attr)
    {
        return $"体魄:{attr.physique} 社交:{attr.social} 生存:{attr.survival} 智慧:{attr.intelligence} 魅力:{attr.charm} 战斗:{attr.combat} 支持:{attr.support}";
    }
}
