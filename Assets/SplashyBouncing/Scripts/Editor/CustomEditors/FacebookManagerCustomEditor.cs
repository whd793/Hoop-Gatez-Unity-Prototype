using System.Collections.Generic;
using UnityEditor;

namespace OnefallGames
{
    [CustomEditor(typeof(FacebookManager))]
    public class FacebookManagerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!ScriptingSymbolsHandler.NamespaceExists(NamespaceData.FacebookSDK))
            {
                EditorGUILayout.HelpBox("Please import Facebook SDK plugin for facebook sharing feature !!!", MessageType.Warning);
            }
            else
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                List<string> currentSymbols = new List<string>(symbolStr.Split(';'));
                if (!currentSymbols.Contains(ScriptingSymbolsData.FB_SDK))
                {
                    List<string> sbs = new List<string>();
                    sbs.Add(ScriptingSymbolsData.FB_SDK);
                    ScriptingSymbolsHandler.AddDefined_ScriptingSymbol(sbs.ToArray(), EditorUserBuildSettings.selectedBuildTargetGroup);
                }
            }
            base.OnInspectorGUI();
        }
    }
}
