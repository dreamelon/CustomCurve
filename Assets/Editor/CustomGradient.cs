using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomGradient
{
    //public enum BlendMode { Linear, Discrete };
    //public BlendMode blendMode;
    //public bool randomizeColour;

    //[SerializeField]
    //List<ColourKey> keys = new List<ColourKey>();

    public CustomCurveWrapper[] AnimationCurves;

    public CustomGradient()
    {
        CustomCurveWrapper wrapper1 = new CustomCurveWrapper(Color.red, new AnimationCurve(), 1);
        CustomCurveWrapper wrapper2 = new CustomCurveWrapper()
        {
            Curve = new AnimationCurve(),
            Color = Color.green,
            Id = 2
        };
        CustomCurveWrapper wrapper3 = new CustomCurveWrapper()
        {
            //new Keyframe(0.0f, 0.0f), new Keyframe(24f, 0.5f)
            Curve = new AnimationCurve(),
            Color = Color.blue,
            Id = 3
        };
        AnimationCurves = new CustomCurveWrapper[] { wrapper1, wrapper2, wrapper3 };
    }

    public Color Evaluate(float time)
    {
        float r = AnimationCurves[0].Evaluate(time);
        float g = AnimationCurves[1].Evaluate(time);
        float b = AnimationCurves[2].Evaluate(time);
        return new Color(r, g, b);
    }

    public int AddKey(Color colour, float time)
    {
        int i = AnimationCurves[0].AddKey(time, colour.r);
        AnimationCurves[1].AddKey(time, colour.g);
        AnimationCurves[2].AddKey(time, colour.b);
        return i;
    }

    public void RemoveKey(int index)
    {
        AnimationCurves[0].RemoveKey(index);
        AnimationCurves[1].RemoveKey(index);
        AnimationCurves[2].RemoveKey(index);
    }

    public int UpdateKeyTime(int index, float time)
    {
        Color col = GetKey(index).Colour;
        RemoveKey(index);
        return AddKey(col, time);
    }

    public void UpdateKeyColour(int index, Color col)
    {
        Keyframe keyframe0 = AnimationCurves[0].GetKeyframe(index);

        Keyframe kf0 = new Keyframe(keyframe0.time, col.r, keyframe0.inTangent, keyframe0.outTangent)
        {
            tangentMode = keyframe0.tangentMode
        };
        AnimationCurves[0].MoveKey(index, kf0);

        Keyframe keyframe1 = AnimationCurves[0].GetKeyframe(index);

        Keyframe kf1 = new Keyframe(keyframe1.time, col.g, keyframe1.inTangent, keyframe1.outTangent)
        {
            tangentMode = keyframe1.tangentMode
        };
        AnimationCurves[1].MoveKey(index, kf1);

        Keyframe keyframe2 = AnimationCurves[2].GetKeyframe(index);

        Keyframe kf2 = new Keyframe(keyframe2.time, col.b, keyframe2.inTangent, keyframe2.outTangent)
        {
            tangentMode = keyframe0.tangentMode
        };
        AnimationCurves[2].MoveKey(index, kf2);

    }

    public int NumKeys
    {
        get
        {
            return AnimationCurves[0].KeyframeCount;
        }
    }

    public ColourKey GetKey(int i)
    {
        float r = AnimationCurves[0].GetKeyframe(i).value;
        float time = AnimationCurves[0].GetKeyframe(i).time;
        Debug.Log("时间" + time);
        float g = AnimationCurves[1].GetKeyframe(i).value;
        float b = AnimationCurves[2].GetKeyframe(i).value;
        ColourKey temp =  new ColourKey(new Color(r,g,b), time);
        Debug.Log("temp的时间" + temp.Time);
        return temp;
    }

    public Texture2D GetTexture(int width)
    {
        Texture2D texture = new Texture2D(width, 1);
        Color[] colours = new Color[width];
        for (int i = 0; i < width; i++)
        {
            float t = i / (width - 1);
            colours[i] = new Color(AnimationCurves[0].Evaluate(t), AnimationCurves[1].Evaluate(t), AnimationCurves[2].Evaluate(t));
        }
        texture.SetPixels(colours);
        texture.Apply();
        return texture;
    }


    [System.Serializable]
    public struct ColourKey
    {
        [SerializeField]
        float r;
        [SerializeField]
        float g;
        [SerializeField]
        float b;
        [SerializeField]
        float time;


        public ColourKey(Color colour, float time)
        {
            r = colour.r;
            g = colour.g;
            b = colour.b;
            this.time = time;
        }

        public Color Colour
        {
            get
            {
                return new Color(r,g,b);
            }
        }

        public float Time
        {
            get
            {
                return time;
            }
        }
    }

}
