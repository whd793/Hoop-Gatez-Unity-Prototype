using UnityEngine;
#if OG_FB_SDK
using Facebook.Unity;
#endif

namespace OnefallGames
{
    public class FacebookManager : MonoBehaviour
    {
        public static FacebookManager Instance { get; private set; }

#if OG_FB_SDK
    [Header("Facebook Sharing Config")]
    [SerializeField] private string gameStoreUrl = "https://play.google.com/store/apps/details?id=com.GameShock.KnifeHitLevels2";
    [SerializeField] private string sharingTitle = "Check this out !!!";
    [SerializeField] private string sharingDescription = "New cool game LOL !";
#endif

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
#if OG_FB_SDK
        if (!FB.IsInitialized)
        {
            FB.Init();
        }
        else
        {
            FB.ActivateApp();
        }
#endif
        }


#if OG_FB_SDK
    /// <summary>
    /// Login to facebook
    /// </summary>
    public void FacebookLogIn()
    {

        FB.LogInWithReadPermissions(callback: OnLogIn);
    }

    private void OnLogIn(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Login successfully");
        }
        else
        {
            Debug.Log("Login failed");
        }
    }


/// <summary>
/// Logout of facebook
/// </summary>
public void FacebookLogOut()
    {
        FB.LogOut();
    }

#endif

        /// <summary>
        /// Share to facebook account
        /// </summary>
        public void FacebookShare()
        {
#if OG_FB_SDK
        FB.ShareLink(contentURL: new System.Uri(gameStoreUrl),
            contentTitle: sharingTitle,
            contentDescription: sharingDescription,
            callback: OnFacebookShare);
#endif
        }

#if OG_FB_SDK
    private void OnFacebookShare(IShareResult result)
    {
        if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("Facebook Share Error: " + result.Error);
        }
        else if (!string.IsNullOrEmpty(result.PostId))
        {
            Debug.Log(result.PostId);
        }
        else
        {
            Debug.Log("Facebook Share Successfully");
        }
    }
#endif
    }
}

