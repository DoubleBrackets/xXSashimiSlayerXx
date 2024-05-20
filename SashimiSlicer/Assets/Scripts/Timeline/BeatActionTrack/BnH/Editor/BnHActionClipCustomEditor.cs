using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BnHActionClip))]
public class BnHActionClipCustomEditor : Editor
{
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view)
    {
        var actionClip = target as BnHActionClip;
        if (actionClip == null)
        {
            return;
        }

        BnHActionBehavior actionData = actionClip.template;
        if (actionData == null)
        {
            return;
        }

        Vector3 pos = actionData.actionData._position;

        Handles.color = Color.red;
        Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);

        Handles.DrawWireDisc(newPos, Vector3.forward, 0.5f);

        if (newPos != pos)
        {
            Undo.RecordObject(actionClip, "Edit BnH Action Clip");
            actionData.actionData._position = newPos;
            Repaint();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}