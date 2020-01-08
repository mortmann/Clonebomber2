using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScore : MonoBehaviour {

    public Teams team;
    public Text Name;
    public Transform TrophyScores;
    public Transform PlayerParent;
    public PlayerScore PlayerScorePrefab;

    void Start() {
        
    }
    public void Show(PlayerData[] playerDatas) {
        foreach(PlayerData pd in playerDatas) {
            PlayerScore ps = Instantiate(PlayerScorePrefab);
            ps.Show(pd.Character, pd.numberOfWins);
        }

    }
}
