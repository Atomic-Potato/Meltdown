using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public static bool enableGlobalDebugging;
    [SerializeField] bool globablDebugging;

    private void Awake() {
        enableGlobalDebugging = globablDebugging;
    }
}
