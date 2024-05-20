using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class BnHActionBehavior : PlayableBehaviour
{
    [ReadOnly]
    [TextArea]
    public string description = "Simple Hit Behavior";

    [Tooltip("This is a property")]
    public BnHActionSO hitConfig;

    public BaseBnHAction.BnHActionInstance actionData;

    private BeatActionManager _beatActionManager;

    private BaseBnHAction _blockAndHit;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_beatActionManager == null)
        {
            _beatActionManager = playerData as BeatActionManager;

            if (_beatActionManager == null)
            {
                return;
            }
        }

        if (_blockAndHit == null)
        {
            _blockAndHit = _beatActionManager.SpawnSimpleHit(hitConfig, actionData);
        }

        Debug.DrawLine(actionData._position, actionData._position + Vector2.up * 10, Color.red);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        Cleanup();
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (_blockAndHit == null)
        {
            return;
        }

        _beatActionManager.CleanupBnHHit(_blockAndHit);
        _blockAndHit = null;
    }
}