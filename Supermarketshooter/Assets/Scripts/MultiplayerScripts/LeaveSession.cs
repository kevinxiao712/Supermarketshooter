using System;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Must be attatched to a button
[RequireComponent(typeof(Button))]

internal class LeaveSession
{
    //[Tooltip("Event invoked when the user has successfully left a session.")]
    //public UnityEvent SessionLeft = new();

    //public ISession Session { get; set; }

    //Button m_Button;

    //void Start()
    //{
    //    m_Button = this.GetComponent<Button>();
    //    m_Button.onClick.AddListener(Leave);
    //    SetButtonActive();
    //}

    //public void OnSessionLeft()
    //{
    //    SessionLeft.Invoke();
    //    SetButtonActive();
    //}

    //public void OnSessionJoined()
    //{
    //    SetButtonActive();
    //}


    //void SetButtonActive()
    //{
    //    m_Button.interactable = Session != null;
    //}

    //async void Leave()
    //{
    //    await SessionManager.Instance.LeaveSession();
    //}
}
