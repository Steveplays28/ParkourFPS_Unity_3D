using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace SceneSaveAdvanced
{
	public class OpenSceneCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			if ( args.argc != 1 )
			{
				throw new System.ArgumentException( "Invalid argument count." );
			}

			Scene scene = EditorSceneManager.OpenScene( args.argv( 0 ) , OpenSceneMode.Additive );
			if ( !scene.IsValid() )
			{
				Debug.Log( Constants.c_LogBumper + "Could not open: " + args.argv( 0 ) );
				return false;
			}

			return true;
		}
	}
}