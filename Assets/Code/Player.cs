using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody _rb;
    public float Speed;    
    private Vector3 _movement;

    /// <summary>
    /// Stores the action possible parent's (MazeManager in this case)
    /// can execute when the Player object collided with the Prize object
    /// </summary>
    public delegate void NotifyPrizeFound();
    public NotifyPrizeFound OnPrizeFound;
    public GameObject Prize { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        this._rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // capture the change in input of the keyboard/controller
        this._movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));        
    }

    /// <summary>
    /// Using FixedUpdate is recommended for movement on a RigidBody
    /// use the stored direction of the keyboard/controller input to move the Player object
    /// </summary>
    private void FixedUpdate()
    {
        _rb.MovePosition(transform.position + (this._movement * Speed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.Equals(Prize))
        {
            // Notify listeners of prize being found
            OnPrizeFound();
        }
    }
}
