#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
#define SUPPORTS_IDLE_DETECTION
#endif

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

namespace SceneSaveAdvanced
{
	[InitializeOnLoad]
	public class SceneSaveAdvanced
	{
		#region Private Variables
		// Have we loaded the prefs yet?
		private static bool m_prefsLoaded = false;
		private static EventManager s_eventManager;
#if SUPPORTS_IDLE_DETECTION
		private static Vector2 s_lastCursorPos = Vector2.zero;
		private static float s_idleTime = Time.realtimeSinceStartup;
#endif
#endregion

		#region Preference Variables
		private static bool m_enabled = true;
		private static bool m_backupSceneOnSave = true;
		private static bool m_limitSceneBackups = true;
		private static int m_sceneBackupLimit = 5;
		private static bool m_saveOnPlay = true;
		private static bool m_saveProject = true;
#if SUPPORTS_IDLE_DETECTION
		private static bool m_saveOnIdle = true;
		private static float m_idleTimout = 60;
#endif
		#endregion

		static SceneSaveAdvanced()
		{
			LoadPrefs();
			EditorApplication.update += Update;
			EditorSceneManager.sceneSaved += OnSceneSaved;
		}

		static CommandBuffer s_commandBuffer;
		public static CommandBuffer GetCommandBuffer()
		{
			if ( s_commandBuffer == null )
			{
				s_commandBuffer = new CommandBuffer();
			}
			return s_commandBuffer;
		}

		public static EventManager GetEventManager()
		{
			if ( s_eventManager == null )
			{
				s_eventManager = new EventManager();
			}

			return s_eventManager;
		}
		 
		static void Update()
		{
			if ( !m_enabled )
			{
				return;
			}

			if ( m_saveOnIdle )
			{
				CheckIdle();
			}

			if ( m_saveOnPlay )
			{ 
				if ( EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying )
				{
#if ENABLE_DEBUG
					Debug.Log(Constants.c_LogBumper + "Switching to play mode");
#endif
					SaveScenes();
				}
			}

			GetCommandBuffer().Update();
		}

		static void OnSceneSaved( Scene scene )
		{
			if( !m_backupSceneOnSave )
			{
				return;
			}

			BackupScene( scene.path );
		}

		static void CheckIdle()
		{
#if SUPPORTS_IDLE_DETECTION
			if ( EditorApplication.isPlaying )
			{
				UpdateIdleTime();
				return;
			}

			Vector2 pos = InputSupport.GetCursorPosition();

			if ( !Mathf.Approximately( pos.x, s_lastCursorPos.x ) || !Mathf.Approximately( pos.y, s_lastCursorPos.y ) )
			{
				UpdateIdleTime();
				s_lastCursorPos = pos;
			}

			if ( InputSupport.GetKeyboardPressed() )
			{
				UpdateIdleTime();
			}

			if ( ( Time.realtimeSinceStartup - s_idleTime ) > m_idleTimout )
			{
				UpdateIdleTime();
				SaveScenes();
			}
#endif
		}

		static void UpdateIdleTime()
		{
#if SUPPORTS_IDLE_DETECTION
			s_idleTime = Time.realtimeSinceStartup;
#endif
		}

		static bool s_Active = false;

		public static bool Active
		{
			get { return s_Active; }
			set { s_Active = value; }
		}

		private static void BackupScene( string path )
		{
			string strippedFilename = Path.GetFileNameWithoutExtension( path );
			string relativePath = path.Replace( Path.GetFileName( path ), "" ).Replace( Constants.s_UnityAssetsFolder, "" ).TrimStart( new char[] { '/', '\\' } ); //FIXME: windows specific?
			string strippedSavePath = Path.Combine( Application.dataPath.Replace( Constants.s_UnityAssetsFolder, "" ), Constants.c_SaveFolder );

			//Validate save path
			string newSavePath = Path.Combine( strippedSavePath, relativePath );
			if ( !Directory.Exists( newSavePath ) )
			{
				Directory.CreateDirectory( newSavePath );
			}

			int incNumber = 0;
			while ( true )
			{
				//Limit it to 4 chars for ease of human-readability. Allows for 36^4 uniques.
				string r = Guid.NewGuid().ToString().Substring(0, 4).ToLower();
				string rootFile = strippedFilename + "." + r + Constants.c_UnityFileExt;
				string filenameToTry = Path.Combine( newSavePath, rootFile );

				if ( File.Exists( filenameToTry ) )
				{
					incNumber++;

					//While it is unlikely that there would ever be 36^4 backups of a given scene, we need to account for the possibility.
					if ( incNumber > 1679616 ) //36^4
					{
						Debug.Log( Constants.c_LogBumper + "Failed to save scene backup! Please limit the Maximum Backup Count in the editor preferences." );
						return;
					}

					continue;
				}

				File.Copy( path, filenameToTry, true );
				break;
			}

			if ( m_sceneBackupLimit > 0 )
			{
				GetCommandBuffer().AddCommand<PruneBackupsCommand>( path, m_sceneBackupLimit.ToString() );
			}

			GetEventManager().FireEvent( "ListUpdated" );
		}

		private static void SaveScenes()
		{
			EditorSceneManager.SaveOpenScenes();

			if ( m_saveProject )
			{
				AssetDatabase.SaveAssets();
			}

			Debug.Log( Constants.c_LogBumper + "Scenes saved." );
		}

