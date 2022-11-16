using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] PlayerController playerController;
    void Update(){
        animator.SetBool("isJustJumped", playerController.isJustJumped);
        animator.SetBool("isSliding", playerController.isGrounded);
    }
}
