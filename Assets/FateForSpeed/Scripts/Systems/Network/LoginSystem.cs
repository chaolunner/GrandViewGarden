using UniEasy.ECS;
using UnityEngine;
using UniEasy.Net;
using UniEasy;
using Common;
using UniRx;
using TMPro;

public class LoginSystem : NetworkSystemBehaviour
{
    [SerializeField]
    private IdentificationObject SignInIdentifier;

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
                EventSystem.Send(new MessageEvent(UserNameEmptyError, LogType.Error));
            }
            else if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
            {
                EventSystem.Send(new MessageEvent(PasswordEmptyError, LogType.Error));
            }
            else
            {
                NetworkSystem.Publish(RequestCode.Login, userNameInput.text + Separator + passwordInput.text);
            }
        }).AddTo(this.Disposer);

        NetworkSystem.OnEvent(RequestCode.Login, OnLogin);
    }

    private void OnLogin(string data)
    {
        string[] strs = data.Split(Separator);
        ReturnCode returnCode = (ReturnCode)int.Parse(strs[0]);
        if (returnCode == ReturnCode.Success)
        {
            string username = strs[1];
            int totalCount = int.Parse(strs[2]);
            int winCount = int.Parse(strs[3]);
            var evt = new TriggerEnterEvent();
            evt.Source = SignInIdentifier;
            EventSystem.Send(evt);
            EventSystem.Send(new MessageEvent(LoginSuccessedLog, LogType.Log));
            EventSystem.Send(new SpawnUserEvent(username, totalCount, winCount, true));
            NetworkSystem.Publish(RequestCode.ListRooms, EmptyStr);
        }
        else
        {
            EventSystem.Send(new MessageEvent(UserNameOrPasswordIncorrectError, LogType.Error));
        }
    }
}
