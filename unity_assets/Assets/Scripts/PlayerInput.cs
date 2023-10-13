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
    private InputActionReference movement, attack, pointerPosition, roll, rotRoomCW, rotRoomCCW;

    // callbacks to rotate room - set by PlayerSpawner
    public UnityEvent<bool> RotRoomCB { get; set; } = new UnityEvent<bool>();

    // callbacks to update HUD - set by PlayerSpawner
    public UnityEvent<int, int> HitCB { get; set; } = new UnityEvent<int, int>();
    public UnityEvent<int> KillsCB { get; set; } = new UnityEvent<int>();
    public UnityEvent DeathCB { get; set; } = new UnityEvent();

    private Vector3 lastPointerVal = Vector3.zero;
    private bool continueAttack = false;
    private int kills = 0;

    private void Start()
    {
        kills = 0;
        var agent = GetComponent<Agent>();
        agent.OnAttacked.AddListener(WasAttacked);
    }

    private void OnEnable()
    {
        attack.action.started += PerformAttack;
        attack.action.canceled += StopAttack;
        roll.action.started += PerformRoll;
        rotRoomCW.action.started += RotateCW;
        rotRoomCCW.action.started += RotateCCW;
    }

    private void OnDisable()
    {
        attack.action.started -= PerformAttack;
        attack.action.canceled -= StopAttack;
        roll.action.started -= PerformRoll;
        rotRoomCW.action.started -= RotateCW;
        rotRoomCCW.action.started -= RotateCCW;
    }

    void Update()
    {
        OnMovementInput?.Invoke(movement.action.ReadValue<Vector2>().normalized);
        OnPointerInput?.Invoke(GetPointerInput());
        if (continueAttack)
            OnAttack?.Invoke(true);
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
        continueAttack = true;
        OnAttack?.Invoke(true);
    }
    private void StopAttack(InputAction.CallbackContext context)
    {
        continueAttack = false;
        OnAttack?.Invoke(false);
    }
    private void PerformRoll(InputAction.CallbackContext context)
    {
        OnRoll?.Invoke();
    }
    private void RotateCW(InputAction.CallbackContext context)
    {
        RotRoomCB?.Invoke(true);
    }
    private void RotateCCW(InputAction.CallbackContext context)
    {
        RotRoomCB?.Invoke(false);
    }

    public void WasAttacked(int hits, int hp)
    {
        HitCB?.Invoke(hits, hp);
        if (hp == 0)
        {
            DeathCB?.Invoke();
        }
    }

    public void EnemyKilled()
    {
        kills++;
        KillsCB?.Invoke(kills);
    }
}
