using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapController;

public enum KeyInputs { Up, Down, Right, Left, Action }
public class PlayerMove : Flyable {

    public AudioSource audioSource;
    public Dictionary<KeyInputs, KeyCode> InputToCode => PlayerData.inputToCode;
    public int Controller => PlayerData.Controller;
    public MapTile Tile => MapController.Instance.GetTile(transform.position);
    List<Vector3> moves;
    public readonly float DefaultMoveSpeed = 2.1f;
    //each speed upgrade is +1 and super speed +15
    public float actualSpeed =>DefaultMoveSpeed + ((float)NumberSpeeds + DefaultMoveSpeed) +(IsSuperFast?15:0);
    int throwDistance = 4;
    const float BombCooldownTime = 0.177f;
    public Vector3 LastDirection => moves.Count>0 ? MovementSign * moves[0] :  Vector3.zero;
    Rigidbody2D Rigidbody;
    int PlacedBombs;
    public PlayerData PlayerData;

    public Dictionary<PowerUPType, int> powerUPTypeToAmount { get; internal set; }
    public int NumberBombs => PlayerController.Instance.GetClampPowerUpValue(PowerUPType.Bomb, powerUPTypeToAmount[PowerUPType.Bomb]);
    public int NumberSpeeds => PlayerController.Instance.GetClampPowerUpValue(PowerUPType.Speed, powerUPTypeToAmount[PowerUPType.Speed]);
    public int Blastradius => PlayerController.Instance.GetClampPowerUpValue(PowerUPType.Blastradius, powerUPTypeToAmount[PowerUPType.Blastradius]);
    public bool CanThrowBombs => powerUPTypeToAmount[PowerUPType.Throw] > 0;

    public bool CanPushBombs => powerUPTypeToAmount[PowerUPType.Push] > 0;
    public bool HasDiarrhea => powerUPTypeToAmount[PowerUPType.Diarrhea] > 0;
    public bool HasInvertedControls => powerUPTypeToAmount[PowerUPType.Joint] > 0;
    public bool IsSuperFast => powerUPTypeToAmount[PowerUPType.Superspeed] > 0;
    public bool HasNegativEffect => lastEffect == PowerUPType.Diarrhea || lastEffect == PowerUPType.Joint || lastEffect == PowerUPType.Superspeed;
    PowerUPType lastEffect;
    readonly float NegativeEffectTime = 10f;
    float NegativeEffectTimer;
    int MovementSign => HasInvertedControls ? -1 : 1;

    internal Dictionary<PowerUPType, int> GetPowerUpsAfterDeath() {
        foreach (PowerUPType put in startUpgrades.Keys) {
            powerUPTypeToAmount[put] -= startUpgrades[put];
        }
        return powerUPTypeToAmount;
    }

    Vector3 LastMove=Vector2.right;
    private bool IsOnIce;
    private Bomb lastPlacedBomb;
    float BombCooldown = 0.177f;

    Dictionary<PowerUPType, int> startUpgrades = new Dictionary<PowerUPType, int> {
        {PowerUPType.Bomb, 1 },
        {PowerUPType.Blastradius,2 },
    };
    public void Start() {
        audioSource = GetComponent<AudioSource>();
        moves = new List<Vector3>();
        Rigidbody = GetComponent<Rigidbody2D>();
        PlayerData = GetComponent<PlayerData>();
        powerUPTypeToAmount = new Dictionary<PowerUPType, int>();
        foreach (PowerUPType pt in Enum.GetValues(typeof(PowerUPType)))
            powerUPTypeToAmount[pt] = 0;
        foreach(PowerUPType put in startUpgrades.Keys) {
            powerUPTypeToAmount[put] = startUpgrades[put];
        }
        isFalling = false;
    }
    void Update() {
        if (IsUPDown()) {
            moves.Remove(Vector3.up);
            moves.Insert(0, Vector3.up);
        }
        if (IsDownDown()) {
            moves.Remove(Vector3.down);
            moves.Insert(0, Vector3.down);
        }
        if (IsRightDown()) {
            moves.Remove(Vector3.right);
            moves.Insert(0, Vector3.right);
        }
        if (IsLeftDown()) {
            moves.Remove(Vector3.left);
            moves.Insert(0, Vector3.left);
        }
        if (IsUPUp()) {
            moves.Remove(Vector3.up);
        }
        if (IsDownUp()) {
            moves.Remove(Vector3.down);
        }
        if (IsRightUp()) {
            moves.Remove(Vector3.right);
        }
        if (IsLeftUp()) {
            moves.Remove(Vector3.left);
        }
        if (isFalling || isFlying || isBouncing)
            return;
        CheckTile();
        if (PlayerData.IsDead)
            return;
        if (IsAction() || HasDiarrhea) {
            DoAction();
        }
        if (BombCooldown > 0) {
            BombCooldown -= Time.deltaTime;
        }
    }

