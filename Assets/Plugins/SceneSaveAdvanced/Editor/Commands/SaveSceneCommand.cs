using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace SceneSaveAdvanced
{
	public class SaveSceneCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			if ( args.argc != 2 )
			{
				throw new System.ArgumentException("Invalid argument count.");
			}
				
			#if ENABLE_DEBUG
			int sceneCount = EditorSceneManager.loadedSceneCount;
			for( int i = 0; i < sceneCount; i++)
			{
				Debug.Log( Constants.c_LogBumper + i.ToString() + " - " + EditorSceneManager.GetSceneAt(i).path );
			}
			#endif

			string path = args.argv(0);
			string newPath = FileUtils.GetUnixCorrectedPath( path.Substring( path.LastIndexOf( Constants.c_SaveFolder ) ) );

			Scene scene = EditorSceneManager.GetSceneByPath( newPath );
			if ( !scene.IsValid() )
			{
				Debug.Log( Constants.c_LogBumper + "Scene is invalid..." + newPath );
				return false;
			}

			SceneSaveAdvanced.Active = true;

			if ( EditorSceneManager.SaveScene( scene, args.argv(1), true ) )
			{
				SceneSaveAdvanced.Active = false;
				return true;
			}

			SceneSaveAdvanced.Active = false;

			Debug.Log(Constants.c_LogBumper + "Could not save: " + newPath + " to " + args.argv(1) );
			return false;
		}
	}
}