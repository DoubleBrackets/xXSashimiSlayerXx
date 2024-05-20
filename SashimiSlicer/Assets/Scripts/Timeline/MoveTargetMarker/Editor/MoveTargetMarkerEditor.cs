using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MoveTargetMarker))]
public class MoveTargetMarkerEditor : Editor
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
        var moveTargetMarker = target as MoveTargetMarker;
        if (moveTargetMarker == null)
        {
            return;
        }

        Vector3 pos = moveTargetMarker.position;

        Handles.color = Color.yellow;

        Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
        Handles.DrawWireDisc(moveTargetMarker.position, Vector3.forward, 0.5f);

        if (newPos != pos)
        {
            Undo.RecordObject(moveTargetMarker, "Move Target Marker");
            moveTargetMarker.position = newPos;
            Repaint();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}