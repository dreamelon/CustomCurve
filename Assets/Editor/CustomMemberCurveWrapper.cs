using System;
using UnityEngine;

public class CustomMemberCurveWrapper
{
    public CustomCurveWrapper[] AnimationCurves;
    
    //public bool IsFoldedOut = true;
    //public bool IsVisible = true;
    //public string PropertyName;
    //public UnityEngine.Texture Texture;
    //public string Type;
    public CustomMemberCurveWrapper()
    {
        CustomCurveWrapper wrapper2 = new CustomCurveWrapper(Color.red, new AnimationCurve(), 1);
        CustomCurveWrapper wrapper3 = new CustomCurveWrapper()
        {
            Curve = new AnimationCurve(),
            Color = Color.green,
            Id = 2
        };
        CustomCurveWrapper wrapper4 = new CustomCurveWrapper()
        {
            Curve = new AnimationCurve(),
            Color = Color.blue,
            Id = 3
        };

        AnimationCurves = new CustomCurveWrapper[] { wrapper2, wrapper3, wrapper4 };
    }

}

