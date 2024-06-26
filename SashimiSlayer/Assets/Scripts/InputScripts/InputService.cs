using System;
using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : BaseUserInputProvider
{

    
    [SerializeField]
    private IntEvent _onControlSchemeChanged;
    
    [SerializeField]
    private BaseUserInputProvider _gamepadInputProvider;

    [SerializeField]
    private SwordInputProvider _swordInputProvider;

    [SerializeField]
    private bool _useHardwareController;

    [SerializeField]
    private VoidEvent _onDrawDebugGUI;

    public static InputService Instance { get; private set; }

    private BaseUserInputProvider InputProvider => _useHardwareController ? _swordInputProvider : _gamepadInputProvider;

    public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<SharedTypes.SheathState> OnSheathStateChanged;
    
    public int ControlScheme => _controlScheme;
    
    private int _controlScheme = 0;
    
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

        EventPassthroughSub();

        if (_useHardwareController)
        {
            _swordInputProvider.ConnectToPort();
        }

        _onDrawDebugGUI.AddListener(HandleDrawDebugGUI);
        
        InputSystem.onDeviceChange += (device, change) =>
        {
            UpdateControlScheme();
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EventPassthroughUnsub();
            _useHardwareController = !_useHardwareController;
            if (_useHardwareController)
            {
                _swordInputProvider.ConnectToPort();
            }
            
            UpdateControlScheme();
            
            EventPassthroughSub();
        }
        
    }
    
    private void UpdateControlScheme()
    {
        if (_useHardwareController)
        {
            _controlScheme = 2;
        }
        else
        {
            // See if gamepad is connected
            if (InputSystem.devices.Count(device => device is Gamepad) > 0)
            {
                _controlScheme = 1;
            }
            else
            {
                _controlScheme = 0;
            }
        }
        
        _onControlSchemeChanged.Raise(_controlScheme);
    }

    private void OnDestroy()
    {
        EventPassthroughUnsub();

        _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
    }

    private void HandleDrawDebugGUI()
    {
        GUILayout.Label($"hw control: {_useHardwareController}");
        GUILayout.Label($"control scheme: {_controlScheme}");
    }

    private void EventPassthroughSub()
    {
        InputProvider.OnBlockPoseChanged += HandleBlockPoseChanged;
        InputProvider.OnSheathStateChanged += HandleSheatheStateChanged;
    }

    private void EventPassthroughUnsub()
    {
        InputProvider.OnBlockPoseChanged -= HandleBlockPoseChanged;
        InputProvider.OnSheathStateChanged -= HandleSheatheStateChanged;
    }

    private void HandleBlockPoseChanged(SharedTypes.BlockPoseStates state)
    {
        OnBlockPoseChanged?.Invoke(state);
    }

    private void HandleSheatheStateChanged(SharedTypes.SheathState state)
    {
        OnSheathStateChanged?.Invoke(state);
    }

    public override float GetSwordAngle()
    {
        return InputProvider.GetSwordAngle();
    }

    public override SharedTypes.SheathState GetSheathState()
    {
        return InputProvider.GetSheathState();
    }

    public override SharedTypes.BlockPoseStates GetBlockPose()
    {
        return InputProvider.GetBlockPose();
    }
}