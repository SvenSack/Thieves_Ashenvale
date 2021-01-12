using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class ConActionSelect: MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform optionCage;

        private List<GameObject> entryButtons = new List<GameObject>();
        
        public void CreateToggles()
        {
            foreach (var card in UIManager.Instance.discardList)
            {
                CreateAndSetDefault(card);
            }
            AdjustPositions();
        }
        
        public void AdjustPositions()
        { // called whenever one piece moves
            for (int i = 0; i < entryButtons.Count; i++)
            {
                int row = Mathf.FloorToInt(i / 5f);
                int column = i - 5*row;
                entryButtons[i].transform.position = optionCage.position + new Vector3(-160 + 80*column, 230 - 70*row, 0); // TODO: fix this
            }
        }

        public void DropAll()
        {
            foreach (var obj in entryButtons)
            {
                Destroy(obj);
            }
            
            entryButtons = new List<GameObject>();
        }

        private void CreateAndSetDefault(CardEntry entry)
        {
            GameObject inst = Instantiate(buttonPrefab, transform);
            ConReclaimButton button = inst.GetComponent<ConReclaimButton>();
            button.SetCard(entry);
            entryButtons.Add(inst);
        }
    }
}
