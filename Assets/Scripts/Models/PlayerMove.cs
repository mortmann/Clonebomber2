using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyInputs { Up, Down, Right, Left, Bomb }
public class PlayerMove : MonoBehaviour {

    Dictionary<KeyInputs, KeyCode> inputToCode = new Dictionary<KeyInputs, KeyCode> {
        { KeyInputs.Up, KeyCode.UpArrow },
        { KeyInputs.Down, KeyCode.DownArrow },
        { KeyInputs.Right, KeyCode.RightArrow },
        { KeyInputs.Left, KeyCode.LeftArrow },
        { KeyInputs.Bomb, KeyCode.RightControl }
    };
    List<Vector3> moves;
    public float DefaultMoveSpeed = 2.25f;
    int throwDistance = 4;
    const float BombCooldownTime = 0.177f;
    public Vector3 LastDirection => moves.Count>0 ? MovementSign * moves[0] :  Vector3.zero;
    Rigidbody2D Rigidbody;
    int PlacedBombs;
    PlayerData PlayerData;
    int MovementSign => PlayerData.HasInvertedControls ? -1 : 1;
    Vector3 LastMove=Vector2.right;
    private bool IsOnIce;
    private Bomb lastPlacedBomb;
    float BombCooldown = 0.177f;
    void Start() {
        moves = new List<Vector3>();
        Rigidbody = GetComponent<Rigidbody2D>();
        PlayerData = GetComponent<PlayerData>();
    }
    
    void Update() {
        if (PlayerData.IsDead)
            return;

        if (Input.GetKeyDown(inputToCode[KeyInputs.Up])) {
            moves.Remove(Vector3.up);
            moves.Insert(0,Vector3.up);
        } 
        if (Input.GetKeyDown(inputToCode[KeyInputs.Down])) {
            moves.Remove(Vector3.down);
            moves.Insert(0, Vector3.down);
        }
        if (Input.GetKeyDown(inputToCode[KeyInputs.Right])) {
            moves.Remove(Vector3.right);
            moves.Insert(0, Vector3.right);
        }
        if (Input.GetKeyDown(inputToCode[KeyInputs.Left])) {
            moves.Remove(Vector3.left);
            moves.Insert(0, Vector3.left);
        }
        if (Input.GetKeyUp(inputToCode[KeyInputs.Up])) {
            moves.Remove(Vector3.up);
        }
        if (Input.GetKeyUp(inputToCode[KeyInputs.Down])) {
            moves.Remove(Vector3.down);
        }
        if (Input.GetKeyUp(inputToCode[KeyInputs.Right])) {
            moves.Remove(Vector3.right);
        }
        if (Input.GetKeyUp(inputToCode[KeyInputs.Left])) {
            moves.Remove(Vector3.left);
        }
        if (Input.GetKeyDown(inputToCode[KeyInputs.Bomb]) || PlayerData.HasDiarrhea) {
            if (lastPlacedBomb!=null&& lastPlacedBomb.gameObject.layer == gameObject.layer) {
                lastPlacedBomb.FlyToTarget(this.transform.position + LastMove * throwDistance,true);
            } else
            if (PlacedBombs < PlayerData.NumberBombs) {
                BombCooldown = BombCooldownTime;
                lastPlacedBomb = BombController.Instance.PlaceBomb(PlayerData.Character, transform.position);
                if (lastPlacedBomb != null) {
                    lastPlacedBomb.OnDestroycb += OnBombExplode;
                    PlacedBombs++;
                }
            }
        }
        if (BombCooldown > 0) {
            BombCooldown -= Time.deltaTime;
        }
        CheckTile();
    }

    internal void Set(Dictionary<KeyInputs, KeyCode> inputToCode) {
        this.inputToCode = inputToCode;
    }

    public void FixedUpdate() {
        float actualSpeed = DefaultMoveSpeed + ((float)PlayerData.NumberSpeeds * 0.444f * DefaultMoveSpeed);
        if (PlayerData.IsSuperFast) {
            actualSpeed = 100;
        }
        if (moves.Count > 0) {
            LastMove = LastDirection;
            Rigidbody.MovePosition(transform.position + LastDirection * actualSpeed * Time.deltaTime);
        }
        if (IsOnIce) {
            Rigidbody.MovePosition(transform.position + LastMove * actualSpeed * Time.deltaTime);
        }
        
    }
    internal void CheckTile() {
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        IsOnIce = false;
        switch (tt.Type) {
            case TileType.Empty:
                Debug.Log("Falling");
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                Rigidbody.MovePosition(tt.GetCenter());
                StartCoroutine(Falling());
                break;
            case TileType.Ice:
                IsOnIce = true;
                break;
        }
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
    private void OnBombExplode(Bomb b) {
        if (lastPlacedBomb == b)
            lastPlacedBomb = null;
        PlacedBombs--;
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (PlayerData.CanPushBombs == false)
            return;
        Bomb b = collision.collider.GetComponent<Bomb>();
        if (b != null) {
            b.GetPushed(LastDirection);
        }
    }
    //public void OnTriggerEnter2D(Collider2D collision) {
    //    if(collision.name == "IceMap") {
    //        IsOnIce = true;
    //    }
    //}
    //public void OnTriggerExit2D(Collider2D collision) {
    //    if (collision.name == "IceMap") {
    //        IsOnIce = false;
    //    }
    //}
}
