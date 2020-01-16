using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPanel : MonoBehaviour {
    private readonly string fileName = "game.ini";
    public static PlayPanel Instance;

    public Slider WinSlider;
    public Slider SuddenDeathSlider;
    public Slider ShakeAmountSlider;

    public Button StartGameButton;
    public Toggle RandomSpawnsToggle;
    public Toggle RandomMapsToggle;
    public Toggle ClampUpgradesToggle;

    public Transform PlayerContent;
    public PlayerSetter PlayerSetterPrefab;
    public List<PlayerSetter> PlayerSettings;
    public List<PlayerSetter> AllPlayerSettings;
    private GameSettings GameDataSave;

    void Start() {
        Instance = this;
        Load();
        if(GameDataSave.clampedPowerUPTypeAmount!=null)
            PlayerController.Instance.clampedPowerUPTypeAmount = GameDataSave.clampedPowerUPTypeAmount;

        foreach (Transform t in PlayerContent)
            Destroy(t.gameObject);
        int player = 0;
        AllPlayerSettings = new List<PlayerSetter>();
        foreach (Character c in Enum.GetValues(typeof(Character))) {
            PlayerSetter playerSetter = Instantiate(PlayerSetterPrefab);
            playerSetter.transform.SetParent(PlayerContent);
            playerSetter.playerNumber = player;
            if (GameDataSave != null) {
                playerSetter.Set(GameDataSave.playerSettings.Find(x => x.playerNumber == player));
            }
            else {
                playerSetter.character = c;
            }
            AllPlayerSettings.Add(playerSetter);
            player++;
        }
        StartGameButton.onClick.AddListener(Save);
        StartGameButton.onClick.AddListener(PlayerController.Instance.StartGame);
        WinSlider.onValueChanged.AddListener(OnWinSliderChange);
        SuddenDeathSlider.onValueChanged.AddListener(OnSuddenDeathSliderChange);
        RandomSpawnsToggle.onValueChanged.AddListener(OnRandomSpawnToggle);
        RandomMapsToggle.onValueChanged.AddListener(OnRandomMapOrderToggle);
        ShakeAmountSlider.onValueChanged.AddListener(OnShakeAmountSliderChange);
        ClampUpgradesToggle.onValueChanged.AddListener(OnClampUgrades);
        WinSlider.value = PlayerController.Instance.NumberOfWins;
        SuddenDeathSlider.value = PlayerController.Instance.SuddenDeathTimerStart/5;
        RandomSpawnsToggle.isOn = PlayerController.Instance.RandomSpawns;
        RandomMapsToggle.isOn = PlayerController.Instance.RandomMapOrder;
        ShakeAmountSlider.value = BombController.ShakeAmount;
        ClampUpgradesToggle.isOn = PlayerController.Instance.ClampUpgrades;

        if (GameDataSave != null) {
            WinSlider.value = GameDataSave.numberOfWins;
            SuddenDeathSlider.value = GameDataSave.timeToSuddenDeath / 5;
            RandomSpawnsToggle.isOn = GameDataSave.RandomSpawns;
            RandomMapsToggle.isOn = GameDataSave.RandomMaps;
            ClampUpgradesToggle.isOn = GameDataSave.ClampUpgrades;
            ShakeAmountSlider.value = GameDataSave.ShakeAmount;
        } 
    }

    private void OnClampUgrades(bool clamp) {
        PlayerController.Instance.ClampUpgrades = clamp;
    }

    private void OnShakeAmountSliderChange(float amount) {
        amount = (Mathf.RoundToInt(Mathf.Clamp(amount, 0f, 2f) * 10)) / 10f;
        BombController.ShakeAmount = amount;
        ShakeAmountSlider.GetComponentInChildren<Text>().text = "Camera Explosion\nAmount: " + amount;
    }

    private void OnRandomMapOrderToggle(bool order) {
        PlayerController.Instance.RandomMapOrder = order;
    }

    private void OnRandomSpawnToggle(bool spawn) {
        PlayerController.Instance.RandomSpawns = spawn;
    }

    private void OnWinSliderChange(float amount) {
        PlayerController.Instance.NumberOfWins = Mathf.Clamp((int)amount, 1, 30);
        WinSlider.GetComponentInChildren<Text>().text = "" + Mathf.Clamp((int)amount, 1, 30);
    }
    private void OnSuddenDeathSliderChange(float amount) {
        amount *= 5;
        PlayerController.Instance.SuddenDeathTimerStart = Mathf.Clamp((int)amount, 30, 300);
        SuddenDeathSlider.GetComponentInChildren<Text>().text = "" + Mathf.Clamp((int)amount, 30, 300);
    }
    public void CheckStartButton() {
        if (PlayerSettings.Count > 1 && FindObjectOfType<MapSelection>().SelectedMaps.Count > 0)
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


    private void Load() {
        string filePath = System.IO.Path.Combine(Application.dataPath.Replace("/Assets", ""), fileName);
        if (File.Exists(filePath) == false) {
            return;
        }
        GameDataSave = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(filePath));
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
        foreach (PlayerSetter ps in AllPlayerSettings) {
            pss.Add(ps.GetSave());
        }
        GameSettings save = new GameSettings {
            playerSettings = pss,
            numberOfWins = PlayerController.Instance.NumberOfWins,
            timeToSuddenDeath = PlayerController.Instance.SuddenDeathTimerStart,
            RandomSpawns = PlayerController.Instance.RandomSpawns,
            RandomMaps = PlayerController.Instance.RandomMapOrder,
            ShakeAmount = BombController.ShakeAmount,
            ClampUpgrades = PlayerController.Instance.ClampUpgrades,
            clampedPowerUPTypeAmount = PlayerController.Instance.clampedPowerUPTypeAmount,
        };
        string filePath = System.IO.Path.Combine(path, fileName);
        File.WriteAllText(filePath, JsonConvert.SerializeObject(save, new JsonSerializerSettings { _formatting = Formatting.Indented }));
    }
    public class GameSettings {
        public int numberOfWins;
        public int timeToSuddenDeath;
        public bool RandomSpawns;
        public bool RandomMaps;
        public bool ClampUpgrades;
        public float ShakeAmount;
        public Dictionary<PowerUPType, int> clampedPowerUPTypeAmount;
        public List<PlayerSetter.PlayerSettingSave> playerSettings;
    }
}
