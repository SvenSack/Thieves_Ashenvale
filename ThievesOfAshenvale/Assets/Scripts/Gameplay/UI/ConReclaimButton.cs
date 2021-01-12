using System;
using Gameplay.CardManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class ConReclaimButton : MonoBehaviour
    {
        [SerializeField] private GameObject textObject;
        
        public CardEntry representedCard;

        private void Start()
        {
            textObject.SetActive(false);
        }

        public void SetCard(CardEntry targetCard)
        {
            representedCard = targetCard;
            ActionCard target = Decklist.Instance.actionCards[targetCard.index];
            SetText(target.cardName);
        }

        public void ToggleHover()
        {
             textObject.SetActive(!textObject.activeSelf);
        }
        
        public void ButtonPress()
        {
            UIManager.Instance.participant.RpcHandCard((byte)representedCard.index,(byte) representedCard.type);
            UIManager.Instance.ResetAfterSelect();
        }

        private void SetText(string text)
        {
            textObject.SetActive(true);
            textObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }
    }
}
