using System.Collections.Generic;

namespace SceneSaveAdvanced
{
	public class CommandBuffer
	{
		Queue<Command> m_commands = new Queue<Command>();

		public void AddCommand( Command cmd )
		{
			m_commands.Enqueue( cmd );
		}

		public void AddCommand<T>( Arguments args, Command.Schedule schedule = Command.Schedule.Immediate ) where T : ICommand, new()
		{
			Command cmd = new Command( new T(), args, schedule );
			AddCommand( cmd );
		}

		public void AddCommand<T>( string arg, Command.Schedule schedule = Command.Schedule.Immediate ) where T : ICommand, new()
		{
			Command cmd = new Command( new T(), arg, schedule );
			AddCommand( cmd );
		}

		public void AddCommand<T>( string arg, string arg2, Command.Schedule schedule = Command.Schedule.Immediate ) where T : ICommand, new()
		{
			Command cmd = new Command( new T(), arg, arg2, schedule );
			AddCommand( cmd );
		}

		public void Update()
		{
			int commandsThisFrame = 0;

			while( true )
			{
				if ( m_commands.Count == 0 )
				{
					return;
				}

				Command cmd = m_commands.Peek();
				if ( cmd.GetSchedule() == Command.Schedule.Deferred )
				{
					//If this is a deferred command and not the first command to run this frame, then wait until the next frame.
					if ( commandsThisFrame > 0 )
					{
						break;
					}
				}

				cmd = m_commands.Dequeue();
				cmd.Execute();

				commandsThisFrame++;
			}
		}
	}
}