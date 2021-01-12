using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class AntiThreatAssigner : AssignmentChoice
    {
        private int totalThreat;
        
        public override void CreateToggles()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                bool isTutorialEnemy = false;
                if (GameMaster.Instance.isTutorial)
                {
                    if (piece.TryGetComponent(out ThreatPiece tPiece))
                    {
                        Debug.LogAssertion("found a tPiece");
                        if (UIManager.Instance.participant.piecesThreateningMe.Contains(tPiece))
                        {
                            Debug.LogAssertion("Sorted out one enemy");
                            isTutorialEnemy = true;
                        }
                    }
                }
                if (piece.pv.IsMine && !isTutorialEnemy)
                {
                    GameObject inst = null;
                    switch (piece.type)
                    {
                        case GameMaster.PieceType.Assassin:
                            inst = Instantiate(togglePrefabs[0], transform);
                            break;
                        case GameMaster.PieceType.Thug:
                            inst = Instantiate(togglePrefabs[1], transform);
                            break;
                    }
                    if (piece.type != GameMaster.PieceType.Worker)
                    {
                        AssignmentToggle toggle = inst.GetComponent<AssignmentToggle>();
                        toggle.assigner = this;
                        toggle.representative = piece;
                        toggle.isPrivate = piece.isPrivate;
                        toggle.isPoisoned = piece.poisoned;
                        ThreatPiece tp = piece.GetComponent<ThreatPiece>();
                        if (UIManager.Instance.participant.character == GameMaster.Character.Adventurer)
                        {
                            tp.damageValue = 0;
                        }
                        if (UIManager.Instance.participant.role == GameMaster.Role.Gangster
                            && GameMaster.Instance.roleRevealTurns[(int) GameMaster.Role.Gangster] == GameMaster.Instance.turnCounter
                            && piece.type == GameMaster.PieceType.Thug)
                        {
                            tp.damageValue = 2+1*GameMaster.Instance.knivesAmount;
                        }
                        toggledOff.Add(toggle);
                    }
                }
            }

            foreach (var tp in UIManager.Instance.participant.piecesThreateningMe)
            {
                if (GameMaster.Instance.FetchPlayerByNumber(tp.originPlayerNumber).role == GameMaster.Role.Gangster
                    && GameMaster.Instance.roleRevealTurns[(int) GameMaster.Role.Gangster] == GameMaster.Instance.turnCounter
                    && tp.thisPiece.type == GameMaster.PieceType.Thug)
                {
                    tp.damageValue = 2+1*GameMaster.Instance.knivesAmount;
                }
                total += tp.damageValue;
            }
            totalText.text = "Total Damage Taken: " + total;
            totalThreat = total;
            AdjustPositions();
        }

        public override void SwitchAssignment(AssignmentToggle target)
        {
            int multiplier;
            if (target.isAssigned)
            {
                toggledOn.Remove(target);
                toggledOff.Add(target);
                multiplier = 1;
            }
            else
            {
                toggledOff.Remove(target);
                toggledOn.Add(target);
                multiplier = -1;
            }

            total += (target.representative.GetComponent<ThreatPiece>().damageValue+1*GameMaster.Instance.knivesAmount) * multiplier;
            
            totalText.text = "Total Damage Taken: " + total;
            AdjustPositions();
        }

        public int TallyAndClean()
        {
            int payAmount = 0;
            foreach (var obj in toggledOn)
            {
                ThreatPiece tp = obj.representative.GetComponent<ThreatPiece>();
                payAmount += tp.damageValue+1*GameMaster.Instance.knivesAmount;
                if (payAmount <= totalThreat)
                {
                    PhotonNetwork.Destroy(obj.representative.pv);
                }
                Destroy(obj.gameObject);
            }
            foreach (var obj in toggledOff)
            {
                Destroy(obj.gameObject);
            }

            int antiPayAmount = 0;
            foreach (var tp in UIManager.Instance.participant.piecesThreateningMe)
            {
                antiPayAmount += tp.damageValue+1*GameMaster.Instance.knivesAmount;
                if (antiPayAmount <= payAmount)
                {
                    tp.DestroySelf();
                }
                else
                {
                    tp.ReturnToOwner();
                    if (GameMaster.Instance.characterIndex.ContainsKey(GameMaster.Character.HeistPlanner))
                    {
                        GameMaster.Instance.characterIndex.TryGetValue(GameMaster.Character.HeistPlanner,
                            out Participant planner);
                        if (tp.thisPiece.type == GameMaster.PieceType.Thug &&
                            tp.originPlayerNumber == planner.playerNumber)
                        {
                            planner.pv.RPC("RpcAddCoin", RpcTarget.Others, UIManager.Instance.PayAmountOwed(2));
                        }
                    }
                }
            }
            toggledOn = new List<AssignmentToggle>();
            toggledOff = new List<AssignmentToggle>();
            if (total < 0)
            {
                return 0;
            }
            else
            {
                return total;
            }
        }
    }
}
