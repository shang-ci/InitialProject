using UnityEngine;

[CreateAssetMenu(fileName = "New CoinCardData", menuName = "Card/Coin Card Data")]
public class CoinCardData : CardData
{
    [Header("金币数量")]
    public int amount;


    private void Awake()
    {
        cardType = CardType.Coin; // 设置卡片类型为 Coin
    }
}