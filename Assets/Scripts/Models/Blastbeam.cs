using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blastbeam : MonoBehaviour {
    SpriteRenderer Renderer;
    public static readonly float TimerTime = .5f;
    private float Timer = TimerTime;
    float StageTime;
    int Stages = 3;
    Sprite[] Sprites;

    public Bomb Bomb { get; internal set; }

    void Start() {
        Renderer = GetComponent<SpriteRenderer>();
        StageTime = Timer / Stages;
        //Whatever mapcontroller will decide if anything happens
        MapController.Instance.ExplodeBox(this.transform.position);
    }
    public void Show(Sprite[] sprites, Bomb bomb, bool playsound = false) {
        Stages--;
        Sprites = sprites;
        this.Bomb = bomb;
        Destroy(GetComponent<PolygonCollider2D>());
    }
    void Update() {
        Timer -= Time.deltaTime;
        if(Stages>0 && Timer < StageTime * Stages) {
            Stages--;
            Renderer.sprite = Sprites[2-Stages];
            Destroy(GetComponent<PolygonCollider2D>());
            gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
        }
        if (Timer <= 0) {
            Destroy(this.gameObject);
            MapController.Instance.RemoveExplodeBox(this.transform.position);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        
    }
}
