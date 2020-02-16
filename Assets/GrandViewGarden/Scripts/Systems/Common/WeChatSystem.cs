using UnityEngine.Networking;
using cn.sharesdk.unity3d;
using System.Collections;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using UniEasy;
using UniRx;

public class WeChatSystem : SystemBehaviour
{
    [Inject]
    private ShareSDK ShareSDK;

    private EasyWriter WeChatWriter;
    private IGroup WeChatComponents;
    private BoolReactiveProperty IsAuthorized = new BoolReactiveProperty();
    private BoolReactiveProperty IsGetUserInfo = new BoolReactiveProperty();
    private Hashtable authInfo;
    private Hashtable userInfo;

    private const string WeChatDataPath = "/WeChat.json";
    private const string AuthInfo = "AuthInfo";
    private const string UserInfo = "UserInfo";
    private const string NickName = "nickname";
    private const string HeadImgURL = "headimgurl";
    private const string IsOn = "IsOn";
    private const string Empty = "";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        WeChatWriter = new EasyWriter(Application.persistentDataPath + WeChatDataPath);
        WeChatComponents = this.Create(typeof(WeChatComponent), typeof(Animator));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        ShareSDK.authHandler = OnAuthResultHandler;
        ShareSDK.shareHandler = OnShareResultHandler;
        ShareSDK.showUserHandler = OnGetUserInfoResultHandler;
        ShareSDK.getFriendsHandler = OnGetFriendsResultHandler;
        ShareSDK.followFriendHandler = OnFollowFriendResultHandler;

        WeChatComponents.OnAdd().Subscribe(entity =>
        {
            var wechatComponent = entity.GetComponent<WeChatComponent>();
            var animator = entity.GetComponent<Animator>();

            IsAuthorized.DistinctUntilChanged().Subscribe(isOn =>
            {
                animator.SetBool(IsOn, isOn);
                if (isOn)
                {
                    ShareSDK.GetUserInfo(PlatformType.WeChat);
                    wechatComponent.NickName.text = authInfo[NickName].ToString();
                    StartCoroutine(LoadImage(authInfo[HeadImgURL].ToString(), wechatComponent.UserIcon));
                }
                else
                {
                    IsGetUserInfo.Value = false;
                    wechatComponent.NickName.text = Empty;
                    wechatComponent.UserIcon.sprite = null;
                }
            }).AddTo(this.Disposer).AddTo(wechatComponent.Disposer);

            IsGetUserInfo.DistinctUntilChanged().Where(b => b).Subscribe(_ =>
            {
                wechatComponent.NickName.text = userInfo[NickName].ToString();
                StartCoroutine(LoadImage(userInfo[HeadImgURL].ToString(), wechatComponent.UserIcon));
            }).AddTo(this.Disposer).AddTo(wechatComponent.Disposer);

            wechatComponent.SignInButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                ShareSDK.Authorize(PlatformType.WeChat);
            }).AddTo(this.Disposer).AddTo(wechatComponent.Disposer);

            wechatComponent.SignOutButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                ShareSDK.CancelAuthorize(PlatformType.WeChat);
                IsAuthorized.Value = false;
            }).AddTo(this.Disposer).AddTo(wechatComponent.Disposer);
        }).AddTo(this.Disposer);

        WeChatWriter.OnAdd().Subscribe(writer =>
        {
            if (ShareSDK.IsAuthorized(PlatformType.WeChat) && writer.HasKey(AuthInfo))
            {
                authInfo = writer.Get<string>(AuthInfo).hashtableFromJson();
                IsAuthorized.Value = true;
            }
            else
            {
                if (ShareSDK.IsAuthorized(PlatformType.WeChat))
                {
                    ShareSDK.CancelAuthorize(PlatformType.WeChat);
                }
                IsAuthorized.DistinctUntilChanged().TakeWhile(_ => !writer.HasKey(AuthInfo)).Where(b => b).Subscribe(_ =>
                {
                    writer.Set(AuthInfo, authInfo.toJson());
                }).AddTo(this.Disposer);
            }

            IsGetUserInfo.DistinctUntilChanged().TakeWhile(_ => !writer.HasKey(UserInfo)).Where(b => b).Subscribe(_ =>
            {
                writer.Set(UserInfo, userInfo.toJson());
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);
    }

    void OnAuthResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
    {
        if (state == ResponseState.Success)
        {
            if (result != null && result.Count > 0)
            {
                Debug.Log("authorize success !" + "Platform :" + type + "result:" + MiniJSON.jsonEncode(result));
                authInfo = result;
                IsAuthorized.Value = true;
            }
            else
            {
                Debug.Log("authorize success !" + "Platform :" + type);
            }
        }
        else if (state == ResponseState.Fail)
        {
#if UNITY_ANDROID
			Debug.Log("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
#elif UNITY_IPHONE
            Debug.Log("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
#endif
        }
        else if (state == ResponseState.Cancel)
        {
            Debug.Log("cancel !");
        }
    }

    void OnGetUserInfoResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
    {
        if (state == ResponseState.Success)
        {
            Debug.Log("get user info result :");
            Debug.Log(MiniJSON.jsonEncode(result));
            Debug.Log("Get userInfo success !Platform :" + type);
            userInfo = result;
            IsGetUserInfo.Value = true;
        }
        else if (state == ResponseState.Fail)
        {
#if UNITY_ANDROID
			Debug.Log("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
#elif UNITY_IPHONE
            Debug.Log("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
#endif
        }
        else if (state == ResponseState.Cancel)
        {
            Debug.Log("cancel !");
        }
    }

    void OnShareResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
    {
        if (state == ResponseState.Success)
        {
            Debug.Log("share successfully - share result :");
            Debug.Log(MiniJSON.jsonEncode(result));
        }
        else if (state == ResponseState.Fail)
        {
#if UNITY_ANDROID
			Debug.Log("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
#elif UNITY_IPHONE
            Debug.Log("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
#endif
        }
        else if (state == ResponseState.Cancel)
        {
            Debug.Log("cancel !");
        }
    }

    void OnGetFriendsResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
    {
        if (state == ResponseState.Success)
        {
            Debug.Log("get friend list result :");
            Debug.Log(MiniJSON.jsonEncode(result));
        }
        else if (state == ResponseState.Fail)
        {
#if UNITY_ANDROID
			Debug.Log("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
#elif UNITY_IPHONE
            Debug.Log("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
#endif
        }
        else if (state == ResponseState.Cancel)
        {
            Debug.Log("cancel !");
        }
    }

    void OnFollowFriendResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
    {
        if (state == ResponseState.Success)
        {
            Debug.Log("Follow friend successfully !");
        }
        else if (state == ResponseState.Fail)
        {
#if UNITY_ANDROID
			Debug.Log("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
#elif UNITY_IPHONE
            Debug.Log("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
#endif
        }
        else if (state == ResponseState.Cancel)
        {
            Debug.Log("cancel !");
        }
    }

    private IEnumerator LoadImage(string url, Image image)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
        image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
    }
}
