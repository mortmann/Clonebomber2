using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

public enum Teams { NoTeam, Gold, Leaf, Blood, Water }
public class PlayerController : MonoBehaviour {
    public Dictionary<int, PlayerData> playerNumberToData;
    public static PlayerController Instance;
    public CharacterSprites[] CharacterGraphics;
    public List<PlayerData> Players;
    float waitForGameOver = 0.5f;
    public int NumberOfWins { get; set; } = 3 ;
    public int SuddenDeathTimerStart { get; set; } = 30; //in seconds
    public float SuddenDeathTimer { get; set; } = 30; //in seconds
    public Color[] teamColors;

    public List<string> SelectedMaps { get; private set; }

    public AudioClip hurryUpSuddenDeathClip;
    public SuddenDeath suddenDeathPrefab;
    public CorpsePart corpsePartPrefab;
    public GameObject PlayerGamePrefab;

    SuddenDeath suddenDeath;
    private bool playedHurryUpWarning;
    float time;

    public bool RandomSpawns;
    public bool RandomMapOrder;
    public int CurrentMap;
    public bool ClampUpgrades;

    public Dictionary<PowerUPType, int> clampedPowerUPTypeAmount = new Dictionary<PowerUPType, int> {
        {PowerUPType.Speed, 5 },
        {PowerUPType.Blastradius, 12 },
        {PowerUPType.Bomb, 9 },
    };

    public string NextMapString { private set; get; }
    public static readonly int MaxPlayer = 8;

    void Awake() {
        if (Instance != null)
            Destroy(this.gameObject);
        Instance = this;
        playerNumberToData = new Dictionary<int, PlayerData>();
        Players = new List<PlayerData>();
        foreach (PlayerData pd in FindObjectsOfType<PlayerData>()) {
            playerNumberToData[pd.PlayerNumber] = pd;
            Players.Add(pd);
        }
        DontDestroyOnLoad(gameObject);
        if (MapSelection.Instance != null)
            MapSelection.Instance.UpdateList(0);
    }

    private void OnRandomMapOrderToggle(bool order) {
        RandomMapOrder = order;
    }

    private void OnRandomSpawnToggle(bool spawn) {
        RandomSpawns = spawn;
    }

    internal void CreateCorpseParts(Vector3 position) {
        for (int i = 0; i < 10; i++) {
            CorpsePart cp = Instantiate(corpsePartPrefab);
            cp.transform.position = position;
            cp.FlyToTarget(MapController.Instance.GetRandomTargetTile());
        }
    }

    private void OnDestroy() {
        foreach (PlayerData pd in Players) {
            if(pd!=null)
                Destroy(pd.gameObject);
        }
    }

    public void CreatePlayers() {
        foreach (PlayerSetter ps in FindObjectsOfType<PlayerSetter>()) {
            if (ps.isDisabled)
                continue;
            GameObject player = Instantiate(PlayerGamePrefab);
            playerNumberToData[ps.playerNumber] = player.GetComponent<PlayerData>();
            playerNumberToData[ps.playerNumber].Set(ps);
            player.GetComponentInChildren<CustomAnimator>()
                .SetSprites(Array.Find<CharacterSprites>(CharacterGraphics, x => x.Character == ps.character));
            Players.Add(playerNumberToData[ps.playerNumber]);
            DontDestroyOnLoad(player);
        }
    }
    public void StartGame() {
        MapSelection mp = FindObjectOfType<MapSelection>();
        SelectedMaps = mp.SelectedMaps;
        NextMapString = GetNextMap();
        CreatePlayers();
        StartNextMap();
    }

    internal void StartNextMap() {
        for (int i = 0; i < Players.Count; i++) {
            PlayerData pd = Players[i]; 
            pd.Reset();
            pd.gameObject.SetActive(true);
            pd.transform.position = new Vector3(9.5f, 7.5f);
        }
        SuddenDeathTimer = SuddenDeathTimerStart;
        SceneManager.LoadScene("GameScene");
    }

