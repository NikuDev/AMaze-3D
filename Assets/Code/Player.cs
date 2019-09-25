using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;
    private Rigidbody _rb;
    
    private Vector3 _movement;

    public delegate void NotifyPrizeFound();
    public NotifyPrizeFound OnPrizeFound;

    // Start is called before the first frame update
    void Start()
    {
        this._rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        this._movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));        
    }

    // use fixedupdate for rigidbody's instead of update
    private void FixedUpdate()
    {
        MovePlayer(this._movement);
    }

    void MovePlayer(Vector3 direction)
    {
        _rb.MovePosition(transform.position + (direction * Speed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Prize"))
        {
            // Notify listener of prize being found
            OnPrizeFound();
        }
    }
}
