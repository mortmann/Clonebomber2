using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimator : MonoBehaviour {
    public Sprite[] SpritesUp;
    public Sprite[] SpritesDown;
    public Sprite[] SpritesLeft;
    public Sprite[] SpritesRight;
    public Sprite[] SpritesDead;
    public float AnimationSpeed = 0.33f;
    public int NumberOfSprites = 9;
    PlayerMove PlayerMove;
    int AnimationPos;
    float Timer;
    SpriteRenderer Renderer;
    void Start() {
        PlayerMove = GetComponent<PlayerMove>();
        Renderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        Timer += Time.deltaTime;
        if (Timer > AnimationSpeed) {
            AnimationPos++;
            AnimationPos %= NumberOfSprites;
            Timer = 0;
        }
        if (PlayerMove.LastDirection.x > 0) {
            Renderer.sprite = SpritesRight[AnimationPos];
        }
        if (PlayerMove.LastDirection.x < 0) {
            Renderer.sprite = SpritesLeft[AnimationPos];
        }
        if (PlayerMove.LastDirection.y > 0) {
            Renderer.sprite = SpritesUp[AnimationPos];
        }
        if (PlayerMove.LastDirection.y < 0) {
            Renderer.sprite = SpritesDown[AnimationPos];
        }
    }
}
