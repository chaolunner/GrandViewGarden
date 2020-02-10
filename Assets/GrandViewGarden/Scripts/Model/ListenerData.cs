using UnityEngine;
using System;

public class ListenerData
{
    public GameObjectListener Listener;
    public Collider Collider;

    public Type Type
    {
        get
        {
            return Listener.GetType();
        }
    }

    public ListenerData(GameObjectListener listener)
    {
        Listener = listener;
    }

    public ListenerData(GameObjectListener listener, Collider col)
    {
        Listener = listener;
        Collider = col;
    }
}
