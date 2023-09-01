using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class EnemyAI : MonoBehaviour
{
    public UnityEvent<Vector2> OnMove, OnDirection; // OnMovementInput, OnPointerInput;
    public UnityEvent OnAttack;

    [SerializeField]
    private PlayerMove player;

    [SerializeField]
    public float chaseThreshold = 5f; //chaseDistanceThreshold

    [SerializeField]
    public float attackThreshold = 3f; //atackDistanceThreshold

    [SerializeField]
    private float atkDelay = 1f;
    private float passTime = 1f;

    private void Update()
    {
        if (player == null)
            return;

        float distance = 0f; //float distance= Vector2.Distance(player.postion, transform.position);

        if (distance < chaseThreshold)
        {
            OnDirection?.Invoke(Vector2.zero);//OnDirection?.Invoke(player.position); //check for direction??
            if (distance <= attackThreshold)
            {
                //Attack function
                OnMove?.Invoke(Vector2.zero); //check to see if the player is close enough to be attacked
                if (passTime >= atkDelay)
                {
                    passTime = 0f;
                    OnAttack?.Invoke();
                }
            }
            else
            {
                //Chase function
                Vector2 direction = Vector2.zero;//Vector2 direction = player.position - transform.position;
                OnMove?.Invoke(direction.normalized);
            }
        }
        //enemy idle charge atk
        if (passTime < atkDelay) 
        {
            passTime += Time.deltaTime;
        }
    }

}
