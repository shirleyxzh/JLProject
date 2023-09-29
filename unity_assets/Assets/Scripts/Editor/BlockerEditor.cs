using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BlockerMaker))]

public class BlockerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BlockerMaker myScript = (BlockerMaker)target;

        if (GUILayout.Button("Create Blockers"))
        {
            Selection.activeGameObject = myScript.createObject();
            EditorUtility.SetDirty(target);
        }
    }
}