using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using System.Text;
using System.Linq;

public class AuthenticationManager : MonoBehaviour
{

    [Header("Authentication Manager")]
    [SerializeField] private Button _signInWithUnityButton;
    [SerializeField] private Button _signInAnonymouslyButton;
    [SerializeField] private Button _signOutButton;
    [SerializeField] private Button _deleteAccountButton;
    [SerializeField] private Button _clearSessionTokenButton;
    [Header("Text")]
    [SerializeField] private TMP_Text _infoText;

    private bool _isWaitingForSignIn = false;

    // Actions

    // Pre Setup
    private Action<string> _showErrorAction = (msg) =>
    {
        ErrorScreen.Show(msg, "OK");
    };

    // Normal
    private Action _cancelUnityPlayerAccountsAction;

    // Funcs
    private Func<Task> _deleteFunc;
    private Func<Task> _initializeServices;
    private Func<Task> _signInAnonymously;
    private Func<Task> _startUnitySignIn;
    private Func<Task> _signInWithUnity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        await Startup();

        RegisterEvents();
    }

    private void OnDestroy()
    {
        DeregisterEvents();
    }

    private void RegisterEvents()
    {
        AuthenticationService.Instance.Expired += OnTokenExpired;
        PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;

        _signInWithUnityButton.onClick.AddListener(OnClickSignInWithUnity);
        _signInAnonymouslyButton.onClick.AddListener(OnClickSignInAnonymously);
        _signOutButton.onClick.AddListener(OnClickSignOut);
        _deleteAccountButton.onClick.AddListener(OnClickDeleteAccount);
        _clearSessionTokenButton.onClick.AddListener(OnClickClearSessionToken);
    }

    private void DeregisterEvents()
    {
        AuthenticationService.Instance.Expired -= OnTokenExpired;
        PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

        _signInWithUnityButton.onClick.RemoveListener(OnClickSignInWithUnity);
        _signInAnonymouslyButton.onClick.RemoveListener(OnClickSignInAnonymously);
        _signOutButton.onClick.RemoveListener(OnClickSignOut);
        _deleteAccountButton.onClick.RemoveListener(OnClickDeleteAccount);
        _clearSessionTokenButton.onClick.RemoveListener(OnClickClearSessionToken);
    }

    private async Task Startup()
    {
        SetupActions();

        LoadingScreen.Show(true, "Init Services...");

        bool success = await AuthenticationTask.Run(_initializeServices, _showErrorAction);

        if (success && AuthenticationService.Instance.SessionTokenExists)
        {
            LoadingScreen.Show(true, "Signing In...");

            await AuthenticationTask.Run(_signInAnonymously, _showErrorAction);
        }

        LoadingScreen.Hide();

        UpdateUI();
    }

    private void SetupActions()
    {
        _cancelUnityPlayerAccountsAction = () =>
        {
            _isWaitingForSignIn = false;

            LoadingScreen.Hide();
        };

        _deleteFunc = async () =>
        {
            await AuthenticationService.Instance.DeleteAccountAsync();

            OnClickSignOut();
#if UNITY_EDITOR
            Debug.Log("Deleted Account!");
#endif
            UpdateUI();
        };

        _initializeServices = async () =>
        {
            await UnityServices.InitializeAsync();
#if UNITY_EDITOR
            Debug.Log("Services Initalized!");
#endif
        };

        _signInAnonymously = async () =>
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
#if UNITY_EDITOR
            Debug.Log($"Signed in anonymously, Player ID: {AuthenticationService.Instance.PlayerId}");
#endif
        };

        _startUnitySignIn = async () =>
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        };

        _signInWithUnity = async () =>
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
#if UNITY_EDITOR
            Debug.Log($"Signed in with Unity, Player ID: {AuthenticationService.Instance.PlayerId}");
