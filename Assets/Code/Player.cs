using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;
    public Rigidbody Rb;
    public AudioClip PrizeSound;
    public AudioSource PrizeSoundSource;
    public Rigidbody DummyObject;

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
        if (collision.gameObject.name.Contains("Prize"))
        {
            Debug.LogError("We've found the prize!");
            PrizeSoundSource.Play();
            this.InitPrizeAnimation();
        }
    }
    void InitPrizeAnimation()
    {
        int amount = 1000;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        var center = 40 / 2;

        for (int i = 0; i < amount; i++)
        {
            float randomXPos = Random.Range(-center, center);
            float randomYPos = Random.Range(30, 80);

            Vector3 startPos = new Vector3(randomXPos, randomYPos, 0);
            DummyObject.drag = Random.Range(0.1f, 1);
            DummyObject.mass = Random.Range(1, 100);

            float randXScale = Random.Range(0.1f, 0.6f);
            float randyScale = Random.Range(0.1f, 0.6f);
            float randzScale = Random.Range(0.1f, 0.6f);

            DummyObject.transform.localScale = new Vector3(randXScale, randyScale, randzScale);
            Instantiate(DummyObject, startPos, new Quaternion(45, 20, 90, 0));
        }
    }
}
