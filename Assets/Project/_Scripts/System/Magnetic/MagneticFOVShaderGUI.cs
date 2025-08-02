using UnityEngine;
using UnityEditor;

public class MagneticFOVShaderGUI : ShaderGUI
{
    private bool showMainColors = true;
    private bool showEdgeSettings = true;
    private bool showTransparency = true;
    private bool showRendering = true;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Magnetic FOV Shader", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Main Colors Section
        showMainColors = EditorGUILayout.Foldout(showMainColors, "Main Colors", true);
        if (showMainColors)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_Color", "FOV Color");
            DrawProperty(materialEditor, properties, "_EdgeColor", "Edge Color");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Edge Settings Section  
        showEdgeSettings = EditorGUILayout.Foldout(showEdgeSettings, "Edge Settings", true);
        if (showEdgeSettings)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_EdgeWidth", "Edge Width");
            DrawProperty(materialEditor, properties, "_EdgeSmoothness", "Edge Smoothness");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Transparency Section
        showTransparency = EditorGUILayout.Foldout(showTransparency, "Transparency", true);
        if (showTransparency)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_MainAlpha", "Main Alpha");
            DrawProperty(materialEditor, properties, "_EdgeAlpha", "Edge Alpha");
            DrawProperty(materialEditor, properties, "_FadeDistance", "Fade Distance");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Rendering Section
        showRendering = EditorGUILayout.Foldout(showRendering, "Rendering", true);
        if (showRendering)
        {
            EditorGUI.indentLevel++;

            // Z Test 옵션
            DrawProperty(materialEditor, properties, "_ZTest", "Z Test");

            // Render Queue 조정
            MaterialProperty queueOffset = FindProperty("_RenderQueue", properties);
            EditorGUI.BeginChangeCheck();
            float newOffset = EditorGUILayout.Slider("Render Queue Offset", queueOffset.floatValue, -100, 100);
            if (EditorGUI.EndChangeCheck())
            {
                queueOffset.floatValue = newOffset;
                int newQueue = (int)(3000 + newOffset); // Transparent = 3000
                target.renderQueue = newQueue;
            }

            EditorGUILayout.LabelField($"Current Render Queue: {target.renderQueue}");

            // 프리셋 버튼들
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Behind Objects"))
            {
                queueOffset.floatValue = -50;
                target.renderQueue = 2950;
            }
            if (GUILayout.Button("Normal"))
            {
                queueOffset.floatValue = 0;
                target.renderQueue = 3000;
            }
            if (GUILayout.Button("In Front"))
            {
                queueOffset.floatValue = 50;
                target.renderQueue = 3050;
            }
            if (GUILayout.Button("Always Visible"))
            {
                // Z Test를 Always로 설정
                MaterialProperty zTest = FindProperty("_ZTest", properties);
                zTest.floatValue = 8; // Always
                queueOffset.floatValue = 100;
                target.renderQueue = 3100;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // 추가 정보
        EditorGUILayout.HelpBox(
            "• Main Alpha/Edge Alpha: 전체적인 투명도 조절\n" +
            "• Edge Width: 가장자리 두께\n" +
            "• Fade Distance: 중심에서 가장자리로 페이드 시작점\n" +
            "• Always Visible: 다른 모든 오브젝트 위에 표시",
            MessageType.Info);
    }

    private void DrawProperty(MaterialEditor materialEditor, MaterialProperty[] properties, string name, string displayName)
    {
        MaterialProperty prop = FindProperty(name, properties);
        if (prop != null)
        {
            materialEditor.ShaderProperty(prop, displayName);
        }
    }
}