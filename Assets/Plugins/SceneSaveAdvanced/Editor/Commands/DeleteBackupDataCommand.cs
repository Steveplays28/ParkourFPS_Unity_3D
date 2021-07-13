using UnityEngine;
using System.IO;

namespace SceneSaveAdvanced
{
	public class DeleteBackupDataCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			try
			{ 
				string savePath = Path.Combine( Application.dataPath.Replace( Constants.s_UnityAssetsFolder, "" ), Constants.c_SaveFolder );

				Directory.Delete( savePath, true );
				SceneSaveAdvanced.GetEventManager().FireEvent( "ListUpdated" );
				Debug.Log( Constants.c_LogBumper + "Deleted all backup data." );
				return true;
			}
			catch ( System.Exception ) { }

			return false;
		}
	}
}