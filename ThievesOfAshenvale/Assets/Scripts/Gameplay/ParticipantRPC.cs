using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using Gameplay.CardManagement;
using TMPro;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public partial class Participant : MonoBehaviourPunCallbacks, IPunObservable
    {


        #region RPC

        // these are all the RPC functions (what gets used by photon to do remote function calls), many of them are just remote versions of the existing interface functions
        [PunRPC]
        public void RpcAddCoin(byte amount)
        {
            if (pv.IsMine)
            {
                AddCoin(amount);
            }
        }

        [PunRPC]
        public void RpcStartTurnAgain()
        {
            UIManager.Instance.StartSelection(UIManager.SelectionType.StartTurnAgain, null);
        }

        [PunRPC]
        public void RpcRemoveCoin(byte amount)
        {
            if (pv.IsMine)
            {
                RemoveCoins(amount);
            }
        }

        [PunRPC]
        public void RpcStealPiece(byte pieceType, byte playerIndexWhoSteals)
        {
            if (pv.IsMine)
            {
                GameObject potPiece = LookForPiece((GameMaster.PieceType) pieceType, false);
                if (potPiece != null)
                {
                    PhotonNetwork.Destroy(potPiece);
                    GameMaster.Instance.FetchPlayerByNumber(playerIndexWhoSteals).pv
                        .RPC("RpcAddPiece", RpcTarget.Others, pieceType, 1);
                }
            }
        }

        [PunRPC]
        public void AnswerFavourCard(bool isArtifact, byte indexOfPlayerWhoAsks)
        {
            if (pv.IsMine)
            {

                foreach (var card in aHand)
                {
                    if (card.cardType == (Decklist.Cardtype) 2 + Convert.ToInt32(isArtifact))
                    {
                        UIManager.Instance.AnswerFavourRequest(isArtifact,
                            GameMaster.Instance.FetchPlayerByNumber(indexOfPlayerWhoAsks));
                        break;
                    }
                }
            }
        }

        [PunRPC]
        public void RpcAddPiece(byte pieceIndex, int amount)
        {
            if (pv.IsMine)
            {
                for (int i = 0; i < amount; i++)
                {
                    AddPiece((GameMaster.PieceType) pieceIndex, false);
                }
            }
        }

        [PunRPC]
        public void RpcAddArtifactCard()
        {
            if (pv.IsMine)
            {
                DrawACard(Decklist.Cardtype.Artifact);
            }
        }
        
        [PunRPC]
        public void RpcAddActionCard()
        {
            if (pv.IsMine)
            {
                DrawACard(Decklist.Cardtype.Action);
            }
        }

        [PunRPC]
        public void RpcHandCard(byte indexOfCard, byte typeOfCard)
        {
            if (pv.IsMine)
            {
                GameObject newCard = Decklist.Instance.CreateCard((Decklist.Cardtype) typeOfCard, indexOfCard);
                int handSize = aHand.Count;
                newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.2f * handSize, .3f, 0);
                newCard.transform.rotation = mySlot.aACardLocation.rotation;
                Card cardPart = newCard.GetComponent<Card>();
                cardPart.hoverLocation = mySlot.hoverLocation;
                aHand.Add(cardPart);
            }
        }

        [PunRPC]
        public void RpcReturnNobleCheck(byte playerIndexOfNoble)
        {
            if (pv.IsMine)
            {
                foreach (var info in informationHand)
                {
                    if (info.isEvidence && info.evidenceTargetIndex == playerIndexOfNoble)
                    {
                        GameMaster.Instance.FetchPlayerByNumber(playerIndexOfNoble).pv
                            .RPC("NobleWinDenied", RpcTarget.Others);
                        break;
                    }
                }
            }
        }

        [PunRPC]
        public void ReceiveLeaderChallenge(int amountOfCampaign, byte playerIndex)
        {
            if (pv.IsMine)
            {
                officeCampaign[0] = 1;
                officeCampaign[1] = amountOfCampaign;
                officeCampaign[2] = playerIndex;
            }
        }

        [PunRPC]
        public void RequestTrade(byte playerIndex)
        {
            if (pv.IsMine)
            {
                UIManager.Instance.RequestTradePopup(GameMaster.Instance.FetchPlayerByNumber(playerIndex));
            }
        }

        [PunRPC]
        public void StartTrade(byte playerIndex)
        {
            if (pv.IsMine)
            {
                UIManager.Instance.StartSelection(UIManager.SelectionType.Trade, null);
                UIManager.Instance.tradePartner = GameMaster.Instance.FetchPlayerByNumber(playerIndex);
                awaitingTrade[playerIndex] = true;
            }
        }

        [PunRPC]
        public void NobleWinDenied()
        {
            if (pv.IsMine && isWaitingForNobleWin)
            {
                DenyNobleWin();
            }
        }

        [PunRPC]
        public void RpcRemoveHealth(byte amount)
        {
            if (pv.IsMine)
            {
                RemoveHealth(amount);
            }
        }

        [PunRPC]
        public void RpcAddHealth(byte amount)
        {
            if (pv.IsMine)
            {
                
                if (health + amount <= Decklist.Instance.characterCards[(int)character].health)
                {
                    AddHealth(amount);
                }
                else
                {
                    AddHealth(Decklist.Instance.characterCards[(int)character].health - health);
                }
            }
        }

        [PunRPC]
        public void DrawTCard(byte cardIndex)
        {
            if (pv.IsMine)
            {
                GameObject newCard = Decklist.Instance.CreateCard(Decklist.Cardtype.Threat, cardIndex);
                int handSize = tHand.Count;
                newCard.transform.position = mySlot.threatLocation.position + new Vector3(.2f * handSize, .3f, 0);
                newCard.transform.rotation = mySlot.threatLocation.rotation;
                Card cardPart = newCard.GetComponent<Card>();
                cardPart.hoverLocation = mySlot.hoverLocation;
                cardPart.cardIndex = cardIndex;
                tHand.Add(cardPart);
            }
        }

        [PunRPC]
        public void RpcRemoveTCard(byte handIndex)
        {
            if (pv.IsMine)
            {
                Destroy(tHand[handIndex].gameObject);
                tHand.RemoveAt(handIndex);
            }
        }

        [PunRPC]
        public void EndGame()
        {
            if (pv.IsMine && health > 0)
            {
                switch (role)
                {
                    case GameMaster.Role.Rogue:
                        if (coins >= 20)
                        {
                            GameOver(true);
                        }
                        else
                        {
                            GameOver(false);
                        }

                        break;
                    case GameMaster.Role.Vigilante:
                    case GameMaster.Role.Paladin:
                        for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                        {
                            Participant p = GameMaster.Instance.FetchPlayerByNumber(i);
                            if (!p.isDead && (p.role == GameMaster.Role.Leader || p.role == GameMaster.Role.Noble ||
                                              p.role == GameMaster.Role.Rogue || p.role == GameMaster.Role.Gangster))
                            {
                                GameOver(false);
                                break;
                            }
                            else
                            {
                                GameOver(true);
                            }
                        }

                        break;
                    case GameMaster.Role.Noble:
                        for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                        {
                            if (i != playerNumber)
                            {
                                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcReturnNobleCheck",
                                    RpcTarget.Others, (byte) playerNumber);
                            }
                        }

                        nobleWin = StartCoroutine(WaitForNobleWin());
                        break;
                    default:
                        if (isLeader)
                        {
                            GameOver(true);
                            break;
                        }

                        GameOver(false);
                        break;
                }
            }
        }

        [PunRPC]
        public void RpcAssignRoleAndChar(byte roleIndex, int charIndex)
        {
            // this is a workaround for a latency related issue I had where the deck would not sync up fast enough and two clients could draw the same card
            character = (GameMaster.Character) charIndex;
            GameMaster.Instance.characterIndex.Add(character, this);

            role = (GameMaster.Role) roleIndex;
            GameMaster.Instance.playerRoles.Add(role, playerNumber);

            if (pv.IsMine)
            {
                GameObject charCard = Decklist.Instance.CreateCard(Decklist.Cardtype.Character, (int) character);
                var position = mySlot.rCCardLocation.position;
                charCard.transform.position = position + new Vector3(0,.3f,0);
                var rotation = mySlot.rCCardLocation.rotation;
                charCard.transform.rotation = rotation;
                charCard.GetComponent<Card>().hoverLocation = mySlot.hoverLocation;
                AddCoin(Decklist.Instance.characterCards[(int) character].wealth);
                AddHealth(Decklist.Instance.characterCards[(int) character].health);
                
                GameObject roleCard = Decklist.Instance.CreateCard(Decklist.Cardtype.Role, (int) role);
                roleCard.transform.position = position + new Vector3(.5f,.3f,.5f);
                roleCard.transform.rotation = rotation;
                roleCard.GetComponent<Card>().hoverLocation = mySlot.hoverLocation;
                
                if (role == GameMaster.Role.Leader)
                {
                    AddHealth(1);
                }
                
                if (character == GameMaster.Character.Adventurer)
                {
                    DrawACard(Decklist.Cardtype.Artifact);
                    DrawACard(Decklist.Cardtype.Artifact);
                    DrawACard(Decklist.Cardtype.Artifact);
                }

                StartCoroutine(PrepAndStart(false));
            }
        }

        [PunRPC]
        public void BaubleInquiry(byte inquiryIndex, byte inquiringPlayer)
        {
            if (pv.IsMine)
            {
                Participant inquirer = GameMaster.Instance.FetchPlayerByNumber(inquiringPlayer);
                foreach (var card in aHand)
                {
                    if (card.cardType == Decklist.Cardtype.Artifact &&
                        card.cardIndex == (int) GameMaster.Artifact.Bauble)
                    {
                        UIManager.Instance.BaubleDecisionSelect((UIManager.TargetingReason) inquiryIndex, inquirer);
                        return;
                    }
                }

                UIManager.Instance.NotBaubledResults((UIManager.TargetingReason) inquiryIndex, inquirer);
            }
        }

        [PunRPC]
        public void LookBehindScreenBy(byte playerIndexOfLooker)
        {
            // this could have gone somewhere else, but it references so many variables from here that it felt silly to do so
            if (pv.IsMine)
            {
                string playerTurnInfo = GameMaster.Instance.turnCounter + " " +
                                        UIManager.Instance.CreateCharPlayerString(this);
                string openingText = "In turn " + playerTurnInfo + " had: \n";
                string headerText = "Behind the screen information: " + playerTurnInfo;

                int workerPieces = 0;
                int thugPieces = 0;
                int assassinPieces = 0;
                foreach (var p in pieces)
                {
                    if (p != null)
                    {
                        Piece piece = p.GetComponent<Piece>();
                        switch (piece.type)
                        {
                            case GameMaster.PieceType.Assassin:
                                assassinPieces++;
                                break;
                            case GameMaster.PieceType.Thug:
                                thugPieces++;
                                break;
                            case GameMaster.PieceType.Worker:
                                workerPieces++;
                                break;
                        }
                    }
                }

                string playerPoolInfo =
                    workerPieces + " Workers\n" + thugPieces + " Thugs\n" + assassinPieces + " Assassins\n" + coins +
                    " Coins\n"
                    + aHand.Count + " Cards in their Arsenal\n" + informationHand.Count + " pieces of Information\n" +
                    health + " remaining Health\n";

                string jobInfo = "";
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber == playerNumber)
                {
                    Board moclBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs]
                        .GetComponent<Board>();
                    jobInfo += "\n They are Master of Clubs with:\n" + moclBoard.pieces.Count + " Thugs\n" +
                               moclBoard.coins + " Coins\n";
                }

                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber == playerNumber)
                {
                    Board mocBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin]
                        .GetComponent<Board>();
                    jobInfo += "\n They are Master of Coin with:\n" + mocBoard.coins + " Coins\n";
                }

                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber == playerNumber)
                {
                    Board mogBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods]
                        .GetComponent<Board>();
                    jobInfo += "\n They are Master of Goods with:\n" + mogBoard.artifactHand.Count +
                               " Artifacts at their disposal\n" + mogBoard.coins + " Coins\n";
                }

                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfKnives).playerNumber == playerNumber)
                {
                    Board mokBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfKnives]
                        .GetComponent<Board>();
                    jobInfo += "\n They are Master of Knives with:\n" + mokBoard.pieces.Count + " Assassins\n";
                }

                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfWhispers).playerNumber == playerNumber)
                {
                    jobInfo += "\n They are Master of Whispers with:\n" + "[REDACTED by the Master of Whispers]\n";
                }

                string informationText = openingText + playerPoolInfo + jobInfo;
                GameMaster.Instance.FetchPlayerByNumber(playerIndexOfLooker).pv.RPC("RpcAddEvidence", RpcTarget.Others,
                    informationText, headerText, true, (byte) playerNumber);
            }
        }

        [PunRPC]
        public void RpcAddEvidence(string content, string header, bool isEvidence, byte targetedPlayer)
        {
            if (pv.IsMine)
            {
                if (targetedPlayer != (byte) playerNumber)
                {
                    string realPlayerString =
                        UIManager.Instance.CreateCharPlayerString(
                            GameMaster.Instance.FetchPlayerByNumber(targetedPlayer));
                    string falsePlayerString =
                        realPlayerString.Remove(realPlayerString.IndexOf("(", StringComparison.Ordinal) + 1);
                    falsePlayerString += "You)";
                    content = content.Replace(falsePlayerString, realPlayerString);
                    header = header.Replace(falsePlayerString, realPlayerString);
                }

                informationHand.Add(new InformationPiece(content, header, isEvidence, targetedPlayer));
                UIManager.Instance.SetInfoNotif(true);
            }
        }

        [PunRPC]
        public void ReceiveTradeGoods(byte amountOfWorkers, byte amountOfThugs, byte amountOfAssassins,
            byte amountPoisoned, List<byte> artifactsIndices, List<byte> actionsIndices, int coinAmount,
            byte playerIndex)
        {
            if (pv.IsMine)
            {
                StartCoroutine(WaitForTradeConfirm(amountOfWorkers, amountOfThugs, amountOfAssassins,
                    amountPoisoned, artifactsIndices, actionsIndices, coinAmount, playerIndex));
            }
        }

        [PunRPC]
        public void RpcEndTurn()
        {
            GameMaster.Instance.passedPlayers[playerNumber] = true;
        }

        [PunRPC]
        public void RpcMoWTradeSecret(string content, string header, byte targetedPlayer, byte secondaryPlayer)
        {
            if (pv.IsMine)
            {
                MoWTradeSecret ts = new MoWTradeSecret(content, header, targetedPlayer, secondaryPlayer);
                foreach (var item in outStandingTrades)
                {
                    if (item.secondaryPlayer == targetedPlayer && item.targetedPlayer == secondaryPlayer)
                    {

                        outStandingTrades.Remove(item);
                        return;
                    }
                }

                outStandingTrades.Add(ts);
            }
        }

        [PunRPC]
        public void RevealRoleOf(byte playerIndexWhoRevealed)
        {
            if (pv.IsMine)
            {
                Participant part = GameMaster.Instance.FetchPlayerByNumber(playerIndexWhoRevealed);
                part.roleRevealed = true;
                GameMaster.Instance.roleRevealTurns[(int) part.role] = GameMaster.Instance.turnCounter;
                if (playerNumber != playerIndexWhoRevealed)
                {
                    string content = UIManager.Instance.CreateCharPlayerString(part) + " is " + Decklist.Instance.roleCards[(int)GameMaster.Instance.FetchPlayerByNumber(playerIndexWhoRevealed).role].cardName;
                    string header = UIManager.Instance.CreateCharPlayerString(part) + " has revealed their role";
                    if (part.role == GameMaster.Role.Noble)
                    {
                        RpcAddEvidence(content, header, GameMaster.Instance.FetchLeader().playerNumber == playerNumber, playerIndexWhoRevealed);
                    }
                    else
                    {
                        RpcAddEvidence(content, header, false, playerIndexWhoRevealed);
                    }
                }
                else
                {
                    if (role == GameMaster.Role.Noble)
                    {
                        AddCoin(10);
                    }

                    if (role == GameMaster.Role.Vigilante)
                    {
                        UIManager.Instance.StartSelection(UIManager.SelectionType.VigilanteReveal, null);
                    }
                }
            }
        }

        #endregion

    }
}