﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Flyable {

    public CircleCollider2D HitBox { get; private set; }

    public float explosionTimer=2.5f;
    int AnimationPos;
    public float AnimationSpeed = 0.33f;
    int NumberOfSprites = 4;
    float Timer;
    Sprite[] BombSprites;
    private int Strength;
    Rigidbody2D Rigidbody;
    public Character PlacedByCharakter { get; private set; }
    public Action<Bomb> OnDestroycb;
    Vector3 pushMove;
    private float Speed = 1.6f;
    MapController.MapTile tile;
    void Start() {
        Renderer = GetComponentInChildren<SpriteRenderer>();
        HitBox = GetComponent<CircleCollider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();

        Collider2D[] c2ds = Physics2D.OverlapCircleAll(transform.position, HitBox.radius);
        foreach(Collider2D c2d in c2ds) {
            if (c2d.GetComponent<PlayerMove>() == null) {
                continue;
            }
            c2d.gameObject.layer = gameObject.layer;
        }
    }

    override protected void FixedUpdate() {
        base.FixedUpdate();
        if (isFlying)
            return;
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        if (Vector3.Distance(transform.position,tt.GetCenter()) < pushMove.magnitude * Time.fixedDeltaTime * Speed){
            CheckTile();
        }
        explosionTimer -= Time.fixedDeltaTime;
        Timer += Time.fixedDeltaTime;
        if(explosionTimer<=0) {
            Explode();
            return;
        }
        if (Timer > AnimationSpeed) {
            AnimationPos++;
            AnimationPos %= NumberOfSprites;
            float scale = 1 + 0.05f * ((float)AnimationPos / (float)NumberOfSprites);
            Renderer.gameObject.transform.localScale =new Vector3(scale, scale);
            Timer = 0;
        }
        Renderer.sprite = BombSprites[AnimationPos];
        Rigidbody.MovePosition(transform.position + pushMove * Time.fixedDeltaTime * Speed);
    }

    private void Explode() {
        Blastbeam middle = Instantiate(BombController.Instance.BlastbeamPrefab);
        middle.Show(BombController.Instance.GetDirectionSprites(Direction.MIDDLE).EndSprites);
        Direction[] directions = new Direction[4] { Direction.DOWN, Direction.UP, Direction.LEFT, Direction.RIGHT };
        Vector3[] dirs = new Vector3[4] { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
        middle.transform.position = transform.position;
        for (int i=0; i<4;i++) {
            for (int x = 1; x < Strength; x++) {
                Vector3 pos = transform.position + x * dirs[i];
                TileType tt = MapController.Instance.GetTileTypeAt(pos);
                if (tt==TileType.Wall) {
                    break;
                }
                Blastbeam beam = Instantiate(BombController.Instance.BlastbeamPrefab);
                if(x == Strength-1) {
                    beam.Show(BombController.Instance.GetDirectionSprites(directions[i]).EndSprites);
                } else {
                    beam.Show(BombController.Instance.GetDirectionSprites(directions[i]).MiddleSprites);
                }
                beam.transform.position = pos;
            }
        }
        Destroy(this.gameObject);
    }
    internal void GetPushed(Vector3 direction) {
        pushMove = direction;
    }

    internal void Set(Sprite[] bombSprites, int strength, Character charakter) {
        NumberOfSprites = bombSprites.Length;
        BombSprites = bombSprites;
        Strength = strength;
        PlacedByCharakter = charakter;
        tile = MapController.Instance.GetTile(transform.position);
        CheckTile();
    }
    protected override void CheckTile() {
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        if (tt.HasBomb && tt.Bomb != this) {
            //TODO: BOUNCE
        } else if(tt.HasBomb==false) {
            tile = tt;
            tile.Bomb = this;
        }
        switch (tt.Type) {
            case TileType.Empty:
                isFlying = true;
                Debug.Log("Falling");
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                StartCoroutine(Falling());
                tile.Bomb = null;
                break;
            case TileType.Hole:
                explosionTimer = 0f; // need to be checked
                transform.position = tt.GetCenter();
                Renderer.gameObject.SetActive(false);
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                Debug.Log("HOLE");
                tile.Bomb = null;
                StartCoroutine(Hole());
                break;
            case TileType.ArrowUp:
                if (pushMove == Vector3.up)
                    return;
                transform.position = tt.GetCenter();
                pushMove = Vector3.up;
                break;
            case TileType.ArrowDown:
                if (pushMove == Vector3.down)
                    return;
                transform.position = tt.GetCenter();
                pushMove = Vector3.down;
                break;
            case TileType.ArrowLeft:
                if (pushMove == Vector3.left)
                    return;
                transform.position = tt.GetCenter();
                pushMove = Vector3.left;
                break;
            case TileType.ArrowRight:
                if (pushMove == Vector3.right)
                    return;
                transform.position = tt.GetCenter();
                pushMove = Vector3.right;
                break;
            case TileType.Box:
            case TileType.ExplodedBox:
                gameObject.layer = LayerMask.NameToLayer("THROWN");
                break;
        }
    }
    public void OnTriggerEnter2D(Collider2D collision) {
        Blastbeam pd = collision.GetComponent<Blastbeam>();
        if (pd != null) {
            explosionTimer = 0.05f;
        }
        //if (collision.GetComponent<TriggerMapCollider>() != null) {
        //    TileType tt = MapController.Instance.GetTileTypeAt(transform.position);
        //    switch (tt) {
        //        case TileType.Hole:
        //            Renderer.gameObject.SetActive(false);
        //            gameObject.layer = LayerMask.NameToLayer("FLYING");
        //            //StartCoroutine(Hole());
        //            break;
        //        case TileType.ArrowUp:
        //            pushMove = Vector3.up;
        //            break;
        //        case TileType.ArrowDown:
        //            pushMove = Vector3.down;
        //            break;
        //        case TileType.ArrowLeft:
        //            pushMove = Vector3.left;
        //            break;
        //        case TileType.ArrowRight:
        //            pushMove = Vector3.right;
        //            break;
        //    }
        //}
    }
    private IEnumerator Hole() {
        float time = 0.34f;
        while (time > 0) {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Vector2 target = MapController.Instance.GetRandomTargetTile(true,true);
        FlyToTarget(target);
        yield return null;
    }
    private IEnumerator Falling() {
        float overtime = 1f;
        while (transform.localScale.x > 0.01f) {
            float modifier = 0.01f * overtime;
            transform.localScale = new Vector3(transform.localScale.x - modifier * 0.981f * Time.fixedDeltaTime,
                                                 transform.localScale.y - modifier * 0.981f * Time.fixedDeltaTime);
            overtime += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
        yield return null;
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.GetComponent<PlayerData>() != null) {
            pushMove = Vector3.zero;
            transform.position = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f);
        } else {
            flyTarget = transform.position;
        }
    }
    public void OnDestroy() {
        OnDestroycb?.Invoke(this);
    }
}
