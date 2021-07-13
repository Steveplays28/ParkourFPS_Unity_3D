using System;
using System.Collections.Generic;

namespace SceneSaveAdvanced
{
	public class SceneEntry
	{
		string m_basePath;
		bool m_show = false;
		List<string> m_sceneSaves = new List<string>();
		List<DateTime> m_sceneTimestamps = new List<DateTime>();

		public SceneEntry( string _basePath )
		{
			m_basePath = _basePath;
		}

		public string Path
		{
			get { return m_basePath; }
		}
		public bool Show
		{
			set { m_show = value; }
			get { return m_show; }
		}

		public void AddScene( string path, DateTime timestamp )
		{
			m_sceneSaves.Add( path );
			m_sceneTimestamps.Add( timestamp );
		}

		public string[] Scenes
		{
			get
			{
				return m_sceneSaves.ToArray();
			}
		}

		public DateTime[] SceneTimestamps
		{
			get
			{
				return m_sceneTimestamps.ToArray();
			}
		}
	}
}