using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class Player : MonoBehaviour
{
    // Move player in 2D space
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    public Camera mainCamera;
    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    Transform t;
    private Animator anim;
    public AudioSource audioSource;
    public AudioClip movementSound;
    public AudioClip attackSound;
    public AudioClip speedBoostSound;
    public AudioClip healthHeartSound;
    public AudioClip enemyKillSound;
    public Transform attackPoint;
    public float attackRange = 1f;

    // Health and score
    public bool speedBoost;

    // Start is called before the first frame update
    void Start()
    {
        t = transform;
        anim = GetComponent<Animator>();
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;
        if (mainCamera)
        {
            cameraPos = mainCamera.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movement controls - left and right
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && (isGrounded || Mathf.Abs(r2d.velocity.x) > 0.01f))
        {
            moveDirection = Input.GetKey(KeyCode.A) ? -1 : 1;
        }
        else
        {
            if (isGrounded || r2d.velocity.magnitude < 0.01f)
            {
                moveDirection = 0;
            }
        }

        // Change the character facing direction to match movement
        if (moveDirection != 0)
        {
            if (moveDirection > 0 && !facingRight)
            {
                facingRight = true;
                t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
            }
            if (moveDirection < 0 && facingRight)
            {
                facingRight = false;
                t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
        }

        // Attacking
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // Camera follow
        if (mainCamera)
        {
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
        }
    }

    private void Attack()
    {
        anim.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSound);

        // Use layer masks to filter colliders
        int mosquitoLayerMask = 1 << LayerMask.NameToLayer("Enemies");

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, mosquitoLayerMask);

        foreach (Collider2D enemy in hitEnemies)
        {
            // If enemy is tagged as mosquito, disable it
            if (enemy.gameObject.CompareTag("Mosquito"))
            {
                audioSource.PlayOneShot(attackSound);
                AdjustHitPoints(5);
                audioSource.PlayOneShot(enemyKillSound);
                // Deactivate instead of destroying
                enemy.gameObject.SetActive(false);  
                print("Blaine killed a mosquito");
            }
        }
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);

        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);

        //Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != mainCollider)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Apply movement velocity
        r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);
        anim.SetFloat("Speed", Mathf.Abs(r2d.velocity.x));

        // Calculate normalized speed
        float normalizedSpeed = Mathf.Abs(r2d.velocity.x) / maxSpeed;

        // Set pitch based on normalized speed
        audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, normalizedSpeed);

        // Play movement sound
        if (moveDirection != 0 && isGrounded && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(movementSound);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CanBePickedUp"))
        {
            Item hitObject = collision.gameObject.GetComponent<Consumables>().item;

            if (hitObject != null)
            {
                print("Blaine picked up a " + hitObject.ObjectName);

                switch (hitObject.itemType)
                {
                    case Item.ItemType.NANAIMO:
                        audioSource.PlayOneShot(speedBoostSound);
                        maxSpeed = 6.0f;
                        speedBoost = true;
                        StartCoroutine(PowerUpCooldown());
                        break;
                    case Item.ItemType.HEALTH:
                        audioSource.PlayOneShot(healthHeartSound);
                        GetComponent<HealthManager>().AdjustHitPoints(hitObject.quantity);
                        break;
                    default:
                        break;
                }

                collision.gameObject.SetActive(false);
            }
        }

        // If player collides with a mosquito, he gets bitten and loses health or score

        if (collision.gameObject.CompareTag("Mosquito"))
        {
            audioSource.PlayOneShot(healthHeartSound);
            GetComponent<HealthManager>().AdjustHitPoints(-1);
            print("Blaine got bitten by a mosquito");
        }
    }

    public void AdjustHitPoints(int amount)
    {
        GetComponent<HealthManager>().AdjustHitPoints(amount);
    }

    IEnumerator PowerUpCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        speedBoost = false;
        maxSpeed = 3.4f;
    }
}


