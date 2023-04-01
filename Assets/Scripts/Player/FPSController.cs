using System.Collections.Generic;
using Demo.Scripts.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Core;
using Kinemation.FPSFramework.Runtime.Layers;
using UnityEngine;
public enum FPSMovementState
{
    Idle,
    Walking,
    Running,
    Sprinting
}

public enum FPSPoseState
{
    Standing,
    Crouching
}

public enum FPSActionState
{
    None,
    Ready,
    Aiming,
    PointAiming,
}
public class FPSController : PlayerComponent
{
    [Header("Character Controls")]
    [SerializeField] private Transform cameraBone;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float sensitivity;

    [Header("Movement")]
    [SerializeField] private bool shouldMove;
    private float speed;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private float gravity = 0.3f;
    [SerializeField] private float walkingSpeed = 10f;
    [SerializeField] private float sprintSpeed = 25f;

    private Vector2 _playerInput;
    private Vector2 _smoothMoveInput;

    private bool _aiming;

    private bool _crouching = true;

    private float _lowerCapsuleOffset;
    private static readonly int Sprint = Animator.StringToHash("sprint");
    private static readonly int Crouch1 = Animator.StringToHash("crouch");
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");

    [SerializeField]
    private bool toggleAim = true;
    [SerializeField]
    private bool toggleCrouch = true;



    private void Awake()
    {
        Player.Crouch.AddStartListener(Crouch);
        if (!toggleCrouch)
        {
            Player.Crouch.AddStopListener(Crouch);
        }

        Player.Lean.AddStartListener(Lean);
        Player.Lean.AddStopListener(() => Player._charAnimData.leanDirection = 0);

        Player.ChangeScope.AddListener(ChangeScope);

        Player.Crouch.SetStartTryer(TryStartCrouch);
        Player.Crouch.SetStopTryer(TryStopCrouch);
        Player.Sprint.SetStartTryer(TryStartSprint);
        Player.Sprint.AddStopListener(OnSprintStop);

        //Player.poseState.AddChangeListener(Crouch);

    }

    private void Start()
    {
        //To reposition
        //Application.targetFrameRate = 120;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _lowerCapsuleOffset = controller.center.y - controller.height / 2f;

        animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();
    }

    private bool TryStartCrouch()
    {


        return true;
    }

    private bool TryStopCrouch()
    {
        return true;
    }

    private bool TryStartSprint()
    {
        if (Player.poseState.Val == FPSPoseState.Crouching || !(Player.MoveInput.Get().y > 0f) || Player.Stamina.Get() < 15f)
            return false;

        Player.movementState.Set(FPSMovementState.Sprinting);
        Player.actionState.Set(FPSActionState.None);

        Player.adsLayer.SetLayerAlpha(0f);
        Player.lookLayer.SetLayerAlpha(0.4f);
        Player.blendLayer.SetLayerAlpha(0f);
        Player.locoLayer.SetSprint(true);
        Player.locoLayer.SetReadyWeight(0f);

        speed = sprintSpeed;
        animator.SetBool(Sprint, true);

        return true;
    }

    private void OnSprintStop()
    {
        if (Player.poseState.Val == FPSPoseState.Crouching)
        {
            return;
        }

        Player.adsLayer.SetLayerAlpha(1f);
        Player.lookLayer.SetLayerAlpha(1f);
        Player.blendLayer.SetLayerAlpha(1f);
        Player.locoLayer.SetSprint(false);

        Player.movementState.Set(FPSMovementState.Walking);

        speed = walkingSpeed;
        animator.SetBool(Sprint, false);
    }

    public void ChangeScope()
    {
        //Player.CoreAnimComponent.OnSightChanged(GetGun().GetScope());
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

            Player.lookLayer.SetPelvisWeight(0f);

            Player.poseState.Set(FPSPoseState.Crouching);
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

        Player.lookLayer.SetPelvisWeight(1f);

        Player.poseState.Set(FPSPoseState.Standing);
        animator.SetBool(Crouch1, false);
    }

    private void Lean()
    {
        if (Player.movementState.Val == FPSMovementState.Sprinting)
            return;

        if (Player.actionState.Val != FPSActionState.Ready)
        {
            Player._charAnimData.leanDirection = (int)Player.Lean.Parameter;
        }
    }

    private void ProcessLookInput()
    {
        float deltaMouseX = Player.LookInput.Get().x * sensitivity;
        float deltaMouseY = -Player.LookInput.Get().y * sensitivity;

        _playerInput.x += deltaMouseX;
        _playerInput.y += deltaMouseY;

        _playerInput.x = Mathf.Clamp(_playerInput.x, -90f, 90f);
        _playerInput.y = Mathf.Clamp(_playerInput.y, -90f, 90f);

        Player._charAnimData.deltaAimInput = new Vector2(deltaMouseX, deltaMouseY);

        if (shouldMove)
        {
            transform.Rotate(Vector3.up * deltaMouseX);
        }
    }

    private void UpdateMovement()
    {
        float moveX = Player.MoveInput.Get().x;
        float moveY = Player.MoveInput.Get().y;

        Player._charAnimData.moveInput = new Vector2(moveX, moveY);

        _smoothMoveInput.x = CoreToolkitLib.Glerp(_smoothMoveInput.x, moveX, 7f);
        _smoothMoveInput.y = CoreToolkitLib.Glerp(_smoothMoveInput.y, moveY, 7f);

        if(Player.Sprint.Active)
        {
            moveX = Mathf.Clamp(moveX, -.5f, .5f);
            moveY = Mathf.Clamp(moveY, 0, 1f);
        }

        bool moving = Mathf.Abs(moveX) >= 0.4f || Mathf.Abs(moveY) >= 0.4f;

        if (moving)
        {
            Player.Walk.ForceStart();
        }
        else
        {
            Player.Walk.ForceStop();
            Player.Sprint.ForceStop();
        }

        animator.SetBool(Moving, moving);
        animator.SetFloat(MoveX, _smoothMoveInput.x);
        animator.SetFloat(MoveY, _smoothMoveInput.y);

        if (IsOwner)
        {
            Vector3 move = transform.right * moveX + transform.forward * moveY;
            move.y -= gravity * Time.deltaTime;
            controller.Move(speed * Time.deltaTime * move);
        }
    }

    private void Update()
    {
        ProcessLookInput();
        UpdateMovement();
    }

    private void UpdateCameraRotation()
    {
        Transform rootBone = Player.CoreAnimComponent.GetRootBone();
        cameraBone.rotation =
            rootBone.rotation * Quaternion.Euler(_playerInput.y, shouldMove ? 0f : _playerInput.x, 0f);
    }

    private void LateUpdate()
    {
        UpdateCameraRotation();
    }
}