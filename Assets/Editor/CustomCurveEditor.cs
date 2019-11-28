using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing;

public class CustomCurveEdit2 : EditorWindow
{
    [MenuItem("Ext/CurveWindow")]
    private static void ShowWindow()
    {
        CustomCurveEdit2 CurveWindow = (CustomCurveEdit2)EditorWindow.GetWindow(typeof(CustomCurveEdit2), false, "CurveWindow", true);
        CurveWindow.Show();
    }

    ControlState state = new ControlState();
    CustomCurveControl curveCtrl = new CustomCurveControl();
    CustomGradientCtrl gradientCtrl = new CustomGradientCtrl();

    Vector2 newScale = new Vector2(1, 1);
    readonly float delta = 10;
    readonly float spacing = 100;
    Vector2 drawOrigin;
    

    private void Awake()
    {
        Debug.Log("Awake");
        //Debug.Log(wrapper2.KeyframeControls.Count);
        curveCtrl.memberCurveWrapper = new CustomMemberCurveWrapper();
        gradientCtrl.gradient = new CustomGradient();
        state.translation = new Vector2(0, 100);
        state.scale = new Vector2(40, 500);       
    }

    private const float TOOLBAR_HEIGHT = 17f;
    private const string CREATE = "Create";
    private const float CONTROL_WIDTH = 300f;
    private const float TIME_HEIGHT = 20F;
    private const float GRADIENT_HEIGHT = 20F;

    private void OnGUI()
    {
        Rect toolbarArea = new Rect(0, 0, base.position.width, TOOLBAR_HEIGHT);
        Rect controlArea = new Rect(0, TOOLBAR_HEIGHT, CONTROL_WIDTH, base.position.height - TOOLBAR_HEIGHT);
        Rect curveArea = new Rect(controlArea.width, TOOLBAR_HEIGHT + TOOLBAR_HEIGHT, base.position.width- CONTROL_WIDTH, base.position.height - TOOLBAR_HEIGHT - TOOLBAR_HEIGHT);
        Rect timeArea = new Rect(controlArea.width, TOOLBAR_HEIGHT, base.position.width - CONTROL_WIDTH, TOOLBAR_HEIGHT);

        GUILayout.BeginArea(toolbarArea);
        UpdateToolbar(toolbarArea);
        GUILayout.EndArea();

        GUILayout.BeginArea(controlArea);
        //GUILayout.BeginVertical(GUILayout.Width(CONTROL_WIDTH));
        GUI.Box(new Rect(0, 0, CONTROL_WIDTH, Screen.height), "item项目");       
        //GUILayout.EndVertical();
        GUILayout.EndArea();
                                                          
        GUILayout.BeginArea(timeArea);
        GUI.Box(new Rect(0, 0, base.position.width - CONTROL_WIDTH, TOOLBAR_HEIGHT), "time");
        DrawTimeline(timeArea);
        GUILayout.EndArea();

  
        GUILayout.BeginArea(curveArea);
        GUILayout.BeginVertical(GUILayout.Width(base.position.width - CONTROL_WIDTH));
        //gradientCtrl.Draw(state);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(base.position.width - CONTROL_WIDTH));
        UpdateTranslationAndScale(curveArea);
        DrawGrid(curveArea);

        //gradientCtrl.PreUpdate(state, curveArea);
        //gradientCtrl.HandleInput(state, curveArea);


        curveCtrl.PreUpdate(state, curveArea);
        curveCtrl.HandleInput(state, curveArea);
        curveCtrl.Draw();

        GUILayout.EndVertical();
        GUILayout.EndArea();
        //每帧都重新绘制
        Repaint();
    }

