using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 3.0f; //public variable for movement speed
    Vector2 movement = new Vector2(); //vector 2 to represent location

    Rigidbody2D rb2D; //holds rigidbody2d

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>(); //get rigidbody2d
    }

    void FixedUpdate()
    {
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        //get user input to assign location data to movement vector 2 variable

        movement.x = Input.GetAxisRaw("Horizontal");

        movement.Normalize(); //normalize vector2 movement to keep movement at the same speed regardless of direction

        rb2D.velocity = movement * movementSpeed; //calculate velocity
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
