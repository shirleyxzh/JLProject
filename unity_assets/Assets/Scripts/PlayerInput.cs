using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent<bool> OnAttack;
    public UnityEvent OnRoll;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, roll;

    // callbacks to update HUD - set by PlayerSpawner
    public UnityEvent<int, int> HitCB { get; set; } = new UnityEvent<int, int>();
    public UnityEvent DeathCB { get; set; } = new UnityEvent();

    private Vector3 lastPointerVal = Vector3.zero;

    private void Start()
    {
        var agent = GetComponent<Agent>();
        agent.OnAttacked.AddListener(WasAttacked);
    }

    private void OnEnable()
    {
        attack.action.started += PerformAttack;
        attack.action.canceled += StopAttack;
        roll.action.started += PerformRoll;
    }

    private void OnDisable()
    {
        attack.action.started -= PerformAttack;
        attack.action.canceled -= StopAttack;
        roll.action.started -= PerformRoll;
    }

    void Update()
    {
        OnMovementInput?.Invoke(movement.action.ReadValue<Vector2>().normalized);
        OnPointerInput?.Invoke(GetPointerInput());
    }

    private Vector2 GetPointerInput()
    {
        Vector3 ponterVal = pointerPosition.action.ReadValue<Vector2>();
        if (ponterVal == Vector3.zero)
            ponterVal = lastPointerVal;     // no new input - use last val

        if (ponterVal.x > 1 || ponterVal.y > 1)
        {
            // assume mouse input
            lastPointerVal = ponterVal;
            ponterVal.z = Camera.main.nearClipPlane;
            return Camera.main.ScreenToWorldPoint(ponterVal);
        }

        // joystick input
        if (ponterVal.sqrMagnitude < 1f)
            ponterVal = lastPointerVal;
        else
            lastPointerVal = ponterVal;
        var pos = ponterVal + transform.position;
        return pos;
    }

    private void PerformAttack(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke(true);
    }
    private void StopAttack(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke(false);
    }
    private void PerformRoll(InputAction.CallbackContext context)
    {
        OnRoll?.Invoke();
    }

    public void WasAttacked(int hits, int hp)
    {
        HitCB?.Invoke(hits, hp);
        if (hp == 0)
        {
            DeathCB?.Invoke();
        }
    }
}
