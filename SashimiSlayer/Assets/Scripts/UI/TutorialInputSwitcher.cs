using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;

public class TutorialInputSwitcher : MonoBehaviour
{
    [System.Serializable]
    private class SpriteRendererArray
    {
        public SpriteRenderer[] sprites;
    }
    
    [SerializeField]
    private List<SpriteRendererArray> _sprites;
    
    [SerializeField]
    private IntEvent _controlSchemeChangedEvent;
    
    private void Awake()
    {
        _controlSchemeChangedEvent.AddListener(HandleControlSchemeChanged);
        HandleControlSchemeChanged(InputService.Instance.ControlScheme);
    }
    
    private void OnDestroy()
    {
        _controlSchemeChangedEvent.RemoveListener(HandleControlSchemeChanged);
    }
    
    private void HandleControlSchemeChanged(int controlScheme)
    {
        for(int i = 0; i < _sprites.Count; i++)
        {
            bool isMatchingControlPrompt = i == controlScheme;
            foreach (SpriteRenderer sprite in _sprites[i].sprites)
            {
                sprite.enabled = isMatchingControlPrompt;
            }
        }
    }
}
