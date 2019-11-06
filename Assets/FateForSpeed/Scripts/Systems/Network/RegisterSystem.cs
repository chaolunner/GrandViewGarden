using UniEasy.ECS;
using UnityEngine;
using UniEasy.Net;
using UniEasy;
using Common;
using UniRx;
using TMPro;

public class RegisterSystem : NetworkSystemBehaviour
{
    private const string UserNameStr = "UserName";
    private const string PasswordStr = "Password";
    private const string RepeatPasswordStr = "RepeatPassword";
    private const string UserNameEmptyError = "User name can't be empty!";
    private const string PasswordEmptyError = "Password can't be empty!";
    private const string RepeatPasswordEmptyError = "Repeat password can't be empty!";
    private const string PasswordsAreInconsistentError = "Passwords are inconsistent!";
    private const string RegisterFailError = "Register failed, user already exists!";
    private const string RegisterSuccessFeedback = "Register successed!";

    public override void OnEnable()
    {
        base.OnEnable();

        EventSystem.OnEvent<RegisterEvent>().Subscribe(evt =>
        {
            var userNameInput = evt.References.GetComponent<TMP_InputField>(UserNameStr);
            var passwordInput = evt.References.GetComponent<TMP_InputField>(PasswordStr);
            var repeatPasswordInput = evt.References.GetComponent<TMP_InputField>(RepeatPasswordStr);

            if (userNameInput == null || string.IsNullOrEmpty(userNameInput.text))
            {
                EventSystem.Send(new MessageEvent(UserNameEmptyError, LogType.Error));
            }
            else if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
            {
                EventSystem.Send(new MessageEvent(PasswordEmptyError, LogType.Error));
            }
            else if (repeatPasswordInput == null || string.IsNullOrEmpty(repeatPasswordInput.text))
            {
                EventSystem.Send(new MessageEvent(RepeatPasswordEmptyError, LogType.Error));
            }
            else if (passwordInput.text != repeatPasswordInput.text)
            {
                EventSystem.Send(new MessageEvent(PasswordsAreInconsistentError, LogType.Error));
            }
            else
            {
                NetworkSystem.Publish(RequestCode.Register, userNameInput.text + Separator + passwordInput.text);
            }
        }).AddTo(this.Disposer);

        NetworkSystem.OnEvent(RequestCode.Register, OnRegister);
    }

    private void OnRegister(string data)
    {
        ReturnCode returnCode = (ReturnCode)int.Parse(data);
        if (returnCode == ReturnCode.Success)
        {
            EventSystem.Send(new MessageEvent(RegisterSuccessFeedback, LogType.Log));
        }
        else
        {
            EventSystem.Send(new MessageEvent(RegisterFailError, LogType.Error));
        }
    }
}