    public void SetSpawnPosition(List<MapController.MapTile> spawnPoints) {
        if (spawnPoints == null)
            return;
        List<MapController.MapTile> spawns = new List<MapController.MapTile>(spawnPoints);
        for (int i = 0; i < Players.Count; i++) {
            PlayerData pd = Players[i];
            MapController.MapTile spawn = null;
            if (RandomSpawns) {
                spawn = spawns[UnityEngine.Random.Range(0, spawns.Count)];
                spawns.Remove(spawn);
            }
            pd.PlayerMove.FlyToTarget(spawn.GetCenter(), false, true);
        }
    }

    void Update() {
        if (Players.Count == 0|| SceneManager.GetActiveScene().name!="GameScene") {
            return;
        }
        bool GameOver=false;
        HashSet<Teams> aliveTeams = new HashSet<Teams>();
        foreach(PlayerData p in Players) {
            if (p.IsDead == false) {
                aliveTeams.Add(p.Team);
            }
        }
        if(aliveTeams.Count() == 1) {
            if(aliveTeams.First() == Teams.NoTeam && Players.FindAll(x => x.IsDead == false).Count <= 1)
                GameOver = true;
            else if(aliveTeams.First() != Teams.NoTeam)
                GameOver = true;
        }
        if (Players.FindAll(x => x.IsDead == false).Count <= 1) {
            GameOver = true;
        }
        if (GameOver) {
            waitForGameOver -= Time.deltaTime;
            if (waitForGameOver > 0)
                return;
            List<PlayerData> pds = Players.FindAll(x => x.IsDead == false);
            //pds.ForEach(x => x.numberOfWins++);
            if (pds.Count > 0)
                pds[0].numberOfWins++;
            foreach (PlayerData pd in Players) {
                pd.gameObject.SetActive(false);
            }
            SceneManager.LoadScene("ScoreScene");
        }
        if (SuddenDeathTimer > 0) {
            if(SuddenDeathTimer<Mathf.Min(30, SuddenDeathTimerStart / 3)) {
                if(playedHurryUpWarning==false) {
                    playedHurryUpWarning = true;
                    GetComponent<AudioSource>().PlayOneShot(hurryUpSuddenDeathClip);
                }
            }
            SuddenDeathTimer -= Time.deltaTime;
        } else {
            if (suddenDeath == null) {
                suddenDeath = Instantiate(suddenDeathPrefab);
            }
        }
    }
    public CharacterSprites GetCharacterSprites(Character character) {
        return Array.Find<CharacterSprites>(CharacterGraphics, x => x.Character == character);
    }
    public void AdvanceNextMap() {
        NextMapString = GetNextMap();
        MapController.Instance.LoadMap(NextMapString, false);
    }
    public int GetClampPowerUpValue(PowerUPType type, int value) {
        if (ClampUpgrades == false || clampedPowerUPTypeAmount==null || clampedPowerUPTypeAmount.ContainsKey(type)==false)
            return value;
        return Mathf.Clamp(value, 0, clampedPowerUPTypeAmount[type]);
    }
    public string GetNextMap() {
        if(RandomMapOrder == false)
            return SelectedMaps[CurrentMap++%SelectedMaps.Count];
        return SelectedMaps[UnityEngine.Random.Range(0, SelectedMaps.Count)];
    }
    [Serializable]
    public struct CharacterSprites {
        public Sprite[] AllSprites;
        public Character Character;
        public Sprite[] SpritesUp => PlayerController.GetPartOfArray(AllSprites,20,28);
        public Sprite[] SpritesDown => PlayerController.GetPartOfArray(AllSprites,0,8);
        public Sprite[] SpritesLeft => PlayerController.GetPartOfArray(AllSprites,10,18);
        public Sprite[] SpritesRight => PlayerController.GetPartOfArray(AllSprites,30,38);
        public Sprite[] SpritesDead => new Sprite[4] { AllSprites[9], AllSprites[19], AllSprites[29], AllSprites[39] };
        public float Offset;
    }

    public static Sprite[] GetPartOfArray(Sprite[] all, int start, int end) {
        int amount = end - start;
        amount++;
        Sprite[] select = new Sprite[amount];
        Array.Copy(all, start, select, 0, amount);
        return select;
    }

}
