using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character { Red, Blue, Green, Yellow, Spider, BSD, Tux, Snake }

public class PlayerData : MonoBehaviour {
    public int PlayerNumber;
    public PowerUPSound[] powerUPsounds;
    public AudioClip OtherPowerUPSound;
    public AudioClip FallSound;
    public AudioClip CorpseExplode;

    public Teams Team;
    public Character Character = Character.Blue;
    public bool IsDead = false;
    public AudioClip DeathClip;
    public AudioClip[] OnDeadWalkedOver;

    public int Controller { get; internal set; }
    public PlayerMove PlayerMove { get; internal set; }
    public CustomAnimator customAnimator;
    public AudioSource audioSource;
    internal bool disabled;
    internal Dictionary<KeyInputs, KeyCode> inputToCode;

    public int numberOfWins = 0;

    Bomb killedByBomb;

    void Start() {
        PlayerMove = GetComponent<PlayerMove>();
        audioSource = GetComponent<AudioSource>();
        customAnimator = GetComponentInChildren<CustomAnimator>();
    }
    
    public void Reset() {
        customAnimator?.gameObject.SetActive(true);
        customAnimator?.Reset();
        if (GetComponent<PlayerMove>()==false)
            gameObject.AddComponent<PlayerMove>();
        PlayerMove = GetComponent<PlayerMove>();
        IsDead = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
        GetComponent<CircleCollider2D>().isTrigger = false;
    }

    public void Set(PlayerSetter setter) {
        PlayerNumber = setter.playerNumber;
        this.inputToCode = setter.inputToCode;
        this.Controller = setter.controller;
        this.disabled = setter.isDisabled;
        this.Character = setter.character;
        this.Team = setter.team;
    }
    public void OnTriggerEnter2D(Collider2D collider) {
        if(collider.GetComponent<Blastbeam>() != null) {
            if(IsDead==false) {
                MapController.Instance.CreateAndFlyPowerUPs(PlayerMove.GetPowerUpsAfterDeath(), this);
                Destroy(PlayerMove);
                PlayerMove = null;
                GetComponent<CircleCollider2D>().isTrigger = true;
                audioSource.PlayOneShot(DeathClip);
                killedByBomb = collider.GetComponent<Blastbeam>().Bomb;
                gameObject.layer = LayerMask.NameToLayer("Default");
                Die();
            } else
            if (IsDead && killedByBomb != collider.GetComponent<Blastbeam>().Bomb&& customAnimator.gameObject.activeSelf) {
                PlayerController.Instance.CreateCorpseParts(transform.position);
                audioSource.PlayOneShot(CorpseExplode);
                customAnimator.gameObject.SetActive(false);
            }
        }
        if(IsDead && collider.GetComponent<PlayerMove>() != null) {
            audioSource.PlayOneShot(OnDeadWalkedOver[UnityEngine.Random.Range(0,OnDeadWalkedOver.Length)]);
        }
    }
    [Serializable]
    public struct PowerUPSound {
        public PowerUPType type;
        public AudioClip clip;
    }

    internal void Die() {
        IsDead = true;
    }
}
