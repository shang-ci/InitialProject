using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("���Ʋ�����")]
    public CardType acceptedCardType;
    public int level;
    public Card child; // ��ǰ�����ڵĿ���



    [Header("������ɫ")]
    public Color roleColor = new Color(0.6f, 0.8f, 1f);   // ��ɫ������
    public Color intelColor = new Color(1f, 0.9f, 0.5f);  // �鱨������
    public Color bookColor = new Color(0.8f, 1f, 0.8f);   // �鼮������
    public Color coinColor = new Color(1f, 0.85f, 0.6f);  // ��ң�����


    private void Awake()
    {
        SetSlotColor(acceptedCardType); // ��ʼ��������ɫ
    }

    /// <summary>
    /// �����������ÿ�����ɫ
    /// </summary>
    public void SetSlotColor(CardType type)
    {
        Image img = GetComponent<Image>();
        if (img == null) return;
        switch (type)
        {
            case CardType.Role:
                img.color = roleColor;
                break;
            case CardType.Intel:
                img.color = intelColor;
                break;
            case CardType.Book:
                img.color = bookColor;
                break;
            case CardType.Coin:
                img.color = coinColor;
                break;
        }
    }


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
        child = GetComponentInChildren<Card>();
        card.SetNewParent(this.transform);
        card.SetupCard(); // ���¿��Ƶ���ʾ
    }


    // �������Ʒ����¼�
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        Card draggedCard = droppedObject.GetComponent<Card>();
        if (draggedCard == null) return;

        if (draggedCard.cardData.cardType == acceptedCardType)
        {
            if (child != null && child != draggedCard)
            {
                // ��������ԭ�п��ƷŻ���ק���Ƶ�ԭ������
                Transform oldParent = draggedCard.OriginalParent;
                child.transform.SetParent(oldParent);
                child.transform.localPosition = Vector3.zero;
                child.SetNewParent(oldParent);

                // ����ԭ�������child�������CardSlot��
                CardSlot oldSlot = oldParent.GetComponent<CardSlot>();
                if (oldSlot != null)
                {
                    oldSlot.child = child;
                }
            }

            // �����¿���
            SetChild(draggedCard);
        }
        else
        {
            Debug.LogWarning($"���Ͳ�ƥ��! ������Ҫ {acceptedCardType}, ���������� {draggedCard.cardData.cardType}.");
        }
    }

    public bool HasCard()
    {
        return child != null;
    }

    public Card GetCard()
    {
        return child;
    }
}
