using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prize : MonoBehaviour
{
    // timeCounter is used as a helper variable for the circular animation
    private float _timeCounter = 0;
    // to be able to animate the prize correctly, we need to base each transform
    // of the base points (initial x and y)
    private float _prizeObjectBaseX;
    private float _prizeObjectBaseY;

    // Start is called before the first frame update
    void Start()
    {
        this._prizeObjectBaseX = this.transform.position.x;
        this._prizeObjectBaseY = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the speed of the circular animation
        this._timeCounter += Time.deltaTime * 4;

        // combine Cos and Sin to achieve the circular motion, divided by 5 so
        // it stays inside the 1x1 cellSize
        float x = Mathf.Cos(this._timeCounter) / 5;
        float y = Mathf.Sin(this._timeCounter) / 5;

        this.transform.position = new Vector3(this._prizeObjectBaseX + x, this._prizeObjectBaseY + y, 0);
    }
}
