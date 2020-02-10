using System.Collections.Generic;
using UnityEngine;
using UniEasy;

public class MultiBlockSetup : ScriptableObject
{
    [Reorderable(elementName: "Block")]
    public List<EasyBlock> Blocks;
}
