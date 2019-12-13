using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using System;
using Common;

public class InputSystem : NetworkSystemBehaviour
{
    private IGroup NetworkPlayerComponents;
    private NetworkGroup Network;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        NetworkPlayerComponents = this.Create(typeof(NetworkIdentityComponent), typeof(NetworkPlayerComponent));
        Network = LockstepFactory.Create(NetworkPlayerComponents);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Network.OnUpdate += UpdateInputs;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Network.OnUpdate -= UpdateInputs;
    }

    private void UpdateInputs()
    {
        var axisInput = new AxisInput();
        axisInput.Horizontal = (Fix64)Input.GetAxis(InputParameters.Horizontal);
        axisInput.Vertical = (Fix64)Input.GetAxis(InputParameters.Vertical);
        LockstepUtility.AddInput(axisInput);

        var keyInput = new KeyInput() { KeyCodes = new List<int>() };
        foreach (var obj in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey((KeyCode)obj))
            {
                keyInput.KeyCodes.Add((int)obj);
            }
        }
        LockstepUtility.AddInput(keyInput);

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
        mouseInput.Delta = new FixVector2((Fix64)Input.GetAxis(InputParameters.MouseX), (Fix64)Input.GetAxis(InputParameters.MouseY));
        LockstepUtility.AddInput(mouseInput);
    }
}
