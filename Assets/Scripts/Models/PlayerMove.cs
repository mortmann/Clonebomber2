using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyInputs { Up, Down, Right, Left, Action }
public class PlayerMove : Flyable {

    public AudioSource audioSource;
    public Dictionary<KeyInputs, KeyCode> InputToCode => PlayerData.inputToCode;

    public int controller=-1;
    //string controllerActionKey = "joystick " + controller + " button 0";
    //string controllerActionKey = "joystick " + controller + " Horizontal";
    //string controllerActionKey = "joystick " + controller + " Vertical";
    List<Vector3> moves;
    public readonly float DefaultMoveSpeed = 2.1f;
    //each speed upgrade is +1 and super speed +15
    public float actualSpeed =>DefaultMoveSpeed + ((float)NumberSpeeds + DefaultMoveSpeed) +(IsSuperFast?15:0);
    int throwDistance = 4;
    const float BombCooldownTime = 0.177f;
    public Vector3 LastDirection => moves.Count>0 ? MovementSign * moves[0] :  Vector3.zero;
    Rigidbody2D Rigidbody;
    int PlacedBombs;
    PlayerData PlayerData;

    public Dictionary<PowerUPType, int> powerUPTypeToAmount { get; internal set; }
    public int NumberBombs => powerUPTypeToAmount[PowerUPType.Bomb];
    public bool CanThrowBombs => powerUPTypeToAmount[PowerUPType.Throw] > 0;
    public bool CanPushBombs => powerUPTypeToAmount[PowerUPType.Push] > 0;
    public int NumberSpeeds => powerUPTypeToAmount[PowerUPType.Speed];
    public int Blastradius => powerUPTypeToAmount[PowerUPType.Blastradius];
    public bool HasDiarrhea => powerUPTypeToAmount[PowerUPType.Diarrhea] > 0;
    public bool HasInvertedControls => powerUPTypeToAmount[PowerUPType.Joint] > 0;
    public bool IsSuperFast => powerUPTypeToAmount[PowerUPType.Superspeed] > 0;
    public bool HasNegativEffect => lastEffect == PowerUPType.Diarrhea || lastEffect == PowerUPType.Joint || lastEffect == PowerUPType.Superspeed;
    PowerUPType lastEffect;

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
    void Start() {
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
    }
    
    void Update() {
        if (PlayerData.IsDead)
            return;
        if (IsUPDown()) {
            moves.Remove(Vector3.up);
            moves.Insert(0,Vector3.up);
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
        if (IsAction() || HasDiarrhea) {
            if (CanThrowBombs&&lastPlacedBomb != null 
                    && lastPlacedBomb == MapController.Instance.GetTile(transform.position).Bomb) {
                lastPlacedBomb.ResetTile();
                //not sure why it doesnt trigger while thrown
                gameObject.layer = 0;
                lastPlacedBomb.FlyToTarget(this.transform.position + LastMove * throwDistance, true);
            } else
            if (PlacedBombs < NumberBombs) {
                BombCooldown = BombCooldownTime;
                lastPlacedBomb = BombController.Instance.PlaceBomb(PlayerData.Character, transform.position, this, true);
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

    private bool IsUPDown() {
        if(controller != -1) {
            return Input.GetAxis("joystick " + controller + " vorizontal") < 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Up]);
    }
    private bool IsDownDown() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " vorizontal") > 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Down]);
    }
    private bool IsLeftDown() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " horizontal") < 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Left]);
    }
    private bool IsRightDown() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " horizontal") > 0;
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Right]);
    }
    private bool IsUPUp() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " vorizontal") >= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Up]);
    }
    private bool IsDownUp() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " vorizontal") <= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Down]);
    }
    private bool IsLeftUp() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " horizontal") >= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Left]);
    }
    private bool IsRightUp() {
        if (controller != -1) {
            return Input.GetAxis("joystick " + controller + " horizontal") <= 0;
        }
        return Input.GetKeyUp(InputToCode[KeyInputs.Right]);
    }
    private bool IsAction() {
        if (controller != -1) {
            return Input.GetKeyDown("joystick " + controller + " button 0");
        }
        return Input.GetKeyDown(InputToCode[KeyInputs.Action]);
    }

    protected override void FixedUpdate() {
        if (moves.Count > 0) {
            LastMove = LastDirection;
            Rigidbody.MovePosition(transform.position + LastDirection * actualSpeed * Time.fixedDeltaTime);
        }
        if (IsOnIce) {
            Rigidbody.MovePosition(transform.position + LastMove * actualSpeed * Time.fixedDeltaTime);
        }
    }
    protected override void CheckTile() {
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        IsOnIce = false;
        switch (tt.Type) {
            case TileType.Empty:
                if (gameObject.layer == LayerMask.NameToLayer("FLYING"))
                    return;
                Debug.Log("Falling");
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                Rigidbody.MovePosition(tt.GetCenter());
                audioSource.PlayOneShot(PlayerData.FallSound);
                PlayerData.Die();
                StartCoroutine(Falling());
                break;
            case TileType.Ice:
                IsOnIce = true;
                break;
        }
    }
    //private IEnumerator Falling() {
    //    float overtime = 1f;
    //    while (transform.localScale.x > 0.01f) {
    //        float modifier = 0.01f * overtime;
    //        transform.localScale = new Vector3(transform.localScale.x - modifier * 9.81f * Time.fixedDeltaTime,
    //                                             transform.localScale.y - modifier * 9.81f * Time.fixedDeltaTime);
    //        overtime += Time.fixedDeltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }
    //    Destroy(this.gameObject);
    //    yield return null;
    //}
    private void OnBombExplode(Bomb b) {
        if (lastPlacedBomb == b)
            lastPlacedBomb = null;
        PlacedBombs--;
    }
    internal void AddPowerUP(PowerUPType powerType) {
        if (HasNegativEffect) {
            powerUPTypeToAmount[lastEffect] = 0;
            PlayerData.customAnimator.SetNegativeEffect(false);
        }
        switch (powerType) {
            case PowerUPType.Diarrhea:
            case PowerUPType.Joint:
            case PowerUPType.Superspeed:
                PlayerData.customAnimator.SetNegativeEffect(true);
                audioSource.PlayOneShot(Array.Find<PlayerData.PowerUPSound>(PlayerData.powerUPsounds, x => x.type == powerType).clip);
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
    
}
