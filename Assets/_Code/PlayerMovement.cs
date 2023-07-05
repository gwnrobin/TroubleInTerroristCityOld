using Kinemation.FPSFramework.Runtime.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class PlayerMovement : PlayerComponent
{
    [Header("Movement")]
    [SerializeField] private bool shouldMove;
    [SerializeField] private float speed = 10f;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [SerializeField] private float crouchHeight;

    private float _lowerCapsuleOffset;
    private CharAnimData _charAnimData;
    private static readonly int Sprint = Animator.StringToHash("sprint");
    private static readonly int Crouch1 = Animator.StringToHash("crouch");

    private bool _crouching = true;

    [SerializeField]
    private bool toggleCrouch = true;

    private void Awake()
    {
        Player.Crouch.AddStartListener(Crouch);
        if (!toggleCrouch)
        {
            Player.Crouch.AddStopListener(Crouch);
        }

        Player.Sprint.AddStartListener(SprintPressed);
        Player.Sprint.AddStopListener(SprintReleased);
    }
    private void SprintPressed()
    {
        if (_charAnimData.poseState == FPSPoseState.Crouching)
        {
            return;
        }

        _charAnimData.movementState = FPSMovementState.Sprinting;
        _charAnimData.actionState = FPSActionState.None;

        speed *= 3f;
        animator.SetBool(Sprint, true);
    }

    private void SprintReleased()
    {
        if (_charAnimData.poseState == FPSPoseState.Crouching)
        {
            return;
        }

        _charAnimData.movementState = FPSMovementState.Walking;

        speed /= 3f;
        animator.SetBool(Sprint, false);
    }

    private void Crouch()
    {
        _crouching = !_crouching;
        if (!_crouching)
        {
            var height = controller.height;
            height *= crouchHeight;
            controller.height = height;
            controller.center = new Vector3(0f, _lowerCapsuleOffset + height / 2f, 0f);
            speed *= 0.7f;

            _charAnimData.poseState = FPSPoseState.Crouching;
            animator.SetBool(Crouch1, true);
        }
        else
        {
            Uncrouch();
        }
    }

    private void Uncrouch()
    {
        var height = controller.height;
        height /= crouchHeight;
        controller.height = height;
        controller.center = new Vector3(0f, _lowerCapsuleOffset + height / 2f, 0f);
        speed /= 0.7f;

        _charAnimData.poseState = FPSPoseState.Standing;
        animator.SetBool(Crouch1, false);
    }
    
    private void UpdateMovement()
    {
        float moveX = Player.MoveInput.Get().x;
        float moveY = Player.MoveInput.Get().y;

        _charAnimData.moveInput = new Vector2(moveX, moveY);

        _smoothMoveInput.x = CoreToolkitLib.Glerp(_smoothMoveInput.x, moveX, 7f);
        _smoothMoveInput.y = CoreToolkitLib.Glerp(_smoothMoveInput.y, moveY, 7f);

        bool moving = Mathf.Abs(moveX) >= 0.4f || Mathf.Abs(moveY) >= 0.4f;

        animator.SetBool(Moving, moving);
        animator.SetFloat(MoveX, _smoothMoveInput.x);
        animator.SetFloat(MoveY, _smoothMoveInput.y);

        Vector3 move = transform.right * moveX + transform.forward * moveY;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void Update()
    {
        //ProcessLookInput();
        //UpdateFiring();
        UpdateMovement();
        //UpdateAnimValues();
    }

    private void UpdateCameraRotation()
    {
        var rootBone = Player.coreAnimComponent.GetRootBone();
        cameraBone.rotation =
            rootBone.rotation * Quaternion.Euler(_playerInput.y, shouldMove ? 0f : _playerInput.x, 0f);
    }

    private void LateUpdate()
    {
        UpdateCameraRotation();
    }
}
*/