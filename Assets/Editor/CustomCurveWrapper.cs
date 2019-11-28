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
        Keyframe key = new Keyframe();
        Keyframe keyframe2 = new Keyframe();
        for (int i = 0; i < (this.curve.length - 1); i++)
        {
            Keyframe keyframe3 = this.curve[i];
            Keyframe keyframe4 = this.curve[i + 1];
            if ((keyframe3.time < time) && (time < keyframe4.time))
            {
                key = keyframe3;
                keyframe2 = keyframe4;
            }
        }
        Keyframe keyframe5 = new Keyframe(time, value);
        int index = this.curve.AddKey(keyframe5);
        if (index > 0)
        {
            this.curve.MoveKey(index - 1, key);
            if (this.IsAuto(index - 1))
            {
                this.SmoothTangents(index - 1, 0f);
            }
            if (this.IsBroken(index - 1) && this.IsRightLinear(index - 1))
            {
                this.SetKeyRightLinear(index - 1);
            }
        }
        if (index < (this.curve.length - 1))
        {
            this.curve.MoveKey(index + 1, keyframe2);
            if (this.IsAuto(index + 1))
            {
                this.SmoothTangents(index + 1, 0f);
            }
            if (this.IsBroken(index + 1) && this.IsLeftLinear(index + 1))
            {
                this.SetKeyLeftLinear(index + 1);
            }
        }
        for (int j = this.curve.length - 1; j >= 0; j--)
        {
            CustomKeyframeWrapper keyframeWrapper = this.GetKeyframeWrapper(j);
            if (keyframeWrapper.index >= index)
            {
                keyframeWrapper.index++;
            }
        }
        this.KeyframeControls.Add(this.createKeyframeWrapper(index));
        return index;
    }

    private CustomKeyframeWrapper createKeyframeWrapper(int index) => 
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
        foreach (CustomKeyframeWrapper wrapper in this.KeyframeControls)
        {
            if (i == wrapper.index)
            {
                return wrapper;
            }
        }
        CustomKeyframeWrapper item = this.createKeyframeWrapper(i);
        this.KeyframeControls.Add(item);
        return item;
    }

    public Vector2 GetOutTangentScreenPosition(int index) => 
        this.GetKeyframeWrapper(index).OutTangentControlPointPosition;

    private void initializeKeyframeWrappers()
    {
        for (int i = 0; i < this.curve.length; i++)
        {
            this.KeyframeControls.Add(this.createKeyframeWrapper(i));
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
        this.GetKeyframeWrapper(index).index = -99;
        this.GetKeyframeWrapper(i).index = index;
        this.GetKeyframeWrapper(-99).index = i;
        if (this.IsAuto(i))
        {
            this.SmoothTangents(i, 0f);
        }
        if (this.IsBroken(i))
        {
            if (this.IsLeftLinear(i))
            {
                this.SetKeyLeftLinear(i);
            }
            if (this.IsRightLinear(i))
            {
                this.SetKeyRightLinear(i);
            }
        }
        if (index > 0)
        {
            if (this.IsAuto(index - 1))
            {
                this.SmoothTangents(index - 1, 0f);
            }
            if (this.IsBroken(index - 1) && this.IsRightLinear(index - 1))
            {
                this.SetKeyRightLinear(index - 1);
            }
        }
        if (index < (this.curve.length - 1))
        {
            if (this.IsAuto(index + 1))
            {
                this.SmoothTangents(index + 1, 0f);
            }
            if (this.IsBroken(index + 1) && this.IsLeftLinear(index + 1))
            {
                this.SetKeyLeftLinear(index + 1);
            }
        }
        return i;
    }

    internal void RemoveAtTime(float time)
    {
        int i = -1;
        for (int j = 0; j < this.curve.length; j++)
        {
            if (this.curve.keys[j].time == time)
            {
                i = j;
            }
        }
        if (i >= 0)
        {
            CustomKeyframeWrapper keyframeWrapper = this.GetKeyframeWrapper(i);
            this.KeyframeControls.Remove(keyframeWrapper);
            for (int k = 0; k < this.curve.length; k++)
            {
                CustomKeyframeWrapper wrapper2 = this.GetKeyframeWrapper(k);
                if (wrapper2.index >= i)
                {
                    wrapper2.index--;
                }
            }
            this.curve.RemoveKey(i);
            if (i > 0)
            {
                if (this.IsAuto(i - 1))
                {
                    this.SmoothTangents(i - 1, 0f);
                }
                if (this.IsBroken(i - 1) && this.IsRightLinear(i - 1))
                {
                    this.SetKeyRightLinear(i - 1);
                }
            }
            if (i < this.curve.length)
            {
                if (this.IsAuto(i))
                {
                    this.SmoothTangents(i, 0f);
                }
                if (this.IsBroken(i) && this.IsLeftLinear(i))
                {
                    this.SetKeyLeftLinear(i);
                }
            }
        }
    }

    public void RemoveKey(int id)
    {
        CustomKeyframeWrapper keyframeWrapper = this.GetKeyframeWrapper(id);
        this.KeyframeControls.Remove(keyframeWrapper);
        for (int i = 0; i < this.curve.length; i++)
        {
            CustomKeyframeWrapper wrapper2 = this.GetKeyframeWrapper(i);
            if (wrapper2.index >= id)
            {
                wrapper2.index--;
            }
        }
        this.curve.RemoveKey(id);
        if (id > 0)
        {
            if (this.IsAuto(id - 1))
            {
                this.SmoothTangents(id - 1, 0f);
            }
            if (this.IsBroken(id - 1) && this.IsRightLinear(id - 1))
            {
                this.SetKeyRightLinear(id - 1);
            }
        }
        if (id < this.curve.length)
        {
            if (this.IsAuto(id))
            {
                this.SmoothTangents(id, 0f);
            }
            if (this.IsBroken(id) && this.IsLeftLinear(id))
            {
                this.SetKeyLeftLinear(id);
            }
        }
    }

    public void SetInTangentScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).InTangentControlPointPosition = screenPosition;
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

    public void SetKeyframeScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).ScreenPosition = screenPosition;
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

    public void SetOutTangentScreenPosition(int index, Vector2 screenPosition)
    {
        this.GetKeyframeWrapper(index).OutTangentControlPointPosition = screenPosition;
    }

    public void SmoothTangents(int index, float weight)
    {
        this.curve.SmoothTangents(index, weight);
    }



    public AnimationCurve Curve
    {
        set
        {
            this.curve = value;
            this.initializeKeyframeWrappers();
        }
        get
        {
            return curve;
        }
    }

    public int KeyframeCount =>
        this.curve.length;
}

