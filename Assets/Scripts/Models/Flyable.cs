﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyable : MonoBehaviour {
    protected SpriteRenderer Renderer;
    protected bool isBouncing, isRising;
    float x, flyHeight;
    protected float flySpeed;
    protected readonly float maxFlySpeed = 13.81f;
    private float currDistance = 0;
    Vector3 heightParable;
    Vector3 flySpeedParable;
    protected Vector3 flyMove;
    protected Vector3 flyTarget;
    private bool _isFlying;
    public bool isFlying { 
        protected set {
            _isFlying = value;
        }
        get {
            return _isFlying;
        } 
    }
    public float flyTime { private set; get; }
    string oldLayer = "";
    public bool isThrown;
    private Vector3 startPosition;
    float flyDistance;
    private float maxHeight = 3;
    public bool isFalling;
    protected int oldGameObjectLayer;
    private void Awake() {
        Renderer = GetComponentInChildren<SpriteRenderer>();
    }
    protected virtual void FixedUpdate() {
        if (isFlying == false && isBouncing == false)
            return;
        if(isBouncing) {
            if (isRising) {
                flyHeight += 6f * Time.deltaTime;
                if (flyHeight > 4.5f)
                    isRising = false;
            } else {
                flyHeight -= 6f * Time.deltaTime;
                if (flyHeight <= 1) {
                    isBouncing = false;
                }
            }
            Renderer.transform.localScale = new Vector3(Mathf.Clamp(flyHeight, 1, 4),
                                            Mathf.Clamp(flyHeight, 1, 4), 1);
        }
        if (isFlying) {
            x = flyDistance - Mathf.Abs(Vector3.Distance(transform.position, flyTarget));
            flyHeight = Mathf.Abs((heightParable.x * Mathf.Pow(x, 2)) + heightParable.y * x + (heightParable.z));
            Renderer.transform.localScale = new Vector3(Mathf.Clamp(flyHeight, 1, int.MaxValue),
                                                        Mathf.Clamp(flyHeight, 1, int.MaxValue), 1);
            if(Input.GetKey(KeyCode.Space)) {
                currDistance += 0.01f * flySpeed * Time.fixedDeltaTime;
            } else {
                currDistance += flySpeed * Time.fixedDeltaTime;
            }
            transform.position = Vector3.Lerp(startPosition, flyTarget, currDistance);
            //transform.position = Vector3.MoveTowards(transform.position, flyTarget, 5 * Time.fixedDeltaTime);
        }
        if (isFlying && transform.position==flyTarget || isFlying==false && isBouncing == false) {
            Vector2 clamp = MapController.Instance.ClampVector(transform.position);
            transform.position = MapController.Instance.GetTile(clamp).GetCenter();
            flyTime = 0;
            Renderer.sortingLayerName = oldLayer;
            isFlying = false;
            Renderer.transform.localScale = new Vector3(1, 1, 1);
            gameObject.layer = oldGameObjectLayer;
            CheckTile();
        }
    }

    protected virtual void CheckTile() {
        
    }

    public void FlyToTarget(Vector2 target, bool thrown = false, bool center = false) {
        if(Renderer==null)
            Renderer = GetComponentInChildren<SpriteRenderer>();
        startPosition = transform.position;
        oldGameObjectLayer = gameObject.layer;
        target = MapController.Instance.ClampVector(target);
        this.flyTarget = MapController.Instance.GetTile(target).GetCenter();
        Vector3 flyDist = transform.position - flyTarget;        
        flySpeed = 6f;
        flySpeed /= Mathf.Sqrt(flyDist.x * flyDist.x + flyDist.y * flyDist.y);
        if (flyTarget == transform.position) {
            isBouncing = true;
            isRising = true;
            oldLayer = Renderer.sortingLayerName;
            gameObject.layer = LayerMask.NameToLayer("FLYING");
            Renderer.sortingLayerName = "Flying";
            return;
        }
        isThrown = thrown;
        if (thrown) {
            gameObject.layer = LayerMask.NameToLayer("THROWN");
        }
        else {
            gameObject.layer = LayerMask.NameToLayer("FLYING");
        }
        flyDistance = Mathf.Abs(Vector3.Distance(transform.position, flyTarget));

        x = Mathf.Abs(Vector3.Distance(startPosition, target));
        if(center == false) {
            heightParable = GetParabel(new Vector2(0, 1), new Vector2(x / 2, maxHeight), new Vector2(x, 1));
            flySpeedParable = GetParabel(new Vector2(0, maxFlySpeed), new Vector2(x / 2, 9.81f), new Vector2(x, maxFlySpeed));
        } else {
            heightParable = GetParabel(new Vector2(-x, 1), new Vector2(0, maxHeight), new Vector2(x, 1));
            flySpeedParable = GetParabel(new Vector2(-x, maxFlySpeed), new Vector2(0, 9.81f), new Vector2(x, maxFlySpeed));
        }

        flyMove.x = target.x - transform.position.x;
        flyMove.y = target.y - transform.position.y;
        flyMove.Normalize();

        currDistance = 0;
        oldLayer = Renderer.sortingLayerName;
        Renderer.sortingLayerName = "Flying";
        isFlying = true;
    }
    public IEnumerator Falling() {
        float overtime = 1f;
        isFalling = true;
        while (Renderer.transform.localScale.x > 0.01f) {
            float modifier = 0.01f * overtime;
            Renderer.transform.localScale = new Vector3(Renderer.transform.localScale.x - modifier * 9.81f * Time.fixedDeltaTime,
                                                        Renderer.transform.localScale.y - modifier * 9.81f * Time.fixedDeltaTime);
            overtime += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(this);
        yield return null;
    }

    public Vector3 GetParabel(Vector2 v1, Vector2 v2, Vector2 v3) {
        float x1 = v1.x;
        float x2 = v2.x;
        float x3 = v3.x;
        float y1 = v1.y;
        float y2 = v2.y;
        float y3 = v3.y;
        if(x1==x2||x1==x3||x1==x2) {
            Debug.LogError("SAME X VALUE DETECTED " + this.name);
            return Vector3.zero;
        }
        float a = (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x3 - x2));
        float b = ((x1 * x1) * (y2 - y3) + (x2 * x2) * (y3 - y1) + (x3 * x3) * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));
        float c = ((x1 * x1) * (x2 * y3 - x3 * y2) + x1 * ((x3 * x3) * y2 - (x2 * x2) * y3) + x2 * x3 * y1 * (x2 - x3)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));
        return new Vector3(a,b,c);
    }

}
