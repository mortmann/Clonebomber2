using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Dictionary<Character, PlayerData> characterToData;
    public static PlayerController Instance;
    public CharacterSprites[] CharacterGraphics;
    static List<PlayerData> Players;
    public Transform PlayerContent;
    public PlayerSetter PlayerPrefab;

    void Start() {
        Instance = this;
        characterToData = new Dictionary<Character, PlayerData>();
        foreach (PlayerData pd in FindObjectsOfType<PlayerData>()) {
            characterToData[pd.Character] = pd;
        }
        foreach (Transform t in PlayerContent)
            Destroy(t.gameObject);
        foreach (Character c in Enum.GetValues(typeof(Character))) {
            PlayerSetter playerSetter = Instantiate(PlayerPrefab);
            playerSetter.transform.SetParent(PlayerContent);
            playerSetter.character = c;
        }


    }
    public void SetPlayer(Character character) {
        
    }
    void Update() {
        
    }
    public CharacterSprites GetCharacterSprites(Character character) {
        return Array.Find<CharacterSprites>(CharacterGraphics, x => x.Character == character);
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
    }
    public static Sprite[] GetPartOfArray(Sprite[] all, int start, int end) {
        int amount = end - start;
        amount++;
        Sprite[] select = new Sprite[amount];
        Array.Copy(all, start, select, 0, amount);
        return select;
    }

}
