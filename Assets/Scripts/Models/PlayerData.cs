using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character { Red, Blue, Green, Yellow, Spider, BSD, Tux, Snake }

public class PlayerData : MonoBehaviour {
    public Character Character = Character.Blue;
    public bool IsDead;
    Dictionary<PowerUPType, int> typeToAmount;
    public int NumberBombs => typeToAmount[PowerUPType.Bomb];
    public bool CanThrowBombs => typeToAmount[PowerUPType.Throw]>0;
    public bool CanPushBombs => typeToAmount[PowerUPType.Push] > 0;
    public int NumberSpeeds => typeToAmount[PowerUPType.Speed];
    public int Blastradius => typeToAmount[PowerUPType.Blastradius];
    public bool HasDiarrhea => typeToAmount[PowerUPType.Diarrhea] > 0;
    public bool HasInvertedControls => typeToAmount[PowerUPType.Joint] > 0;
    public bool IsSuperFast => typeToAmount[PowerUPType.Superspeed] > 0;
    public bool HasNegativEffect => lastEffect == PowerUPType.Diarrhea || lastEffect == PowerUPType.Joint || lastEffect == PowerUPType.Superspeed;
    PowerUPType lastEffect;
    void Start() {
        typeToAmount = new Dictionary<PowerUPType, int>();
        foreach (PowerUPType pt in Enum.GetValues(typeof(PowerUPType)))
            typeToAmount[pt] = 0;

        typeToAmount[PowerUPType.Bomb] = 2;
        typeToAmount[PowerUPType.Blastradius] = 2;
        typeToAmount[PowerUPType.Push] = 2;

    }
    public void Set(Dictionary<KeyInputs, KeyCode> inputToCode, Character character) {
        GetComponent<PlayerMove>().Set(inputToCode);
        Character = character;
    }
    void Update() {
        
    }
   
    internal void AddPowerUP(PowerUPType powerType) {
        if(HasNegativEffect) {
            typeToAmount[lastEffect] = 0;
        }
        typeToAmount[powerType]++;
        lastEffect = powerType;
    }

    
}
