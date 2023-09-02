using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonMove : MonoBehaviour
{
    public float rotSpeed = 30;
    private Vector3 movement = Vector3.zero;

    private void LateUpdate()
    {
        if (movement != Vector3.zero)
        {
            var face = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, face, rotSpeed * Time.deltaTime);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        movement = new Vector3(move.x, 0, 0);
    }
}
