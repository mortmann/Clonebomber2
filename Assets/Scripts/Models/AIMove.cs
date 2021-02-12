using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove : PlayerMove {

    KeyInputs? currentInput = null;
    bool doAction;
    public void SetDirection(Vector3 dir) {
        if(dir.x > dir.y) {
            if (dir.x > 0) {
                currentInput = KeyInputs.Right;
            }
            if (dir.x < 0) {
                currentInput = KeyInputs.Left;
            }
        } else {
            if (dir.y > 0) {
                currentInput = KeyInputs.Up;
            }
            if (dir.y < 0) {
                currentInput = KeyInputs.Down;
            }
        }
    }
    protected override bool IsAction() {
        return doAction;
    }
    protected override bool IsUPDown() {
        return currentInput == KeyInputs.Up;
    }
    protected override bool IsDownDown() {
        return currentInput == KeyInputs.Down;
    }
    protected override bool IsLeftDown() {
        return currentInput == KeyInputs.Left;
    }
    protected override bool IsRightDown() {
        return currentInput == KeyInputs.Right;
    }
    protected override bool IsUPUp() {
        return currentInput != KeyInputs.Up;
    }
    protected override bool IsDownUp() {
        return currentInput != KeyInputs.Down;
    }
    protected override bool IsLeftUp() {
        return currentInput != KeyInputs.Left;
    }
    protected override bool IsRightUp() {
        return currentInput != KeyInputs.Right;
    }

}
