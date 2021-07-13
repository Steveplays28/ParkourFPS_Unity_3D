using UnityEngine;
using UnityEditor.SceneManagement;

namespace SceneSaveAdvanced
{
	public class CloseSceneCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			if ( args.argc != 1 )
			{
				throw new System.ArgumentException( "Invalid argument count." );
			}

			if ( EditorSceneManager.CloseScene( EditorSceneManager.GetSceneByPath( args.argv( 0 ) ), true ) )
			{
				return true;
			}

			string path = FileUtils.GetRelativePath( args.argv( 0 ) );
			if ( EditorSceneManager.CloseScene( EditorSceneManager.GetSceneByPath( path ), true ) )
			{
				return true;
			}

			Debug.Log( Constants.c_LogBumper + "Could not close: " + args.argv( 0 ) );
			return false;
		}
	}
}