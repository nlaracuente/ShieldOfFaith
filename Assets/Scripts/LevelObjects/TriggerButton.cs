using System.Collections.Generic;
using UnityEngine;

public class TriggerButton : MonoBehaviour, IShieldEnterTriggerHandler, IShieldCollisionHandler
{
    [SerializeField, Tooltip("True: button can be turned on/off")]
    bool isToggleButton = true;

    [SerializeField, Tooltip("Current/Default button state. True: is On")]
    bool isOn = false;

    [SerializeField]
    GameObject disabledModel;

    [SerializeField]
    GameObject activeModel;

    [SerializeField]
    ParticleSystem activateParticles;

    [SerializeField, Tooltip("A list of objects that are affected by the state of the button")]
    List<GameObject> handlerObjects;

    List<IOnButtonPressedHandler> handlers;

    bool hasToggled = false;

    private void Start()
    {
        // TODO: research how to expose interfaces in the editor
        //       or a better way to achieve this effect
        handlers = new List<IOnButtonPressedHandler>();
        foreach (var go in handlerObjects)
        {
            if (go == null)
                return;

            var handler = go.GetComponent<IOnButtonPressedHandler>();
            if (handler != null && !handlers.Contains(handler))
                handlers.Add(handler);
        }

        SetModelStates();
        //SetHandlerStates();
    }

    private void SetModelStates()
    {
        activeModel?.SetActive(isOn);
        disabledModel?.SetActive(!isOn);
    }

    void SetHandlerStates()
    {
        foreach (var handler in handlers)
            handler.Toggle();
    }

    public void ToggleButton()
    {
        if (!isToggleButton && hasToggled)
            return;

        hasToggled = true;
        isOn = !isOn;

        if (activateParticles != null)
            activateParticles.Play();

        AudioManager.instance.Play(SFXLibrary.instance.buttonOn);
        AudioManager.instance.Play(SFXLibrary.instance.spikes);

        SetModelStates();
        SetHandlerStates();
    }

    public virtual void OnShieldEnterTrigger(Shield shield) => ToggleButton();
    public void OnShieldCollisionEnter(Shield shield) => ToggleButton();
}
