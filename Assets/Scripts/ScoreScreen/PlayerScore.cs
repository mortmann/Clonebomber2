using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour {
    public Transform Scores;
    public Text Name;
    public Image Image;

    public GameObject TrophyPrefab;

    void Start() {
        
    }
    public void Show(Character character, int scores) {
        Name.text = character.ToString();
        Image.sprite = PlayerController.Instance.GetCharacterSprites(character).SpritesDown[0];
        for (int i = 0; i < scores; i++) {
            GameObject Trophy = Instantiate(TrophyPrefab);
            Trophy.transform.SetParent(Scores);
        }
    }
    void Update() {
        
    }
}
