using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentAnimations : MonoBehaviour
{
    private Animator animator;
    private float moveDir;
    private float lookDirX;
    private float lookDirY;
    private float lastSpeed;

    private bool isRolling;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        isRolling = false;
    }

    public void RotateToPointer(Vector2 lookAt)
    {
        lookDirX = lookAt.x > transform.position.x ? 1 : -1;
        lookDirY = Mathf.Clamp(lookAt.y - transform.position.y, -1f, 1f);
        animator.SetFloat("move_x", lookDirX);
        animator.SetFloat("move_y", lookDirY);
    }

    public void PlayAnimation(Vector2 moveTo)
    {
        if (isRolling)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("player_roll"))
            {
                var time = info.normalizedTime;
                isRolling = (lastSpeed > 0 && time < 1f) || (lastSpeed < 0 && time > 0f);
            }
        }
        else if (moveTo == Vector2.zero)
        {
            setAnimSpeed(1);
            animator.Play("Base Layer.player_idle");
        }
        else
        {
            moveDir = moveTo.x > 0 ? 1 : -1;
            setAnimSpeed(moveDir == lookDirX ? 1 : -1);
            animator.Play("Base Layer.player_walk");
        }
    }

    public void PlayDead()
    {
        animator.Play("Base Layer.player_death");
    }

    public void PlayRoll()
    {
        isRolling = true;
        animator.Play("Base Layer.player_roll", -1, lastSpeed > 0 ? 0 : 1);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("atk");
    }

    private void setAnimSpeed(float speed)
    {
        lastSpeed = speed;
        animator.SetFloat("animSpeed", speed);
    }
}
