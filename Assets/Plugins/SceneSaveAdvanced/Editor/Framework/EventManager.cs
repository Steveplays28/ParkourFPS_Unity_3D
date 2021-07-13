using System.Collections.Generic;

namespace SceneSaveAdvanced
{
	public class EventManager
	{
		List<IEventListener> m_listners = new List<IEventListener>();

		public void Subscribe( IEventListener evtListener )
		{
			try
			{ 
				m_listners.Add( evtListener );
			}
			catch ( System.Exception )
			{ }
		}

		public void Unsubscribe( IEventListener evtListener )
		{
			try
			{ 
				m_listners.Remove( evtListener );
			}
			catch ( System.Exception )
			{ }
		}

		public void FireEvent( string evt )
		{
			for ( int i = 0; i < m_listners.Count; i++ )
			{
				try
				{ 
					m_listners[i].OnEvent( evt );
				}
				catch( System.Exception )
				{
				}
			}
		}
	}
}