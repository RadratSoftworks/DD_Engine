using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConstructionSiteHarryController : MonoBehaviour
{
    private const string AnimationPath = "ch3/animations/Harry_fly.anim";

    [SerializeField]
    private SpriteAnimatorController animationController;

    [SerializeField]
    private Vector2 flyVelocityCap = new Vector2(2.5f, 1.5f);

    private Rigidbody2D rigidBody;
    private Vector2 moveVector;

    private PlayerInput playerInput;

    public void Setup(Vector2 position)
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        transform.localPosition = GameUtils.ToUnityCoordinates(position);
        animationController.Setup(Vector2.zero, SpriteAnimatorController.SortOrderNotSet,
            AnimationPath, origin: new Vector2(0.5f, 0.5f));
    }

    private void FixedUpdate()
    {
        // Apply wind force
        rigidBody.AddForce(Vector2.right * 0.3f, ForceMode2D.Impulse);
        rigidBody.AddForce(moveVector, ForceMode2D.Impulse);

        rigidBody.velocity = new Vector2(
            Math.Clamp(rigidBody.velocity.x, -flyVelocityCap.x, flyVelocityCap.x),
            Math.Clamp(rigidBody.velocity.y, -flyVelocityCap.y, flyVelocityCap.y));

        moveVector = Vector2.zero;
    }

    private void OnLeftPressed()
    {
        moveVector += Vector2.left;
    }

    private void OnRightPressed()
    {
        moveVector += Vector2.right;
    }

    private void OnUpPressed()
    {
        moveVector += Vector2.up;
    }

    private void OnDownPressed()
    {
        moveVector += Vector2.down;
    }

    public void Kill()
    {
        rigidBody.simulated = false;
        playerInput.enabled = !enabled;
    }
}
