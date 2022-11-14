using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GroundCreator))]
public class GroundEditor : Editor{
    GroundCreator creator;

    void OnSceneGUI() {
        if(creator.autoUpdate && Event.current.type == EventType.Repaint)
            creator.UpdateGround();    
    }

    void OnEnable() {
        creator = (GroundCreator)target;
    }
}
