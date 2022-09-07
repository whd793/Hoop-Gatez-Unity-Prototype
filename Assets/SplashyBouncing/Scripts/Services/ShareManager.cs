using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OnefallGames
{
    public class ShareManager : MonoBehaviour
    {

        public static ShareManager Instance { get; private set; }

        [Header("Sharing Config")]
        [SerializeField] private string screenshotName = "screenshot.png";
        [SerializeField] private string shareText = "Can you beat my score!!!";
        [SerializeField] private string shareSubject = "Share Via";
		[SerializeField] private string appUrl = "https://itunes.apple.com/us/app/rush-hoop/id1422945811?ls=1&mt=8";


        public string ScreenshotPath { private set; get; }
        public string AppUrl { private set; get; }

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

            AppUrl = appUrl;
        }

        /// <summary>
        /// Create the screenshot
        /// </summary>
        public void CreateScreenshot()
        {
            ScreenshotPath = Path.Combine(Application.persistentDataPath, screenshotName);
#if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot(ScreenshotPath);
#else
            ScreenCapture.CaptureScreenshot(screenshotName);
#endif
        }


        /// <summary>
        /// Share screenshot with text
        /// </summary>
        public void ShareScreenshotWithText()
        {
            Share(shareText, ScreenshotPath, appUrl, shareSubject);
        }

        private void Share(string shareText, string imagePath, string url, string subject)
        {
#if UNITY_ANDROID
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + imagePath);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            intentObject.Call<AndroidJavaObject>("setType", "image/png");

            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText + "  " + url);

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, subject);
            currentActivity.Call("startActivity", jChooser);
#elif UNITY_IOS

            CallSocialShareAdvanced(shareText, subject, url, imagePath);
#else
			Debug.Log("No sharing set up for this platform.");
#endif
        }


#if UNITY_IOS
    public struct ConfigStruct
    {
        public string title;
        public string message;
    }

    [DllImport("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);

    public struct SocialSharingStruct
    {
        public string text;
        public string url;
        public string image;
        public string subject;
    }

    [DllImport("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);

    public static void CallSocialShare(string title, string message)
    {
        ConfigStruct conf = new ConfigStruct();
        conf.title = title;
        conf.message = message;
        showAlertMessage(ref conf);
    }


    public static void CallSocialShareAdvanced(string defaultTxt, string subject, string url, string img)
    {
        SocialSharingStruct conf = new SocialSharingStruct();
        conf.text = defaultTxt;
        conf.url = url;
        conf.image = img;
        conf.subject = subject;

        showSocialSharing(ref conf);
    }
#endif
    }

}