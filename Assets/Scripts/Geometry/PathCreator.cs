using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour{
    [SerializeField, HideInInspector] 
    public Path path;

    public Color anchorCol = Color.red;
    public Color controlCol = Color.blue;
    public Color segementCol = Color.yellow;
    public float anchorDiameter = 1f;
    public float controlDiameter = 0.75f;
    public bool displayControlPoints = true;

    public void CreatePath(){
        path = new Path(transform.position);
    }

    //Is automatically called when the scrip component is Reset
    void Reset() {
        CreatePath();
    }
}
