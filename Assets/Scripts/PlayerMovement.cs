using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class Player : MonoBehaviour
{
    // Move player in 2D space
    public Transform playerBlaine;
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
    public AudioClip grassWalkingSound;
    public AudioClip woodWalkingSound;
    public AudioClip attackSound;
    public AudioClip speedBoostSound;
    public AudioClip healthHeartSound;
    public AudioClip enemyKillSound;
    public Transform attackPoint;
    public float attackRange = 1f;
    public float attackRate = 2f;
    float attackCooldown = 0f;
    private bool isChasing = true;
    public Vector3 textOffset = new Vector3(0f, 1.5f, 0f);

    // Health and score
    public bool speedBoost;
    public bool slowDown; //bool for syrup puddle debuff
    public AudioClip puddleSound;

    [SerializeField]
    private bool powerUpReady; //bool variable for power up cooldown
    public Transform HealthHeart;

    public TextMeshPro textMsg; //variable to hold messages to display to the player

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

        StartCoroutine(PowerUpCooldown());

        textMsg.enabled = false; //start with an empty text field
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && Mathf.Abs(r2d.velocity.y) < 0.01f)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
            Debug.Log("***JUMPING***");
        }

        // Attacking
        if (Time.time >= attackCooldown) 
        { 
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                attackCooldown = Time.time + 1f / attackRate;
            }
        }

        // Camera follow
        if (mainCamera)
        {
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
        }

        // Text follow 
        if (textMsg)
        {
            textMsg.transform.position = playerBlaine.position + textOffset;
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
                //AdjustHitPoints(5);
                audioSource.PlayOneShot(enemyKillSound);

                //random powerup drops
                if (powerUpReady == true)
                {
                    //implement random chance for mosquitos to drop a health heart upon death
                    int randomChance = Random.Range(1, 101);

                    if (randomChance < 40)
                    {
                        Instantiate(HealthHeart, enemy.transform.position, enemy.transform.rotation);
                        powerUpReady = false;
                        StartCoroutine(PowerUpCooldown());
                        Debug.Log("Power up on Cooldown");
                    }
                }

                //random messages from Blaine
                if (textMsg.enabled == false)
                {
                    //implement random chance for Blaine to say something
                    int randomChance = Random.Range(1, 101);

                    if (randomChance < 10)
                    {
                        textMsg.enabled = true;
                        textMsg.text = "Got 'em";
                        StartCoroutine(textCooldown());
                    }
                    if (randomChance >= 21 && randomChance <= 40)
                    {
                        textMsg.enabled = true;
                        textMsg.text = "Squished!";
                        StartCoroutine(textCooldown());
                    }
                }

                GetComponent<ScoreManager>().AdjustScorePoints(+10);

                // Deactivate instead of destroying
                enemy.gameObject.SetActive(false);
                print("Blaine killed a mosquito");
            } 
            
            if (enemy.gameObject.CompareTag("MosquitoBoss"))
            {
                audioSource.PlayOneShot(attackSound);
                //AdjustHitPoints(5);
                audioSource.PlayOneShot(enemyKillSound);

                GetComponent<ScoreManager>().AdjustScorePoints(+50);

                // Deactivate instead of destroying
                enemy.gameObject.SetActive(false);
                print("Blaine killed a MosquitoBoss");         
            }
        }
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);

        // Use LayerMask to filter colliders with the "Ground" tag
        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius, groundLayerMask);

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
        anim.SetBool("OnGround", isGrounded);

        // Calculate normalized speed
        float normalizedSpeed = Mathf.Abs(r2d.velocity.x) / maxSpeed;
        
        // Set pitch based on normalized speed
        audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, normalizedSpeed);
            if (moveDirection != 0 && isGrounded && !audioSource.isPlaying && Mathf.Abs(r2d.velocity.y) < 0.01f)
            {
                audioSource.PlayOneShot(grassWalkingSound);
            }
       else if (moveDirection != 0 && isGrounded && !audioSource.isPlaying && Mathf.Abs(r2d.velocity.y) < 0.01f)
            {
                audioSource.PlayOneShot(woodWalkingSound);
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
                    case Item.ItemType.NANAIMO: //speed boost for 5 seconds
                        audioSource.PlayOneShot(speedBoostSound);
                        maxSpeed = 5.0f;
                        speedBoost = true;
                        GetComponent<ScoreManager>().AdjustScorePoints(+20);
                        print("Blaine is speedy!");
                        textMsg.enabled = true;
                        textMsg.text = "Sugar Rush!";
                        StartCoroutine(NanaimoCooldown());
                        StartCoroutine(textCooldown());
                        break;
                    case Item.ItemType.HEALTH: //5hp restored
                        audioSource.PlayOneShot(healthHeartSound);
                        GetComponent<HealthManager>().AdjustHitPoints(+20);
                        print("Blaine restored health");
                        break;
                    case Item.ItemType.DOUBLE_DOUBLE: //20hp restored + double speed for 10 seconds
                        audioSource.PlayOneShot(healthHeartSound);
                        GetComponent<HealthManager>().AdjustHitPoints(+40);
                        GetComponent<ScoreManager>().AdjustScorePoints(+20);
                        maxSpeed = 6.8f;
                        speedBoost = true;
                        textMsg.enabled = true;
                        textMsg.text = "Double Double Time!";
                        StartCoroutine(DoubleDoubleCooldown());
                        StartCoroutine(textCooldown());
                        print("Double double time!");
                        break;
                    default:
                        break;
                }

                collision.gameObject.SetActive(false);
            }
        }

        // If player walks through syrup puddle, speed debuff

        if (collision.CompareTag("Puddle"))
        {
            audioSource.PlayOneShot(puddleSound);
            maxSpeed = 0.7f;
            slowDown = true;
            StartCoroutine(puddleCooldown());
            print("Blaine is slowed!");
        }

        // If player collides with a mosquito, he gets bitten and loses health or score
        if (collision.gameObject.CompareTag("Mosquito") || collision.gameObject.CompareTag("MosquitoBoss"))
        {
            audioSource.PlayOneShot(healthHeartSound);
            GetComponent<HealthManager>().AdjustHitPoints(-20);
            GetComponent<ScoreManager>().AdjustScorePoints(-5);
            print("Blaine got bitten by a mosquito");

            //implement random chance for Blaine to say something
            int randomChance = Random.Range(1, 101);

            if (randomChance < 20)
            {
                textMsg.enabled = true;
                textMsg.text = "Ouch, eh?";
                StartCoroutine(textCooldown());
            }

            // Mosquito retreats for a few seconds
            collision.gameObject.GetComponent<FlyingEnemy>().chase = false;
            // Add 3 second timer then mosquito chases player again
            isChasing = true;
            StartCoroutine(wait4it(collision));
        }

        // If player reaches finish line (camping tent)
        if (collision.gameObject.CompareTag("Finish"))
        {
            print("Blaine reached the finish line");
      
            // Load next scene
            // Get current scene index
            // int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            // UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
            // Or Win screen
            UnityEngine.SceneManagement.SceneManager.LoadScene("Win");
        }
    }

    // If player exits chase trigger zone, enemy stops chasing player
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ChaseTrigger"))
        {
            print("Blaine left enemy territory");

            // Stop wait4it coroutine by name
            StopCoroutine("wait4it");

            // Stop chase if FlyingEnemy component exists
            FlyingEnemy flyingEnemy = collision.gameObject.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null)
            {
                flyingEnemy.chase = false;
                
            }
               
            // Set the flag to stop the coroutine            
            isChasing = false;
        }
    }

    IEnumerator wait4it(Collider2D collision)
    {
        float waitTime = 2f;

        while (waitTime > 0 && isChasing)
        {
            yield return new WaitForSeconds(0.1f); // Adjust this interval as needed
            waitTime -= 0.1f;
        }

        // Untint mosquito
        //collision.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        if (isChasing)
        {
            collision.gameObject.GetComponent<FlyingEnemy>().chase = true;
        }
    }

    public void AdjustHitPoints(int amount)
    {
        GetComponent<HealthManager>().AdjustHitPoints(amount);
    }

    public void AdjustScorePoints(int amount)
    {
        GetComponent<ScoreManager>().AdjustScorePoints(amount);
    }

    IEnumerator NanaimoCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        speedBoost = false;
        maxSpeed = 3.4f;
        powerUpReady = true;
        textMsg.enabled = false;
        print("Speed boost gone");
    }
    IEnumerator DoubleDoubleCooldown()
    {
        yield return new WaitForSeconds(10.0f);
        speedBoost = false;
        maxSpeed = 3.4f;
        powerUpReady = true;
        print("Double Double effect gone");
    }
    IEnumerator PowerUpCooldown()
    {
        yield return new WaitForSeconds(Random.Range(4, 6));
        powerUpReady = true;
        print("Power up ready");
    }

    IEnumerator puddleCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        slowDown = false;
        maxSpeed = 3.4f;
        print("Speed debuff gone");
    }

    IEnumerator textCooldown()
    {
        yield return new WaitForSeconds(3.0f);
        textMsg.enabled = false;
    }
}