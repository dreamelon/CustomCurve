using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class CustomCurveWrapper
{
    public UnityEngine.Color Color;
    private AnimationCurve curve;
    public int Id;
    private List<CustomKeyframeWrapper> KeyframeControls = new List<CustomKeyframeWrapper>();
    //public string Label;

    public CustomCurveWrapper(Color color, AnimationCurve curve, int id)
    {
        Color = color;
        Curve = curve;
        Id = id;
    }

    public CustomCurveWrapper()
    {

    }
    ~CustomCurveWrapper() { }

    public int AddKey(float time, float value)
    {
        Keyframe kf = new Keyframe(time, value);
        int index = this.curve.AddKey(kf);

        //for (int j = this.curve.length - 1; j >= 0; j--)
        //{
        //    //修改KeyframeControls中存储的关键点的序号，都加上1
        //    CustomKeyframeWrapper keyframeWrapper = this.GetKeyframeWrapper(j);
        //    //if (keyframeWrapper.index >= index)
        //    //{
        //    //    keyframeWrapper.index++;
        //    //}
        //}
        this.KeyframeControls.Add(this.CreateKeyframeWrapper(curve.length - 1));
        return index;
    }

    private CustomKeyframeWrapper CreateKeyframeWrapper(int index) => 
        new CustomKeyframeWrapper { index = index };

    public float Evaluate(float time) => 
        this.curve.Evaluate(time);

    internal void FlattenKey(int index)
    {
        Keyframe keyframe2 = this.curve[index];
        Keyframe keyframe3 = this.curve[index];
        Keyframe key = new Keyframe(keyframe2.time, keyframe3.value, 0f, 0f) {
            tangentMode = 0
        };
        //AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Auto);
        this.curve.MoveKey(index, key);
    }

    public Vector2 GetInTangentScreenPosition(int index) => 
        this.GetKeyframeWrapper(index).InTangentControlPointPosition;

    public Keyframe GetKeyframe(int index) => 
        this.curve[index];

    public Vector2 GetKeyframeScreenPosition(int index) => 
        this.GetKeyframeWrapper(index).ScreenPosition;

    public CustomKeyframeWrapper GetKeyframeWrapper(int i)
    {
        //foreach (CustomKeyframeWrapper wrapper in this.KeyframeControls)
        //{
        //    if (i == wrapper.index)
        //    {
        //        return wrapper;
        //    }
        //}
        //CustomKeyframeWrapper item = this.CreateKeyframeWrapper(i);
        //this.KeyframeControls.Add(item);
        //return item;
        return KeyframeControls[i];
    }

    public Vector2 GetOutTangentScreenPosition(int index) => 
        this.GetKeyframeWrapper(index).OutTangentControlPointPosition;

    private void InitializeKeyframeWrappers()
    {
        for (int i = 0; i < this.curve.length; i++)
        {
            this.KeyframeControls.Add(this.CreateKeyframeWrapper(i));
        }
    }

    internal bool IsAuto(int index)
    {
        Keyframe keyframe = this.curve[index];
        return (keyframe.tangentMode == 10);
    }

    internal bool IsBroken(int index)
    {
        Keyframe keyframe = this.curve[index];
        return ((keyframe.tangentMode % 2) == 1);
    }

    internal bool IsFreeSmooth(int index)
    {
        Keyframe keyframe = this.curve[index];
        return (keyframe.tangentMode == 0);
    }

    internal bool IsLeftConstant(int index)
    {
        if (this.IsBroken(index))
        {
            Keyframe keyframe = this.curve[index];
            return ((keyframe.tangentMode % 8) == 7);
        }
        return false;
    }

    internal bool IsLeftFree(int index)
    {
        if (this.IsBroken(index))
        {
            Keyframe keyframe = this.curve[index];
            return ((keyframe.tangentMode % 8) == 1);
        }
        return false;
    }

    internal bool IsLeftLinear(int index)
    {
        if (this.IsBroken(index))
        {
            Keyframe keyframe = this.curve[index];
            return ((keyframe.tangentMode % 8) == 5);
        }
        return false;
    }

    internal bool IsRightConstant(int index)
    {
        Keyframe keyframe = this.curve[index];
        return ((keyframe.tangentMode / 8) == 3);
    }

    internal bool IsRightFree(int index)
    {
        if (this.IsBroken(index))
        {
            Keyframe keyframe = this.curve[index];
            return ((keyframe.tangentMode / 8) == 0);
        }
        return false;
    }

    internal bool IsRightLinear(int index)
    {
        Keyframe keyframe = this.curve[index];
        return ((keyframe.tangentMode / 8) == 2);
    }

    public int MoveKey(int index, Keyframe kf)
    {
        int i = this.curve.MoveKey(index, kf);
        return i;
    }

    public void RemoveKey(int id)
    {        
        this.KeyframeControls.RemoveAt(curve.length-1);      
        this.curve.RemoveKey(id);
    }

    public void SetInTangentScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).InTangentControlPointPosition = screenPosition;
    }

    public void SetOutTangentScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).OutTangentControlPointPosition = screenPosition;
    }

    public void SetKeyframeScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).ScreenPosition = screenPosition;
    }

    internal void SetKeyAuto(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent) {
            tangentMode = 10
        };       
        this.curve.MoveKey(index, key);
        //AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.ClampedAuto);
        //AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.ClampedAuto);
        this.curve.SmoothTangents(index, 0f);
    }

    internal void SetKeyBroken(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent) {
            tangentMode = 1
        };
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyFreeSmooth(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent) {
            tangentMode = 0
        };
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyLeftConstant(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, float.PositiveInfinity, keyframe.outTangent);
        int num = (keyframe.tangentMode > 0x10) ? ((keyframe.tangentMode / 8) * 8) : 0;
        key.tangentMode = (6 + num) + 1;
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyLeftFree(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
        int num = (keyframe.tangentMode > 0x10) ? ((keyframe.tangentMode / 8) * 8) : 0;
        key.tangentMode = num + 1;
        this.curve.MoveKey(index, key);

    }

    internal void SetKeyLeftLinear(int index)
    {
        Keyframe keyframe = this.curve[index];
        float inTangent = keyframe.inTangent;
        if (index > 0)
        {
            Keyframe keyframe2 = this.curve[index - 1];
            inTangent = (keyframe.value - keyframe2.value) / (keyframe.time - keyframe2.time);
        }
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, inTangent, keyframe.outTangent);
        int num2 = (keyframe.tangentMode > 0x10) ? ((keyframe.tangentMode / 8) * 8) : 0;
        key.tangentMode = (num2 + 1) + 4;
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyRightConstant(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, float.PositiveInfinity);
        int num = ((keyframe.tangentMode == 10) || (keyframe.tangentMode == 0)) ? 0 : ((keyframe.tangentMode % 8) - 1);
        key.tangentMode = (0x18 + num) + 1;
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyRightFree(int index)
    {
        Keyframe keyframe = this.curve[index];
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
        int num = ((keyframe.tangentMode == 10) || (keyframe.tangentMode == 0)) ? 0 : ((keyframe.tangentMode % 8) - 1);
        key.tangentMode = num + 1;
        this.curve.MoveKey(index, key);
    }

    internal void SetKeyRightLinear(int index)
    {
        Keyframe keyframe = this.curve[index];
        float outTangent = keyframe.outTangent;
        if (index < (this.curve.length - 1))
        {
            Keyframe keyframe2 = this.curve[index + 1];
            outTangent = (keyframe2.value - keyframe.value) / (keyframe2.time - keyframe.time);
        }
        Keyframe key = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, outTangent);
        int num2 = ((keyframe.tangentMode == 10) || (keyframe.tangentMode == 0)) ? 0 : ((keyframe.tangentMode % 8) - 1);
        key.tangentMode = (num2 + 0x10) + 1;
        this.curve.MoveKey(index, key);
    }

    public AnimationCurve Curve
    {
        set
        {
            this.curve = value;
            this.InitializeKeyframeWrappers();
        }
        get
        {
            return curve;
        }
    }

    public int KeyframeCount =>
        this.curve.length;
}

