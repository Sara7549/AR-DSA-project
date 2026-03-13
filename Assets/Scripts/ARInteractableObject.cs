using UnityEngine;
using System.Collections.Generic;

public abstract class ARInteractableObject : MonoBehaviour
{
    private List<ARInteractableObject> _interactables = new List<ARInteractableObject> ();
    protected enum State
    {
        Idle, Active
    }
    protected State ARObjectState = State.Idle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ARInteractableObject>(out var interactable))
        {
            AddInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ARInteractableObject>(out var interactable))
        {
            RemoveInteractable(interactable);
        }
    }
    protected void AddInteractable(ARInteractableObject interactable)
    {
        _interactables.Add(interactable);
        SetState(State.Active);
    }

    protected void RemoveInteractable(ARInteractableObject interactable) {
        _interactables.Remove(interactable);
        if (_interactables.Count == 0) SetState(State.Idle);
    }

    private void OnDisable()
    {
        foreach (var interactable in _interactables)
        {
            interactable.RemoveInteractable(this);
        }
        _interactables.Clear();
        SetState(State.Idle);
    }

    protected virtual void SetState(State state)
    {
        ARObjectState = state;
    }
}