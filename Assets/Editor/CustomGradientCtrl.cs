using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomGradientCtrl : CommonCtrl
{
    public CustomGradient gradient;
    protected CurveSelection selection = new CurveSelection();
    private const float GRADIENT_HEIGHT = 20F;

    Vector2 translation;//逻辑坐标系原点相对屏幕坐标系偏移量
    Vector2 scale;//逻辑坐标系缩放量

    private Texture2D colorHandle = new Texture2D(8, 16);
    private Texture2D colorHandleHighlight = new Texture2D(8, 16);
    Rect[] keyRects;
    int selectedKeyIndex = -1;
    const float keyWidth = 8;
    const float keyHeight = 16;
    bool mouseIsDownOverKey;
    Rect gradientPreviewRect;

    public CustomGradientCtrl()
    {
        colorHandle = EditorGUIUtility.Load("Assets/Resources/GradientColorKey.png") as Texture2D;
        colorHandleHighlight = EditorGUIUtility.Load("Assets/Resources/GradientColorKey.Highlight.png") as Texture2D;
    }

    public void Draw(ControlState state, Rect curveArea, Vector2 baseWindow)
    {
        DrawCurve(state, curveArea, baseWindow);
        DrawKey();
        DrawGradient(state, curveArea);
    }

    private void DrawGradient(ControlState state, Rect curveArea)
    {       

        gradientPreviewRect = new Rect(0, 0, curveArea.width, GRADIENT_HEIGHT);
        Texture2D texture = GetTexture(state, curveArea);
        GUI.DrawTexture(gradientPreviewRect, texture);

        //draw gradientkey
        keyRects = new Rect[gradient.NumKeys];
        for (int i = 0; i < gradient.NumKeys; i++)
        {
            CustomGradient.ColourKey key = gradient.GetKey(i);
            Rect keyRect = new Rect(state.translation.x + state.scale.x * key.Time - keyWidth / 2f, gradientPreviewRect.yMax, keyWidth, keyHeight);
            GUI.color = key.Colour;
            if (i == selectedKeyIndex)
            {
                //GUI.color = Color.white;
                GUI.color = key.Colour;
                GUI.DrawTexture(keyRect, colorHandleHighlight);
            }
            else
            {
                
                GUI.DrawTexture(keyRect, colorHandle);
            }
            //EditorGUI.ColorField(keyRect, key.Colour);
            keyRects[i] = keyRect;
        }
        
        Rect settingsRect = new Rect(0, GRADIENT_HEIGHT, 50, 20);
        EditorGUI.BeginChangeCheck();
        Color newColour = new Color();
        if (selectedKeyIndex != -1) 
            newColour = EditorGUI.ColorField(settingsRect, gradient.GetKey(selectedKeyIndex).Colour);

        if (EditorGUI.EndChangeCheck())
        {
            gradient.UpdateKeyColour(selectedKeyIndex, newColour);
        }
    }

    private Texture2D GetTexture(ControlState state, Rect curveArea)
    {
        int width = (int)curveArea.width;
        Texture2D texture = new Texture2D(width, 1);
        Color[] colours = new Color[width];
        for (int i = 0; i < width; i++)
        {
            float t = (i  - state.translation.x) / scale.x;
            colours[i] = gradient.Evaluate(t);
        }
        texture.SetPixels(colours);
        texture.Apply();
        return texture;
    }
    private void DrawCurve(ControlState state, Rect curveArea, Vector2 baseWindow)
    {
        foreach (var wrapper in gradient.AnimationCurves)
        {
            //if(wrapper.KeyframeCount == 0)
            //{
            //    Handles.color = Color.yellow;
            //    float temp = (baseWindow.y / 2 - curveArea.height + state.translation.y)*2 / baseWindow.y;
            //    Handles.DrawDottedLine(new Vector3(0, temp), new Vector3(1, temp), 0.05f);
            //}
            //else if(wrapper.KeyframeCount == 1)
            //{
            //    CustomKeyframeWrapper kfWrapper = wrapper.GetKeyframeWrapper(0); 
            //    Handles.color = wrapper.Color;
            //    Handles.DrawDottedLine(new Vector3(0, kfWrapper.ScreenPosition.y), new Vector3(curveArea.width, kfWrapper.ScreenPosition.y), 10f);
            //}
            for (int i = 0; i < (wrapper.KeyframeCount - 1); i++)
            {
                CustomKeyframeWrapper keyframeWrapper = wrapper.GetKeyframeWrapper(i);
                CustomKeyframeWrapper wrapper3 = wrapper.GetKeyframeWrapper(i + 1);

                Handles.DrawBezier(new Vector3(keyframeWrapper.ScreenPosition.x, keyframeWrapper.ScreenPosition.y),
                                   new Vector3(wrapper3.ScreenPosition.x, wrapper3.ScreenPosition.y),
                                   new Vector3(keyframeWrapper.OutTangentControlPointPosition.x, keyframeWrapper.OutTangentControlPointPosition.y),
                                   new Vector3(wrapper3.InTangentControlPointPosition.x, wrapper3.InTangentControlPointPosition.y),
                                   wrapper.Color, null, 4f);
            }
        }
    }

    private void DrawKey()
    {
        foreach (var wrapper in gradient.AnimationCurves)
        {
            for (int i = 0; i < (wrapper.KeyframeCount); i++)
            {
                Keyframe keyframe = wrapper.GetKeyframe(i);
                CustomKeyframeWrapper keyframeWrapper = wrapper.GetKeyframeWrapper(i);

                bool flag = (this.selection.KeyId == i) && (this.selection.CurveId == wrapper.Id);

                //画菱形标志
                Texture2D texture = Texture2D.whiteTexture;
                Rect rect3 = new Rect(keyframeWrapper.ScreenPosition.x - 4f, keyframeWrapper.ScreenPosition.y - 4f, 8f, 8f);
                GUI.Label(rect3, texture);

                //画左右切线
                if (flag)
                {
                    if (((i > 0) && !wrapper.IsAuto(i)) && (!wrapper.IsLeftLinear(i) && !wrapper.IsLeftConstant(i)))
                    {
                        Vector2 vector = new Vector2(keyframeWrapper.InTangentControlPointPosition.x - keyframeWrapper.ScreenPosition.x, keyframeWrapper.InTangentControlPointPosition.y - keyframeWrapper.ScreenPosition.y);
                        vector.Normalize();
                        vector = (Vector2)(vector * 30f);
                        Handles.color = Color.gray;
                        Handles.DrawLine(new Vector3(keyframeWrapper.ScreenPosition.x, keyframeWrapper.ScreenPosition.y, 0f), new Vector3(keyframeWrapper.ScreenPosition.x + vector.x, keyframeWrapper.ScreenPosition.y + vector.y, 0f));
                        Rect rect = new Rect((keyframeWrapper.ScreenPosition.x + vector.x) - 4f, (keyframeWrapper.ScreenPosition.y + vector.y) - 4f, 8f, 8f);
                        //依赖timelinetrackcontrl
                        //GUI.Label(rect, string.Empty, TimelineTrackControl.styles.tangentStyle);
                        GUI.Label(rect, texture);
                    }
                    if (((i < (wrapper.KeyframeCount - 1)) && !wrapper.IsAuto(i)) && (!wrapper.IsRightLinear(i) && !wrapper.IsRightConstant(i)))
                    {
                        Vector2 vector2 = new Vector2(keyframeWrapper.OutTangentControlPointPosition.x - keyframeWrapper.ScreenPosition.x, keyframeWrapper.OutTangentControlPointPosition.y - keyframeWrapper.ScreenPosition.y);
                        vector2.Normalize();
                        vector2 = (Vector2)(vector2 * 30f);
                        Handles.color = Color.gray;
                        Handles.DrawLine(new Vector3(keyframeWrapper.ScreenPosition.x, keyframeWrapper.ScreenPosition.y, 0f), new Vector3(keyframeWrapper.ScreenPosition.x + vector2.x, keyframeWrapper.ScreenPosition.y + vector2.y, 0f));
                        Rect rect2 = new Rect((keyframeWrapper.ScreenPosition.x + vector2.x) - 4f, (keyframeWrapper.ScreenPosition.y + vector2.y) - 4f, 8f, 8f);
                        GUI.Label(rect2, texture);
                    }
                }

            }
        }
    }

    public void PreUpdate(ControlState state, Rect curveArea)
    {
        UpdateKeyframes(state, curveArea);      
    }

    void UpdateKeyframes(ControlState state, Rect curveArea)
    {
        scale = state.scale;
        translation = state.translation;
        foreach (var wrapper in gradient.AnimationCurves)
        {
            for (int i = 0; i < wrapper.KeyframeCount; i++)
            {
                Keyframe keyframe = wrapper.GetKeyframe(i);

                Vector2 screenPosition = new Vector2(keyframe.time * scale.x + translation.x, curveArea.height - (keyframe.value * scale.y + translation.y));
                wrapper.SetKeyframeScreenPosition(i, screenPosition);

                if (i < (wrapper.KeyframeCount - 1))
                {
                    float num3 = Mathf.Abs((float)(wrapper.GetKeyframe(i + 1).time - keyframe.time)) * 0.3333333f;
                    float outTangent = keyframe.outTangent;
                    if (float.IsPositiveInfinity(keyframe.outTangent))
                    {
                        outTangent = 0f;
                    }
                    Vector2 vector2 = new Vector2(keyframe.time + num3, keyframe.value + (num3 * outTangent));
                    Vector2 vector3 = new Vector2(vector2.x * scale.x + translation.x, curveArea.height - (vector2.y * scale.y + translation.y));
                    wrapper.SetOutTangentScreenPosition(i, vector3);
                }
                if (i > 0)
                {
                    float num5 = Mathf.Abs((float)(wrapper.GetKeyframe(i - 1).time - keyframe.time)) * 0.3333333f;
                    float inTangent = keyframe.inTangent;
                    if (float.IsPositiveInfinity(keyframe.inTangent))
                    {
                        inTangent = 0f;
                    }
                    Vector2 vector4 = new Vector2(keyframe.time - num5, keyframe.value - (num5 * inTangent));
                    Vector2 vector5 = new Vector2(vector4.x * scale.x + translation.x, curveArea.height - (vector4.y * scale.y + translation.y));
                    wrapper.SetInTangentScreenPosition(i, vector5);
                }
            }
        }
    }

    public void HandleInput(ControlState state, Rect curveArea)
    {
        HandleKeyframeInput(state, curveArea);
        HandleGradientInput(state);
    }

    private void HandleGradientInput(ControlState state)
    {
        Event guiEvent = Event.current;
        Rect temp = gradientPreviewRect;
        temp.x = 0;
        temp.y = 0;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            for (int i = 0; i < keyRects.Length; i++)
            {
                if (keyRects[i].Contains(guiEvent.mousePosition))
                {
                    mouseIsDownOverKey = true;
                    selectedKeyIndex = i;

                    break;
                }
            }

            if (!mouseIsDownOverKey && temp.Contains(guiEvent.mousePosition) && guiEvent.clickCount == 2)
            {
                float keyTime = (guiEvent.mousePosition.x - state.translation.x) / state.scale.x;
                Color interpolatedColour = gradient.Evaluate(keyTime);    

                selectedKeyIndex = gradient.AddKey(interpolatedColour, keyTime);
                mouseIsDownOverKey = true;
            }
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            mouseIsDownOverKey = false;
        }

        if (mouseIsDownOverKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
        {
            float keyTime = (guiEvent.mousePosition.x - state.translation.x) / state.scale.x;
            for(int i = 0; i<gradient.NumKeys; i++)
            {
                if (keyTime.Equals(gradient.GetKey(i).Time))
                    keyTime += 0.1f;
            }
            selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, keyTime);
        }
        
      
        if (guiEvent.keyCode == KeyCode.Delete && guiEvent.type == EventType.KeyDown)
        {
            for (int i = 0; i < keyRects.Length; i++)
            {
                if (keyRects[i].Contains(guiEvent.mousePosition))
                {
                    gradient.RemoveKey(selectedKeyIndex);
                    if (selectedKeyIndex >= gradient.NumKeys)
                    {
                        selectedKeyIndex--;
                    }

                    break;
                }
            }
        }
    }

    private void HandleKeyframeInput(ControlState state, Rect curveArea)
    {
        scale = state.scale;
        translation = state.translation;
        int controlID1 = GUIUtility.GetControlID("KeyframeControl".GetHashCode(), FocusType.Passive);
        for (int j = gradient.AnimationCurves.Length - 1; j >= 0; j--)
        {
            var wrapper = gradient.AnimationCurves[j];
            for (int i = 0; i < wrapper.KeyframeCount; i++)
            {
                Keyframe keyframe = wrapper.GetKeyframe(i);
                bool flag = (this.selection.KeyId == i) && (this.selection.CurveId == wrapper.Id);

                Vector2 keyframeScreenPosition = wrapper.GetKeyframeScreenPosition(i);
                Rect rect = new Rect(keyframeScreenPosition.x - 4f, keyframeScreenPosition.y - 4f, 8f, 8f);
                if (rect.Contains(Event.current.mousePosition))
                {                   
                    
                    //EditorGUILayout.LabelField("坐标:", "(" + keyframe.time.ToString() + ","+ keyframe.value.ToString() + ")");
                    Rect pos = new Rect(keyframeScreenPosition.x - 40f, keyframeScreenPosition.y - 20f, 80f, 40f);
                    EditorGUI.LabelField(pos, "(" + keyframe.time.ToString("0.00") + "," + keyframe.value.ToString("0.00") + ")");
                }
                switch (Event.current.GetTypeForControl(controlID1))
                {
                    case EventType.MouseDown:
                        {
                            if (rect.Contains(Event.current.mousePosition))
                            {
                                GUIUtility.hotControl = controlID1;
                                this.selection.CurveId = wrapper.Id;
                                this.selection.KeyId = i;
                                Event.current.Use();
                            }
                            continue;
                        }
                    case EventType.MouseUp:
                        {
                            if ((GUIUtility.hotControl != controlID1) || (Event.current.button != 1))
                            {
                                break;
                            }
                            if (flag)
                            {
                                this.ShowKeyframeContextMenu(wrapper, i);
                                GUIUtility.hotControl = 0;
                                Event.current.Use();
                            }
                            continue;
                        }
                    case EventType.MouseMove:
                        {
                            continue;
                        }
                    case EventType.MouseDrag:
                        {
                            if (((GUIUtility.hotControl == controlID1) && (Event.current.button == 0)) && flag)
                            {
                                Keyframe keyframe2 = wrapper.GetKeyframe(i);
                                //float time = (Event.current.mousePosition.x - translation.x) / scale.x;
                                float num5 = (curveArea.height - translation.y - Event.current.mousePosition.y) / scale.y;

                                Keyframe kf = new Keyframe(keyframe2.time, num5, keyframe2.inTangent, keyframe2.outTangent)
                                {
                                    tangentMode = keyframe2.tangentMode
                                };
                                this.selection.KeyId = wrapper.MoveKey(i, kf);
                            }
                            continue;
                        }
                    default:
                        {
                            continue;
                        }
                }
                if ((GUIUtility.hotControl == controlID1) && flag)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }

        }
        this.HandleKeyframeTangentInput(state, curveArea);
    }


    //调整左右切线的斜率
    private void HandleKeyframeTangentInput(ControlState state, Rect curveArea)
    {
        scale = state.scale;
        translation = state.translation;
        foreach (var wrapper in gradient.AnimationCurves)
        {
            for (int i = 0; i < wrapper.KeyframeCount; i++)
            {
                int controlID;
                int num5;
                Keyframe keyframe = wrapper.GetKeyframe(i);
                CustomKeyframeWrapper keyframeWrapper = wrapper.GetKeyframeWrapper(i);
                bool flag = ((this.selection.KeyId == i)) && (this.selection.CurveId == wrapper.Id);

                if (!flag || wrapper.IsAuto(i))
                {
                    continue;
                }
                if (((i > 0) && !wrapper.IsLeftLinear(i)) && !wrapper.IsLeftConstant(i))
                {
                    Vector2 vector = new Vector2(keyframeWrapper.InTangentControlPointPosition.x - keyframeWrapper.ScreenPosition.x, keyframeWrapper.InTangentControlPointPosition.y - keyframeWrapper.ScreenPosition.y);
                    vector.Normalize();
                    vector = (Vector2)(vector * 30f);
                    Rect rect = new Rect((keyframeWrapper.ScreenPosition.x + vector.x) - 4f, (keyframeWrapper.ScreenPosition.y + vector.y) - 4f, 8f, 8f);
                    controlID = GUIUtility.GetControlID("TangentIn".GetHashCode(), FocusType.Passive);
                    switch (Event.current.GetTypeForControl(controlID))
                    {
                        case EventType.MouseDown:
                            if (rect.Contains(Event.current.mousePosition))
                            {
                                GUIUtility.hotControl = controlID;
                                Event.current.Use();
                            }
                            break;

                        case EventType.MouseUp:
                            if (GUIUtility.hotControl == controlID)
                            {
                                GUIUtility.hotControl = 0;
                            }
                            break;

                        case EventType.MouseDrag:
                            goto Label_0213;
                    }
                }
                goto Label_031E;
            Label_0213:
                if (GUIUtility.hotControl == controlID)
                {
                    Vector2 vector2 = new Vector2((Event.current.mousePosition.x - translation.x) / scale.x, (curveArea.height - translation.y - Event.current.mousePosition.y) / scale.y);
                    Vector2 vector3 = vector2 - new Vector2(keyframe.time, keyframe.value);
                    float inTangent = vector3.y / vector3.x;
                    float outTangent = keyframe.outTangent;
                    if (wrapper.IsFreeSmooth(i))
                    {
                        outTangent = inTangent;
                    }
                    Keyframe kf = new Keyframe(keyframe.time, keyframe.value, inTangent, outTangent)
                    {
                        tangentMode = keyframe.tangentMode
                    };
                    wrapper.MoveKey(i, kf);
                }
            Label_031E:
                if (((i < (wrapper.KeyframeCount - 1)) && !wrapper.IsRightLinear(i)) && !wrapper.IsRightConstant(i))
                {
                    Vector2 vector4 = new Vector2(keyframeWrapper.OutTangentControlPointPosition.x - keyframeWrapper.ScreenPosition.x, keyframeWrapper.OutTangentControlPointPosition.y - keyframeWrapper.ScreenPosition.y);
                    vector4.Normalize();
                    vector4 = (Vector2)(vector4 * 30f);
                    Rect rect2 = new Rect((keyframeWrapper.ScreenPosition.x + vector4.x) - 4f, (keyframeWrapper.ScreenPosition.y + vector4.y) - 4f, 8f, 8f);
                    num5 = GUIUtility.GetControlID("TangentOut".GetHashCode(), FocusType.Passive);
                    switch (Event.current.GetTypeForControl(num5))
                    {
                        case EventType.MouseDown:
                            if (rect2.Contains(Event.current.mousePosition))
                            {
                                GUIUtility.hotControl = num5;
                                Event.current.Use();
                            }
                            break;

                        case EventType.MouseUp:
                            if (GUIUtility.hotControl == num5)
                            {
                                GUIUtility.hotControl = 0;
                            }
                            break;

                        case EventType.MouseDrag:
                            if (GUIUtility.hotControl == num5)
                            {
                                Vector2 vector5 = new Vector2((Event.current.mousePosition.x - translation.x) / scale.x, (curveArea.height - translation.y - Event.current.mousePosition.y) / scale.y);
                                Vector2 vector6 = new Vector2(keyframe.time, keyframe.value) - vector5;
                                float num6 = vector6.y / vector6.x;
                                float num7 = keyframe.inTangent;
                                if (wrapper.IsFreeSmooth(i))
                                {
                                    num7 = num6;
                                }
                                Keyframe keyframe3 = new Keyframe(keyframe.time, keyframe.value, num7, num6)
                                {
                                    tangentMode = keyframe.tangentMode
                                };
                                wrapper.MoveKey(i, keyframe3);
                            }
                            break;
                    }
                }
                continue;
            }
        }
    }

    //关键点的功能菜单
    private void ShowKeyframeContextMenu(CustomCurveWrapper animationCurve, int i)
    {
        GenericMenu menu = new GenericMenu();
        CustomKeyframeWrapper keyframeWrapper = animationCurve.GetKeyframeWrapper(i);
        KeyframeContext userData = new KeyframeContext(animationCurve, i, keyframeWrapper);
        Keyframe keyframe = animationCurve.GetKeyframe(i);

        menu.AddItem(new GUIContent("Delete Key"), false, new GenericMenu.MenuFunction2(this.DeleteKey), userData);
        menu.AddSeparator(string.Empty);

        menu.AddItem(new GUIContent("Auto"), animationCurve.IsAuto(i), new GenericMenu.MenuFunction2(this.SetKeyAuto), userData);
        menu.AddItem(new GUIContent("Free Smooth"), animationCurve.IsFreeSmooth(i), new GenericMenu.MenuFunction2(this.SetKeyFreeSmooth), userData);
        menu.AddItem(new GUIContent("Flat"), (animationCurve.IsFreeSmooth(i) && (keyframe.inTangent == 0f)) && (keyframe.outTangent == 0f), new GenericMenu.MenuFunction2(this.SetKeyFlat), userData);
        menu.AddItem(new GUIContent("Broken"), animationCurve.IsBroken(i), new GenericMenu.MenuFunction2(this.SetKeyBroken), userData);
        menu.AddSeparator(string.Empty);
        menu.AddItem(new GUIContent("Left Tangent/Free"), animationCurve.IsLeftFree(i), new GenericMenu.MenuFunction2(this.SetKeyLeftFree), userData);
        menu.AddItem(new GUIContent("Left Tangent/Linear"), animationCurve.IsLeftLinear(i), new GenericMenu.MenuFunction2(this.SetKeyLeftLinear), userData);
        menu.AddItem(new GUIContent("Left Tangent/Constant"), animationCurve.IsLeftConstant(i), new GenericMenu.MenuFunction2(this.SetKeyLeftConstant), userData);
        menu.AddItem(new GUIContent("Right Tangent/Free"), animationCurve.IsRightFree(i), new GenericMenu.MenuFunction2(this.SetKeyRightFree), userData);
        menu.AddItem(new GUIContent("Right Tangent/Linear"), animationCurve.IsRightLinear(i), new GenericMenu.MenuFunction2(this.SetKeyRightLinear), userData);
        menu.AddItem(new GUIContent("Right Tangent/Constant"), animationCurve.IsRightConstant(i), new GenericMenu.MenuFunction2(this.SetKeyRightConstant), userData);
        menu.AddItem(new GUIContent("Both Tangents/Free"), animationCurve.IsLeftFree(i) && animationCurve.IsRightFree(i), new GenericMenu.MenuFunction2(this.SetKeyBothFree), userData);
        menu.AddItem(new GUIContent("Both Tangents/Linear"), animationCurve.IsLeftLinear(i) && animationCurve.IsRightLinear(i), new GenericMenu.MenuFunction2(this.SetKeyBothLinear), userData);
        menu.AddItem(new GUIContent("Both Tangents/Constant"), animationCurve.IsLeftConstant(i) && animationCurve.IsRightConstant(i), new GenericMenu.MenuFunction2(this.SetKeyBothConstant), userData);
        menu.ShowAsContext();
    }

    //曲线关键点的功能的实现
    private void DeleteKey(object userData)
    {
        if (userData is KeyframeContext context)
        {
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.RemoveKey(context.key);
            }
            this.selection.CurveId = -1;
            this.selection.KeyId = -1;
        }
    }

    private void SetKeyAuto(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsAuto(context.key))
        {
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyAuto(context.key);
            }
        }
    }

    private void SetKeyFlat(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if (context != null)
        {
            Keyframe keyframe = context.curveWrapper.GetKeyframe(context.key);
            if (((keyframe.tangentMode != 0) || (keyframe.inTangent != 0f)) || (keyframe.outTangent != 0f))
            {
                foreach (var wrapper in gradient.AnimationCurves)
                {
                    wrapper.FlattenKey(context.key);
                }
            }
        }
    }

    private void SetKeyFreeSmooth(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsFreeSmooth(context.key))
        {
            //Undo.RecordObject(Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            //context.curveWrapper.SetKeyFreeSmooth(context.key);
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyFreeSmooth(context.key);
            }
        }
    }

    private void SetKeyBroken(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsRightFree(context.key))
        {
            //context.curveWrapper.SetKeyBroken(context.key);
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyBroken(context.key);
            }
        }
    }

    private void SetKeyLeftConstant(object userData)
    {
        if ((userData is KeyframeContext context) && !context.curveWrapper.IsLeftConstant(context.key))
        {
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftConstant(context.key);
            }
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyLeftFree(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsLeftFree(context.key))
        {
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            //context.curveWrapper.SetKeyLeftFree(context.key);
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftFree(context.key);
            }
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyLeftLinear(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsLeftLinear(context.key))
        {
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftLinear(context.key);
            }
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");          
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyRightConstant(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsRightConstant(context.key))
        {
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyRightConstant(context.key);
            }
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyRightFree(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && !context.curveWrapper.IsRightFree(context.key))
        {
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            //context.curveWrapper.SetKeyRightFree(context.key);
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyRightFree(context.key);
            }
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyRightLinear(object userData)
    {
        if ((userData is KeyframeContext context) && !context.curveWrapper.IsRightLinear(context.key))
        {
            //Undo.RecordObject(base.Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            //context.curveWrapper.SetKeyRightLinear(context.key);
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyRightLinear(context.key);
            }
            //this.haveCurvesChanged = true;
            //EditorUtility.SetDirty(base.Wrapper.Behaviour);
        }
    }

    private void SetKeyBothConstant(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && (!context.curveWrapper.IsRightConstant(context.key) || !context.curveWrapper.IsLeftConstant(context.key)))
        {
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftConstant(context.key);
                wrapper.SetKeyRightConstant(context.key);
            }
        }
    }

    private void SetKeyBothFree(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && (!context.curveWrapper.IsRightFree(context.key) || !context.curveWrapper.IsLeftFree(context.key)))
        {
            //Undo.RecordObject(Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftFree(context.key);
                wrapper.SetKeyRightFree(context.key);
            }
            //EditorUtility.SetDirty(Wrapper.Behaviour);
        }
    }

    private void SetKeyBothLinear(object userData)
    {
        KeyframeContext context = userData as KeyframeContext;
        if ((context != null) && (!context.curveWrapper.IsRightLinear(context.key) || !context.curveWrapper.IsLeftLinear(context.key)))
        {
            //Undo.RecordObject(Wrapper.Behaviour, "Changed Keyframe Tangent Mode");
            foreach (var wrapper in gradient.AnimationCurves)
            {
                wrapper.SetKeyLeftLinear(context.key);
                wrapper.SetKeyRightLinear(context.key);
            }
        }
    }
}
