using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggleable : MonoBehaviour, IOnButtonPressedHandler
{
    [SerializeField, Tooltip("Current/Default state")]
    bool isActive = false;
    public bool IsActive { get { return isActive; } }

    [SerializeField]
    Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    /// <summary>
    /// Why doe it on start if it's going to happen on Update anyways?
    /// To make it happen before the first frame update
    /// </summary>
    private void Start() => Animator.SetBool("IsActive", isActive);
    private void Update() => Animator.SetBool("IsActive", isActive);
    public void Toggle() => isActive = !isActive;
}
