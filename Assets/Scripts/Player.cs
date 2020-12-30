using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Constants
    // Used in _lastGroundedTime
    private static float GROUNDED = -1;

    // Game scalars
    [SerializeField]
    private float _horizAcceleration = 5f;
    [SerializeField]
    private float _horizAccelerationAirborne = 2f;
    [SerializeField]
    private float _maxHorizSpeed = 90f;
    [SerializeField]
    private float _jumpForce = 2000f;
    [SerializeField]
    private float _jumpingGravityReduction = 4f;
    [SerializeField]
    private int _doubleJumps = 1;
    [SerializeField]
    private float _jumpCooldown = 0.1f;
    [SerializeField]
    private float _coyoteTime = 0.1f;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private PhysicsMaterial2D _extraBouncyMat;
    [SerializeField]
    private Sprite _deathSprite;
    [SerializeField]
    private AudioClip _jumpAudio, _stompAudio, _deathAudio;

    // State vars
    private int _numDoubleJumpsRemaining;
    private float _nextJumpTime;
    private float _lastGroundedTime = GROUNDED;
    private bool _invertedControls = false;
    private float _currentJumpForce;

    // Component references
    private Rigidbody2D _rb;
    private Collider2D[] _colliders;
    private Animator _anim;
    private GameManager _gameManager;
    private PhysicsMaterial2D _originalMat;
    private SpriteRenderer _renderer;
    private float _originalGravityScale;
    private bool _dead = false;

    public void SetGroundMask(LayerMask layerMask) {
        _groundMask = layerMask;
    }

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _originalMat = _rb.sharedMaterial;
        _colliders = GetComponents<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        _numDoubleJumpsRemaining = _doubleJumps;
        _nextJumpTime = Time.time + _jumpCooldown;
        _originalGravityScale = _rb.gravityScale;
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        _currentJumpForce = _jumpForce;
    }

    private void GetInputAndCalculateMoment() {
        bool isGrounded = _lastGroundedTime == GROUNDED || Time.time < _lastGroundedTime + _coyoteTime;

        float newXVelocity = _rb.velocity.x;
        float horizInput = GetHorizontalInput();
        float horiz = isGrounded ? horizInput * _horizAcceleration : horizInput * _horizAccelerationAirborne;
        newXVelocity = Mathf.Clamp(newXVelocity + horiz, -_maxHorizSpeed, _maxHorizSpeed);

        _rb.velocity = new Vector2(newXVelocity, _rb.velocity.y);


        bool canJump = isGrounded || _numDoubleJumpsRemaining > 0;

        if (JumpInputThisFrame() && Time.time > _nextJumpTime && canJump) {
            Jump(true);
            if (!isGrounded) {
                _numDoubleJumpsRemaining--;
            }
        }
        // Reduce gravity while jump is held, so that the player can more
        // granularly choose how high to jump.
        if (JumpInputBeingHeldDown() && _rb.velocity.y > 0f) {

            _rb.gravityScale = _originalGravityScale / _jumpingGravityReduction;
        } else {
            _rb.gravityScale = _originalGravityScale;
        }
    }

    private void Jump(bool playJumpAudio) {
        if (playJumpAudio) {
            AudioSource.PlayClipAtPoint(_jumpAudio, transform.position);
        }
        if (_dead) return;
        _rb.velocity = new Vector2(_rb.velocity.x, _currentJumpForce);
        _nextJumpTime = Time.time + _jumpCooldown;
    }

    private void Update() {
        GetInputAndCalculateMoment();
        _anim.SetFloat("Horizontal Speed", _rb.velocity.x);
    }

    private void FixedUpdate() {
        UpdateGrounded();
    }

    public void FreezePlayer() {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.gameObject.tag) {
            case "KillsPlayer":
                Die();
                return;
            case "Enemy":
                HitEnemy(other.gameObject.GetComponent<Enemy>());
                return;
            case "Goal":
                other.GetComponent<SceneTransitioner>().LoadScene();
                FreezePlayer();
                return;
            default: return;
        }
    }

    private void HitEnemy(Enemy enemy) {
        // If the enemy isn't set to dynamic movement, we've frozen it with a Relay ability, so don't trigger this.
        if (enemy.GetComponent<Rigidbody2D>().bodyType != RigidbodyType2D.Dynamic) {
            return;
        }
        // If we're moving upwards we're just passing the enemy so don't stomp it.
        if (_rb.velocity.y > 0) {
            return;
        }

        // Check if we stomped the enemy. 
        float verticalHeightAboveEnemy = transform.position.y - enemy.transform.position.y;
        if (verticalHeightAboveEnemy > _colliders[0].bounds.extents.y) {
            Jump(false);
            AudioSource.PlayClipAtPoint(_stompAudio, transform.position);
            _anim.SetTrigger("Stomped");
            enemy.Stomp();
            // if we stomped an enemy we shouldn't we lose.
            if (!enemy.PlayerCanStompThis()) {
                Die();
            }
        }
        // If we didn't stomp the enemy, check if we're allowed to touch this enemy. If not, we die.
        else if (!enemy.GetPlayerCanTouch()) {
            Die();
        }
    }

    private void Die() {
        _dead = true;
        // turn off animations and switch the sprite to the dead player
        _anim.enabled = false;
        _renderer.sprite = _deathSprite;
        // Give a jump so we pop up in the air, and disable collisions so we fall off screen
        AudioSource.PlayClipAtPoint(_deathAudio, transform.position);
        Jump(false);
        foreach (Collider2D col in _colliders) {
            col.enabled = false;
        }
        _gameManager.PlayerDied();
    }

    private void UpdateGrounded() {
        float distance = _colliders[0].bounds.extents.y + 0.1f;

        RaycastHit2D hit = Physics2D.BoxCast(_colliders[0].bounds.center, _colliders[0].bounds.size, 0f, Vector2.down, distance, _groundMask);
        if (hit.collider != null) {
            _lastGroundedTime = GROUNDED;
            _anim.SetBool("Grounded", true);
            _numDoubleJumpsRemaining = _doubleJumps;
        } else {
            if (_lastGroundedTime == GROUNDED) { // if we just jumped, set the current time to the last grounded time
                _lastGroundedTime = Time.time;
                _anim.SetBool("Grounded", false);
                // If we are set to bouncy mode, set the bounce sound to play now to give the effect desired
                if (_rb.sharedMaterial == _extraBouncyMat) {
                    AudioSource.PlayClipAtPoint(_jumpAudio, transform.position);
                }
            }
        }
    }

    private float GetHorizontalInput() {
        if (_invertedControls) {
            return Input.GetAxisRaw("Vertical");
        }
        return Input.GetAxisRaw("Horizontal");
    }

    private bool JumpInputThisFrame() {
        if (_invertedControls) {
            return Input.GetKeyDown(KeyCode.D);
        }
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool JumpInputBeingHeldDown() {
        if (_invertedControls) {
            return Input.GetKey(KeyCode.D);
        }
        return Input.GetKey(KeyCode.Space);
    }

    public void InvertControls() {
        _invertedControls = true;
        _renderer.color = Color.green;
    }
    public void ResetControls() {
        _invertedControls = false;
        ResetColor();
    }

    public void SetBouncy() {
        _rb.sharedMaterial = _extraBouncyMat;
        _currentJumpForce *= 1.25f;
        _renderer.color = Color.cyan;
    }

    public void ResetPhysics() {
        _rb.sharedMaterial = _originalMat;
        _currentJumpForce = _jumpForce;
        ResetColor();
    }

    public void SetOpacityForSlowedEffect(float opacity) {
        _renderer.color = new Vector4(_renderer.color.r, _renderer.color.g, _renderer.color.b, opacity);
    }

    public void ResetSlowedTime() {
        ResetColor();
    }

    public void ResetColor() {
        _renderer.color = Color.white;
    }
}
