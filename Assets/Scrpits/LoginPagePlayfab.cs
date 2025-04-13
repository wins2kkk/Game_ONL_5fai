using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;

public class LoginPagePlayfab : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI TopText;
    [SerializeField] TextMeshProUGUI MessageText;

    [Header("Login")]
    [SerializeField] TMP_InputField EmailLoginInput;
    [SerializeField] TMP_InputField PasswordLoginInput;
    [SerializeField] GameObject LoginPage;

    [Header("Register")]
    [SerializeField] TMP_InputField UserNameRegisterInput;
    [SerializeField] TMP_InputField EmailRegisterInput;
    [SerializeField] TMP_InputField PassworRegisterInput;
    [SerializeField] GameObject ResgisterPage;

    [Header("Recovery")]
    [SerializeField] TMP_InputField EmailRecoveryInput;
    [SerializeField] GameObject RecoveryPage;

    [SerializeField]
    private GameObject WelcomeObject;

    [SerializeField]
    private Text WelcomeText;

    [SerializeField]
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Buttom Fuctions
    public void RegisterUser()
    {
        // Kiểm tra độ dài mật khẩu
        if (PassworRegisterInput.text.Length < 6)
        {
            MessageText.text = "Password is too short";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            DisplayName = UserNameRegisterInput.text,
            Email = EmailRegisterInput.text,
            Password = PassworRegisterInput.text,

            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = EmailLoginInput.text,
            Password = PasswordLoginInput.text,


            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSucces, OnError);
    }

    private void OnLoginSucces(LoginResult result)
    {
        string name = result.InfoResultPayload?.PlayerProfile?.DisplayName ?? "Player";
        WelcomeObject.SetActive(true);
        WelcomeText.text = "Welcome " + name;

        if (gameManager != null)
        {
            gameManager.playerName = name;
        }

        StartCoroutine(LoadNextScene());
    }

    public void RecoveryUser()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = EmailLoginInput.text,
            TitleId = "1A92DA"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnRecoverySucces, OnErrorRecovery);
    }

    private void OnErrorRecovery(PlayFabError result)
    {
        MessageText.text = "No Email Found";
    }

    private void OnRecoverySucces(SendAccountRecoveryEmailResult obj)
    {
        OpenLoginPage();
        MessageText.text = "Recovery Mail Sent";
    }


    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        MessageText.text = "New Account Is Created";
        OpenLoginPage();
    }

    private void OnError(PlayFabError error)
    {
        MessageText.text = "Error: " + error.ErrorMessage;
    }


    public void OpenLoginPage()
    {
        LoginPage.SetActive(true);
        ResgisterPage.SetActive(false);
        RecoveryPage.SetActive(false);
        TopText.text = "Login";
    }
    public void OpenRegiserPage()
    {
        LoginPage.SetActive(false);
        ResgisterPage.SetActive(true);
        RecoveryPage.SetActive(false);
        TopText.text = "Register";
    }
    public void OpenRecoveryPage()
    {
        LoginPage.SetActive(false);
        ResgisterPage.SetActive(false);
        RecoveryPage.SetActive(true);
        TopText.text = "Recovery";
    }
    #endregion

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(2);
        MessageText.text = "Login Succes";
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
