using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    public float speed;
    public int maxHealth = 100;
    int currentHealth;
    public bool chase = false;
    public Transform startingPoint;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        if (chase == true)
            Chase();
        else
            ReturnStartPoint();
        Flip();
    }

    private void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void ReturnStartPoint()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (transform.position.x > player.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

   /* private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //audioSource.PlayOneShot(healthHeartSound);
            collision.gameObject.GetComponent<HealthManager>().AdjustHitPoints(-1);
            print("Blaine2 got bitten by a mosquito");
            Retreat();
        }
    }  */

    /* public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log("Enemy died!");

        // Death animation

        // Disable enemy
        gameObject.SetActive(false);
    } */
}


/* public class FlyingEnemy : MonoBehaviour
{
    public float speed;
    public int maxHealth = 100;
    int currentHealth;
    public bool chase = false;
    public Transform startingPoint;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        if (chase == true)
            Chase();
        else
            ReturnStartPoint();
        Flip();
    }
    private void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void ReturnStartPoint()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (transform.position.x > player.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

   /*  public void TakeDamage(int damage)
    {
        currentHealth =- damage;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log("Enemy died!");
    } 
}
 */