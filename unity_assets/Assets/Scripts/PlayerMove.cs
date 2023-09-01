using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float Speed = 5;
    public LayoutMgr layout;
    public Animator animator;

    //bool isRolling=false;
    public float hpCount = 0f;

    private Vector3 movement = Vector3.zero;
    private bool animLock; // its locking up for getting hit <- this should be used to lock up before doing roll???????
    private bool canMove;  // this locks out movement for when dead...o.o//// 


    void Start()
    {
        layout.PlayerHP(hpCount);
        animLock = false;
        canMove = true;
    }
    private void LateUpdate()
    {
        if (movement != Vector3.zero && !animLock && canMove)
        {
            // if animation is not locked, and player not dead, do 
            animator.SetFloat("move_x", movement.x+movement.y); //temp, this is so the character doesn't disapear when movement stops for now.
            animator.SetFloat("move_y", 0);
        }
        if (!animLock && canMove)
        {
            if (movement != Vector3.zero)
                animator.Play("Base Layer.player_walk");
            else
                animator.Play("Base Layer.player_idle");
        }
        else if (animLock)
        { 
            var animStateInfo = animator.GetCurrentAnimatorStateInfo(0); 
            var NTime = animStateInfo.normalizedTime;
            if (NTime > 1.0f)
                animLock = false;
            if (!animLock && !canMove)
                animator.Play("Base Layer.player_death");
        }
        if (canMove)
        {
            var move=new Vector3(movement.x, 0 , movement.y)* Speed * Time.deltaTime;
            transform.Translate(move, Space.World);
            layout.LimitMovement(this.gameObject);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        //isWalking = ( move.x>0.1f || move.x <-0.1f) || (move.y>0.1f || move.y <-0.1f)? true:false;
    }

    public void HitByBullet()
    {
        layout.PlayerWasHit();
    }
}
