using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyable : MonoBehaviour {
    protected SpriteRenderer Renderer;
    protected virtual void FixedUpdate() {
        if(isFlying) {
            double sqrt = Mathf.Pow(x, 2);
            float tempX2 = (float)(a * sqrt);
            flySpeed = Mathf.Abs(tempX2 + b * x + (c));
            x = -Mathf.Abs(Vector3.Distance(transform.position, flyTarget));
            Renderer.transform.localScale = new Vector3(flySpeed,flySpeed,1);
            transform.position = Vector3.Slerp(transform.position, flyTarget, flyTime);
            flyTime += 0.5f*( flySpeed/Mathf.Abs(x) * Time.fixedDeltaTime);
        }
        if (transform.position==flyTarget) {
            transform.position = MapController.Instance.GetTile(transform.position).GetCenter();
            flyTime = 0;
            gameObject.layer = LayerMask.NameToLayer("Default");
            CheckTile();
            isFlying = false;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    protected virtual void CheckTile() {
        
    }

    float x, a, b, c, flySpeed;
    protected Vector3 flyMove;
    protected Vector3 flyTarget;
    public bool isFlying { protected set; get; }
    public float flyTime { private set; get; }

    public void FlyToTarget(Vector2 target, bool thrown = true) {
        target = MapController.Instance.ClampVector(target);
        if (thrown) {
            gameObject.layer = LayerMask.NameToLayer("THROWN");
        }
        else {
            gameObject.layer = LayerMask.NameToLayer("FLYING");
        }
        this.flyTarget = MapController.Instance.GetTile(target).GetCenter();
        if (transform.position != new Vector3(target.x,target.y)) {
            x = -Mathf.Abs(Vector3.Distance(transform.position, target));
            //x = Mathf.Abs(Vector3.Distance(transform.position,target));
        }
        else {
            x = -1.5f;
        }

        float x1 = 0;
        float x2 = x / 2;
        float x3 = x;
        float y1 = 1;
        float y2 = Mathf.Abs(x);
        float y3 = 1;
        //float x1 = -x / 2;
        //float x2 = Mathf.Abs(x);
        //float x3 = x / 2;
        //float y1 = Mathf.Abs(x);
        //float y2 = 1;
        //float y3 = Mathf.Abs(x);
        //x = -x / 2;

        a = (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x3 - x2));
        b = ((x1 * x1) * (y2 - y3) + (x2 * x2) * (y3 - y1) + (x3 * x3) * (y1 - y2)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));
        c = ((x1 * x1) * (x2 * y3 - x3 * y2) + x1 * ((x3 * x3) * y2 - (x2 * x2) * y3) + x2 * x3 * y1 * (x2 - x3)) / ((x1 - x2) * (x1 - x3) * (x2 - x3));

        if (flyMove.x > target.x) {
            flyMove.x = transform.position.x - target.x;
        }
        else {
            flyMove.x = target.x - transform.position.x;
        }
        if (flyMove.y > target.y) {
            flyMove.y = transform.position.y - target.y;
        }
        else {
            flyMove.y = target.y - transform.position.y;
        }
        flyMove.Normalize();
        isFlying = true;
    }
}
