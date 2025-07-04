using UnityEngine;

[CreateAssetMenu(fileName = "New IntelCardData", menuName = "Card/Intel Card Data")]
public class IntelCardData : CardData
{


    private void Awake()
    {
        cardType = CardType.Intel; // 设置卡片类型为 Intel
    }
}