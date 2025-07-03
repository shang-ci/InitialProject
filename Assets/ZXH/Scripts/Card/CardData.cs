// CardData.cs
using UnityEngine;

// 使用CreateAssetMenu特性，我们可以在Unity编辑器的 "Create" 菜单中直接创建这种资源文件。
[CreateAssetMenu(fileName = "New CardData", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("卡牌基础信息")]
    public string id;
    public string cardName;
    public CardType cardType;

    [Header("卡牌视觉表现")]
    public Sprite artwork; // 卡牌的图画
    // public GameObject cardPrefab; // 如果卡牌有复杂的3D模型或特效，可以在这里引用
}