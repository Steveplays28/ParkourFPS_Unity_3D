using System.IO;

namespace SceneSaveAdvanced
{
	public class CopyFileCommand : ICommand
	{
		public bool Execute( Arguments args )
		{
			if ( args.argc != 2 )
			{
				throw new System.ArgumentException( "Invalid argument count." );
			}

			string[] arguments = args.args;

			try
			{ 
				File.Copy( arguments[0], arguments[1], true );
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
	}
}