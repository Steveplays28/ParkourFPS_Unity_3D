using System.Collections.Generic;

namespace SceneSaveAdvanced
{	
	public class Arguments
	{
		private List<string> m_args = new List<string>();

		public int argc
		{
			get { return m_args.Count; }
		}

		public string[] args
		{
			get { return m_args.ToArray(); }
		}

		public string argv( int index )
		{
			if ( index < 0 || index >= argc )
			{
				return string.Empty;
			}

			return m_args[index];
		}

		public Arguments( string arg )
		{
			m_args.Add( arg );
		}

		public Arguments( string arg, string arg2 )
		{
			m_args.Add( arg );
			m_args.Add( arg2 );
		}

	}
}