using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace SceneSaveAdvanced
{ 
	public class SceneRestoreEditorWindow : EditorWindow, IEventListener
	{
		#region Private Variables
		private int m_sceneHashes = 0;
		private bool m_rebuildSceneList = true;
		private Vector2 m_scrollPosition = Vector2.zero;
		private List<SceneEntry> m_entries = new List<SceneEntry>();
		#endregion

		[MenuItem("Window/Scene Save Advanced/Open Restore Scene Window")]
		public static void Open()
		{
			SceneRestoreEditorWindow window = ( SceneRestoreEditorWindow )EditorWindow.GetWindow( typeof( SceneRestoreEditorWindow ) );
			window.titleContent = new GUIContent( "Restore Scene" );
			window.minSize = new Vector2( 600.0f, 300.0f );
			window.Show();
		}

		public void OnEvent( string evt )
		{
			if ( evt.CompareTo("ListUpdated") == 0 )
			{
				m_rebuildSceneList = true;
			}
		}

		private void Awake()
		{
			SceneSaveAdvanced.GetEventManager().Subscribe( this );
		}

		private void OnDestroy()
		{
			SceneSaveAdvanced.GetEventManager().Unsubscribe( this );
		}

		void BuildEntries()
		{
			m_entries.Clear();

			int count = EditorSceneManager.sceneCount;
			for ( int i = 0; i < count; i++ )
			{
				Scene scene = EditorSceneManager.GetSceneAt( i );

				if ( scene.path.Length == 0 )
				{
					continue;
				}

				SceneEntry entry = new SceneEntry( scene.path );

				//Now get all the incremental builds...
				string savePath = FileUtils.GetSavePath( scene.path );
				if ( Directory.Exists( savePath ) )
				{ 
					var files = Directory.GetFiles( savePath ).OrderByDescending( f => new FileInfo(f).CreationTime );

					foreach( string file in files )
					{
						string searchString = Path.GetFileNameWithoutExtension( file ).Replace( Path.GetFileNameWithoutExtension( scene.path ), "" ).TrimStart( new char[] { '.' } );

						Regex reg = new Regex("^[a-z0-9]*$");
						if ( reg.IsMatch( searchString ) )
						{
							DateTime dateTime = File.GetLastWriteTime( file );

							string relativeFile = FileUtils.GetRelativePath( file );

							entry.AddScene( relativeFile, dateTime );
						}
					}
				}
				m_entries.Add( entry ); 
			}

			Repaint();
		}

		private void Update()
		{
			if ( EditorApplication.isCompiling )
			{
				Close();
				return;
			}

			if ( m_rebuildSceneList )
			{
				m_rebuildSceneList = false;
				BuildEntries();
				return;
			}

			if ( EditorApplication.isCompiling )
			{
				m_rebuildSceneList = true;
				return;
			}

			int sceneHashes = 0;
			for( int i = 0; i < EditorSceneManager.sceneCount; i++ )
			{
				Scene curScene = EditorSceneManager.GetSceneAt( i );
				sceneHashes += curScene.name.GetHashCode();
			}

			if ( sceneHashes != m_sceneHashes )
			{
				m_sceneHashes = sceneHashes;
				m_rebuildSceneList = true;
				return;	
			}
		}

		private void OnGUI()
		{
			m_scrollPosition = EditorGUILayout.BeginScrollView( m_scrollPosition );

			for( int i = 0; i < m_entries.Count; i++ )
			{
				m_entries[i].Show = EditorGUILayout.Foldout( m_entries[i].Show, m_entries[i].Path );
				if ( m_entries[i].Show )
				{
					string[] scenes = m_entries[i].Scenes;
					DateTime[] timestamps = m_entries[i].SceneTimestamps;

					for ( int j = 0; j < scenes.Length; j++ )
					{
						Rect r = EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField( "    " + FileUtils.GetPlatformCorrectedPath( scenes[j] ) );
						EditorGUILayout.LabelField( timestamps[j].ToString() );
						if ( GUI.Button( new Rect( ( r.x + r.width ) - 100, r.y, 98, r.height ), "Restore" ) )
						{
							string messageTitle = Path.GetFileName( scenes[j] ) + "?\n" + timestamps[j].ToString();
							if ( EditorUtility.DisplayDialog( "Restore Scene", "Would you like to restore " + messageTitle , "Ok", "Cancel" ) )
							{ 
								SceneSaveAdvanced.GetCommandBuffer().AddCommand<OpenSceneCommand>( scenes[j] );
								SceneSaveAdvanced.GetCommandBuffer().AddCommand<CloseSceneCommand>( m_entries[i].Path );
								SceneSaveAdvanced.GetCommandBuffer().AddCommand<SaveSceneCommand>( FileUtils.GetPlatformCorrectedPath( scenes[j] ), m_entries[i].Path, Command.Schedule.Deferred );
								SceneSaveAdvanced.GetCommandBuffer().AddCommand<OpenSceneCommand>( m_entries[i].Path );
								SceneSaveAdvanced.GetCommandBuffer().AddCommand<CloseSceneCommand>( scenes[j] );
							}
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}
	}
}