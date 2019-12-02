using System.Collections.Generic;
using UnityEngine;
using System;
using Common;

public class InputSystem : LockstepSystemBehaviour
{
    private Vector3 lastMousePosition;

    public override void OnRestart()
    {
        base.OnRestart();
        lastMousePosition = Input.mousePosition;
    }

    public override IInput[] UpdateInputs()
    {
        var inputs = new IInput[3];

        var axisInput = new AxisInput();
        axisInput.Horizontal = (Fix64)Input.GetAxis(InputParameters.Horizontal);
        axisInput.Vertical = (Fix64)Input.GetAxis(InputParameters.Vertical);
        inputs[0] = axisInput;

        var keyInput = new KeyInput() { KeyCodes = new List<int>() };
        foreach (var obj in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey((KeyCode)obj))
            {
                keyInput.KeyCodes.Add((int)obj);
            }
        }
        inputs[1] = keyInput;

        var mouseInput = new MouseInput() { MouseButtons = new List<int>() };
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetMouseButton(i))
            {
                mouseInput.MouseButtons.Add(i);
            }
        }
        mouseInput.ScrollDelta = (FixVector2)Input.mouseScrollDelta;
        mouseInput.Position = (FixVector3)Input.mousePosition;
        mouseInput.Delta = (FixVector3)(Input.mousePosition - lastMousePosition);
        lastMousePosition = Input.mousePosition;
        inputs[2] = mouseInput;

        return inputs;
    }
}