    void DrawGrid(Rect gridArea)
    {
        Handles.color = Color.grey;
        float strideX = spacing * state.scale.x;
        float strideY = spacing * state.scale.y;
        while(gridArea.height / strideY < 2)
        {
            strideY /= 10;
        }
        while(gridArea.height / strideY > 20)
        {
            strideY *= 10;
        }
        while (gridArea.width / strideX < 10)
        {
            strideX /= 10;
        }
        while (gridArea.width / strideX > 20)
        {
            strideX *= 10;
        }

        drawOrigin = new Vector2(state.translation.x, gridArea.height - state.translation.y);
         
        while(drawOrigin.x < 0)
        {
            drawOrigin.x += strideX;
        }
        while(drawOrigin.x > gridArea.width)
        {
            drawOrigin.x -= strideX;
        }
        while(drawOrigin.y < 0)
        {
            drawOrigin.y += strideY;
        }
        while(drawOrigin.y > gridArea.height)
        {
            drawOrigin.y -= strideY;
        }

        //计算drawOrigin最近的画线点
        for (float i = drawOrigin.x; i <= gridArea.width; i += strideX)
        {
            Vector2 upPos = new Vector2(i, 0);
            Vector2 downPos = new Vector2(i, gridArea.height);
            Handles.DrawLine(upPos, downPos);

        }
        for (float i = drawOrigin.x; i >=0; i -= strideX)
        {
            Vector2 upPos = new Vector2(i, 0);
            Vector2 downPos = new Vector2(i, gridArea.height);

            Handles.DrawLine(upPos, downPos);
        }

        for (float i = drawOrigin.y; i >= 0; i -= strideY)
        {
            Vector2 leftPos = new Vector2(0, i);
            Vector2 rightPos = new Vector2(gridArea.width, i);

            //float num10 = Mathf.Floor(this.FrameToPixel((float)frame, frameRate, base.rect));
            //string text = this.FormatFrame(frame, frameRate);
            //for(float j = i; j >= i - strideY; )
            float num = gridArea.height - i - state.translation.y;
            //GUI.Label(new Rect(3f, i-3f, 40f, 20f), Mathf.Round(((gridArea.height - i - translation.y) / state.scale.y)).ToString());
            GUI.Label(new Rect(3f, i-3f, 40f, 20f), AccuracyFloat(num, state.scale.y).ToString());
            Handles.DrawLine(leftPos, rightPos);
        }
        for (float i = drawOrigin.y + strideY; i <= gridArea.height; i += strideY)
        {
            Vector2 leftPos = new Vector2(0, i);
            Vector2 rightPos = new Vector2(gridArea.width, i);
            float num = gridArea.height - i - state.translation.y;
            GUI.Label(new Rect(3f, i-3f, 40f, 20f), AccuracyFloat(num, state.scale.y).ToString());

            //GUI.Label(new Rect(3f, i+3f, 40f, 20f), Mathf.Round(((gridArea.height - i - translation.y) / state.scale.y)).ToString());
            Handles.DrawLine(leftPos, rightPos);
        }
    }