#endif
        };
    }

    private void OnTokenExpired()
    {
        OnClickSignOut();
#if UNITY_EDITOR
        // You need to implement code here to tell the player that the session expired.
        Debug.Log("Your session has expired and couldn't be refreshed, you will need to sign in again in order to use services.");
#endif
        ErrorScreen.Show("Your session has expired and couldn't be refreshed, you will need to sign in again in order to use services.", "OK");

        UpdateUI();
    }

    private async void OnPlayerAccountSignedIn()
    {
        if (!_isWaitingForSignIn)
        {
            // This means that we shouldn't sign in... as it has been cancelled.
            return;
        }

        await AuthenticationTask.Run(_signInWithUnity, _showErrorAction);

        UpdateUI();

        LoadingScreen.Hide();
    }

    private void OnPlayerAccountSignInFailed(RequestFailedException ex)
    {
        ErrorScreen.Show(AuthenticationErrorHandler.GetErrorMessage(ex), "OK");
    }

    private bool ServicesInitialized()
    {
        return UnityServices.State == ServicesInitializationState.Initialized;
    }


    // UI BT

    private async void OnClickSignInWithUnity()
    {
        if (!ServicesInitialized()) return;

        _isWaitingForSignIn = true;

        LoadingScreen.Show(true, "Signing In...", true, "Cancel", _cancelUnityPlayerAccountsAction);

        bool success = await AuthenticationTask.Run(_startUnitySignIn, _showErrorAction);

        if (!success)
        {
            // This means that we got an exception.

            _isWaitingForSignIn = false;

            LoadingScreen.Hide();
        }
    }

    private async void OnClickSignInAnonymously()
    {
        if (!ServicesInitialized()) return;

        LoadingScreen.Show(true, "Signing In...");

        await AuthenticationTask.Run(_signInAnonymously, _showErrorAction);

        UpdateUI();

        LoadingScreen.Hide();
    }

    private void OnClickSignOut()
    {
        if (!ServicesInitialized()) return;

        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        UpdateUI();
    }

    private async void OnClickDeleteAccount()
    {
        if (!ServicesInitialized()) return;

        if (AuthenticationService.Instance.IsSignedIn)
        {
            LoadingScreen.Show(true, "Deleting Account...");

            await AuthenticationTask.Run(_deleteFunc, _showErrorAction);

            LoadingScreen.Hide();
        }
    }

    private void OnClickClearSessionToken()
    {
        if (!ServicesInitialized()) return;

        if (AuthenticationService.Instance.SessionTokenExists && !AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.ClearSessionToken();

            UpdateUI();
        }
    }

    // UI TT

    private void UpdateUI()
    {
        // Text
        StringBuilder sb = new StringBuilder();

        bool authSignedIn = AuthenticationService.Instance.IsSignedIn;

        sb.AppendLine($"Player Accounts State: {(PlayerAccountService.Instance.IsSignedIn ? "Signed In" : "Signed Out")}");
        sb.AppendLine($"Player Accounts Access Token: <b>{(string.IsNullOrEmpty(PlayerAccountService.Instance.AccessToken) ? "Missing" : "Exists")}</b>");
        sb.AppendLine($"Authentication Service State: <b>{(authSignedIn ? "Signed In" : "Signed out")}</b>");

        if (authSignedIn)
        {
            var externalIDs = FormatExternalIds(AuthenticationService.Instance.PlayerInfo);
            sb.AppendLine($"Player ID: <b>{AuthenticationService.Instance.PlayerId}</b>");
            sb.AppendLine($"Session Token: <b>{(AuthenticationService.Instance.SessionTokenExists ? "Exists" : "Missing")}</b>");
            sb.AppendLine($"Linked External ID Providers: <b>{externalIDs}</b>");
        }

        _infoText.text = sb.ToString();

        // Buttons.
        _signInAnonymouslyButton.interactable = !authSignedIn;
        _signInWithUnityButton.interactable = !authSignedIn;
        _signOutButton.interactable = authSignedIn;
        _deleteAccountButton.interactable = authSignedIn;
        _clearSessionTokenButton.interactable = !authSignedIn && AuthenticationService.Instance.SessionTokenExists;
    }

    private static string FormatExternalIds(PlayerInfo playerInfo)
    {
        if (playerInfo.Identities == null)
        {
            return "None";
        }

        var sb = new StringBuilder();
        foreach (var id in playerInfo.Identities)
        {
            sb.Append(" " + id.TypeId);
        }

        return sb.ToString();
    }
}

public static class AuthenticationErrorHandler
{
    public static string GetErrorMessage(AuthenticationException ex)
    {
        string msg = $"Authentication Failed with Error: {FormatErrorMessage(ex.Message)}";

        msg = $"{msg}\nError Code {ex.ErrorCode}";
#if UNITY_EDITOR
        Debug.Log($"Error with Code {ex.ErrorCode}");
#endif
        return msg;
    }

    public static string GetErrorMessage(RequestFailedException ex)
    {
        string msg = $"Request Failed with Error: {FormatErrorMessage(ex.Message)}";

        if (ex.ErrorCode == CommonErrorCodes.Unknown) msg = "An unexpected error occured, Please try again";
        if (ex.ErrorCode == CommonErrorCodes.TransportError) msg = "Network issue when connecting to server. Check your internet.";
        if (ex.ErrorCode == CommonErrorCodes.Timeout) msg = "Server took too long to respond, Please retry";
        if (ex.ErrorCode == CommonErrorCodes.ServiceUnavailable) msg = "Service is temporarily unavailable. Please try again later.";
        if (ex.ErrorCode == CommonErrorCodes.ApiMissing) msg = "Requested Feature is currently unavailable.";
        if (ex.ErrorCode == CommonErrorCodes.RequestRejected) msg = "Your request was rejected - please try again or contect support.";
        if (ex.ErrorCode == CommonErrorCodes.TooManyRequests) msg = "You're making requests too quickly. Please slow down.";
        if (ex.ErrorCode == CommonErrorCodes.InvalidToken || ex.ErrorCode == CommonErrorCodes.TokenExpired) msg = "Your session expired. Please Sign in again";
        if (ex.ErrorCode == CommonErrorCodes.Forbidden) msg = "You don't have permission to perform this action.";
        if (ex.ErrorCode == CommonErrorCodes.NotFound) msg = "Requested item not found.";
        if (ex.ErrorCode == CommonErrorCodes.InvalidRequest) msg = "Invalid Request, Please check your input.";

        msg = $"{msg}\nError Code {ex.ErrorCode}";
#if UNITY_EDITOR
        Debug.Log($"Error with Code {ex.ErrorCode}");
#endif
        return msg;
    }

    public static string FormatErrorMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        return string.Join(" ", raw.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
}

public static class AuthenticationTask
{
    public static async Task<bool> Run(Func<Task> task, Action<string> onError)
    {
        try
        {
            await task();

            return true;
        }
        catch (AuthenticationException ex)
        {
            onError(AuthenticationErrorHandler.GetErrorMessage(ex));
        }
        catch (RequestFailedException ex)
        {
            onError(AuthenticationErrorHandler.GetErrorMessage(ex));
        }

        // This means our try failed.
        return false;
    }
}