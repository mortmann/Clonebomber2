using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetter : MonoBehaviour {
    public Dropdown InputDevicesDropDown;
    public Button TeamButton;
    public bool isDisabled = false;
    public Button UpButton;
    public Button DownButton;
    public Button LeftButton;
    public Button RightButton;
    public Button ActionButton;
    public Button CharacterButton;
    public Image PlayerImage;
    
    private List<string> inputs;
    private List<string> controllers;
    bool setKey;
    KeyInputs selectedKey;
    public Character character;
    public Teams team;
    public Dictionary<KeyInputs, KeyCode> inputToCode = new Dictionary<KeyInputs, KeyCode> {
        { KeyInputs.Up, KeyCode.UpArrow },
        { KeyInputs.Down, KeyCode.DownArrow },
        { KeyInputs.Right, KeyCode.RightArrow },
        { KeyInputs.Left, KeyCode.LeftArrow },
        { KeyInputs.Action, KeyCode.RightControl }
    };
    public int controller=-1;
    internal int playerNumber;
    public bool isAI;

    void Start() {
        SetUpInputDropDown();
        
        PlayerImage.sprite = PlayerController.Instance.GetCharacterSprites(character).SpritesDown[0];
        TeamButton.GetComponent<Image>().color = PlayerController.Instance.teamColors[(int)team];
        PlayerImage.preserveAspect = true;
        PlayPanel.Instance.AddPlayerSetter(this);
        foreach (KeyInputs ki in inputToCode.Keys) {
            switch (ki) {
                case KeyInputs.Up:
                    UpButton.GetComponentInChildren<Text>().text = inputToCode[ki].ToString();
                    break;
                case KeyInputs.Down:
                    DownButton.GetComponentInChildren<Text>().text = inputToCode[ki].ToString();
                    break;
                case KeyInputs.Right:
                    RightButton.GetComponentInChildren<Text>().text = inputToCode[ki].ToString();
                    break;
                case KeyInputs.Left:
                    LeftButton.GetComponentInChildren<Text>().text = inputToCode[ki].ToString();
                    break;
                case KeyInputs.Action:
                    ActionButton.GetComponentInChildren<Text>().text = inputToCode[ki].ToString();
                    break;
            }
        }
        TeamButton.onClick.AddListener(() => { ChangeTeam(); });
        UpButton.onClick.AddListener(() => { WaitForKeyDown(KeyInputs.Up, UpButton); });
        DownButton.onClick.AddListener(() => { WaitForKeyDown(KeyInputs.Down, DownButton); });
        LeftButton.onClick.AddListener(() => { WaitForKeyDown(KeyInputs.Left, LeftButton); });
        RightButton.onClick.AddListener(() => { WaitForKeyDown(KeyInputs.Right, RightButton); });
        ActionButton.onClick.AddListener(() => { WaitForKeyDown(KeyInputs.Action, ActionButton); });
        CharacterButton.onClick.AddListener(() => { ChangeCharcter(); });
        controller = -1;
        if (isDisabled) {
            InputDevicesDropDown.value = 0;
            OnValueChange(0);
        }
        else if (controller != -1) {
            InputDevicesDropDown.value = controller;
            OnValueChange(controller);
        } 
        if(isAI) {
            InputDevicesDropDown.value = 2;
            OnValueChange(2);
        }
        InputDevicesDropDown.RefreshShownValue();
    }

    private void SetUpInputDropDown() {
        InputDevicesDropDown.ClearOptions();
        inputs = new List<string> {
            "Disabled",
            "Keyboard",
            "Computer"
        };
        string[] joysticks = new string[8];
        Array.Copy(Input.GetJoystickNames(), joysticks, Mathf.Clamp(Input.GetJoystickNames().Length, 0, 8));
        inputs.AddRange(joysticks);
        inputs.RemoveAll(x => string.IsNullOrEmpty(x));
        controllers = new List<string>(joysticks);
        controllers.RemoveAll(x => x == null);
        InputDevicesDropDown.AddOptions(inputs);
        InputDevicesDropDown.value = 1;
        InputDevicesDropDown.onValueChanged.AddListener(OnValueChange);
        InputDevicesDropDown.RefreshShownValue();
    }

    private void ChangeCharcter() {
        character = (Character)((((int)character) + 1) % Enum.GetValues(typeof(Character)).Length);
        PlayerImage.sprite = PlayerController.Instance.GetCharacterSprites(character).SpritesDown[0];
    }

    internal void Set(PlayerSetter.PlayerSettingSave playerSetter) {
        playerNumber = playerSetter.playerNumber;
        inputToCode = playerSetter.inputToCode;
        controller = playerSetter.controller;
        character = playerSetter.character;
        isDisabled = playerSetter.IsDisabled;
        team = playerSetter.team;
        isAI = playerSetter.IsAI;
    }

    private void ChangeTeam() {
        team = (Teams)((((int)team)+1) % Enum.GetValues(typeof(Teams)).Length);
        TeamButton.GetComponent<Image>().color = PlayerController.Instance.teamColors[(int)team];
    }

    private void WaitForKeyDown(KeyInputs key,Button button) {
        setKey = true;
        Cursor.visible = false;
        selectedKey = key;
        button.GetComponentInChildren<Text>().text = "> <";
    }

    public void OnGUI() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            setKey = false;
            Cursor.visible = true;
        }
        else
            if (Event.current != null && (Event.current.type == EventType.KeyDown) ) {
            if (setKey == false) {
                return;
            }
            KeyCode s = Event.current.keyCode;
            switch (selectedKey) {
                case KeyInputs.Up:
                    UpButton.GetComponentInChildren<Text>().text = s.ToString();
                    break;
                case KeyInputs.Down:
                    DownButton.GetComponentInChildren<Text>().text = s.ToString();
                    break;
                case KeyInputs.Right:
                    RightButton.GetComponentInChildren<Text>().text = s.ToString();
                    break;
                case KeyInputs.Left:
                    LeftButton.GetComponentInChildren<Text>().text = s.ToString();
                    break;
                case KeyInputs.Action:
                    ActionButton.GetComponentInChildren<Text>().text = s.ToString();
                    break;
            }
            inputToCode[selectedKey] = s;
            Cursor.visible = true;
            setKey = false;
        }
    }

    private void OnValueChange(int select) {
        Color color = PlayerImage.color;
        if (inputs[select]=="Disabled") {
            UpButton.interactable = false;
            DownButton.interactable = false;
            LeftButton.interactable = false;
            RightButton.interactable = false;
            ActionButton.interactable = false;
            isDisabled = true;
            PlayPanel.Instance.RemovePlayerSettings(this);
            color.a = 0.5f;
            PlayerImage.color = color;
            isAI = false;
            return;
        } else 
        if(inputs[select] == "Computer") {
            UpButton.interactable = false;
            DownButton.interactable = false;
            LeftButton.interactable = false;
            RightButton.interactable = false;
            ActionButton.interactable = false;
            isDisabled = false;
            isAI = true;
            return;
        }
        else
        if (controllers.Contains(inputs[select]) == false) {
            UpButton.interactable = true;
            DownButton.interactable = true;
            LeftButton.interactable = true;
            RightButton.interactable = true;
            ActionButton.interactable = true;
        } else {
            UpButton.interactable = false;
            DownButton.interactable = false;
            LeftButton.interactable = false;
            RightButton.interactable = false;
            ActionButton.interactable = false;
        }
        if (controllers.Contains(inputs[select])) {
            controller = controllers.IndexOf(inputs[select]);
        }
        PlayPanel.Instance.AddPlayerSetter(this);
        isDisabled = false;
        isAI = false;
        color.a = 1f;
        PlayerImage.color = color;
    }

    void Update() {
        if(Input.GetJoystickNames().Length != InputDevicesDropDown.options.Count - 3) { //-3 for keyboard and disabled and ai
            string option = InputDevicesDropDown.options[InputDevicesDropDown.value].text;
            SetUpInputDropDown();
            InputDevicesDropDown.value = Mathf.Clamp(InputDevicesDropDown.options.FindIndex(x => x.text == option),0,int.MaxValue);
        }
    }
    public PlayerSettingSave GetSave() {
        return new PlayerSettingSave {
            character = character,
            team = team,
            inputToCode = inputToCode,
            controller = controller,
            playerNumber = playerNumber,
            IsDisabled = isDisabled,
            IsAI = isAI
        };
    }
    private void OnDisable() {
        Cursor.visible = true;
    }
    [JsonObject]
    public class PlayerSettingSave {
        [JsonProperty]
        public bool IsDisabled;
        [JsonProperty]
        public Character character;
        [JsonProperty]
        public Teams team;
        [JsonProperty]
        public bool IsAI;
        [JsonProperty]
        public Dictionary<KeyInputs, KeyCode> inputToCode = new Dictionary<KeyInputs, KeyCode> {
        { KeyInputs.Up, KeyCode.UpArrow },
        { KeyInputs.Down, KeyCode.DownArrow },
        { KeyInputs.Right, KeyCode.RightArrow },
        { KeyInputs.Left, KeyCode.LeftArrow },
        { KeyInputs.Action, KeyCode.RightControl }
        };
        [JsonProperty]
        public int controller;
        [JsonProperty]
        internal int playerNumber;
    }
}
