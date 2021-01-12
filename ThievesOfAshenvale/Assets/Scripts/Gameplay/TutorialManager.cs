using System.Collections;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;
        public TutorialStep currentStep = TutorialStep.Bearings;
        public GameObject[] tutorialUI = new GameObject[21];
        public bool[] viewedThreats = new bool[2];
        public bool hasOneThreatDealt;
        public Participant[] tutorialAi = new Participant[3];

        [SerializeField] private Color highLightLightColor;
        [SerializeField] private Light[] tutorialGuidanceLights = new Light[10];

        private Participant participant;

        public enum TutorialStep
        {
            Bearings,
            KnowingYourCharacter,
            LearningAboutYourRole,
            LearningAboutRoles,
            CheckingYourNotes,
            LearningAboutJobs,
            LearningAboutWorkers,
            RobbingTheBank,
            StimulatingTheFlow,
            OrganizingOperations,
            PocketingMerchandise,
            UsingTheTavern,
            EndingYourFirstTurn,
            WhatAreThreats,
            MoreWorkers,
            MoreOrganizing,
            MorePocketing,
            UsingArtifacts,
            DealingWithThreats,
            SkimmingOffTheTreasuryAndJobChanges,
            UnderAttack,
            EndingTheSecondTurn,
            Resolution
        }

        private void Start()
        {
            Instance = this;
            StartCoroutine(RunTutorial());
            foreach (var ui in tutorialUI)
            {
                if (ui != null)
                {
                    ui.SetActive(false);
                }
            }
        }

        IEnumerator RunTutorial()
        {
            yield return new WaitForSeconds(1.1f);
            CursorFollower.Instance.active = false;
            participant = UIManager.Instance.participant;
            yield return new WaitForSeconds(1f);
            currentStep++;
            participant.TutorialAssignChar();
            yield return  new WaitForSeconds(.5f);
            CursorFollower.Instance.transform.position = participant.mySlot.rCCardLocation.position;
            yield return  new WaitForSeconds(3f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.KnowingYourCharacter)
            {
                yield return new WaitForSeconds(.5f);
            }
            CursorFollower.Instance.transform.position = participant.mySlot.rCCardLocation.position + new Vector3(.5f, 0, .5f);
            yield return  new WaitForSeconds(.5f);
            participant.TutorialAssignRole();
            yield return  new WaitForSeconds(3f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.LearningAboutYourRole)
            {
                yield return new WaitForSeconds(.5f);
            }
            CursorFollower.Instance.transform.position = Vector3.zero;
            yield return  new WaitForSeconds(3f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.LearningAboutRoles)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return  new WaitForSeconds(1f);
            CursorFollower.Instance.active = true;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.CheckingYourNotes)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(3f);
            GameMaster.Instance.jobBoards[0].GetComponent<Board>().ChangeJobHolder(0, 0);
            GameMaster.Instance.jobBoards[4].GetComponent<Board>().ChangeJobHolder(1, 0);
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.LearningAboutJobs)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(3f);
            participant.RpcAddPiece((byte)GameMaster.PieceType.Worker, 5);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.LearningAboutWorkers)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[0].color = highLightLightColor;
            tutorialGuidanceLights[0].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.RobbingTheBank)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[0].color = Color.white;
            tutorialGuidanceLights[0].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[1].color = highLightLightColor;
            tutorialGuidanceLights[1].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.StimulatingTheFlow)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[1].color = Color.white;
            tutorialGuidanceLights[1].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[2].color = highLightLightColor;
            tutorialGuidanceLights[2].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.OrganizingOperations)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[2].color = Color.white;
            tutorialGuidanceLights[2].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[3].color = highLightLightColor;
            tutorialGuidanceLights[3].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.PocketingMerchandise)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[3].color = Color.white;
            tutorialGuidanceLights[3].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[4].color = highLightLightColor;
            tutorialGuidanceLights[4].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.UsingTheTavern)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[4].color = Color.white;
            tutorialGuidanceLights[4].intensity = 2.4f;
            UIManager.Instance.StartSelection(UIManager.SelectionType.StartTurnAgain, null);
            GameMaster.Instance.turnCounter = 1;
            GameMaster.Instance.firstTurnHad = true;
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.EndingYourFirstTurn)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.WhatAreThreats && viewedThreats.Contains(false))
            {
                yield return new WaitForSeconds(.5f);
            }
            currentStep++;
            yield return new WaitForSeconds(2f);
            participant.RpcAddPiece((byte)GameMaster.PieceType.Worker, 4);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.MoreWorkers)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[5].color = highLightLightColor;
            tutorialGuidanceLights[5].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.MoreOrganizing)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[5].color = Color.white;
            tutorialGuidanceLights[5].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[6].color = highLightLightColor;
            tutorialGuidanceLights[6].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.MorePocketing)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[6].color = Color.white;
            tutorialGuidanceLights[6].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[7].color = highLightLightColor;
            tutorialGuidanceLights[7].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.UsingArtifacts)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[7].color = Color.white;
            tutorialGuidanceLights[7].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.DealingWithThreats)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(2f);
            tutorialGuidanceLights[8].color = highLightLightColor;
            tutorialGuidanceLights[8].intensity = 10;
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.SkimmingOffTheTreasuryAndJobChanges)
            {
                yield return new WaitForSeconds(.5f);
            }
            tutorialGuidanceLights[8].color = Color.white;
            tutorialGuidanceLights[8].intensity = 2.4f;
            yield return new WaitForSeconds(2f);
            Participant enemy = GameMaster.Instance.FetchPlayerByNumber(2);
            enemy.AddPiece(GameMaster.PieceType.Assassin, false);
            enemy.AddPiece(GameMaster.PieceType.Thug, false);
            foreach (var piece in enemy.pieces)
            {
                ThreatPiece tPiece = piece.GetComponent<ThreatPiece>();
                tPiece.ThreatenPlayer(0);
                tPiece.originPlayerNumber = 2;
            }
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.UnderAttack)
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.EndingTheSecondTurn)
            {
                yield return new WaitForSeconds(.5f);
            }
            UIManager.Instance.participant.RemoveHealth(1);
            yield return new WaitForSeconds(2f);
            UIManager.Instance.StartSelection(UIManager.SelectionType.Tutorial, null);
            while (currentStep == TutorialStep.Resolution)
            {
                yield return new WaitForSeconds(.5f);
            }
            PlayerPrefs.SetInt("TutorialFinished", 1);
            yield return new WaitForSeconds(10f);
            SceneManager.LoadScene(0);
        }
    }
}
