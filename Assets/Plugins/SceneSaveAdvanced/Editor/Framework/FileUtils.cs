using System.IO;
using UnityEngine;

namespace SceneSaveAdvanced
{
	public class FileUtils
	{
		public static string GetSavePath( string path )
		{ 
			string relativePath = path.Replace( Path.GetFileName( path ), "" ).Replace( Constants.s_UnityAssetsFolder, "" ).TrimStart( new char[] { '/', '\\' } );
			string strippedSavePath = Path.Combine( Application.dataPath.Replace( Constants.s_UnityAssetsFolder, "" ), Constants.c_SaveFolder );
			return Path.Combine( strippedSavePath, relativePath );
		}

		public static string GetRelativePath( string path )
		{
			return FileUtils.GetUnixCorrectedPath( path.Substring( path.LastIndexOf( Constants.c_SaveFolder ) ) );
		}

		public static string GetPlatformCorrectedPath( string path )
		{
			switch( Application.platform )
			{
				case RuntimePlatform.WindowsEditor:
					return path.Replace( "/", "\\" );
				default:
					break;
			}

			return path.Replace( "\\", "/" );
		}

		public static string GetUnixCorrectedPath( string path )
		{
			return path.Replace( "\\", "/" );
		}
	}
}