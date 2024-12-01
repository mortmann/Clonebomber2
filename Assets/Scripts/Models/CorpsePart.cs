using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpsePart : Flyable {
    public Sprite[] Parts;
    public float Speed = 10;
    public float LifeTimer = 2f;
    SpriteRenderer spriteRenderer;
    private Vector2 target;

    protected override void IsDoneFalling() {
        Destroy(this);
    }

    void Start() {
        flySpeed = Speed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Parts[UnityEngine.Random.Range(0, Parts.Length)];
    }

    
    void Update() {
        LifeTimer -= Time.deltaTime;
        if (LifeTimer < 0)
            Destroy(this.gameObject);
    }

}
