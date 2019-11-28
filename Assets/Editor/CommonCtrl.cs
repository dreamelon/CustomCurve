using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCtrl : Singleton<CommonCtrl>
{ 
    protected class CurvesContext
    {
        public float time;

        public CurvesContext(float time)
        {
            this.time = time;
        }
    }
    protected class KeyframeContext
    {
        public CustomKeyframeWrapper ckw;
        public CustomCurveWrapper curveWrapper;
        public int key;

        public KeyframeContext(CustomCurveWrapper curveWrapper, int key, CustomKeyframeWrapper ckw)
        {
            this.curveWrapper = curveWrapper;
            this.key = key;
            this.ckw = ckw;
        }
    }
}
