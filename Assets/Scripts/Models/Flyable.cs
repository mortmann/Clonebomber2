using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Flyable : MonoBehaviour {
    protected SpriteRenderer Renderer;
    protected bool isBouncing, isRising;
    float x, flyHeight;
    protected float flySpeed;
    protected readonly float maxFlySpeed = 1f;
    Vector3 heightParable;
    Vector3 flySpeedParable;
    protected Vector3 flyMove;
    protected Vector3 flyTarget;
    public bool isFlying { private set; get; }
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
            flyTime += Time.fixedDeltaTime;
            x = flyDistance - Mathf.Abs(Vector3.Distance(transform.position, flyTarget));
            flyHeight = CalculateParableValue(heightParable, x);
            float T = Vector3.Distance(startPosition, flyTarget) / flySpeed;
            Vector3 vector3 = Vector3.Lerp(startPosition, flyTarget, Mathf.Clamp01(flyTime / T));
            Vector3 scale = new Vector3(Mathf.Clamp(flyHeight, 1, int.MaxValue),
                                                                    Mathf.Clamp(flyHeight, 1, int.MaxValue), 1);
            Renderer.transform.localScale = scale;
            transform.position = vector3;
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

    private float CalculateParableValue(Vector3 parable, float x) {
        return Mathf.Abs((parable.x * Mathf.Pow(x, 2)) + parable.y * x + (parable.z));
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
        flySpeed = flyDist.magnitude / 1.5f;
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

        flyTime = 0;
        oldLayer = Renderer.sortingLayerName;
        Renderer.sortingLayerName = "Flying";
        isFlying = true;
    }
    public IEnumerator Falling() {
        float overtime = 1f;
        isFalling = true;
        while (Renderer.transform.localScale.x > 0.01f) {
            float modifier = 0.1f * overtime;
            this.gameObject.transform.position = Vector3.Slerp(this.gameObject.transform.position, flyTarget, .1f * overtime);
            Renderer.transform.localScale = new Vector3(Renderer.transform.localScale.x - modifier * 9.81f * Time.fixedDeltaTime,
                                                        Renderer.transform.localScale.y - modifier * 9.81f * Time.fixedDeltaTime);
            Debug.Log(Renderer.transform.localScale);
            overtime += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        IsDoneFalling();
        yield return null;
    }

    protected abstract void IsDoneFalling();

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
