using System;
using UnityEngine;

namespace Gameplay.CardManagement
{
    [System.Serializable] public class BaseCard
    {
        public int cardIndex;
        public Decklist.Cardtype type;
        public string cardName;
        public string text;
        public Sprite illustration;
    }

    [System.Serializable] public class RoleCard : BaseCard
    {
        public bool isGuild;
    }

    [System.Serializable] public class CharacterCard : BaseCard
    {
        public int health;
        public int wealth;
    }

    [System.Serializable] public class ActionCard : BaseCard
    {
    }

    [System.Serializable] public class ArtifactCard : BaseCard
    {
        public int weaponStrength;
    }

    [System.Serializable] public class ThreatCard : BaseCard
    {
        public int[] requirements = new int[6]; // thugs, thugs(D), assassins, assassins(D), coins, weaponstrength

        public void RequireText()
        {
            if (requirements[0] != 0)
            {
                text += requirements[0] + " Thugs, ";
            }

            if (requirements[1] != 0)
            {
                text += requirements[1] + " Thugs(D), ";
            }

            if (requirements[2] != 0)
            {
                text += requirements[2] + " Assassins, ";
            }

            if (requirements[3] != 0)
            {
                text += requirements[3] + " Assassins(D), ";
            }

            if (requirements[4] != 0)
            {
                text += requirements[4] + " Coins, ";
            }

            if (requirements[5] != 0)
            {
                text += requirements[5] + " Weaponstrength, ";
            }

            text = text.Remove(text.Length - 2);
        }
    }
}
