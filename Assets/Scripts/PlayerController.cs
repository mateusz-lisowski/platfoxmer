using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    const float rayLength = 1.5f;

    [Header("Movement parameters")]
    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 0.1f; // moving speed of the player
    [Range(0.01f, 20.0f)] [SerializeField] private float jumpForce = 6.0f; // jump force of the player
    [Space(10)]
    public LayerMask groundLayer;

    [SerializeField] private AudioClip bonusSound;
    [SerializeField] private AudioClip LevelEndSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip enemyKillSound;
    [SerializeField] private AudioClip deathSound;

    private AudioSource source;

    private Rigidbody2D rigidBody;
    private Animator animator;

    private bool isFacingRight = true;
    private bool isWalking;

    private Vector2 startPosition;


    private bool IsGrounded()
    {
        return Physics2D.Raycast(this.transform.position, Vector2.down, rayLength, groundLayer.value);
    }

	private void Jump() 
    {
        if (IsGrounded())
        {
            source.PlayOneShot(jumpSound, AudioListener.volume);
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
		Vector3 theScale = transform.localScale;
        theScale.x = -theScale.x;
        transform.localScale = theScale;
	}

    private void Death()
    {
        source.PlayOneShot(deathSound, AudioListener.volume);
        GameManager.instance.RemoveLive();
        if (!GameManager.instance.IsDead())
        {
            this.transform.position = startPosition;
        }
        else
        {
			GameManager.instance.GameOver();
        }
    }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Bonus"))
		{
            source.PlayOneShot(bonusSound, AudioListener.volume);
            GameManager.instance.AddPoints(1);
            other.gameObject.SetActive(false);
		}

        if (other.CompareTag("LevelEnd") && GameManager.instance.currentGameState == GameState.GS_GAME)
            if (GameManager.instance.IsEnoughKeys())
		    {
                source.PlayOneShot(LevelEndSound, AudioListener.volume);
                GameManager.instance.score += 100 * GameManager.instance.lives;
                GameManager.instance.LevelCompleted();
		    }
            else
            {
                Debug.Log("Not enough keys");
            }

        if (other.CompareTag("Enemy"))
        {
            if (this.transform.position.y > other.transform.position.y)
            {
                source.PlayOneShot(enemyKillSound, AudioListener.volume);
                GameManager.instance.KilledEnemy();
                Debug.Log("Killed an enemy");
            }
            else
            {
                Death();
            }
        }

        if (other.CompareTag("Key"))
        {
            source.PlayOneShot(bonusSound, AudioListener.volume);
            GameManager.instance.AddKeys();
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("Heart"))
        {
            source.PlayOneShot(bonusSound, AudioListener.volume);
            GameManager.instance.AddLive();
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("FallLevel"))
        {
            Death();
        }

        if (other.CompareTag("MovingPlatform"))
        {
            transform.SetParent(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
        }
    }

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        source = GetComponent<AudioSource>();

        startPosition = this.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isWalking = false;

        if (GameManager.instance.currentGameState == GameState.GS_GAME
            || GameManager.instance.currentGameState == GameState.GS_LEVELCOMPLETED)
        {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isWalking = true;
                if (!isFacingRight)
                {
                    Flip();
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isWalking = true;
                if (isFacingRight)
                {
                    Flip();
                }
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            // Debug.DrawRay(transform.position, rayLength * Vector3.down, Color.white, 0.1f, false);
        }

        animator.SetBool("isGrounded", IsGrounded());
        animator.SetBool("isWalking", isWalking);
	}
}
