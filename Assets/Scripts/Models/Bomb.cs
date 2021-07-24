using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapController;

public class Bomb : Flyable {

    public CircleCollider2D HitBox { get; private set; }
    public AudioClip fallingClip;

    public float explosionTimer = 2.5f;
    int AnimationPos;
    public float AnimationSpeed = 0.33f;
    int NumberOfSprites = 4;
    float Timer;
    Sprite[] BombSprites;
    private int Strength;
    Rigidbody2D Rigidbody;
    public Character PlacedByCharakter { get; private set; }
    public Action<Bomb> OnExplodecb { get; internal set; }
    public Action<Bomb> OnDestroycb;
    Vector3 pushMove;
    private readonly float Speed = 4f;
    MapController.MapTile tile;
    internal int finalLayer;
    internal int insideLayer;
    internal int startLayer;

    void Start() {
        Renderer = GetComponentInChildren<SpriteRenderer>();
        HitBox = GetComponent<CircleCollider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Collider2D[] c2ds = Physics2D.OverlapCircleAll(transform.position, HitBox.radius - 0.1f);
        foreach (Collider2D c2d in c2ds) {
            if (c2d.GetComponent<PlayerMove>() == null) {
                continue;
            }
            c2d.gameObject.layer = insideLayer;
        }
        Renderer.sprite = BombSprites[0];
        CheckTile();
    }

    override protected void FixedUpdate() {
        base.FixedUpdate();
        if (isFlying || isBouncing || isFalling)
            return;
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        if (Vector3.Distance(transform.position, tt.GetCenter()) <= pushMove.magnitude * Time.fixedDeltaTime * Speed) {
            CheckTile();
        }
        explosionTimer -= Time.fixedDeltaTime;
        Timer += Time.fixedDeltaTime;
        if (explosionTimer <= 0) {
            Explode();
            return;
        }
        if (Timer > AnimationSpeed) {
            AnimationPos++;
            AnimationPos %= NumberOfSprites;
            float scale = 1 + 0.05f * ((float)AnimationPos / (float)NumberOfSprites);
            Renderer.gameObject.transform.localScale = new Vector3(scale, scale);
            Timer = 0;
        }
        Renderer.sprite = BombSprites[AnimationPos];
        Rigidbody.MovePosition(transform.position + pushMove * Time.fixedDeltaTime * Speed);
    }

    private void Explode() {
        Blastbeam middle = Instantiate(BombController.Instance.BlastbeamPrefab);
        middle.Show(BombController.Instance.GetDirectionSprites(Direction.MIDDLE).EndSprites, this, true);
        Direction[] directions = new Direction[4] { Direction.DOWN, Direction.UP, Direction.LEFT, Direction.RIGHT };
        Vector3[] dirs = new Vector3[4] { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
        middle.transform.position = transform.position;
        for (int i = 0; i < 4; i++) {
            for (int x = 1; x < Strength; x++) {
                Vector3 pos = transform.position + x * dirs[i];
                TileType tt = MapController.Instance.GetTileTypeAt(pos);
                if (tt == TileType.Wall) {
                    break;
                }
                Blastbeam beam = Instantiate(BombController.Instance.BlastbeamPrefab);
                if (tt == TileType.Box) {
                    x = Strength - 1;
                }
                if (x == Strength - 1) {
                    beam.Show(BombController.Instance.GetDirectionSprites(directions[i]).EndSprites, this);
                }
                else {
                    beam.Show(BombController.Instance.GetDirectionSprites(directions[i]).MiddleSprites, this);
                }
                beam.transform.position = pos;
                if (tt == TileType.Box) {
                    break;
                }
            }
        }
        OnExplodecb?.Invoke(this);
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
    }

    internal void ResetTile() {
        if (tile.Bomb == this) {
            tile.Bomb = null;
        }
    }

    protected override void CheckTile() {
        Collider2D[] c2ds = Physics2D.OverlapCircleAll(transform.position, HitBox.radius - 0.1f);
        if (Array.Exists(c2ds, x => x.GetComponent<PlayerMove>() != null) == false) {
            //noo player so change layer
            gameObject.layer = finalLayer;
        }
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        if (tt.HasBomb && tt.Bomb != this && pushMove.magnitude == 0) {
            if (isThrown) {
                Debug.Log("BOUNCE");
                //go one move space over But on if Space
                TileType nextType = MapController.Instance.GetTileTypeAt(tt.GetCenter() + flyMove);
                if (nextType != TileType.Wall)
                    FlyToTarget(tt.GetCenter() + flyMove, true); //go to next
                else
                    FlyToTarget(transform.position, true); //bounce
            }
            else {
                FlyToTarget(transform.position);
                return;
            }
        }
        else if (tt.HasBomb == false) {
            tile.Bomb = null;
            tile = tt;
            tile.Bomb = this;
        }
        switch (tt.Type) {
            case TileType.Empty:
                if (gameObject.layer == LayerMask.NameToLayer("FLYING"))
                    return;
                isFalling = true;
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                GetComponent<AudioSource>().PlayOneShot(fallingClip);
                StartCoroutine(Falling());
                tile.Bomb = null;
                break;
            case TileType.Hole:
                explosionTimer = 12f;
                transform.position = tt.GetCenter();
                Renderer.gameObject.SetActive(false);
                gameObject.layer = LayerMask.NameToLayer("FLYING");
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
        if (isFlying)
            return;
        Blastbeam pd = collision.GetComponent<Blastbeam>();
        if (pd != null) {
            explosionTimer = 0.05f;
        }

    }
    private IEnumerator Hole() {
        float time = 0.2f;
        while (time > 0) {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Renderer.gameObject.SetActive(true);
        Vector2 target = MapController.Instance.GetRandomTargetTile(transform.position, true, false);
        FlyToTarget(target);
        explosionTimer = 0f; // need to be checked
        yield return null;
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (isFlying == false) {
            pushMove = Vector3.zero;
            transform.position = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f);
            CheckTile();
        }
        else {
            flyTarget = transform.position;
        }
    }
    public void OnDestroy() {
        tile.Bomb = null;
        OnDestroycb?.Invoke(this);
        if (gameObject != null)
            Destroy(gameObject);
    }
    public HashSet<MapTile> BlastBeamTiles() {
        return BlastBeamTiles(transform.position, Strength);
    }
    public static HashSet<MapTile> BlastBeamTiles(Vector3 postition, int strength) {
        HashSet<MapTile> tiles = new HashSet<MapTile> { MapController.Instance.GetTile(postition) };
        Vector3[] dirs = new Vector3[4] { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
        for (int i = 0; i < 4; i++) {
            for (int x = 1; x < strength; x++) {
                Vector3 pos = postition + x * dirs[i];
                tiles.Add(MapController.Instance.GetTile(pos));
            }
        }
        return tiles;
    }
}