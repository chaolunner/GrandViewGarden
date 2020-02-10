using UnityEngine;

public class MessageEvent
{
    public string Message;
    public LogType Type;
    public float Duration;

    public MessageEvent(string msg, LogType type = LogType.Log, float duration = 3)
    {
        Message = msg;
        Type = type;
        Duration = duration;
    }
}
