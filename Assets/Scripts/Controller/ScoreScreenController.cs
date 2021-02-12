using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ScoreScreenController : MonoBehaviour {
    public AudioClip WinSound;
    public AudioClip GameOverSound;

    public Transform Scores;
    public PlayerScore PlayerScorePrefab;
    public TeamScore TeamScorePrefab;

    public Button PressSpaceContinue;
    public Button PressESCtoQuit;

    public Text winText;
    public Text minuteSuddendeathText;
    public Text hasWonText;
    private bool GameWon;

    // Start is called before the first frame update
    void Start() {
        foreach (Transform t in Scores)
            Destroy(t.gameObject);
        winText.text = PlayerController.Instance.NumberOfWins + " required to win!";
        minuteSuddendeathText.text = PlayerController.Instance.SuddenDeathTimerStart+ "s to Suddendeath!";
        List<PlayerData> playerDatas = PlayerController.Instance.Players;
        Dictionary<Teams, int> teamToScore = new Dictionary<Teams, int>();
        foreach (PlayerData pd in playerDatas) {
            if (pd.Team == Teams.NoTeam)
                continue;
            if (teamToScore.ContainsKey(pd.Team)) {
                teamToScore[pd.Team] += pd.numberOfWins;
            } else {
                teamToScore[pd.Team] = pd.numberOfWins;
            }
        }
        if(teamToScore.Count>0) {
            foreach (Teams t in teamToScore.Keys) {
                TeamScore TeamScore = Instantiate(TeamScorePrefab);
                TeamScore.Show(playerDatas.FindAll(x => x.Team == t).ToArray(), teamToScore[t]);
                TeamScore.transform.SetParent(Scores, false);
            }
        }
        foreach (PlayerData pd in playerDatas) {
            if (pd.Team != Teams.NoTeam)
                continue;
            PlayerScore PlayerScore = Instantiate(PlayerScorePrefab);
            PlayerScore.Show(pd.Character,pd.numberOfWins);
            PlayerScore.transform.SetParent(Scores, false);
        }
        string WonText="";
        GameWon = false;
        if(teamToScore.Count>0) {
            foreach (Teams t in teamToScore.Keys) {
                if (teamToScore[t] == PlayerController.Instance.NumberOfWins) {
                    WonText = t + " has won!";
                    GameWon = true;
                }
            }
        } else {
            foreach (PlayerData pd in PlayerController.Instance.Players) {
                if(pd.numberOfWins == PlayerController.Instance.NumberOfWins) {
                    WonText = "Player "+ pd.PlayerNumber + " (" + pd.Character + ")\n has won!";
                    GameWon = true;
                }
            }
        }
        if(GameWon) {
            winText.gameObject.transform.parent.gameObject.SetActive(false);
            hasWonText.gameObject.SetActive(true);
            hasWonText.text = WonText;
            GetComponent<AudioSource>().PlayOneShot(GameOverSound);
            PressSpaceContinue.onClick.AddListener(ReturnMenu);
            PressESCtoQuit.onClick.AddListener(ReturnMenu);
        } else {
            GetComponent<AudioSource>().PlayOneShot(WinSound);
            PressSpaceContinue.onClick.AddListener(NextRound);
            PressESCtoQuit.onClick.AddListener(ReturnMenu);
        }
        PlayerController.Instance.AdvanceNextMap();
    }

    private void NextRound() {
        PlayerController.Instance.StartNextMap();
    }
    private void ReturnMenu() {
        Destroy(PlayerController.Instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(GameWon==false)
                NextRound();
            else
                ReturnMenu();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ReturnMenu();
        }
    }
}
