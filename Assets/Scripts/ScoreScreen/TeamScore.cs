using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScore : MonoBehaviour {

    public Text Name;
    public Transform TrophyScores;
    public Transform PlayerParent;
    public PlayerScore PlayerScorePrefab;
    public GameObject TrophyPrefab;

    void Start() {
        
    }
    public void Show(PlayerData[] playerDatas, int score) {
        GetComponent<Image>().color = PlayerController.Instance.teamColors[(int)playerDatas[0].Team];
        Name.color = PlayerController.Instance.teamColors[(int)playerDatas[0].Team];
        Name.text = playerDatas[0].Team.ToString();
        foreach (Transform t in TrophyScores)
            Destroy(t.gameObject);
        foreach (Transform t in PlayerParent)
            Destroy(t.gameObject);
        foreach (PlayerData pd in playerDatas) {
            PlayerScore ps = Instantiate(PlayerScorePrefab);
            ps.Show(pd.Character, 0);
            ps.transform.SetParent(PlayerParent, false);
        }
        for (int i = 0; i < score; i++) {
            GameObject Trophy = Instantiate(TrophyPrefab);
            Trophy.transform.SetParent(TrophyScores, false);
        }

        Vector2 sizeDelta = TrophyScores.GetComponent<RectTransform>().sizeDelta;
        Vector2 cellSize = TrophyScores.GetComponent<GridLayoutGroup>().cellSize;
        float rows = sizeDelta.y / cellSize.y;
        float count = sizeDelta.x / cellSize.x;
        if(score > count*rows) {
            float newSize = sizeDelta.y / (rows+1);
            TrophyScores.GetComponent<GridLayoutGroup>().cellSize = new Vector2(newSize, newSize);
        }
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta += new Vector2(0, 80) * Mathf.FloorToInt(playerDatas.Length / 3);
    }
}
