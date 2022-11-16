using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour{
    [SerializeField, HideInInspector] 
    public Path path;

    public void CreatePath(){
        path = new Path(transform.position);
    }

    //Is automatically called when the scrip component is Reset
    void Reset() {
        CreatePath();
    }
}
