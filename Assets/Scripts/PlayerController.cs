using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;

    Rigidbody _rb;
    Renderer _renderer;
    Vector3 _vectorMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        MovementSetup();    
    }

    void MovementSetup()
    {
        _vectorMovement.x = Input.GetAxisRaw("Horizontal");
        _vectorMovement.z = Input.GetAxisRaw("Vertical");

        _rb.MovePosition(_rb.position + _vectorMovement * speed * Time.fixedDeltaTime);
    }
}
