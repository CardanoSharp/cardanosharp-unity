using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Utils
{
    public interface IGameEventListener
    {
        void OnEventRaised();
    }

    [Serializable]
    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        [SerializeField] private GameEvent @event;

        [SerializeField] private UnityEvent _response;

        public void OnEventRaised()
        {
            _response?.Invoke();
        }

        public void OnEnable()
        {
            if (@event != null) @event.RegisterListener(this);
        }

        public void OnDisable()
        {
            @event.UnregisterListener(this);
        }
    }
}
