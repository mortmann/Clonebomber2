using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetter : MonoBehaviour {
    public Dropdown InputDevicesDropDown;

    public Button UpButton;
    public Button DownButton;
    public Button LeftButton;
    public Button RightButton;
    public Button ActionButton;
    public Image PlayerImage;
    public Character character;
    private List<string> inputs;

    void Start() {
        InputDevicesDropDown.ClearOptions();
        inputs = new List<string> {
            "Disabled",
            "Keyboard"
        };
        inputs.AddRange(Input.GetJoystickNames());
        InputDevicesDropDown.AddOptions(inputs);
        InputDevicesDropDown.value = 1;
        InputDevicesDropDown.onValueChanged.AddListener(OnValueChange);
        PlayerImage.sprite = PlayerController.Instance.GetCharacterSprites(character).SpritesDown[0];
        PlayerImage.preserveAspect = true;
    }

    private void OnValueChange(int select) {
        if(inputs[select]=="Disabled") {
            UpButton.interactable = false;
            DownButton.interactable = false;
            LeftButton.interactable = false;
            RightButton.interactable = false;
            ActionButton.interactable = false;
            Color color = PlayerImage.color;
            color.a = 0.5f;
            PlayerImage.color = color;
        } else {
            UpButton.interactable = true;
            DownButton.interactable = true;
            LeftButton.interactable = true;
            RightButton.interactable = true;
            ActionButton.interactable = true;
            Color color = PlayerImage.color;
            color.a = 1f;
            PlayerImage.color = color;
        }
    }

    void Update() {
        
    }
}
