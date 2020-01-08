using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ScoreScreenController : MonoBehaviour {
    public AudioClip WinSound;
    public AudioClip GameOverSound;

    public Transform PlayerScores;
    public Transform TeamScores;
    public PlayerScore PlayerScorePrefab;
    public TeamScore TeamScorePrefab;

    public Button PressSpaceContinue;
    public Button PressESCtoQuit;

    public Text winText;
    public Text minuteSuddendeathText;

    // Start is called before the first frame update
    void Start() {
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
            TeamScores.gameObject.SetActive(true);
            PlayerScores.gameObject.SetActive(false);
            foreach (Teams t in teamToScore.Keys) {
                TeamScore TeamScore = Instantiate(TeamScorePrefab);
                TeamScore.Show(playerDatas.FindAll(x => x.Team == t).ToArray());
                TeamScore.transform.SetParent(TeamScores);
            }
        } else {
            TeamScores.gameObject.SetActive(false);
            PlayerScores.gameObject.SetActive(true);
            foreach (PlayerData pd in playerDatas) {
                PlayerScore PlayerScore = Instantiate(PlayerScorePrefab);
                PlayerScore.Show(pd.Character,pd.numberOfWins);
                PlayerScore.transform.SetParent(PlayerScores);
            }
        }
        bool win = false;
        if(teamToScore.Count>0) {
            foreach (Teams t in teamToScore.Keys) {
                if (teamToScore[t] == PlayerController.Instance.NumberOfWins) {
                    Debug.Log(t + " wins!");
                    win = true;
                }
            }
        } else {
            foreach (PlayerData pd in PlayerController.Instance.Players) {
                if(pd.numberOfWins == PlayerController.Instance.NumberOfWins) {
                    Debug.Log(pd.Character+ " wins!");
                    win = true;
                }
            }
        }
        if(win) {
            GetComponent<AudioSource>().PlayOneShot(GameOverSound);
            PressSpaceContinue.onClick.AddListener(ReturnMenu);
            PressESCtoQuit.onClick.AddListener(ReturnMenu);
        } else {
            GetComponent<AudioSource>().PlayOneShot(WinSound);
            PressSpaceContinue.onClick.AddListener(NextRound);
            PressESCtoQuit.onClick.AddListener(ReturnMenu);
        }
        
    }

    private void NextRound() {
        PlayerController.Instance.NextMap();
        SceneManager.LoadScene("GameScene");
    }
    private void ReturnMenu() {
        foreach(PlayerData pd in FindObjectsOfType<PlayerData>()) {
            Destroy(pd.gameObject);
        }
        SceneManager.LoadScene("MainMenu");
    }
    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            NextRound();
        } 
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ReturnMenu();
        }
    }
}
