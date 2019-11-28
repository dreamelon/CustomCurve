using System;
using UnityEngine;

public class ControlState
{
    public bool IsInPreviewMode;
    public bool IsRippleEnabled;
    public bool IsRollingEnabled;
    public bool IsSnapEnabled;
    public Vector2 scale;//逻辑坐标系缩放量
    public float ScrubberPosition;
    public float TickDistance;
    public Vector2 translation;//逻辑坐标系原点相对屏幕坐标系偏移量

    public float SnappedTime(float time)
    {
        if ((this.IsSnapEnabled && !Event.current.control) || (!this.IsSnapEnabled && Event.current.control))
        {
            time = ((int) ((time + (this.TickDistance / 2f)) / this.TickDistance)) * this.TickDistance;
        }
        return time;
    }

    public float TimeToPosition(float time) => 
        ((time * this.scale.x) + this.translation.x);
}

