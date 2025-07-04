// CardSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("���Ʋ�����")]
    public CardType acceptedCardType;
    public int level;
    public Card child; // ��ǰ�����ڵĿ��ƣ�����еĻ���


    /// <summary>
    /// ���õ�ǰ�����ڵĿ���
    /// </summary>
    /// <param name="card">�������Ŀ���</param>
    public void SetChild(Card card)
    {
        //card.SetNewParent(transform); // ���¿��Ƶĸ�����Ϊ��ǰ����
        //card.SetupCard(); // ���¿��Ƶ���ʾ
        //card.SetCardPos(); // ���ÿ���λ��Ϊ��������

        //child = card;

        if (card == null) return;
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
        child = card;
        card.SetNewParent(this.transform);

    }


    // �����Ʒ����¼�
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop event triggered on " + gameObject.name);

        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        Card draggedCard = droppedObject.GetComponent<Card>();
        if (draggedCard == null) return;

        // �����жϣ������Ƿ�ƥ��
        if (draggedCard.cardData.cardType == acceptedCardType)
        {
            // �жϿ����Ƿ��Ѿ���ռ��
            if (transform.childCount > 0)
            {
                // --- ���ƽ����߼� ---
                Debug.Log("�����ѱ�ռ�ã�ִ�н���������");

                // ��ȡ������ԭ�еĿ���
                Card cardInSlot = transform.GetChild(0).GetComponent<Card>();
                if (cardInSlot != null && cardInSlot != draggedCard) // ȷ�����Ǻ��Լ�����
                {
                    // ��ȡ����ק���Ƶ�ԭʼλ��
                    Transform originalCardParent = draggedCard.OriginalParent;

                    // 1. �������еľɿ����ƶ�������ק���Ƶ�ԭʼλ��
                    cardInSlot.transform.SetParent(originalCardParent);
                    cardInSlot.transform.localPosition = Vector3.zero;
                    cardInSlot.SetNewParent(originalCardParent); // ���¾ɿ��Ƶġ��ҡ�
                    SetChild(cardInSlot); // ����ɿ��Ƶ��ӿ�������

                    // 2. ������ק���¿��Ʒ��õ���ǰ����
                    draggedCard.transform.SetParent(this.transform);
                    draggedCard.transform.localPosition = Vector3.zero;
                    draggedCard.SetNewParent(this.transform); // �����¿��Ƶġ��ҡ�
                    SetChild(draggedCard); // ���µ�ǰ���۵��ӿ���
                }
            }
            else
            {
                // --- �򵥷����߼� (����Ϊ��) ---
                Debug.Log($"�ɹ����ÿ���: {draggedCard.cardData.cardName} ������: {gameObject.name}");

                draggedCard.transform.SetParent(this.transform);
                draggedCard.transform.localPosition = Vector3.zero;
                draggedCard.SetNewParent(this.transform);
                SetChild(draggedCard); // ���µ�ǰ���۵��ӿ���
            }
        }
        else
        {
            Debug.LogWarning($"���Ͳ�ƥ��! ������Ҫ {acceptedCardType}, ���������� {draggedCard.cardData.cardType}.");
            // ���Ͳ�ƥ��ʱ��Card.cs�е�OnEndDrag���Զ�������ԭλ
        }
    }
}
