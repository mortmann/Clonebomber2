﻿using System;
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
    public AudioClip explode;
    List<AudioSource> explodeSources;
    private float currentMagnitude;
    private Vector3 normalCameraPos;
    private float magnitudeDecreaseFactor=3f;

    public bool Shake { get; private set; }

    void Start() {
        normalCameraPos = Camera.main.transform.position;
        Instance = this;
        explodeSources = new List<AudioSource>();
    }

    void Update() {
        for (int i = explodeSources.Count-1; i >= 0 ; i--) {
            if(explodeSources[i].isPlaying==false) {
                Destroy(explodeSources[i].gameObject);
                explodeSources.RemoveAt(i);
            }
        }
        if(Shake) {
            //Random number between 150 and 210
            float randomAngle = 150.0f + UnityEngine.Random.Range(-1f, 1f) * 60;
            float viewportOffsetX = UnityEngine.Random.Range(-1, 2) * 0.25f * (float)(Mathf.Sin(randomAngle / 180.0f * Mathf.PI) * currentMagnitude - .75f);
            float viewportOffsetY = UnityEngine.Random.Range(-1, 2) * 0.25f * (float)(Mathf.Cos(randomAngle / 180.0f * Mathf.PI) * currentMagnitude - .75f);
            Camera.main.transform.position = new Vector3(
                    normalCameraPos.x + viewportOffsetX,
                    normalCameraPos.y + viewportOffsetY, -10);
            currentMagnitude -= magnitudeDecreaseFactor * Time.deltaTime;
            if (currentMagnitude <= 0.11f) {
                Camera.main.transform.position = (normalCameraPos);
                Shake = false;
            }
        }
    }


    public Bomb PlaceBomb(Character type, Vector3 Position) {
        MapTile mt = MapController.Instance.GetTile(Position);
        if (mt.HasBomb)
            return null;
        Bomb bomb = Instantiate(BombPrefab);
        bomb.gameObject.layer = 8 + (int)type;
        bomb.transform.position = mt.GetCenter();// new Vector3(mt.x+0.5f,mt.y + 0.5f);
        bomb.Set(BombSpritesList.Find(x => x.type == type).Sprites, PlayerController.Instance.characterToData[type].PlayerMove.Blastradius, type);
        mt.Bomb = bomb;
        bomb.OnDestroycb += (b) => { PlayExplodeSound(bomb); };
        return bomb;
    }

    private void PlayExplodeSound(Bomb b) {
        GameObject go = new GameObject();
        AudioSource source = go.AddComponent<AudioSource>();
        source.PlayOneShot(explode);
        explodeSources.Add(source);
        go.transform.position = b.gameObject.transform.position;
        currentMagnitude = 1.75f;
        Shake = true;
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