/* // From Week #3 - Camera Controls & Character Controller

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class Player : MonoBehaviour
{

    // Move player in 2D space
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    public Camera mainCamera;
    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    Transform t;
    private Animator anim;
    public AudioSource audioSource;
    public AudioClip movementSound;
    public AudioClip attackSound;
    public AudioClip speedBoostSound; //Sound for the speed boost power up
    public AudioClip healthHeartSound; //Sound for health heart pick up
    public AudioClip enemyKillSound; //Sound for defeating an enemy
    public Transform attackPoint;
    public float attackRange = 1f;

    // Health and score
    public int hitPoints = 100;
    //public int score = 0;


    public bool speedBoost; //bool variable for power up cooldown

    // Start is called before the first frame update
    void Start()
    {
        t = transform;
        anim = GetComponent<Animator>();
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;
        if (mainCamera)
        {
            cameraPos = mainCamera.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movement controls - left and right
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && (isGrounded || Mathf.Abs(r2d.velocity.x) > 0.01f))
        {
            moveDirection = Input.GetKey(KeyCode.A) ? -1 : 1;
        }
        else
        {
            if (isGrounded || r2d.velocity.magnitude < 0.01f)
            {
                moveDirection = 0;
            }
        }

        // Change the character facing direction to match movement
        if (moveDirection != 0)
        {
            if (moveDirection > 0 && !facingRight)
            {
                facingRight = true;
                t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
            }
            if (moveDirection < 0 && facingRight)
            {
                facingRight = false;
                t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
        }

        // Attacking
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // Camera follow
        if (mainCamera)
        {
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
        }
    }

    private void Attack()
    {
        anim.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSound);

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // If enemy is tagged as mosquito, destroy it   
            if (enemy.gameObject.CompareTag("Mosquito"))
            {
                audioSource.PlayOneShot(attackSound);
                AdjustHitPoints(5);
                audioSource.PlayOneShot(enemyKillSound);
                Destroy(enemy.gameObject);
                                
                print("Blaine killed a mosquito");
            }
        } 
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        
        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        
        //Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != mainCollider)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Apply movement velocity
        r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);
        anim.SetFloat("Speed", Mathf.Abs(r2d.velocity.x));

        // Calculate normalized speed
        float normalizedSpeed = Mathf.Abs(r2d.velocity.x) / maxSpeed;

        // Set pitch based on normalized speed
        audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, normalizedSpeed);

        // Play movement sound
        if (moveDirection != 0 && isGrounded && !audioSource.isPlaying)
        {
                audioSource.PlayOneShot(movementSound);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        //AudioSource audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        //AudioClip powerUpSound = Resources.Load<AudioClip>("Assets/Audio/Consumables/NanaimoBar.mp3");

        if (collision.gameObject.CompareTag("CanBePickedUp"))
        {
            Item hitObject = collision.gameObject.GetComponent<Consumables>().item;

            if (hitObject != null)
            {

                print("Blaine picked up a " + hitObject.ObjectName);

                switch (hitObject.itemType)
                {
                    case Item.ItemType.NANAIMO:
                        audioSource.PlayOneShot(speedBoostSound);
                        maxSpeed = 6.0f;
                        speedBoost = true;
                        StartCoroutine(PowerUpCooldown());
                        break;
                    case Item.ItemType.HEALTH:
                        audioSource.PlayOneShot(healthHeartSound);
                        AdjustHitPoints(hitObject.quantity);
                        break;
                    default:
                        break;
                }

                collision.gameObject.SetActive(false);
            }           
        }

        // If player collides with a mosquito, he gets bitten and loses health or score

        if (collision.gameObject.CompareTag("Mosquito"))
        {
            audioSource.PlayOneShot(healthHeartSound);
            AdjustHitPoints(-1);
            print("Blaine got bitten by a mosquito");
        }         
    }

    public void AdjustHitPoints(int amount)
    {
       hitPoints += amount;
       print("Adjusted hit points by " + amount + " - Health is now " + hitPoints);
       
       // If player's health is 0, he dies
         if (hitPoints <= 0)
         {
              Death();
         }

    }

    public void Death()
    {
        Debug.Log("Player died!");

        // Death animation

        // Disable player
        gameObject.SetActive(false);
    }

    IEnumerator PowerUpCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        speedBoost = false;
        maxSpeed = 3.4f;
    }
} */