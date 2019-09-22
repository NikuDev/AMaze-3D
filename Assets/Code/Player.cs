using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;
    public Rigidbody Rb;
    public AudioClip PrizeSound;
    public AudioSource PrizeSoundSource;

    private Vector3 _movement;

    // Start is called before the first frame update
    void Start()
    {
        Rb = this.GetComponent<Rigidbody>();
        PrizeSoundSource.clip = PrizeSound;
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
        Rb.MovePosition(transform.position + (direction * Speed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("We hit something! (" + collision.gameObject.name + ")");
        if (collision.gameObject.name.Contains("Prize"))
        {
            Debug.LogError("We've found the prize!");
            PrizeSoundSource.Play();
        }
    }
}
