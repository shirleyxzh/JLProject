using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentAnimations : MonoBehaviour
{
    public Animator animator;
    private float moveDir;
    private float lookDir;

    public void RotateToPointer(Vector2 lookAt)
    {
        lookDir = lookAt.x > transform.position.x ? 1 : -1;
        animator.SetFloat("move_x", lookDir);
    }

    public void PlayAnimation(Vector2 moveTo)
    {
        if (moveTo == Vector2.zero)
        {
            animator.Play("Base Layer.player_idle");
            animator.SetFloat("animSpeed", 1);
        }
        else
        {
            animator.Play("Base Layer.player_walk");
            
            moveDir = moveTo.x > 0 ? 1 : -1;
            var speed = moveDir == lookDir ? 1 : -1;
            animator.SetFloat("animSpeed", speed);
        }
    }
}
