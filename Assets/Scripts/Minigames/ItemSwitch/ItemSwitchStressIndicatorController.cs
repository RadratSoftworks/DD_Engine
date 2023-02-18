using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSwitchStressIndicatorController : MonoBehaviour
{
    private const int FailSurfaceLayer = 9;
    private const int BitStressLayer = 10;
    private const int HardStressLayer = 11;
    private const int StableStressLayer = 12;

    [SerializeField]
    private float maxSpeedX = 1.2f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    private float forceFactor;
    private float maxSpeedFactor;
    private Vector2 nextForceVector;
    private bool initialForceInitiated;
    
    // While we may have enter another region, the indicator may not have exited the other region yet
    // This bool flag will check if we still in stable
    private bool stillInStable = false;

    public event System.Action<ItemSwitchStressStatus> StressStatusEntered;

    public bool Passed => stillInStable;

    public event System.Action ConfirmPressed;

    private void RegOrUnregControl(bool register)
    {
        var itemSwitchActionMap = GameInputManager.Instance.ItemSwitchMinigameActionMap;

        InputAction leftPressed = itemSwitchActionMap.FindAction("Left Pressed");
        InputAction rightPressed = itemSwitchActionMap.FindAction("Right Pressed");
        InputAction confirmPressed = itemSwitchActionMap.FindAction("Confirm Pressed");

        if (register)
        {
            itemSwitchActionMap.Enable();

            leftPressed.performed += OnLeftPressed;
            rightPressed.performed += OnRightPressed;
            confirmPressed.performed += OnConfirmPressed;
        } else
        {
            itemSwitchActionMap.Disable();

            leftPressed.performed -= OnLeftPressed;
            rightPressed.performed -= OnRightPressed;
            confirmPressed.performed -= OnConfirmPressed;
        }
    }

    private void Start()
    {
        RegOrUnregControl(true);
    }

    private void OnDestroy()
    {
        RegOrUnregControl(false);
    }

    public void Setup(ItemSwitchStressMachineInfo stressInfo, float forceFactor, float maxSpeedFactor)
    {
        this.forceFactor = forceFactor;
        this.maxSpeedFactor = maxSpeedFactor;

        initialForceInitiated = true;
        stillInStable = true;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                stressInfo.IndicatorImagePath, new Vector2(0.5f, 0.5f));

            // Set the Y position based on sprite height
            CircleCollider2D collider2D = GetComponent<CircleCollider2D>();
            if (collider2D != null)
            {
                float previousRadius = collider2D.radius;

                // Setup the collider size and position
                // The position of indicator in the prefab already is centered, so we just need to move by delta
                collider2D.radius = spriteRenderer.localBounds.size.y / 2;

                Vector3 previousLoc = transform.localPosition;
                previousLoc.y += (collider2D.radius - previousRadius);

                transform.localPosition = previousLoc;
            }
        }

        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.simulated = false;
        
        // Random direction based on current time!
        nextForceVector = (Random.Range(0, 2) % 2 == 0) ? Vector2.left : Vector2.right;
        StressStatusEntered?.Invoke(ItemSwitchStressStatus.Stable);
    }

    private void OnLeftPressed(InputAction.CallbackContext context)
    {
        nextForceVector = Vector2.left;
    }

    private void OnRightPressed(InputAction.CallbackContext context)
    {
        nextForceVector = Vector2.right;
    }

    private void OnConfirmPressed(InputAction.CallbackContext context)
    {
        ConfirmPressed?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!rigidBody.simulated)
        {
            return;
        }

        if (!initialForceInitiated)
        {
            rigidBody.AddForce((Time.fixedTime % 2 == 0) ? Vector2.left : Vector2.right);
            initialForceInitiated = true;
        }

        if (nextForceVector != Vector2.zero)
        {
            // Add a bit smaller force
            rigidBody.AddForce(nextForceVector * forceFactor, ForceMode2D.Impulse);
            nextForceVector = Vector2.zero;
        }

        // Limit the velocity so that it won't go too fast, also for the user-controlled force to combat the gravity + initial force
        rigidBody.velocity = new Vector2(Mathf.Clamp(rigidBody.velocity.x, -maxSpeedFactor * maxSpeedX, maxSpeedFactor * maxSpeedX),
            rigidBody.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == FailSurfaceLayer)
        {
            // Stop the physics and tell that the game is over!
            rigidBody.simulated = false;
            stillInStable = false;

            StressStatusEntered?.Invoke(ItemSwitchStressStatus.Shutdown);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        int layer = collider.gameObject.layer;

        switch (layer)
        {
            case BitStressLayer:
                StressStatusEntered?.Invoke(ItemSwitchStressStatus.Average);
                break;

            case HardStressLayer:
                StressStatusEntered?.Invoke(ItemSwitchStressStatus.Hard);
                break;

            case StableStressLayer:
                StressStatusEntered?.Invoke(ItemSwitchStressStatus.Stable);
                stillInStable = true;
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == StableStressLayer)
        {
            stillInStable = false;
        }
    }

    public void KickOff()
    {
        rigidBody.simulated = true;
    }

    public void Freeze()
    {
        rigidBody.simulated = false;
    }
}
