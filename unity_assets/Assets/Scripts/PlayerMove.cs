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
    private Vector3 mouseAim = Vector3.zero;
    private bool animLock; // its locking up for getting hit <- this should be used to lock up before doing roll???????
    private bool canMove;  // this locks out movement for when dead...o.o//// 
    private float nMin = -1f;
    private float nMax = 1f;

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
            float aimDirX = mouseAim.x-movement.x;
            float aimDirY = mouseAim.y-movement.y;
            aimDirX = Mathf.Clamp(aimDirX, nMin, nMax);
            aimDirY = Mathf.Clamp(aimDirY, nMin, nMax);
            // force set value to not be 1 so the character doesn't end up stock vertical to the camera
            if (aimDirX == 0)
                aimDirX = 1;
            if (aimDirY == 0)
                aimDirY = 1;
            // if animation is not locked, and player not dead, do 
            animator.SetFloat("move_x", aimDirX); //temp, this is so the character doesn't disapear when movement stops for now.
            animator.SetFloat("move_y", aimDirY);
            Debug.Log("aimDirY" + aimDirX + "|" + aimDirY); // text output
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

    public void Look(InputAction.CallbackContext context)  // well the cannon is moving.. for no reason.. i am missing something again -_-//
    {
        var lookAt = context.ReadValue<Vector2>();
        mouseAim = new Vector3(lookAt.x, 0, lookAt.y);
        Debug.Log("aim at"+ mouseAim.x+"|"+ mouseAim.y+"|"+mouseAim.z); // text output
    }
    public void HitByBullet()
    {
        layout.PlayerWasHit();
    }
}
