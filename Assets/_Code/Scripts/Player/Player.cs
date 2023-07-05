using Demo.Scripts.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Core;
using Kinemation.FPSFramework.Runtime.Layers;
using Unity.Netcode;
using UnityEngine;
public class Player : Humanoid
{
    public Camera Camera { get => m_PlayerCamera; }
    public CoreAnimComponent CoreAnimComponent { get => coreAnimComponent; }
    public EquipmentController EquipmentController { get => equipmentController; }

    //Only in Single Player
    public readonly Activity Pause = new Activity();

    // Movement
    public readonly Value<float> MoveCycle = new Value<float>();
    //public readonly Message MoveCycleEnded = new Message();

    //public readonly Value<RaycastInfo> RaycastInfo = new Value<RaycastInfo>(null);

    /// <summary>Is there any object close to the camera? Eg. A wall</summary>
    //public readonly Value<Collider> ObjectInProximity = new Value<Collider>();

    //public readonly Value<bool> ViewLocked = new Value<bool>();

    public readonly Value<float> Stamina = new Value<float>(100f);

    public readonly Value<Vector2> MoveInput = new Value<Vector2>(Vector2.zero);
    public readonly Value<Vector2> LookInput = new Value<Vector2>(Vector2.zero);
    public readonly Value<int> ScrollValue = new Value<int>(0);

    public readonly Attempt DestroyEquippedItem = new Attempt();
    //public readonly Attempt ChangeUseMode = new Attempt();

    //public readonly Activity Swimming = new Activity();
    //public readonly Activity OnLadder = new Activity();
    //public readonly Activity Sliding = new Activity();

    [Header("Camera")]
    [SerializeField] private Camera m_PlayerCamera = null;

    [Header("FPS Framework")]
    private CoreAnimComponent coreAnimComponent;
    [SerializeField] private EquipmentController equipmentController;
    public RecoilAnimation _recoilAnimation;

    public CharAnimData _charAnimData = new CharAnimData();
    public CharAnimStates _charAnimStates = new CharAnimStates();

    public Value<FPSActionState> actionState = new(0);
    public Value<FPSMovementState> movementState = new(0);
    public Value<FPSPoseState> poseState = new(0);

    protected NetworkVariable<CharAnimData> charAnimData = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<CharAnimStates> charAnimStates = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] public LookLayer lookLayer;
    [SerializeField] public AdsLayer adsLayer;
    [SerializeField] public BlendingLayer blendLayer;
    [SerializeField] public SwayLayer swayLayer;
    [SerializeField] public DemoLoco locoLayer;

    protected virtual void Start()
    {
        lookLayer = GetComponent<LookLayer>();
        adsLayer = GetComponent<AdsLayer>();
        blendLayer = GetComponent<BlendingLayer>();
        locoLayer = GetComponent<DemoLoco>();
        swayLayer = GetComponent<SwayLayer>();
        _recoilAnimation = GetComponent<RecoilAnimation>();

        coreAnimComponent = GetComponent<CoreAnimComponent>();
    }

    protected virtual void Update()
    {
        _charAnimData.recoilAnim = new LocRot(_recoilAnimation.OutLoc, Quaternion.Euler(_recoilAnimation.OutRot));

        coreAnimComponent.SetCharData(_charAnimData);
    }
}

public struct CharAnimStates : INetworkSerializable
{
    public int action;
    public int movement;
    public int pose;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref action);
        serializer.SerializeValue(ref movement);
        serializer.SerializeValue(ref pose);
    }
}