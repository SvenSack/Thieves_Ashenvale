using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Gameplay.CardManagement
{
    public class Decklist : MonoBehaviour
    {
    
        public static Decklist Instance;
        
        [SerializeField] Sprite[] roleSprites = new Sprite[6];
        [SerializeField] Sprite[] characterSprites = new Sprite[10];
        [SerializeField] Sprite[] actionSprites = new Sprite[10];
        [SerializeField] Sprite[] artifactSprites = new Sprite[10];
        [SerializeField] Sprite[] threatSprites = new Sprite[20];

        public enum Cardtype
        {
            Role,
            Character,
            Action,
            Artifact,
            Threat
        }
        
        [HideInInspector] public RoleCard[] roleCards;
        [HideInInspector] public CharacterCard[] characterCards;
        [HideInInspector] public ActionCard[] actionCards;
        [HideInInspector] public ArtifactCard[] artifactCards;
        [HideInInspector] public ThreatCard[] threatCards;

        [SerializeField] private GameObject[] cardPrefabs = new GameObject[5];
        [SerializeField] private TextAsset[] jsonFiles = new TextAsset[5];
        
        public Dictionary<GameMaster.Character, string> characterNames;

        void Start()
        {
            Instance = this;
            
            characterNames = new Dictionary<GameMaster.Character, string>
            {
                {GameMaster.Character.Adventurer, "Ott"},
                {GameMaster.Character.Necromancer, "Aria"},
                {GameMaster.Character.Poisoner, "Aden"},
                {GameMaster.Character.Ruffian, "Mary"},
                {GameMaster.Character.Scion, "Adeline"},
                {GameMaster.Character.Seducer, "Harkon"},
                {GameMaster.Character.Sheriff, "John"},
                {GameMaster.Character.BurglaryAce, "Steven"},
                {GameMaster.Character.OldFox, "Selene"},
                {GameMaster.Character.PitFighter, "Ruko"}
            };

            ReadData();
            
        }

        public GameObject CreateCard(Cardtype type, int index)
        { // create a card instance from a template
            GameObject inst = PhotonNetwork.Instantiate(cardPrefabs[(int)type].name, Vector3.zero, Quaternion.identity);
            switch (type)
            {
                case Cardtype.Action:
                    Card parts = inst.GetComponent<Card>();
                    ActionCard thisCard = actionCards[index];
                    parts.cardType = type;
                    parts.illustration.sprite = thisCard.illustration;
                    parts.cardName.text = thisCard.cardName;
                    parts.text.text = thisCard.text;
                    parts.cardIndex = index;
                    break;
                case Cardtype.Artifact:
                    Card partsA = inst.GetComponent<Card>();
                    ArtifactCard thisACard = artifactCards[index];
                    partsA.cardType = type;
                    partsA.illustration.sprite = thisACard.illustration;
                    partsA.cardName.text = thisACard.cardName;
                    partsA.text.text = thisACard.text;
                    partsA.extraText1.text = "Strength: " + thisACard.weaponStrength;
                    partsA.cardIndex = index;
                    break;
                case Cardtype.Character:
                    Card partsC = inst.GetComponent<Card>();
                    CharacterCard thisCCard = characterCards[index];
                    partsC.cardType = type;
                    partsC.illustration.sprite = thisCCard.illustration;
                    partsC.cardName.text = thisCCard.cardName;
                    partsC.text.text = thisCCard.text;
                    partsC.extraText1.text = thisCCard.health.ToString();
                    partsC.extraText2.text = thisCCard.wealth.ToString();
                    break;
                case Cardtype.Role:
                    Card partsR = inst.GetComponent<Card>();
                    RoleCard thisRCard = roleCards[index];
                    partsR.cardType = type;
                    partsR.illustration.sprite = thisRCard.illustration;
                    partsR.cardName.text = thisRCard.cardName;
                    partsR.text.text = thisRCard.text;
                    if (!thisRCard.isGuild)
                    {
                        partsR.icon.gameObject.SetActive(false);
                    }
                    break;
                
                case Cardtype.Threat:
                    Card partsT = inst.GetComponent<Card>();
                    ThreatCard thisTCard = threatCards[index];
                    partsT.cardType = type;
                    partsT.illustration.sprite = thisTCard.illustration;
                    partsT.cardName.text = thisTCard.cardName;
                    partsT.text.text = thisTCard.text;
                    break;
            }
            return inst;
        }

        private void ReadData()
        { // Read the data from the json files and parse it into the arrays of card templates
            roleCards = JsonUtility.FromJson<RoleCards>(jsonFiles[0].text).roleCards;
            for (int i = 0; i < roleCards.Length; i++)
            {
                roleCards[i].cardIndex = i;
                roleCards[i].illustration = roleSprites[i];
                roleCards[i].type = Cardtype.Role;
            }
            characterCards = JsonUtility.FromJson<CharacterCards>(jsonFiles[1].text).characterCards;
            for (int i = 0; i < characterCards.Length; i++)
            {
                characterCards[i].cardIndex = i;
                characterCards[i].illustration = characterSprites[i];
                characterCards[i].type = Cardtype.Character;
            }
            actionCards = JsonUtility.FromJson<ActionCards>(jsonFiles[2].text).actionCards;
            for (int i = 0; i < actionCards.Length; i++)
            {
                actionCards[i].cardIndex = i;
                actionCards[i].illustration = actionSprites[i];
                actionCards[i].type = Cardtype.Action;
            }
            artifactCards = JsonUtility.FromJson<ArtifactCards>(jsonFiles[3].text).artifactCards;
            for (int i = 0; i < artifactCards.Length; i++)
            {
                artifactCards[i].cardIndex = i;
                artifactCards[i].illustration = artifactSprites[i];
                artifactCards[i].type = Cardtype.Artifact;
            }
            threatCards = JsonUtility.FromJson<ThreatCards>(jsonFiles[4].text).threatCards;
            for (int i = 0; i < threatCards.Length; i++)
            {
                threatCards[i].cardIndex = i;
                threatCards[i].illustration = threatSprites[i];
                threatCards[i].type = Cardtype.Threat;
                threatCards[i].RequireText();
            }
        }
    }

    [Serializable]
    public class RoleCards
    {
        public RoleCard[] roleCards;
    }
    [Serializable]
    public class CharacterCards
    {
        public CharacterCard[] characterCards;
    }
    [Serializable]
    public class ActionCards
    {
        public ActionCard[] actionCards;
    }
    [Serializable]
    public class ArtifactCards
    {
        public ArtifactCard[] artifactCards;
    }
    [Serializable]
    public class ThreatCards
    {
        public ThreatCard[] threatCards;
    }
}