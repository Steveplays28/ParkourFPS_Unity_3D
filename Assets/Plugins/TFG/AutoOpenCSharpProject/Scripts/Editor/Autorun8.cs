using System.IO;
using UnityEditor;
using UnityEngine;

namespace Com.TheFallenGames.DBD.AutoOpenCSharpProject
{
    [InitializeOnLoad]
	class Autorun8
	{
		const string PREF_AUTO_LAUNCH_SCRIPT_EDITOR = "pref_frame8_auto_launch_script_editor_on_unity_start";
		const string PREF_LAST_TMP_FILE = "pref_frame8_auto_launch_script_editor_on_unity_start__last_tmp_file";

		/// <summary>Increase this if your project loads very slow</summary>
		const int MAX_ALLOWED_SECONDS_SINCE_EDITOR_STARTED = 600;


		[MenuItem("Tools/Auto-Open C# project/On")]
		static void AutoOpenONClicked()
		{
			EditorPrefs.SetBool(PREF_AUTO_LAUNCH_SCRIPT_EDITOR, true);
		}

		[MenuItem("Tools/Auto-Open C# project/On", isValidateFunction: true)]
		static bool AutoOpenON_Validate()
		{
			return !EditorPrefs.GetBool(PREF_AUTO_LAUNCH_SCRIPT_EDITOR, false);
		}

		[MenuItem("Tools/Auto-Open C# project/Off")]
		static void AutoOpenOFFClicked()
		{
			EditorPrefs.SetBool(PREF_AUTO_LAUNCH_SCRIPT_EDITOR, false);
		}

		[MenuItem("Tools/Auto-Open C# project/Off", isValidateFunction: true)]
		static bool AutoOpenOFF_Validate()
		{
			return EditorPrefs.GetBool(PREF_AUTO_LAUNCH_SCRIPT_EDITOR, false);
		}

		static void SubscribeToQuit()
		{
#if UNITY_2018_1_OR_NEWER
			EditorApplication.quitting += EditorApplication_quitting;
#else
			string err;
			var fieldInfo = GetOldFieldByReflection(out err);
			if (fieldInfo == null)
			{
				Debug.Log(err);
				return;
			}
			var unityAction = fieldInfo.GetValue(null) as UnityAction;
			var subscriberUnityAction = new UnityAction(EditorApplication_quitting);
			if (unityAction == null)
			{
				fieldInfo.SetValue(null, subscriberUnityAction);
			}
			else
				unityAction += subscriberUnityAction;
#endif
		}

		static void UnsubscribeFromQuit()
		{
#if UNITY_2018_1_OR_NEWER
			EditorApplication.quitting -= EditorApplication_quitting;
#else
			string err;
			var fieldInfo = GetOldFieldByReflection(out err);
			// Not possible, because we didn't subscribe to event if it didn't exist in the first place, so this method wouldn't be called
			//if (fieldInfo == null)
			//{
			//	Debug.Log(err);
			//	return;
			//}
			var unityAction = fieldInfo.GetValue(null) as UnityAction;
			var subscriberUnityAction = new UnityAction(EditorApplication_quitting);
			if (unityAction == null)
			{
				// Already set to null. Good
			}
			else
				unityAction -= subscriberUnityAction;
#endif
		}

#if !UNITY_2018_1_OR_NEWER
		static System.Reflection.FieldInfo GetOldFieldByReflection(out string error)
		{
			var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
			var fieldInfo = typeof(EditorApplication).GetField("editorApplicationQuit", flags);
			if (fieldInfo == null)
			{
				fieldInfo = typeof(EditorApplication).GetField("quitting", flags);
				if (fieldInfo == null)
				{
					error = "[AutoOpenCSharpProject] Couldn't get editorApplicationQuit delegate in EditorApplication class. Please report this bug (contact@thefallengames.com). Unity Version: " + Application.unityVersion;
					return null;
				}
			}
			if (fieldInfo.FieldType != typeof(UnityAction))
			{

				error = "[AutoOpenCSharpProject] Got " + fieldInfo.Name + " field in EditorApplication class, but it's of type " + fieldInfo.FieldType + ". Please report this bug (contact@thefallengames.com). Unity Version: " + Application.unityVersion;
				return null;
			}

			error = null;
			return fieldInfo;
		}
#endif

