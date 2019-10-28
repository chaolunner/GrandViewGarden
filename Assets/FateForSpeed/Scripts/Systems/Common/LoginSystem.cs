using UniEasy.ECS;
using UnityEngine;
using UniEasy.Net;
using UniEasy.DI;
using UniEasy;
using Common;
using UniRx;
using TMPro;

public class LoginSystem : SystemBehaviour
{
    [Inject]
    private INetworkSystem NetworkSystem;

    private const char Separator = ',';
    private const string UserNameInputStr = "UserName";
    private const string PasswordInputStr = "Password";
    private const string UserNameEmptyError = "User name can't be empty!";
    private const string PasswordEmptyError = "Password can't be empty!";
    private const string UserNameOrPasswordIncorrectError = "User name or Password incorrect!";

    public override void OnEnable()
    {
        base.OnEnable();

        EventSystem.OnEvent<LoginEvent>().Subscribe(evt =>
        {
            var userNameInput = evt.References.GetComponent<TMP_InputField>(UserNameInputStr);
            var passwordInput = evt.References.GetComponent<TMP_InputField>(PasswordInputStr);
            if (userNameInput == null || string.IsNullOrEmpty(userNameInput.text))
            {
                EventSystem.Publish(new MessageEvent(UserNameEmptyError, LogType.Error));
            }
            if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
            {
                EventSystem.Publish(new MessageEvent(PasswordEmptyError, LogType.Error));
            }
            if (userNameInput != null && !string.IsNullOrEmpty(userNameInput.text) && passwordInput != null && !string.IsNullOrEmpty(passwordInput.text))
            {
                NetworkSystem.Publish(RequestCode.Login, userNameInput.text + Separator + passwordInput.text);
            }
        }).AddTo(this.Disposer);

        NetworkSystem.Receive(RequestCode.Login, OnLogin);
    }

    private void OnLogin(string data)
    {
        ReturnCode returnCode = (ReturnCode)int.Parse(data);
        if (returnCode == ReturnCode.Success)
        {
            Debug.Log("login success!");
        }
        else
        {
            EventSystem.Publish(new MessageEvent(UserNameOrPasswordIncorrectError, LogType.Error));
        }
    }
}
