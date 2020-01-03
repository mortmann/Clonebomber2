using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapController;

public enum Direction { UP, DOWN, LEFT, RIGHT, MIDDLE }

public class BombController : MonoBehaviour {
    public static BombController Instance;
    public Bomb BombPrefab;
    public Blastbeam BlastbeamPrefab;
    public List<BombSprites> BombSpritesList;
    public List<BlastbeamSprites> BlastbeamList;

    void Start() {
        Instance = this;

    }

    void Update() {
        
    }


    public Bomb PlaceBomb(Character type, Vector3 Position) {
        MapTile mt = MapController.Instance.GetTile(Position);
        if (mt.HasBomb)
            return null;
        Bomb bomb = Instantiate(BombPrefab);
        bomb.gameObject.layer = 8 + (int)type;
        bomb.transform.position = mt.GetCenter();// new Vector3(mt.x+0.5f,mt.y + 0.5f);
        bomb.Set(BombSpritesList.Find(x => x.type == type).Sprites, PlayerController.Instance.characterToData[type].Blastradius, type);
        mt.Bomb = bomb;
        bomb.OnDestroycb += (b) => { mt.Bomb = null; };
        return bomb;
    }
    public BlastbeamSprites GetDirectionSprites(Direction dir) {
        return BlastbeamList.Find(x => x.type == dir);
    }
    [Serializable]
    public struct BombSprites {
        public Character type;
        public Sprite[] Sprites;
    }
    [Serializable]
    public struct BlastbeamSprites {
        public Direction type;
        public Sprite[] EndSprites;
        public Sprite[] MiddleSprites;
    }
}
