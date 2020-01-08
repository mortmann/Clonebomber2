using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour {
    public Image image;
    public Text text;
    public Sprite clockCrossSprite;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Mathf.CeilToInt(PlayerController.Instance.SuddenDeathTimer) > 0)
            text.text = Mathf.RoundToInt(PlayerController.Instance.SuddenDeathTimer) + "s";
        else
            image.sprite = clockCrossSprite;
    }
}
