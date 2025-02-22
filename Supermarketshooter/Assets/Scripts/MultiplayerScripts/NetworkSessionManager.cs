using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using System.Threading.Tasks; // tasks

public class SessionManager : MonoBehaviour //Singleton<SessionManager>
{
    ISession activeSession;

    ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log("Session: " + activeSession);
        }
    }

    const string playerNamePropertyKey = "playerName";

    // Async methods do not need to wait for the client and can run in background
    // Common for networking, without it the client will not respond until the server responds
    async void Start()
    {
        // 
        try
        {
            await UnityServices.InitializeAsync(); // initialize unity service at lpb3551
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // no anticheat/lobby protection
            Debug.Log("Sign in anon successful! PlayerID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        StartSessionAsHost();
    }

    // Properties of each player currently only the player name
    async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        // Game specific properties unique to each player
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty>
        {
            {
                playerNamePropertyKey, playerNameProperty
            }
        };
    }

    async void StartSessionAsHost()
    {
        var playerProperties = await GetPlayerProperties();

        var options = new SessionOptions
        {
            MaxPlayers = 4,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties
        }.WithRelayNetwork(); /*Switch to WithDistributedAuthorityNetwork() once I figure out how it works*/

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"session: {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
    }

    async Task LeaveSession()
    {
        if(activeSession != null)
        {
            try
            {
                await ActiveSession.LeaveAsync();
            }
            catch
            {
                // shouldnt be needed
            }
            finally
            {
                ActiveSession = null;
            }
        }
    }
}