    float AccuracyFloat(float num, float standard)
    {
        if (standard > 100 && standard < 1000)
        {
            return Mathf.Round((num * 100) / standard) / 100;
        }
        else if (standard < 100)
        {
            return Mathf.Round(num / standard);
        }
        else return Mathf.Round((num * 1000) / standard) / 1000;               
    }
    //计算逻辑坐标系缩放和偏移
    private void UpdateTranslationAndScale(Rect gridArea)
    {
        Rect drawArea = gridArea;
        drawArea.x = 0f;
        drawArea.y = 0f;
        int controlID = GUIUtility.GetControlID("ZoomableArea".GetHashCode(), FocusType.Passive);
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.ScrollWheel:
                if (drawArea.Contains(Event.current.mousePosition))
                {
                    //计算缩放和偏移
                    Vector2 pos = Event.current.mousePosition;
                    float temp = delta - Event.current.delta.y;
                    newScale.x = temp / delta;
                    newScale.y = newScale.x;
                    state.scale.x *= newScale.x;
                    state.scale.y *= newScale.y;

                    //if (state.scale.x  * newScale.x > 1000)
                    //{
                    //    newScale.x = 1000 / state.scale.x;
                    //    state.scale.x = 1000;
                    //}
                    //else if (state.scale.x * newScale.x < 10)
                    //{
                    //    newScale.x = 10 / state.scale.x;
                    //    state.scale.x = 10;
                    //}
                    //else
                    //{
                    //    state.scale.x *= newScale.x;
                    //}

                    //if (state.scale.y * newScale.y > 1000)
                    //{
                    //    newScale.y = 1000 / state.scale.y;
                    //    state.scale.y = 1000;
                    //}
                    //else if (state.scale.y * newScale.y < 10)
                    //{
                    //    newScale.y = 10 / state.scale.y;                   
                    //    state.scale.y = 10;
                    //}
                    //else
                    //{
                    //    state.scale.y *= newScale.y;
                    //}
                    state.translation.x = (state.translation.x - pos.x) * newScale.x + pos.x;
                    state.translation.y = gridArea.height - ((gridArea.height - state.translation.y - pos.y) * newScale.y + pos.y);
                }
                break;
            case EventType.MouseDrag:
                {
                    if (Event.current.button == 2)
                    {
                        state.translation.x += Event.current.delta.x;
                        state.translation.y -= Event.current.delta.y;
                    }
                }
                break;
        }
    }

    void DrawTimeline(Rect timeArea)
    {
        Handles.color = Color.black;
        float strideX = spacing * state.scale.x;
        while (timeArea.width / strideX < 2)
        {
            strideX /= 10;
        }
        while (timeArea.width / strideX > 20)
        {
            strideX *= 10;
        }
        for (float i = drawOrigin.x; i >= 0; i -= strideX)
        {
            Vector2 upPos = new Vector2(i, timeArea.height);
            Vector2 downPos = new Vector2(i, timeArea.height/2);
            GUI.Label(new Rect(i + 3f, 0f, 40f, 20f), AccuracyFloat(i - state.translation.x, state.scale.x).ToString());
            //GUI.Label(new Rect(i+3f, 3f, 40f, 20f), Mathf.Round(((i -  translation.x) / state.scale.x)).ToString());
            Handles.DrawLine(upPos, downPos);
            for (float j = i; j >= i - strideX; j -= strideX / 10)
            {
                Vector2 uPos = new Vector2(j, timeArea.height);
                Vector2 dPos = new Vector2(j, timeArea.height/1.5f);
                Handles.DrawLine(uPos, dPos);
            }
        }
        for (float i = drawOrigin.x; i <= timeArea.width; i += strideX)
        {
            Vector2 upPos = new Vector2(i, timeArea.height);
            Vector2 downPos = new Vector2(i, timeArea.height / 2);
            GUI.Label(new Rect(i + 3f, 0f, 40f, 20f), AccuracyFloat(i - state.translation.x, state.scale.x).ToString());
            //GUI.Label(new Rect(i + 3f, 3f, 40f, 20f), Mathf.Round(((i - translation.x) / state.scale.x)).ToString());

            Handles.DrawLine(upPos, downPos);
            for (float j = i; j <= i + strideX; j += strideX / 10)
            {
                Vector2 uPos = new Vector2(j, timeArea.height);
                Vector2 dPos = new Vector2(j, timeArea.height / 1.5f);
                Handles.DrawLine(uPos, dPos);
            }
        }
    }
    //绘制工具栏
    private void UpdateToolbar(Rect toolbarArea)
    {
        
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button(CREATE, EditorStyles.toolbarDropDown, GUILayout.Width(60)))
        {

        }

        GUILayout.Button("test", EditorStyles.toolbarButton, GUILayout.Width(60));
        GUILayout.FlexibleSpace();
        GUILayout.Button("时间", EditorStyles.toolbarButton, GUILayout.Width(60));
        GUILayout.Space(10f);
        EditorGUILayout.EndHorizontal();
       
        //int controlID = GUIUtility.GetControlID("Curve".GetHashCode(), FocusType.Passive);
        //if ((Event.current.GetTypeForControl(controlID) == EventType.MouseDown) && (Event.current.button == 1))
        //{
        //    showCurveCanvasContextMenu();
        //    Event.current.Use();
        //}
    }

    private class KeyframeContext
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
