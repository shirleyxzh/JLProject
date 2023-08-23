using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    
    public float speed;
    public PlayerControl playerControls;

    Vector2 moveDirection=Vector2.zero;
    private InputAction move;
    private InputAction fire;

    private void Awake()
    {
        playerControls = new PlayerControl();

    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
        
        fire = playerControls.Player.Fire;
        fire.Enable();
        fire.performed += Fire;
    }
    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
    }
    void Update()
    {
        //transform.position += new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime;
        moveDirection = move.ReadValue<Vector2>();
    }
    private void LateUpdate()
    {
        transform.position += new Vector3(moveDirection.x, moveDirection.y) * speed * Time.deltaTime;
    }
    private void Fire(InputAction.CallbackContext context) 
    {
        Debug.Log("FIRE!!!!");
    }
}