    protected virtual void DoAction() {
        if (CanThrowBombs && MapController.Instance.GetTile(transform.position).Bomb != null) {
            Bomb b = MapController.Instance.GetTile(transform.position).Bomb;
            b.ResetTile();
            b.FlyToTarget(this.transform.position + LastMove * throwDistance, true);
        }
        else
        if (PlacedBombs < NumberBombs) {
            MapController.MapTile mt = MapController.Instance.GetTile(transform.position);
            if (HasDiarrhea || mt.HasBomb == false) {
                BombCooldown = BombCooldownTime;
                lastPlacedBomb = BombController.Instance.PlaceBomb(PlayerData.Character, transform.position, this, HasDiarrhea);
                lastPlacedBomb.OnDestroycb += OnBombExplode;
                PlacedBombs++;
            }
        }
    }

    protected virtual bool IsUPDown() {
        if(Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " vorizontal") < 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Up]);
    }
    protected virtual bool IsDownDown() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " vorizontal") > 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Down]);
    }
    protected virtual bool IsLeftDown() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " horizontal") < 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Left]);
    }
    protected virtual bool IsRightDown() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " horizontal") > 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Right]);
    }
    protected virtual bool IsUPUp() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " vorizontal") >= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Up]);
    }
    protected virtual bool IsDownUp() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " vorizontal") <= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Down]);
    }
    protected virtual bool IsLeftUp() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " horizontal") >= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Left]);
    }
    protected virtual bool IsRightUp() {
        if (Controller != -1) {
            return Input.GetAxis("joystick " + Controller + " horizontal") <= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Right]);
    }
    protected virtual bool IsAction() {
        if (Controller != -1) {
            return Input.GetKeyDown("joystick " + Controller + " button 0");
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Action]);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (isFalling || isFlying || isBouncing) 
            return;
        if (moves.Count > 0) {
            LastMove = LastDirection;
            Rigidbody.MovePosition(transform.position + LastDirection * actualSpeed * Time.fixedDeltaTime);
        }
        if (IsOnIce) {
            Rigidbody.MovePosition(transform.position + LastMove * actualSpeed * Time.fixedDeltaTime);
        }
        if (NegativeEffectTimer > 0) {
            NegativeEffectTimer -= Time.fixedDeltaTime;
        }
        if (NegativeEffectTimer < 0) {
            NegativeEffectTimer = 0;
            RemoveNegativeEffect();
        }
    }

    private void RemoveNegativeEffect() {
        if (HasNegativEffect == false)
            return;
        powerUPTypeToAmount[lastEffect] = 0;
        lastEffect = PowerUPType.Bomb;
    }

    protected override void CheckTile() {
        IsOnIce = false;
        switch (Tile.Type) {
            case TileType.Empty:
                if (gameObject.layer == LayerMask.NameToLayer("FLYING"))
                    return;
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                flyTarget = Tile.GetCenter();
                audioSource.PlayOneShot(PlayerData.FallSound);
                StartCoroutine(Falling());
                break;
            case TileType.Ice:
                IsOnIce = true;
                break;
        }
    }
    
    private void OnBombExplode(Bomb b) {
        PlacedBombs--;
    }

    internal void AddPowerUP(PowerUPType powerType) {
        if (HasNegativEffect) {
            MapController.Instance.CreateAndFlyPowerUP(lastEffect, PlayerData);
            RemoveNegativeEffect();
        }
        switch (powerType) {
            case PowerUPType.Diarrhea:
            case PowerUPType.Joint:
            case PowerUPType.Superspeed:
                audioSource.PlayOneShot(Array.Find(PlayerData.powerUPsounds, x => x.type == powerType).clip);
                NegativeEffectTimer = NegativeEffectTime;
                break;
            default:
                audioSource.PlayOneShot(PlayerData.OtherPowerUPSound);
                break;
        }
        powerUPTypeToAmount[powerType]++;
        lastEffect = powerType;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (CanPushBombs == false)
            return;
        Bomb b = collision.collider.GetComponent<Bomb>();
        if (b != null) {
            b.GetPushed(LastDirection);
        }
    }

    protected override void IsDoneFalling() {
        Renderer.gameObject.SetActive(false);
        Destroy(this);
    }
}
