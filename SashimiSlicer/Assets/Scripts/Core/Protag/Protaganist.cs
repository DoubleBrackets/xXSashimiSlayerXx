using Cinemachine;
using Events;
using Events.Core;
using UnityEngine;

public class Protaganist : MonoBehaviour
{
    public struct ProtagSwordState
    {
        public SharedTypes.BlockPoseStates BlockPose;
        public SharedTypes.SheathState SheathState;
        public SharedTypes.BlockPoseStates RecentHitBlockPose;
        public Vector3 SwordPosition;
        public float SwordAngle;
    }

    [Header("Emitting Events")]

    [SerializeField]
    private FloatEvent _healthChangeEvent;

    [SerializeField]
    private FloatEvent _maxHealthChangeEvent;

    [SerializeField]
    private VoidEvent _damageTakenEvent;

    [SerializeField]
    private VoidEvent _successfulBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _tryBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _trySliceEvent;

    [SerializeField]
    private ProtagSwordStateEvent _swordStateChangeEvent;

    [SerializeField]
    private VoidEvent _playerDeadEvent;

    [SerializeField]
    private ProtagSwordStateEvent _unsheatheEvent;

    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    [SerializeField]
    private VoidEvent _onDrawDebugGuiEvent;

    [Header("SFX")]

    [SerializeField]
    private AudioClip _hurtSFX;

    [SerializeField]
    private AudioClip _blockSFX;

    [SerializeField]
    private AudioClip _sliceSFX;

    public static Protaganist Instance { get; private set; }
    public SharedTypes.SheathState ProtagSheathState => _protagSheathState;
    public Vector3 SpritePosition { get; set; }

    public Vector3 SwordPosition => _swordPosition;

    private BaseUserInputProvider _inputProvider;

    private float _health;
    private float _maxHealth;

    private SharedTypes.SheathState _protagSheathState = SharedTypes.SheathState.Sheathed;
    private float _swordAngle;

    private Vector3 _swordPosition;

    private SharedTypes.BlockPoseStates _currentBlockPose;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _inputProvider = InputService.Instance;

        _health = _maxHealth;
        _healthChangeEvent.Raise(_health);

        _inputProvider.OnSheathStateChanged += OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged += OnPoseStateChanged;
        _beatmapLoadedEvent.AddListener(HandleBeatmapLoaded);
        _onDrawDebugGuiEvent.AddListener(HandleDrawDebugGUI);
    }

    private void Update()
    {
        _swordAngle = _inputProvider.GetSwordAngle();
        _swordStateChangeEvent.Raise(new ProtagSwordState
        {
            SwordPosition = _swordPosition,
            SwordAngle = _swordAngle,
            SheathState = _protagSheathState,
            BlockPose = _inputProvider.GetBlockPose()
        });
    }

    private void OnDestroy()
    {
        _inputProvider.OnSheathStateChanged -= OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged -= OnPoseStateChanged;
        _beatmapLoadedEvent.RemoveListener(HandleBeatmapLoaded);
        _onDrawDebugGuiEvent.RemoveListener(HandleDrawDebugGUI);
    }

    private void HandleDrawDebugGUI()
    {
        GUILayout.Label($"Sword angle: {_swordAngle}");
        GUILayout.Label($"Sword position: {_swordPosition}");
        GUILayout.Label($"Sheath state: {_protagSheathState}");
        GUILayout.Label($"Pose state: {_inputProvider.GetBlockPose()}");
    }

    private void HandleBeatmapLoaded(BeatmapConfigSo beatmap)
    {
        _maxHealth = beatmap.PlayerMaxHealth;
        _health = _maxHealth;
        _maxHealthChangeEvent.Raise(_maxHealth);
        _healthChangeEvent.Raise(_health);
    }

    private void OnPoseStateChanged(SharedTypes.BlockPoseStates blockPoseStates)
    {
        // Only if a new button is pressed, not released
        if ((_currentBlockPose & blockPoseStates) != _currentBlockPose)
        {
            _currentBlockPose = blockPoseStates;
            return;
        }

        _currentBlockPose = blockPoseStates;

        var pose = new ProtagSwordState
        {
            BlockPose = blockPoseStates,
            SwordPosition = _swordPosition,
            SwordAngle = _swordAngle
        };

        _tryBlockEvent.Raise(pose);
    }

    private void OnSheathStateChanged(SharedTypes.SheathState newState)
    {
        SharedTypes.SheathState oldState = _protagSheathState;
        _protagSheathState = newState;

        // Sword is sheathed from unsheathed means a slice
        if (newState == SharedTypes.SheathState.Sheathed
            && oldState == SharedTypes.SheathState.Unsheathed)
        {
            _trySliceEvent.Raise(new ProtagSwordState
            {
                SheathState = newState,
                SwordPosition = _swordPosition,
                SwordAngle = _swordAngle
            });
        }

        else if (newState == SharedTypes.SheathState.Unsheathed && oldState == SharedTypes.SheathState.Sheathed)
        {
            _unsheatheEvent.Raise(new ProtagSwordState
            {
                SheathState = newState,
                SwordPosition = _swordPosition,
                SwordAngle = _swordAngle
            });
        }
    }

    public void SetSwordPosition(Vector3 position)
    {
        _swordPosition = position;
    }

    public float DistanceToSwordPlane(Vector3 position)
    {
        Vector3 swordPlaneNormal = Quaternion.Euler(0, 0, _swordAngle) * Vector3.up;
        Vector3 swordPlanePoint = _swordPosition;

        Vector3 pointOnPlane = position - swordPlanePoint;
        float distance = Mathf.Abs(Vector3.Dot(pointOnPlane, swordPlaneNormal));
        return distance;
    }

    public void TakeDamage(float damage)
    {
        AudioSource.PlayClipAtPoint(_hurtSFX, Vector3.zero, 1f);
        ScreenShakeService.Instance.ShakeScreen(0.1f, 1f, CinemachineImpulseDefinition.ImpulseShapes.Rumble);

        if (_health <= 0)
        {
            return;
        }

        _health -= damage;
        _healthChangeEvent.Raise(_health);
        _damageTakenEvent.Raise();

        if (_health <= 0)
        {
            _playerDeadEvent.Raise();
        }
    }

    public void SuccessfulBlock()
    {
        _successfulBlockEvent.Raise();
        AudioSource.PlayClipAtPoint(_blockSFX, Vector3.zero, 1f);
        ScreenShakeService.Instance.ShakeScreen(0.05f, 0.15f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SuccessfulSlice()
    {
        AudioSource.PlayClipAtPoint(_sliceSFX, Vector3.zero, 1f);
        ScreenShakeService.Instance.ShakeScreen(0.1f, 0.5f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SetSlashPosition(Vector3 position)
    {
        _swordPosition = position;
        _swordStateChangeEvent.Raise(new ProtagSwordState
        {
            SwordPosition = _swordPosition,
            SwordAngle = _swordAngle,
            SheathState = _protagSheathState,
            BlockPose = _inputProvider.GetBlockPose()
        });
    }
}