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
        foreach (Transform t in Scores)
            Destroy(t.gameObject);
        Name.text = character.ToString();
        Image.sprite = PlayerController.Instance.GetCharacterSprites(character).SpritesDown[0];
        for (int i = 0; i < scores; i++) {
            GameObject Trophy = Instantiate(TrophyPrefab);
            Trophy.transform.SetParent(Scores);
        }

        Vector2 sizeDelta = Scores.GetComponent<RectTransform>().sizeDelta;
        Vector2 cellSize = Scores.GetComponent<GridLayoutGroup>().cellSize;
        float rows = sizeDelta.y / cellSize.y;
        float count = sizeDelta.x / cellSize.x;
        if (scores > count * rows) {
            float newSize = sizeDelta.y / (rows + 1);
            Scores.GetComponent<GridLayoutGroup>().cellSize = new Vector2(newSize, newSize);
        }
    }
    void Update() {
        
    }
}
