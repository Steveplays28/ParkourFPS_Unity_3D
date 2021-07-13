using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace SceneSaveAdvanced
{
	public class PruneBackupsCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			if ( args.argc != 2 )
			{
				throw new System.ArgumentException( "Invalid argument count." );
			}

			string scenePath	= args.args[ 0 ];
			string backupCount	= args.args[ 1 ];

			int maxBackupCount = int.MaxValue;
			if ( !int.TryParse( backupCount, out maxBackupCount ) )
			{
				return false;
			}

			//Now get all the incremental builds...
			string savePath = FileUtils.GetSavePath( scenePath );
			if ( Directory.Exists( savePath ) )
			{ 
				//string [] files = Directory.GetFiles( savePath );
				int iteration = 0;
				var files = Directory.GetFiles( savePath ).OrderByDescending( f => new FileInfo(f).CreationTime );
				foreach( string file in files )
				{
					string searchString = Path.GetFileNameWithoutExtension( file ).Replace( Path.GetFileNameWithoutExtension( scenePath ), "" ).TrimStart( new char[] { '.' } );

					Regex reg = new Regex("^[a-z0-9]*$");
					if ( reg.IsMatch( searchString ) )
					{
						if ( iteration >= maxBackupCount )
						{
							//delete the file...
							try
							{
								File.Delete( file );
							}
							catch( System.Exception)
							{
								//Do nothing...
							}
						}
						iteration++;
					}
				}

				SceneSaveAdvanced.GetEventManager().FireEvent( "ListUpdated" );

				return true;
			}

			return false;
		}
	}
}