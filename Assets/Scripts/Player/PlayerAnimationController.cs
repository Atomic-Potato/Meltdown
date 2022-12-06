using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] string[] tricksNames;

    [SerializeField] Animator animator;
    [SerializeField] TypeRacing typeRacing;
    void Update(){
        if(typeRacing.isFinishedWord){
            int trick = Random.Range(0, tricksNames.Length);
            Debug.Log(tricksNames[trick]);
            animator.SetTrigger(tricksNames[trick]);
        }
    }
}
