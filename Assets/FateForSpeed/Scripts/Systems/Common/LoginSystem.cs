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

    private const string UserNameInputStr = "UserName";
    private const string PasswordInputStr = "Password";
    private const string UserNameEmptyError = "User name can't be empty!";
    private const string PasswordEmptyError = "Password can't be empty!";

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
                NetworkSystem.Publish(RequestCode.Login, "");
            }
        }).AddTo(this.Disposer);
    }
}
