using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using System.Threading.Tasks;
using TMPro;
using System.Text;
using System;
using System.Linq;

public class AuthenticationManager : MonoBehaviour
{

    [Header("Authentication Manager")]
    [Header("Buttons")]
    [SerializeField] private Button _signInWithUnityButton;
    [SerializeField] private Button _signInAnonymouslyButton;
    [SerializeField] private Button _signOutButton;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _clearSessionTokenButton;
    [Header("Text")]
    [SerializeField] private TMP_Text _completeTextInfo;

    private bool _isWaitingForSignIn = false;

    // Actions...

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
        _deleteButton.onClick.AddListener(OnClickDelete);
        _clearSessionTokenButton.onClick.AddListener(OnClickClearToken);
    }

    private void DeregisterEvents()
    {
        AuthenticationService.Instance.Expired -= OnTokenExpired;
        PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

        _signInWithUnityButton.onClick.RemoveListener(OnClickSignInWithUnity);
        _signInAnonymouslyButton.onClick.RemoveListener(OnClickSignInAnonymously);
        _signOutButton.onClick.RemoveListener(OnClickSignOut);
        _deleteButton.onClick.RemoveListener(OnClickDelete);
        _clearSessionTokenButton.onClick.RemoveListener(OnClickClearToken);
    }

    private async Task Startup()
    {
        SetupActions();

        LoadingScreen.Show(true, "Init Services...");

        #region OLD
        //try
        //{
        //    LoadingScreen.Show(true, "Init Services...");

        //    await UnityServices.InitializeAsync();

        //    Debug.Log("Services Initialized!");

        //    if (AuthenticationService.Instance.SessionTokenExists)
        //    {
        //        LoadingScreen.Show(true, "Signing In...");

        //        // Auto sign in.
        //        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //        Debug.Log("Sign in automatically succeeded!");
        //    }

        //    UpdateUI();
        //}
        //catch (AuthenticationException ex)
        //{
        //    // Compare error code to AuthenticationErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        //catch (RequestFailedException ex)
        //{
        //    // Compare error code to CommonErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}

        #endregion

        bool success = await AuthenticationTask.Run(_initializeServices, _showErrorAction);
        
        if (success && AuthenticationService.Instance.SessionTokenExists)
        {
            LoadingScreen.Show(true, "Signing In...");

            // Attempt Auto Sign in.
            await AuthenticationTask.Run(_signInAnonymously, _showErrorAction);

            //if (AuthenticationService.Instance.IsSignedIn)
            //{
            //    Debug.Log("Sign in automatically succeeded!");
            //}
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

            Debug.Log("Deleted Account!");

            UpdateUI();
        };

        _initializeServices = async () =>
        {
            await UnityServices.InitializeAsync();

            Debug.Log("Services Initialized!");
        };

        _signInAnonymously = async () =>
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"Signed in Anonymously with player ID {AuthenticationService.Instance.PlayerId}");
        };

        _startUnitySignIn = async () =>
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        };

        _signInWithUnity = async () =>
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);

            Debug.Log($"Signed in with Unity {AuthenticationService.Instance.PlayerId}");
        };
    }

    private void OnTokenExpired()
    {
        #region OLD
        //if (AuthenticationService.Instance.SessionTokenExists)
        //{
        //try
        //{
        //    //await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //    //Debug.Log("Sign in anonymously succeeded!");
        //    OnClickSignOut();

        //    // Let the player know there session expired...
        //    Debug.Log("Your Session has expired and couldn't be refreshed, you will need to sign in again.");
        //}
        //catch (AuthenticationException ex)
        //{
        //    // Compare error code to AuthenticationErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);
        //}
        //catch (RequestFailedException ex)
        //{
        //    // Compare error code to CommonErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);
        //}
        //}
        #endregion

        // Ensure to sign out.
        OnClickSignOut();

        // Let the player know there session expired...
        Debug.Log("Your Session has expired and couldn't be refreshed, you will need to sign in again.");

        ErrorScreen.Show("Your Session has expired and couldn't be refreshed, you will need to sign in again", "OK");

        UpdateUI();
    }

    private async void OnPlayerAccountSignedIn()
    {
        if (!_isWaitingForSignIn)
        {
            // This means that we shouldn't sign in... as it has been cancelled.
            return;
        }

        #region OLD
        //try
        //{
        //    await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);

        //    UpdateUI();
        //}
        //catch (AuthenticationException ex)
        //{
        //    // Compare error code to AuthenticationErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        //catch (RequestFailedException ex)
        //{
        //    // Compare error code to CommonErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        #endregion
        await AuthenticationTask.Run(_signInWithUnity, _showErrorAction);

        UpdateUI();

        LoadingScreen.Hide();
    }

    private void OnPlayerAccountSignInFailed(RequestFailedException ex)
    {
        // Compare error code to CommonErrorCodes
        // Notify the player with the proper error message

        ErrorScreen.Show(UGSErrorHandler.GetErrorMessage(ex), "OK");
    }

    private bool ServicesInitialized()
    {
        return UnityServices.State == ServicesInitializationState.Initialized;
    }

    private async void OnClickSignInWithUnity()
    {
        if (!ServicesInitialized()) return;

        _isWaitingForSignIn = true;

        LoadingScreen.Show(true, "Signing In...", true, "Cancel", _cancelUnityPlayerAccountsAction);
        #region OLD
        //try
        //{
        //    await PlayerAccountService.Instance.StartSignInAsync();

        //    return;
        //}
        //catch (AuthenticationException ex)
        //{
        //    // Compare error code to AuthenticationErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        //catch (RequestFailedException ex)
        //{
        //    // Compare error code to CommonErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        #endregion
        bool success = await AuthenticationTask.Run(_startUnitySignIn, _showErrorAction);

        if (!success)
        {
            // This means that we got an exception...

            _isWaitingForSignIn = false;

            LoadingScreen.Hide();
        }
    }

    private async void OnClickSignInAnonymously()
    {
        if (!ServicesInitialized()) return;

        LoadingScreen.Show(true, "Signing In...");

        #region OLD
        //try
        //{
        //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //    Debug.Log("Sign in anonymously succeeded!");

        //    // Shows how to get the playerID
        //    Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        //    UpdateUI();
        //}
        //catch (AuthenticationException ex)
        //{
        //    // Compare error code to AuthenticationErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        //catch (RequestFailedException ex)
        //{
        //    // Compare error code to CommonErrorCodes
        //    // Notify the player with the proper error message
        //    Debug.Log("EXCEPTION " + ex);

        //    ErrorScreen.Show(ex.ToString(), "OK");
        //}
        #endregion
        await AuthenticationTask.Run(_signInAnonymously, _showErrorAction);

        //if (success)
        //{
        //    Debug.Log("Sign in anonymously succeeded!");
        //    Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        //}

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

    private async void OnClickDelete()
    {
        if (!ServicesInitialized()) return;

        if (AuthenticationService.Instance.IsSignedIn)
        {
            LoadingScreen.Show(true, "Deleting Account...");
            #region OLD
            //try
            //{
            //    // Note, you must delete the associated data manually, this only deletes the account
            //    await AuthenticationService.Instance.DeleteAccountAsync();

            //    OnClickSignOut();

            //    Debug.Log("Deleted Account!");

            //    UpdateUI();
            //}
            //catch (AuthenticationException ex)
            //{
            //    // Compare error code to AuthenticationErrorCodes
            //    // Notify the player with the proper error message
            //    Debug.Log("EXCEPTION " + ex);

            //    ErrorScreen.Show(ex.ToString(), "OK");
            //}
            //catch (RequestFailedException ex)
            //{
            //    // Compare error code to CommonErrorCodes
            //    // Notify the player with the proper error message
            //    Debug.Log("EXCEPTION " + ex);

            //    ErrorScreen.Show(ex.ToString(), "OK");
            //}

            //Func<Task> deleteAction = async () =>
            //{
            //    await AuthenticationService.Instance.DeleteAccountAsync();

            //    OnClickSignOut();

            //    Debug.Log("Deleted Account!");

            //    UpdateUI();
            //};

            //Action<string> showErrorAction = (msg) =>
            //{
            //    ErrorScreen.Show(msg, "OK");
            //};
            #endregion
            await AuthenticationTask.Run(_deleteFunc, _showErrorAction);

            LoadingScreen.Hide();
        }
    }

    private void OnClickClearToken()
    {
        if (!ServicesInitialized()) return;

        if (AuthenticationService.Instance.SessionTokenExists && !AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.ClearSessionToken();

            UpdateUI();
        }
    }


    private void UpdateUI()
    {
        // Text
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Player Accounts State: {(PlayerAccountService.Instance.IsSignedIn ? "Signed In" : "Signed Out")}");
        sb.AppendLine($"Player Accounts access token: <b>{(string.IsNullOrEmpty(PlayerAccountService.Instance.AccessToken) ? "Missing" : "Exists")}</b>");
        sb.AppendLine($"Authentication service state: <b>{(AuthenticationService.Instance.IsSignedIn ? "Signed in" : "Signed out")}</b>");

        if (AuthenticationService.Instance.IsSignedIn)
        {
            var externalIds = FormatExternalIds(AuthenticationService.Instance.PlayerInfo);
            sb.AppendLine($"Player ID: <b>{AuthenticationService.Instance.PlayerId}</b>");
            sb.AppendLine($"Session Token: <b>{(AuthenticationService.Instance.SessionTokenExists ? "Exists" : "Missing")}</b>");
            sb.AppendLine($"Linked external ID providers: <b>{externalIds}</b>");
        }

        _completeTextInfo.text = sb.ToString();

        // Buttons
        _signInAnonymouslyButton.interactable = !AuthenticationService.Instance.IsSignedIn;
        _signInWithUnityButton.interactable = !AuthenticationService.Instance.IsSignedIn;
        _signOutButton.interactable = AuthenticationService.Instance.IsSignedIn;
        _deleteButton.interactable = AuthenticationService.Instance.IsSignedIn;
        _clearSessionTokenButton.interactable = AuthenticationService.Instance.SessionTokenExists && !AuthenticationService.Instance.IsSignedIn;
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



    public static class UGSErrorHandler
    {
        public static string GetErrorMessage(AuthenticationException ex)
        {
            string msg = $"Authentication Failed With Error: {FormatErrorMessage(ex.Message)}";

            Debug.Log(ex.ErrorCode);

            return msg;
        }

        public static string GetErrorMessage(RequestFailedException ex)
        {
            string msg = $"Request Failed With Error : {FormatErrorMessage(ex.Message)}";

            if (ex.ErrorCode == CommonErrorCodes.Unknown) msg = "An unexpected error occurred. Please try again.";
            if (ex.ErrorCode == CommonErrorCodes.TransportError) msg = "Network issue connecting to server. Check your internet.";
            if (ex.ErrorCode == CommonErrorCodes.Timeout) msg = "Server took too long to respond. Please retry";
            if (ex.ErrorCode == CommonErrorCodes.ServiceUnavailable) msg = "Service is temporarily unavailable. Please try again later.";
            if (ex.ErrorCode == CommonErrorCodes.ApiMissing) msg = "Requested feature is currently unavailable.";
            if (ex.ErrorCode == CommonErrorCodes.RequestRejected) msg = "Your request was rejected — please try again or contact support.";
            if (ex.ErrorCode == CommonErrorCodes.TooManyRequests) msg = "You're making requests too quickly. Please slow down.";
            if (ex.ErrorCode == CommonErrorCodes.InvalidToken || ex.ErrorCode == CommonErrorCodes.TokenExpired) msg = "Your session expired. Please sign in again.";
            if (ex.ErrorCode == CommonErrorCodes.Forbidden) msg = "You don't have permission to perform this action.";
            if (ex.ErrorCode == CommonErrorCodes.NotFound) msg = "Requested item not found.";
            if (ex.ErrorCode == CommonErrorCodes.InvalidRequest) msg = "Invalid request. Please check your input.";

            msg = $"{msg}\nError Code {ex.ErrorCode}";

            Debug.Log(ex.ErrorCode);

            return msg;
        }

        public static string FormatErrorMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            return string.Join(" ", raw
                .Split('_')
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
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
                onError(UGSErrorHandler.GetErrorMessage(ex));
            }
            catch (RequestFailedException ex)
            {
                onError(UGSErrorHandler.GetErrorMessage(ex));
            }

            // This means our try failed...
            return false;
        }
    }











































    //[Header("Sign in Buttons")]
    //[SerializeField] private Button _signInWithUnityButton;
    //[SerializeField] private Button _signInAnonymouslyButton;
    //[SerializeField] private Button _signOutButton;
    //[SerializeField] private Button _deleteButton;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //async void Start()
    //{
    //    await InitialFlow();

    //    RegisterCallbacks();
    //}

    //private void OnDestroy()
    //{
    //    DeregisterCallbacks();
    //}

    //private async Task InitialFlow()
    //{
    //    try
    //    {
    //        await UnityServices.InitializeAsync();

    //        if (AuthenticationService.Instance.SessionTokenExists)
    //        {
    //            // try auto sign in...
    //            bool success = await SignInAnonymously();
    //        }
    //    }
    //    catch (RequestFailedException ex)
    //    {
    //        // Failed to initialize services..., Show your error screen or something,
    //        // also you can't use any service without initializing it.
    //        Debug.Log(ex);
    //    }
    //}

    //private void RegisterCallbacks()
    //{
    //    _signInWithUnityButton.onClick.AddListener(OnClickSignInWithUnity);
    //    _signInAnonymouslyButton.onClick.AddListener(OnClickSignInAnonymously);
    //    _signOutButton.onClick.AddListener(OnClickSignOut);
    //    _deleteButton.onClick.AddListener(OnClickDelete);

    //    PlayerAccountService.Instance.SignedIn += OnPlayerAccountsSignedIn;
    //}

    //private void DeregisterCallbacks()
    //{
    //    _signInWithUnityButton.onClick.RemoveListener(OnClickSignInWithUnity);
    //    _signInAnonymouslyButton.onClick.RemoveListener(OnClickSignInAnonymously);
    //    _signOutButton.onClick.RemoveListener(OnClickSignOut);
    //    _deleteButton.onClick.RemoveListener(OnClickDelete);

    //    PlayerAccountService.Instance.SignedIn -= OnPlayerAccountsSignedIn;
    //}

    //private async void OnPlayerAccountsSignedIn()
    //{
    //    // Sign in with Unity.
    //    bool success = await SignInWithUnity();

    //    if (success)
    //    {
    //        Debug.Log($"Signed in with Unity {AuthenticationService.Instance.PlayerId}");
    //    }
    //    else
    //    {
    //        Debug.Log("Failed to sign in with Unity");
    //    }
    //}

    //private void OnClickSignInWithUnity()
    //{
    //    if (!CanUseServices()) return;

    //    StartSignInWithUnity();
    //}

    //private async void OnClickSignInAnonymously()
    //{
    //    if (!CanUseServices()) return;

    //    bool success = await SignInAnonymously();

    //    if (success)
    //    {
    //        // This will use the saved session token from playerprefs if it exists.
    //        Debug.Log($"Signed in Anonymously {AuthenticationService.Instance.PlayerId}");
    //    }
    //    else
    //    {
    //        Debug.Log("Failed to sign in anonymously");
    //    }
    //}

    //private void OnClickSignOut()
    //{
    //    if (!CanUseServices()) return;

    //    if (AuthenticationService.Instance.IsSignedIn)
    //    {
    //        AuthenticationService.Instance.SignOut();

    //        Debug.Log("Signed Out of Auth Service");
    //    }

    //    if (PlayerAccountService.Instance.IsSignedIn)
    //    {
    //        PlayerAccountService.Instance.SignOut();

    //        Debug.Log("Signed Out of Player Accounts Service");
    //    }
    //}

    //private async void OnClickDelete()
    //{
    //    if (!CanUseServices()) return;

    //    if (AuthenticationService.Instance.IsSignedIn)
    //    {
    //        // Note, This only deletes the account stored on cloud, this doesn't delete the data stored with it.
    //        // if you use other services, cloud save e.g. then you will have to delete that manually using those Service's API's

    //        await AuthenticationService.Instance.DeleteAccountAsync();

    //        Debug.Log("Deleted Account From This Projects User Accounts...");
    //    }
    //}

    //private bool CanUseServices()
    //{
    //    return UnityServices.State == ServicesInitializationState.Initialized;
    //}


    //// --Sign in Functions--


    //private async Task<bool> SignInAnonymously()
    //{
    //    try
    //    {
    //        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //        return true;
    //    }
    //    catch (RequestFailedException e)
    //    {
    //        Debug.Log("Failed to Sign In Anonymously with exception " +  e);
    //        return false;
    //    }
    //}

    //private async void StartSignInWithUnity()
    //{
    //    if (PlayerAccountService.Instance.IsSignedIn)
    //    {
    //        bool success = await SignInWithUnity();

    //        if (success)
    //        {
    //            Debug.Log($"Signed in with Unity {AuthenticationService.Instance.PlayerId}");
    //        }
    //        else
    //        {
    //            Debug.Log("Failed to sign in with Unity");
    //        }
    //    }

    //    try
    //    {
    //        await PlayerAccountService.Instance.StartSignInAsync();
    //    }
    //    catch (RequestFailedException e)
    //    {
    //        Debug.Log(e);
    //    }
    //}

    //private async Task<bool> SignInWithUnity()
    //{
    //    try
    //    {
    //        await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
    //        await PlayerAccountService.Instance.RefreshTokenAsync();

    //        return true;
    //    }
    //    catch (RequestFailedException e)
    //    {
    //        Debug.Log("Failed to Sign In with Unity with exception " + e);

    //        return false;
    //    }
    //}
}