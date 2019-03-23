using System.Collections.Generic;
using UniEasy.ECS;
using UniEasy;
using System;

public class SceneLoader : ComponentBehaviour
{
    public enum Operation
    {
        Load,
        Unload,
    }

    [Serializable]
    public struct MultiSceneSetupBlock
    {
        public Operation Operation;
        public MultiSceneSetup SceneSetup;
    }

    [Reorderable(elementName: "Step")]
    public List<MultiSceneSetupBlock> LoadQueue;
}