		static Autorun8()
		{
			SubscribeToQuit();
			var prevTMPFile = EditorPrefs.GetString(PREF_LAST_TMP_FILE, "");
			if (prevTMPFile == "")
			{
				var uniquePath = FileUtil.GetUniqueTempPathInProject();
				File.WriteAllText(uniquePath, "");
				EditorPrefs.SetString(PREF_LAST_TMP_FILE, uniquePath);
			}
			else
			{
				// File already exists => the editor was already opened in this session or, if this session has just started, it means unity has crashed previously
				if (File.Exists(prevTMPFile))
				{
					if (EditorApplication.timeSinceStartup <= MAX_ALLOWED_SECONDS_SINCE_EDITOR_STARTED)
					{

						// This may be the rare case where unity crashes, and the prev temp file isn't deleted => continue
					}
					else
						// Session start already processed for sure
						return;
				}
				else
					File.WriteAllText(prevTMPFile, "");
			}

			// Another layer of checks, which happens in some unity versions (or in all), sometimes
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			EditorApplication.update += RunOnceAtEditorLaunch;
		}

		static void EditorApplication_quitting()
		{
			UnsubscribeFromQuit();

			var prevTMPFile = EditorPrefs.GetString(PREF_LAST_TMP_FILE, "");
			try
			{
				// Deleting the previously created tmp file so on the next launch a new one will be created
				if (prevTMPFile != "")
				{
					if (File.Exists(prevTMPFile))
						File.Delete(prevTMPFile);
					EditorPrefs.DeleteKey(PREF_LAST_TMP_FILE);
				}
			}
			catch { }
		}

		static void RunOnceAtEditorLaunch()
		{
			EditorApplication.update -= RunOnceAtEditorLaunch;
			if (EditorPrefs.GetBool(PREF_AUTO_LAUNCH_SCRIPT_EDITOR, false))
			{
				string assetsDirName = "Assets";
				string tfgDirName = "TFG";
				string tfgDirPath = assetsDirName + "/" + tfgDirName;
				string pluginName = "AutoOpenCSharpProject";
				string fpRelativeToPluginName = "Scripts/Editor/Welcome8.txt";
				string expectedFPRelativeToAssetsDir = tfgDirName + "/" + pluginName + "/" + fpRelativeToPluginName;

				if (!Directory.Exists(assetsDirName))
				{
					Debug.Log("Assets folder doesn't exist? Aborting...");
					return;
				}

				string fpToUse = null;
				fpToUse = assetsDirName + "/" + expectedFPRelativeToAssetsDir;
				if (!Directory.Exists(tfgDirPath))
				{
					// User moved the plugin's folder in a sub-folder of the Assets folder
					foreach (var dirInAssets in Directory.GetDirectories(assetsDirName))
					{
						var directChildrenOfDirInAssets = Directory.GetDirectories(dirInAssets, tfgDirName, SearchOption.TopDirectoryOnly);
						if (directChildrenOfDirInAssets.Length == 0)
							continue;

						var subDirsInTFGDir = Directory.GetDirectories(directChildrenOfDirInAssets[0], pluginName, SearchOption.TopDirectoryOnly);
						if (subDirsInTFGDir.Length == 0)
							continue;

						fpToUse = dirInAssets + "/" + expectedFPRelativeToAssetsDir;
					}
				}

				if (!File.Exists(fpToUse))
				{
					string dirName = Path.GetDirectoryName(fpToUse);
					if (!Directory.Exists(dirName))
						Directory.CreateDirectory(dirName);

					File.WriteAllText(fpToUse, "");
				}


				AssetDatabase.ImportAsset(fpToUse, ImportAssetOptions.ForceUpdate);
				var asset = AssetDatabase.LoadAssetAtPath(fpToUse, typeof(TextAsset)) as TextAsset;
				AssetDatabase.OpenAsset(asset);
			}
		}
	}
}
