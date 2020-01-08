using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character { Red, Blue, Green, Yellow, Spider, BSD, Tux, Snake }

public class PlayerData : MonoBehaviour {
    public PowerUPSound[] powerUPsounds;
    public AudioClip OtherPowerUPSound;
    public AudioClip FallSound;

    public Teams Team;
    public Character Character = Character.Blue;
    public bool IsDead = false;
    public AudioClip DeathClip;
    public AudioClip[] OnDeadWalkedOver;
    public AudioClip CorpseplodeClip;

    public int Controller { get; internal set; }
    public PlayerMove PlayerMove { get; internal set; }

    AudioSource audioSource;
    internal bool disabled;
    internal Dictionary<KeyInputs, KeyCode> inputToCode;

    public int numberOfWins = 0;

    Bomb killedByBomb;

    void Start() {
        PlayerMove = GetComponent<PlayerMove>();
        audioSource = GetComponent<AudioSource>();
    }
    public void Reset() {
        GetComponentInChildren<CustomAnimator>().gameObject.SetActive(true);
        if(GetComponent<PlayerMove>()==false)
            gameObject.AddComponent<PlayerMove>();
        IsDead = false;
    }

    public void Set(PlayerSetter setter) {
        this.inputToCode = setter.inputToCode;
        this.Controller = setter.controller;
        this.disabled = setter.isDisabled;
        this.Character = setter.character;
        this.Team = setter.team;
    }
    public void OnTriggerEnter2D(Collider2D collider) {
        if(IsDead == false && collider.GetComponent<Blastbeam>() != null) {
            MapController.Instance.CreateAndFlyPowerUPs(PlayerMove.powerUPTypeToAmount,this);
            Destroy(PlayerMove);
            PlayerMove = null;
            GetComponent<CircleCollider2D>().isTrigger = true;
            audioSource.PlayOneShot(DeathClip);
            killedByBomb = collider.GetComponent<Blastbeam>().Bomb;
            Die();
        }
        if (IsDead && killedByBomb != collider.GetComponent<Blastbeam>().Bomb) {
            //TODO: CREATE Chunks
            Debug.Log(killedByBomb == collider.GetComponent<Blastbeam>().Bomb);
            audioSource.PlayOneShot(CorpseplodeClip);
            GetComponentInChildren<CustomAnimator>().gameObject.SetActive(false);
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
