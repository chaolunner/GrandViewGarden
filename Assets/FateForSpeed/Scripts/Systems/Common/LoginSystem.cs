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
    private const string UserNameStr = "UserName";
    private const string PasswordStr = "Password";
    private const string UserNameEmptyError = "User name can't be empty!";
    private const string PasswordEmptyError = "Password can't be empty!";
    private const string UserNameOrPasswordIncorrectError = "User name or Password incorrect!";
    private const string LoginSuccessedLog = "Login successed!";

    public override void OnEnable()
    {
        base.OnEnable();

        EventSystem.OnEvent<LoginEvent>().Subscribe(evt =>
        {
            var userNameInput = evt.References.GetComponent<TMP_InputField>(UserNameStr);
            var passwordInput = evt.References.GetComponent<TMP_InputField>(PasswordStr);

            if (userNameInput == null || string.IsNullOrEmpty(userNameInput.text))
            {
                EventSystem.Publish(new MessageEvent(UserNameEmptyError, LogType.Error));
            }
            else if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
            {
                EventSystem.Publish(new MessageEvent(PasswordEmptyError, LogType.Error));
            }
            else
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
            EventSystem.Publish(new MessageEvent(LoginSuccessedLog, LogType.Log));
        }
        else
        {
            EventSystem.Publish(new MessageEvent(UserNameOrPasswordIncorrectError, LogType.Error));
        }
    }
}
