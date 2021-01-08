using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CardManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameMaster : MonoBehaviourPunCallbacks
    {
        #region Lists

        public List<Character> characterDeck = new List<Character> {
            Character.Adventurer, 
            Character.Necromancer,
            Character.Poisoner,
            Character.Ruffian,
            Character.Scion,
            Character.Seducer,
            Character.Sheriff,
            Character.BurglaryAce,
            Character.OldFox,
            Character.PitFighter,
            Character.ConArtist,
            Character.Counterfeiter,
            Character.ExSpy,
            Character.HighPriest,
            Character.Kidnapper,
            Character.Smuggler,
            Character.FleshStitcher,
            Character.UnderBoss,
            Character.LookOut,
            Character.HeistPlanner
        };
        public List<Role> roleDeck = new List<Role>
        {
            Role.Leader,
            Role.Rogue,
            Role.Paladin,
            Role.Gangster,
            Role.Vigilante,
            Role.Noble
        };
        public List<Action> actionDeck = new List<Action>
        {
            Action.Improvise, Action.Improvise,
            Action.DoubleAgent, Action.DoubleAgent,
            Action.SecretCache, Action.SecretCache,
            Action.AskForFavours, Action.AskForFavours,
            Action.CallInBackup, Action.CallInBackup,
            Action.ExecuteAHeist, Action.ExecuteAHeist,
            Action.RunForOffice, Action.RunForOffice,
            Action.SwearTheOaths, Action.SwearTheOaths,
            Action.BribeTheTaxOfficer, Action.BribeTheTaxOfficer,
            Action.DealWithItYourself, Action.DealWithItYourself
        };
        public List<Artifact> artifactDeck = new List<Artifact>
        {
            Artifact.Ball, Artifact.Ball, Artifact.Ball, Artifact.Ball,
            Artifact.Bauble, Artifact.Bauble, Artifact.Bauble, Artifact.Bauble,
            Artifact.Key, Artifact.Key, Artifact.Key, Artifact.Key,
            Artifact.Cloak, Artifact.Cloak,
            Artifact.Phasing, Artifact.Phasing, Artifact.Phasing,
            Artifact.Midas, Artifact.Midas,
            Artifact.Cape, Artifact.Cape,
            Artifact.Bow, Artifact.Bow, Artifact.Bow,
            Artifact.Dagger, Artifact.Dagger, Artifact.Dagger, Artifact.Dagger,
            Artifact.Periapt, Artifact.Periapt, Artifact.Periapt, Artifact.Periapt,
            Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion,
            Artifact.Scepter,
            Artifact.Serum, Artifact.Serum, Artifact.Serum,
            Artifact.Venom, Artifact.Venom, Artifact.Venom,
            Artifact.Wand, Artifact.Wand
        };
        public List<Threat> threatDeck = new List<Threat>
        {
        }; // TODO: when implementing the other threats, add them to the gameobject which overwrites this atm...

            #endregion
        
        [SerializeField] private GameObject[] piecePrefabs = new GameObject[3];
        [SerializeField] private GameObject participantObject = null;
        
        public static GameMaster Instance;
        public PhotonView[] playerSlots;
        public PhotonView[] jobBoards = new PhotonView[5];
        public bool isTesting;
        public Dictionary<Role, int> playerRoles = new Dictionary<Role, int>();
        public Dictionary<Character, Participant> characterIndex = new Dictionary<Character, Participant>();
        public List<Piece> workerPieces = new List<Piece>();
        public byte turnCounter;
        public byte seatsClaimed;
        public PhotonView pv;
        public GameObject threatObject = null;
        public bool[] passedPlayers;
        public int[] roleRevealTurns = {-1,-1,-1,-1,-1,-1};
        public TurnStep turnStep = TurnStep.Normal;
        public bool isTutorial;
        public bool firstTurnHad = false;
        
        #region Enums

        public enum TurnStep
        {
            Normal,
            PayAndReset,
            Effects,
            ThreatPieces,
            Threats,
            Start
        }
        
        public enum Character
        {
            Adventurer,
            Necromancer,
            Poisoner,
            Ruffian,
            Scion,
            Seducer,
            Sheriff,
            BurglaryAce,
            OldFox,
            PitFighter,
            ConArtist,
            Counterfeiter,
            ExSpy,
            HighPriest,
            Kidnapper,
            Smuggler,
            FleshStitcher,
            UnderBoss,
            LookOut,
            HeistPlanner
        }
        
        public enum Role
        {
            Leader,
            Rogue,
            Paladin,
            Gangster,
            Vigilante,
            Noble
        }

        public enum Artifact
        {
            Ball,
            Bauble,
            Bow,
            Dagger,
            Periapt,
            Potion,
            Serum,
            Scepter,
            Venom,
            Wand,
            Cloak,
            Key,
            Phasing,
            Midas,
            Cape
        }
        
        public enum Action
        {
            Improvise,
            DoubleAgent,
            SecretCache,
            AskForFavours,
            CallInBackup,
            ExecuteAHeist,
            RunForOffice,
            SwearTheOaths,
            BribeTheTaxOfficer,
            DealWithItYourself
        }

        public enum Threat
        {// TODO: this is out of order, if it is ever used in context, please fix
            CrimesRevealed,
            ZealEbbing,
            WarWatch,
            LocalHeroes,
            RoyalDecree,
            AmbitionsDoom,
            DraconicDemands,
            ProblematicMayor,
            TurfWar,
            ArtifactMaintenance,
            NewKnives,
            ProblematicPolitician,
            BattleGuard,
            WaningDominance,
            StoppedFearing,
            NewSheriff,
            SecretCaches,
            ReligiousUnrest,
            CivilianUnrest,
            ShowForce,
        }

        public enum PieceType
        {
            Worker,
            Thug,
            Assassin
        }

        public enum Job
        {
            MasterOfCoin,
            MasterOfKnives,
            MasterOfWhispers,
            MasterOfClubs,
            MasterOfGoods
        }

        #endregion
        
        private void Start()
        {
            if (isTutorial)
            {
                for (int i = 0; i < 2; i++)
                {
                    roleDeck.RemoveAt(roleDeck.Count-1);
                }
            }
            else
            {
                for (int i = 0; i < 6-PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    roleDeck.RemoveAt(roleDeck.Count-1);
                }
            }
            pv = GetComponent<PhotonView>();
            Instance = this;
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CommenceGame());
            }
            PhotonNetwork.Instantiate(participantObject.name, Vector3.zero, Quaternion.identity, 0);
            CursorFollower.Instance.active = true;
            passedPlayers = new bool[PhotonNetwork.CurrentRoom.PlayerCount];
        }

        IEnumerator CommenceGame()
        { // this is used to avoid network latency interfering. I wanted to use buffered RPC calls, but their order was not guaranteed, this is a bit ugly in my opinion
            yield return new WaitUntil(StartCondition);
            yield return new WaitForSeconds(1);
            Participant[] players = FindObjectsOfType<Participant>();
            if (isTutorial)
            {
                RemoveFromDeck(1, 7);
                RemoveFromDeck(0, 1);
                foreach (var player in players)
                {
                    byte roleNum = (byte) DrawCard(Decklist.Cardtype.Role);
                    int charNum = DrawCard(Decklist.Cardtype.Character);
                    if (player.isAI)
                    {
                        player.pv.RPC("RpcAssignRoleAndChar", RpcTarget.AllBufferedViaServer, roleNum, charNum);
                    }
                    else
                    {
                    }
                }
            }
            else
            {
                foreach (var player in players)
                {
                    byte roleNum = (byte) DrawCard(Decklist.Cardtype.Role);
                    int charNum = DrawCard(Decklist.Cardtype.Character);
                    player.pv.RPC("RpcAssignRoleAndChar", RpcTarget.AllBufferedViaServer, roleNum, charNum);
                }
            }
        }

        private bool StartCondition()
        {
            return seatsClaimed == PhotonNetwork.CurrentRoom.PlayerCount;
        }
        
        private void Update()
        {
            if (artifactDeck.Count < 1)
            {
                artifactDeck = new List<Artifact>
                {
                    Artifact.Ball, Artifact.Ball, Artifact.Ball, Artifact.Ball,
                    Artifact.Bauble, Artifact.Bauble, Artifact.Bauble, Artifact.Bauble,
                    Artifact.Key, Artifact.Key, Artifact.Key, Artifact.Key,
                    Artifact.Cloak, Artifact.Cloak,
                    Artifact.Phasing, Artifact.Phasing, Artifact.Phasing,
                    Artifact.Midas, Artifact.Midas,
                    Artifact.Cape, Artifact.Cape,
                    Artifact.Bow, Artifact.Bow, Artifact.Bow,
                    Artifact.Dagger, Artifact.Dagger, Artifact.Dagger, Artifact.Dagger,
                    Artifact.Periapt, Artifact.Periapt, Artifact.Periapt, Artifact.Periapt,
                    Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion,
                    Artifact.Scepter,
                    Artifact.Serum, Artifact.Serum, Artifact.Serum,
                    Artifact.Venom, Artifact.Venom, Artifact.Venom,
                    Artifact.Wand, Artifact.Wand
                };
            }
            if (actionDeck.Count < 1)
            {
                actionDeck = new List<Action>
                {
                    Action.Improvise, Action.Improvise,
                    Action.DoubleAgent, Action.DoubleAgent,
                    Action.SecretCache, Action.SecretCache,
                    Action.AskForFavours, Action.AskForFavours,
                    Action.CallInBackup, Action.CallInBackup,
                    Action.ExecuteAHeist, Action.ExecuteAHeist,
                    Action.RunForOffice, Action.RunForOffice,
                    Action.SwearTheOaths, Action.SwearTheOaths,
                    Action.BribeTheTaxOfficer, Action.BribeTheTaxOfficer,
                    Action.DealWithItYourself, Action.DealWithItYourself
                };
            }

            if (!passedPlayers.Contains(false) && UIManager.Instance.turnEnded)
            {
                if (turnStep == TurnStep.Normal)
                {
                    StartCoroutine(EndTurn(!firstTurnHad));
                }
                AdvanceTurn();
            }
        }

        public GameObject CreatePiece(PieceType type)
        { // used by other classes to create pieces
             GameObject piece = PhotonNetwork.Instantiate(piecePrefabs[(int) type].name, transform.position, Quaternion.identity);
             if (type == PieceType.Worker)
             {
                 workerPieces.Add(piece.GetComponent<Piece>());
             }
             return piece;
        }

        public int DrawCard(Decklist.Cardtype type)
        {// used by other classes to draw cards from the central deck
            switch (type)
            {
                case Decklist.Cardtype.Character:
                    int randomPick = Random.Range(0, characterDeck.Count);
                    Character returnValue = characterDeck[randomPick];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue);
                    return (int)returnValue;
                case Decklist.Cardtype.Role:
                    int randomPick2 = Random.Range(0, roleDeck.Count);
                    Role returnValue2 = roleDeck[randomPick2];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue2);
                    return (int)returnValue2;
                case Decklist.Cardtype.Artifact:
                    int randomPick3 = Random.Range(0, artifactDeck.Count);
                    Artifact returnValue3 = artifactDeck[randomPick3];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue3);
                    return (int)returnValue3;
                case Decklist.Cardtype.Action:
                    int randomPick4 = Random.Range(0, actionDeck.Count);
                    Action returnValue4 = actionDeck[randomPick4];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue4);
                    return (int)returnValue4;
                case Decklist.Cardtype.Threat:
                    int randomPick5 = Random.Range(0, threatDeck.Count);
                    Threat returnValue5 = threatDeck[randomPick5];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue5);
                    return (int)returnValue5;
                default: return -1;
            }
        }

        [PunRPC]
        public void RemoveFromDeck(int cardType, int index)
        { // used to synchronize the deck on all clients
            switch ((Decklist.Cardtype)cardType)
            {
                case Decklist.Cardtype.Character:
                    Character target = (Character) index;
                    for (int i = 0; i < characterDeck.Count; i++)
                    {
                        if (characterDeck[i] == target)
                        {
                            characterDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case Decklist.Cardtype.Role:
                    Role target2 = (Role) index;
                    for (int i = 0; i < roleDeck.Count; i++)
                    {
                        if (roleDeck[i] == target2)
                        {
                            roleDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case Decklist.Cardtype.Action:
                    Action target3 = (Action) index;
                    for (int i = 0; i < actionDeck.Count; i++)
                    {
                        if (actionDeck[i] == target3)
                        {
                            actionDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case Decklist.Cardtype.Artifact:
                    Artifact target4 = (Artifact) index;
                    for (int i = 0; i < artifactDeck.Count; i++)
                    {
                        if (artifactDeck[i] == target4)
                        {
                            artifactDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case Decklist.Cardtype.Threat:
                    Threat target5 = (Threat) index;
                    for (int i = 0; i < threatDeck.Count; i++)
                    {
                        if (threatDeck[i] == target5)
                        {
                            threatDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
            }
            
        }

        public void EndTurn()
        {
            turnStep = TurnStep.Normal;
            for (var i = 0; i < passedPlayers.Length; i++)
            {
                passedPlayers[i] = true;
            }

            UIManager.Instance.turnEnded = true;
        }

        private void AdvanceTurn()
        {
            for (var i = 0; i < passedPlayers.Length; i++)
            {
                passedPlayers[i] = false;
            }

            if (turnStep != TurnStep.Start)
            {
                turnStep++;
            }
            else
            {
                turnStep = TurnStep.Normal;
            }
        }

        private IEnumerator EndTurn(bool isFirst)
        {
            if (isFirst)
            {
                turnStep = TurnStep.Start;
                firstTurnHad = true;
            }
            else
            {
                if (!UIManager.Instance.participant.PayAndReset())
                {
                    UIManager.Instance.EndTurn(false);
                }
                Debug.LogAssertion("Paid");
                while (turnStep == TurnStep.PayAndReset)
                {
                    yield return new WaitForSeconds(.5f);
                }
                UIManager.Instance.participant.EndOfTurnEffects();
                Debug.LogAssertion("Did end of turn");
                while (turnStep == TurnStep.Effects)
                {
                    yield return new WaitForSeconds(.5f);
                }
                if (!UIManager.Instance.participant.DealWithThreatPieces())
                {
                    UIManager.Instance.EndTurn(false);
                }
                Debug.LogAssertion("dealt with threatpieces");
                while (turnStep == TurnStep.ThreatPieces)
                {
                    yield return new WaitForSeconds(.5f);
                }

                if (UIManager.Instance.participant.isLeader)
                {
                    UIManager.Instance.participant.LeaderThreatResolution();
                }
                else
                {
                    UIManager.Instance.EndTurn(false);
                }
                Debug.LogAssertion("cleared threats");
                while (turnStep == TurnStep.Threats)
                {
                    yield return new WaitForSeconds(.5f);
                }
            }
            if (UIManager.Instance.participant.isLeader)
            {
                UIManager.Instance.participant.LeaderTurnStart();
            }
            turnCounter++;
            Debug.LogAssertion("started new turn");
            UIManager.Instance.participant.pv.RPC("RpcStartTurnAgain", RpcTarget.AllBuffered);
            if (isTutorial)
            {
                if (TutorialManager.Instance.currentStep == TutorialManager.TutorialStep.EndingYourFirstTurn ||
                    TutorialManager.Instance.currentStep == TutorialManager.TutorialStep.EndingTheSecondTurn)
                {
                    TutorialManager.Instance.currentStep++;
                }
            }
        }

        public void MakeNewLeader()
        {
            ThreatPiece[] tps = FindObjectsOfType<ThreatPiece>();
            List<int> forceValues = new List<int>();
            foreach (var slot in playerSlots)
            {
                Participant part = slot.gameObject.GetComponent<PlayerSlot>().player;
                if (!part.isDead && !(part.roleRevealed && (part.role == Role.Paladin || part.role == Role.Vigilante)))
                {
                    int forceValue = 0;
                    foreach (var tp in tps)
                    {
                        if (tp.originPlayerNumber == part.playerNumber)
                        {
                            forceValue += tp.damageValue;
                        }
                    }
                    forceValues.Add(forceValue);
                }
                else forceValues.Add(0);
            }
            int maxValue = Mathf.Max(forceValues.ToArray());
            for (int i = 0; i < forceValues.Count; i++)
            {
                if (forceValues[i] == maxValue)
                {
                    FetchLeader().isLeader = false;
                    FetchPlayerByNumber(i).isLeader = true;
                    return;
                }
            }
        }

        #region Fetches
        // all of these are helper functions for other classes to access the correct participants. this happens very often since I am using their index number when sending data via RPC
        public Participant FetchPlayerByNumber(int playerNumber)
        {
            if (isTutorial)
            {
                switch (playerNumber)
                {
                    case 1:
                        return TutorialManager.Instance.tutorialAi[0];
                    case 2:
                        return TutorialManager.Instance.tutorialAi[1];
                    case 3:
                        return TutorialManager.Instance.tutorialAi[2];
                }
            }
            return playerSlots[playerNumber].gameObject.GetComponent<PlayerSlot>().player;
        }

        public Participant FetchPlayerByPlayer(Player player)
        {
            foreach (var slot in playerSlots)
            {
                if (Equals(slot.Controller, player))
                {
                    return slot.gameObject.GetComponent<PlayerSlot>().player;
                }
            }

            return null;
        }

        public Participant FetchPlayerByJob(Job job)
        {
            if (jobBoards[(int) job] != null)
            {
                Player player = jobBoards[(int) job].Controller;
                return FetchPlayerByPlayer(player);
            }
            else
            {
                return FetchLeader();
            }
        }

        public Participant FetchLeader()
        {
            foreach (var slot in playerSlots)
            {
                Participant part = slot.gameObject.GetComponent<PlayerSlot>().player;
                if (part.isLeader)
                {
                    return part;
                }
            }
            Debug.LogAssertion("Things went wrong");
            return null;
        }
    }

    #endregion
    
}
