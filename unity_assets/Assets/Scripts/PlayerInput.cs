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

    public TextMeshProUGUI hud;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, roll;

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
        Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
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
        hud.text = $"Hits: {hits}\nHP: {hp}";
    }
}
