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
    public Transform PlayerContent;
    public PlayerSetter PlayerSetterPrefab;
    public GameObject PlayerGamePrefab;
    public List<PlayerSetter> PlayerSettings;
    public List<PlayerSetter> AllPlayerSettings;
    float waitForGameOver = 0.5f;
    public int NumberOfWins { get; private set; } = 3 ;
    public int SuddenDeathTimerStart { get; private set; } = 30; //in seconds
    public float SuddenDeathTimer { get; private set; } = 30; //in seconds

    public List<string> SelectedMaps { get; private set; }

    public Slider WinSlider;
    public Slider SuddenDeathSlider;
    public Button StartGameButton;
    private static string fileName = "game.ini";
    PlayerSave PlayerSaveData;

    public AudioClip hurryUpSuddenDeathClip;
    public SuddenDeath suddenDeathPrefab;
    public CorpsePart corpsePartPrefab;
    SuddenDeath suddenDeath;
    private bool playedHurryUpWarning;
    float time;
    private MapController.MapTile[] spawnPoints;

    void Awake() {
        if (Instance != null)
            Destroy(this.gameObject);
        Instance = this;
        Load();

        if (PlayerContent!=null) {
            foreach (Transform t in PlayerContent)
                Destroy(t.gameObject);
            int player = 0;
            AllPlayerSettings = new List<PlayerSetter>();
            foreach (Character c in Enum.GetValues(typeof(Character))) {
                PlayerSetter playerSetter = Instantiate(PlayerSetterPrefab);
                playerSetter.transform.SetParent(PlayerContent);
                playerSetter.playerNumber = player;
                if (PlayerSaveData!=null) {
                    playerSetter.Set(PlayerSaveData.playerSettings.Find(x => x.playerNumber == player));
                } else {
                    playerSetter.character = c;
                }
                AllPlayerSettings.Add(playerSetter);
                player++;
            }
            StartGameButton.onClick.AddListener(StartGame);
            WinSlider.onValueChanged.AddListener(OnWinSliderChange);
            SuddenDeathSlider.onValueChanged.AddListener(OnSuddenDeathSliderChange);
            if(PlayerSaveData!=null) {
                WinSlider.value = PlayerSaveData.numberOfWins;
                SuddenDeathSlider.value = PlayerSaveData.timeToSuddenDeath;
            }
        }
        playerNumberToData = new Dictionary<int, PlayerData>();
        Players = new List<PlayerData>();
        foreach (PlayerData pd in FindObjectsOfType<PlayerData>()) {
            playerNumberToData[pd.PlayerNumber] = pd;
            Players.Add(pd);
        }
        DontDestroyOnLoad(gameObject);
        //SceneManager.activeSceneChanged += SceneChange;
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
        foreach (PlayerSetter ps in PlayerSettings) {
            if (ps.isDisabled)
                continue;
            GameObject player = Instantiate(PlayerGamePrefab);
            playerNumberToData[ps.playerNumber] = player.GetComponent<PlayerData>();
            playerNumberToData[ps.playerNumber].Set(ps);
            //Destroy(player.GetComponent<PlayerMove>());
            //player.AddComponent<PlayerMove>();
            player.GetComponentInChildren<CustomAnimator>()
                .SetSprites(Array.Find<CharacterSprites>(CharacterGraphics, x => x.Character == ps.character));
            Players.Add(playerNumberToData[ps.playerNumber]);
            DontDestroyOnLoad(player);
        }
    }
    private void StartGame() {
        Save();
        MapSelection mp = FindObjectOfType<MapSelection>();
        SelectedMaps = mp.SelectedMaps;
        CreatePlayers();
        NextMap();
        SceneManager.LoadScene("GameScene");
    }

    private void OnWinSliderChange(float amount) {
        NumberOfWins = Mathf.Clamp((int)amount,1,30);
        WinSlider.GetComponentInChildren<Text>().text = "" + Mathf.Clamp((int)amount, 1, 30);
    }

    internal void NextMap() {
        MapController.SetMap(GetNextMap());
        for (int i = 0; i < Players.Count; i++) {
            PlayerData pd = Players[i]; 
            pd.Reset();
            pd.gameObject.SetActive(true);
            pd.transform.position = new Vector3(9.5f, 7.5f);
            //pd.PlayerMove.FlyToTarget(spawnPoints[i].GetCenter(), false, true);
        }
        SuddenDeathTimer = SuddenDeathTimerStart;
    }

    private void OnSuddenDeathSliderChange(float amount) {
        SuddenDeathTimerStart = Mathf.Clamp((int)amount, 30, 300);
        SuddenDeathSlider.GetComponentInChildren<Text>().text = "" + Mathf.Clamp((int)amount, 30, 300);
    }
    public void SetSpawnPosition(MapController.MapTile[] spawnPoints) {
        this.spawnPoints = spawnPoints;
        for (int i = 0; i < Players.Count; i++) {
            PlayerData pd = Players[i];
            pd.PlayerMove.FlyToTarget(spawnPoints[i].GetCenter(), false, true);
        }
    }

    void Update() {
        if (Players.Count == 0|| SceneManager.GetActiveScene().name!="GameScene") {
            return;
        }
        bool GameOver=false;
        HashSet<Teams> aliveTeams = new HashSet<Teams>();
        foreach(PlayerData p in Players) {
            if (p.Team == Teams.NoTeam)
                continue;
            if (p.IsDead == false) {
                aliveTeams.Add(p.Team);
            }
        }
        if(aliveTeams.Count() == 1) {
            GameOver = true;
            Debug.Log("aliveTeams.Count()");
        }

        if (Players.FindAll(x => x.IsDead == false).Count <= 1) {
            GameOver = true;
        }
        if (GameOver) {
            waitForGameOver -= Time.deltaTime;
            if (waitForGameOver > 0)
                return;
            List<PlayerData> pds = Players.FindAll(x => x.IsDead == false);
            pds.ForEach(x => x.numberOfWins++);
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
    public string GetNextMap() {
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

    public void CheckStartButton() {
        if (PlayerSettings.Count > 1 && FindObjectOfType<MapSelection>().SelectedMaps.Count>0)
            StartGameButton.interactable = true;
        if (PlayerSettings.Count <= 1 || FindObjectOfType<MapSelection>().SelectedMaps.Count == 0)
            StartGameButton.interactable = false;
    }

    internal void AddPlayerSetter(PlayerSetter playerSetter) {
        if (PlayerSettings.Contains(playerSetter))
            return;
        CheckStartButton();
        PlayerSettings.Add(playerSetter);
        FindObjectOfType<MapSelection>().UpdateList(PlayerSettings.Count);
    }
    internal void RemovePlayerSettings(PlayerSetter playerSetter) {
        PlayerSettings.Remove(playerSetter);
        CheckStartButton();
        FindObjectOfType<MapSelection>().UpdateList(PlayerSettings.Count);
    }

    public static Sprite[] GetPartOfArray(Sprite[] all, int start, int end) {
        int amount = end - start;
        amount++;
        Sprite[] select = new Sprite[amount];
        Array.Copy(all, start, select, 0, amount);
        return select;
    }


    private void Load() {
        string filePath = System.IO.Path.Combine(Application.dataPath.Replace("/Assets", ""), fileName);
        if (File.Exists(filePath) == false) {
            return;
        }
        PlayerSaveData = JsonConvert.DeserializeObject<PlayerSave>(File.ReadAllText(filePath));
    }

    private void Save() {
        string path = Application.dataPath.Replace("/Assets", "");
        if (Directory.Exists(path) == false) {
            // NOTE: This can throw an exception if we can't create the folder,
            // but why would this ever happen? We should, by definition, have the ability
            // to write to our persistent data folder unless something is REALLY broken
            // with the computer/device we're running on.
            Directory.CreateDirectory(path);
        }
        List<PlayerSetter.PlayerSettingSave> pss = new List<PlayerSetter.PlayerSettingSave>();
        foreach(PlayerSetter ps in AllPlayerSettings) {
            pss.Add(ps.GetSave());
        }
        PlayerSave save = new PlayerSave {
            playerSettings = pss,
            numberOfWins = NumberOfWins,
            timeToSuddenDeath = SuddenDeathTimerStart
        };
        string filePath = System.IO.Path.Combine(path, fileName);
        File.WriteAllText(filePath, JsonConvert.SerializeObject(save, new JsonSerializerSettings { }));
    }
    public class PlayerSave {
        public int numberOfWins;
        public int timeToSuddenDeath;
        public List<PlayerSetter.PlayerSettingSave> playerSettings;
    }
}