		private static void LoadPrefs()
		{
			if ( m_prefsLoaded )
			{
				return;
			}

			m_enabled				= EditorPrefs.GetBool( "SceneSaveAdvanced.Enabled", false );
			m_backupSceneOnSave		= EditorPrefs.GetBool( "SceneSaveAdvanced.BackupSceneOnSave", false );
			m_limitSceneBackups		= EditorPrefs.GetBool( "SceneSaveAdvanced.LimitBackups", false );
			m_sceneBackupLimit		= EditorPrefs.GetInt( "SceneSaveAdvanced.BackupLimit", 5 );
			m_saveOnPlay			= EditorPrefs.GetBool( "SceneSaveAdvanced.SaveOnPlay", false );
			m_saveProject			= EditorPrefs.GetBool( "SceneSaveAdvanced.SaveProject", false );

#if SUPPORTS_IDLE_DETECTION
			m_saveOnIdle			= EditorPrefs.GetBool( "SceneSaveAdvanced.SaveOnIdle", false );
			m_idleTimout			= EditorPrefs.GetFloat( "SceneSaveAdvanced.IdleTimeout", 60 );
#endif
			m_prefsLoaded = true;
		}

		private static void SavePrefs()
		{
			EditorPrefs.SetBool( "SceneSaveAdvanced.Enabled", m_enabled );
			EditorPrefs.SetBool( "SceneSaveAdvanced.BackupSceneOnSave", m_backupSceneOnSave );
			EditorPrefs.SetBool( "SceneSaveAdvanced.LimitBackups", m_limitSceneBackups );
			EditorPrefs.SetBool( "SceneSaveAdvanced.SaveOnPlay", m_saveOnPlay );
			EditorPrefs.SetBool( "SceneSaveAdvanced.SaveProject", m_saveProject );
#if SUPPORTS_IDLE_DETECTION
			EditorPrefs.SetBool( "SceneSaveAdvanced.SaveOnIdle", m_saveOnIdle );
			EditorPrefs.SetFloat( "SceneSaveAdvanced.IdleTimeout", m_idleTimout );
#endif
		}

#pragma warning disable CS0618 // Type or member is obsolete
        [PreferenceItem("Scene Save Advanced")]
#pragma warning restore CS0618 // Type or member is obsolete
        public static void PreferencesGUI()
		{
			LoadPrefs();

			// Preferences GUI
			m_enabled = EditorGUILayout.Toggle( new GUIContent( "Enable", "Enable " + Constants.c_Title ), m_enabled );

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField( "Automatic Save", EditorStyles.boldLabel );

			m_saveOnPlay = EditorGUILayout.Toggle( new GUIContent( "Save on Play", "Save the scene when starting Play Mode" ), m_saveOnPlay );

#if SUPPORTS_IDLE_DETECTION
			m_saveOnIdle = EditorGUILayout.Toggle( new GUIContent( "Save on Idle", "Save the scene when not using the mouse or keyboard."), m_saveOnIdle);

			if ( !m_saveOnIdle ) { GUI.enabled = false; }
			m_idleTimout = EditorGUILayout.Slider( "Idle Timer (seconds)", m_idleTimout, 10, 120 );
			if ( !m_saveOnIdle ) { GUI.enabled = true; }
#else
			bool _b = false;
			GUI.enabled = false;
			EditorGUILayout.Toggle( new GUIContent( "Save on Idle", "Not supported on this platform."), _b);
			EditorGUILayout.Slider( new GUIContent( "Idle Timer (seconds)", "Not supported on this platform."), 0, 10, 120 );
			GUI.enabled = true;
#endif

			if ( m_saveOnIdle || m_saveOnPlay )
			{
				m_saveProject = EditorGUILayout.Toggle( new GUIContent( "Save Project", "Save the project during idle save or save on play" ), m_saveProject );
			}
			else
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle( new GUIContent( "Save Project", "Save the project during idle save or save on play" ), m_saveProject );
				GUI.enabled = true;
			}

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField( "Scene Backup", EditorStyles.boldLabel );
			m_backupSceneOnSave = EditorGUILayout.Toggle( new GUIContent( "Enable", "Save a backup copy of the scene when the scene is saved." ), m_backupSceneOnSave);
			m_limitSceneBackups = EditorGUILayout.Toggle( new GUIContent( "Limit Backups", "Limit the number of backup copies to keep (per scene)." ), m_limitSceneBackups);

			if ( !m_limitSceneBackups ) { GUI.enabled = false; }
			m_sceneBackupLimit = EditorGUILayout.IntSlider( new GUIContent("Maximum Backup Count","The number of backup copies to keep."), m_sceneBackupLimit, 1, 20 );
			if ( !m_limitSceneBackups ) { GUI.enabled = true; }

			GUILayout.Space( 5.0f );

			GUILayout.BeginHorizontal();
			if ( GUILayout.Button( "Open Restore Scene Window") )
			{
				SceneRestoreEditorWindow.Open();
			}
			if ( GUILayout.Button( "Delete Backup Data" ) )
			{
				if ( EditorUtility.DisplayDialog( "Delete Backup Data", "Would you like to delete all backup data?", "Ok", "Cancel" ) )
				{
					GetCommandBuffer().AddCommand<DeleteBackupDataCommand>( string.Empty );
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space( 105.0f );

			Rect r = EditorGUILayout.BeginHorizontal();
			r.x = 200.0f;
			r.width = 400.0f;
			r.height = 25.0f;
			GUI.color = Color.grey;
			GUI.Label( r, "" + Constants.c_Title + " " + Constants.c_Version );
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal();

			// Save the preferences
			if ( GUI.changed )
			{
				SavePrefs();
				UpdateIdleTime();
			}
		}
	}
}