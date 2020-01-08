using System;
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
    PlayerData PlayerData;
    int AnimationPos;
    float Timer;
    SpriteRenderer Renderer;
    Direction lastDirection = Direction.LEFT;
    void Start() {
        PlayerMove = GetComponentInParent<PlayerMove>();
        PlayerData = GetComponentInParent<PlayerData>();
        Renderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        Timer += Time.deltaTime;
        if (Timer > AnimationSpeed) {
            AnimationPos++;
            AnimationPos %= NumberOfSprites;
            Timer = 0;
        }
        if(PlayerData.IsDead) {
            switch (lastDirection) {
                case Direction.UP:
                    Renderer.sprite = SpritesDead[2];
                    break;
                case Direction.DOWN:
                    Renderer.sprite = SpritesDead[0];
                    break;
                case Direction.LEFT:
                    Renderer.sprite = SpritesDead[1];
                    break;
                case Direction.RIGHT:
                    Renderer.sprite = SpritesDead[3];
                    break;
            }
            return;
        }
        if (PlayerMove.LastDirection.x > 0) {
            lastDirection = Direction.RIGHT;
            Renderer.sprite = SpritesRight[AnimationPos];
        }
        if (PlayerMove.LastDirection.x < 0) {
            lastDirection = Direction.LEFT;
            Renderer.sprite = SpritesLeft[AnimationPos];
        }
        if (PlayerMove.LastDirection.y > 0) {
            lastDirection = Direction.UP;
            Renderer.sprite = SpritesUp[AnimationPos];
        }
        if (PlayerMove.LastDirection.y < 0) {
            lastDirection = Direction.DOWN;
            Renderer.sprite = SpritesDown[AnimationPos];
        }
    }

    internal void SetSprites(PlayerController.CharacterSprites characterSprites) {
        SpritesUp = characterSprites.SpritesUp;
        SpritesDown = characterSprites.SpritesDown;
        SpritesLeft = characterSprites.SpritesLeft;
        SpritesRight = characterSprites.SpritesRight;
        SpritesDead = characterSprites.SpritesDead;
        transform.localPosition = new Vector3(0, characterSprites.Offset, 0);
        GetComponent<SpriteRenderer>().sprite = SpritesLeft[AnimationPos];
    }
}
