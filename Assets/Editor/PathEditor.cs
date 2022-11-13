using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor{
    
    PathCreator creator;
    Path Path{
        get{
            return creator.path;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUI.BeginChangeCheck();

        if(GUILayout.Button("Reset path")){
            Undo.RecordObject(creator, "Reset Path");
            creator.CreatePath();
        }

        bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto set control points");
        if(autoSetControlPoints != Path.AutoSetControlPoints){
            Undo.RecordObject(creator, "Toggle auto set control points");
            Path.AutoSetControlPoints = autoSetControlPoints;
        }

        if(EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
    }


    //This function refreshes the scene view while in the editor mode
    void OnSceneGUI() {
        Input();
        Draw();
    }

    //Adding a new segment if we shift click in the scene
    void Input(){
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift){
            Undo.RecordObject(creator, "Add Segment");
            Path.AddSegment(mousePos);
        }
    }

    void Draw(){
        //Drawing the lines and the Bezier for each segment
        for(int i=0; i < Path.NumSegments; i++){
            Vector2[] points = Path.GetPointsInSegment(i);
            Handles.color = Color.white;
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        //Drawing the handles for every point in every segment
        Handles.color = Color.red;
        for(int i=0; i < Path.NumPoints; i++){
            //This function also returns the handle position if its moved
            Vector2 newPosition = Handles.FreeMoveHandle(Path[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);

            //If we detect a change in position
            if(Path[i] != newPosition){
                Undo.RecordObject(creator, "Move point"); //records this action so it can be undone
                Path.MovePoint(i, newPosition);
            }
        }
    }

    private void OnEnable() {
        creator = (PathCreator)target;  
        if(creator.path == null)
            creator.CreatePath();
    }
}
