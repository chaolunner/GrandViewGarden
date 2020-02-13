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
    private IdentificationObject LobbyId;

    private const string UserNameStr = "UserName";
    private const string PasswordStr = "Password";
    private const string UserNameEmptyError = "User name can't be empty!";
    private const string PasswordEmptyError = "Password can't be empty!";
    private const string UserNameOrPasswordIncorrectError = "User name or Password incorrect!";
    private const string LoginSuccessFeedback = "Login successed!";

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

        NetworkSystem.Receive(RequestCode.Login).Subscribe(OnLogin).AddTo(this.Disposer);
    }

    private void OnLogin(ReceiveData data)
    {
        string[] strs = data.StringValue.Split(Separator);
        ReturnCode returnCode = (ReturnCode)int.Parse(strs[0]);
        if (returnCode == ReturnCode.Success)
        {
            int userId = int.Parse(strs[1]);
            string username = strs[2];
            int totalCount = int.Parse(strs[3]);
            int winCount = int.Parse(strs[4]);
            var evt = new TriggerEnterEvent();
            evt.Source = LobbyId;
            EventSystem.Send(evt);
            EventSystem.Send(new MessageEvent(LoginSuccessFeedback, LogType.Log));
            EventSystem.Send(new SpawnUserEvent(userId, username, totalCount, winCount, true, false));
            NetworkSystem.Publish(RequestCode.ListRooms, EmptyStr);
        }
        else
        {
            EventSystem.Send(new MessageEvent(UserNameOrPasswordIncorrectError, LogType.Error));
        }
    }
}
