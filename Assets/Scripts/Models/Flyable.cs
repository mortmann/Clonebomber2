using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyable : MonoBehaviour {
    protected SpriteRenderer Renderer;
    protected bool isBouncing, isRising;
    float x, flyHeight;
    protected float flySpeed;
    protected readonly float maxFlySpeed = 19.81f;
    private float currDistance = 0;
    Vector3 heightParable;
    Vector3 flySpeedParable;
    protected Vector3 flyMove;
    protected Vector3 flyTarget;
    public bool isFlying { protected set; get; }
    public float flyTime { private set; get; }
    string oldLayer = "";
    public bool isThrown;
    private Vector3 startPosition;
    float flyDistance;
    private float maxHeight = 3;
    public bool isFalling;

    private void Awake() {
        Renderer = GetComponent<SpriteRenderer>();
    }
    protected virtual void FixedUpdate() {
        if (isFlying == false && isBouncing == false)
            return;
        if(isBouncing) {
            if (isRising) {
                flyHeight += 0.5f * 9.81f * Time.deltaTime;
                if (flyHeight > 4.5f)
                    isRising = false;
            } else {
                flyHeight -= 0.5f * 9.81f * Time.deltaTime;
                if (flyHeight <= 1) {
                    isBouncing = false;
                }
            }
            Renderer.transform.localScale = new Vector3(Mathf.Clamp(flyHeight, 1, 4),
                                            Mathf.Clamp(flyHeight, 1, 4), 1);
        }
        if (isFlying) {
            x = -Mathf.Abs(Vector3.Distance(transform.position, flyTarget));

            flyHeight = Mathf.Abs((heightParable.x * Mathf.Pow(x, 2)) + heightParable.y * x + (heightParable.z));
            Renderer.transform.localScale = new Vector3(Mathf.Clamp(flyHeight, 1, int.MaxValue), 
                                                        Mathf.Clamp(flyHeight, 1, int.MaxValue), 1);
            if (BombController.Instance.ParableFlight) {
                flySpeed = Mathf.Abs((flySpeedParable.x * Mathf.Pow(x, 2)) + flySpeedParable.y * x + (flySpeedParable.z));
                currDistance += flySpeed * Time.fixedDeltaTime;
            } else {
                Debug.Log(5 / flyDistance);
                currDistance += (5 / flyDistance) * Time.fixedDeltaTime;
            }
            transform.position = Vector3.Lerp(startPosition, flyTarget, Mathf.Clamp01(currDistance / flyDistance));
        }
        if (isFlying && transform.position==flyTarget || isFlying==false && isBouncing == false) {
            Vector2 clamp = MapController.Instance.ClampVector(transform.position);
            transform.position = MapController.Instance.GetTile(clamp).GetCenter();
            flyTime = 0;
            Renderer.sortingLayerName = oldLayer;
            isFlying = false;
            transform.localScale = new Vector3(1, 1, 1);
            gameObject.layer = LayerMask.NameToLayer("Default");
            CheckTile();
        }
    }

    protected virtual void CheckTile() {
        
    }

    public void FlyToTarget(Vector2 target, bool thrown = false) {
        startPosition = transform.position;
        target = MapController.Instance.ClampVector(target);
        this.flyTarget = MapController.Instance.GetTile(target).GetCenter();
        if(flyTarget == transform.position) {
            isBouncing = true;
            isRising = true;
            gameObject.layer = LayerMask.NameToLayer("FLYING");
            oldLayer = Renderer.sortingLayerName;
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

        x = -Mathf.Abs(Vector3.Distance(transform.position, target));
        heightParable = GetParabel(new Vector2(0, 1), new Vector2(x / 2, maxHeight), new Vector2(x, 1));
        flySpeedParable = GetParabel(new Vector2(0, maxFlySpeed), new Vector2(x / 2, 9.81f), new Vector2(x, maxFlySpeed));
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
        while (transform.localScale.x > 0.01f) {
            float modifier = 0.01f * overtime;
            transform.localScale = new Vector3(transform.localScale.x - modifier * 9.81f * Time.fixedDeltaTime,
                                                 transform.localScale.y - modifier * 9.81f * Time.fixedDeltaTime);
            overtime += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
        yield return null;
    }

    public Vector3 GetParabel(Vector2 v1, Vector2 v2, Vector2 v3) {
        float x1 = v1.x;
        float x2 = v2.x;
        float x3 = v3.x;
        float y1 = v1.y;
        float y2 = v2.y;
        float y3 = v3.y;

        float a = (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x3 - x2));
        float b = ((x1 * x1) * (y2 - y3) + (x2 * x2) * (y3 - y1) + (x3 * x3) * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));
        float c = ((x1 * x1) * (x2 * y3 - x3 * y2) + x1 * ((x3 * x3) * y2 - (x2 * x2) * y3) + x2 * x3 * y1 * (x2 - x3)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));
        return new Vector3(a,b,c);
    }

}
