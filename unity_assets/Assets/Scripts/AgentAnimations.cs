using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentAnimations : MonoBehaviour
{
    public Transform eyeLevel { get; set; }

    private Animator animator;
    private float moveDir;
    private float lookDirX;
    private float lookDirY;

    public bool isRolling { get; private set; }
    private float blinkTimer;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        blinkTimer = Random.Range(5f, 8f);
        isRolling = false;
    }

    public void RotateToPointer(Vector2 lookAt)
    {
        lookDirX = lookAt.x > transform.position.x ? 1 : -1;
        lookDirY = Mathf.Clamp(lookAt.y - eyeLevel.position.y, -1f, 1f);
        animator.SetFloat("move_x", lookDirX);
        animator.SetFloat("move_y", lookDirY);
        animator.SetFloat("eye_y", lookDirY);
    }

    public void PlayAnimation(Vector2 moveTo)
    {
        if (isRolling)
        {
            isRolling = GetRollTime() < 1f;
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

        blinkTimer -= Time.deltaTime;
        if (blinkTimer < 0)
        {
            animator.SetTrigger("face_blink");
            blinkTimer = Random.Range(5f, 8f);
        }
    }

    public float GetRollTime()
    {
        var time = 0f;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("player_roll"))
            time = info.normalizedTime;
        return Mathf.Clamp01(time);
    }

    public void PlayDead()
    {
        animator.SetTrigger("face_death");
        animator.Play("Base Layer.player_death");
    }

    public void PlayRoll()
    {
        setAnimSpeed(1);
        animator.Play("Base Layer.player_roll");
        isRolling = true;
    }

    public void PlayAttack(bool AttackStarted)
    {
        if (AttackStarted)
            animator.SetTrigger("atk");
        else
            animator.ResetTrigger("atk");
    }

    private void setAnimSpeed(float speed)
    {
        animator.SetFloat("animSpeed", speed);
    }
}
