using System.Collections.Generic;

//TODO: move to an ICommand interface.
// public class OpenScene : ICommand
// { public void Execute() { do things.. }
//
// new Command<OpenScene>( args... )

namespace SceneSaveAdvanced
{
	public class Command
	{
		public enum Schedule
		{
			Immediate	= 0,
			Deferred	= 1		//Delay execution by one update cycle
		}

		private ICommand	m_command;
		private Schedule	m_schedule;
		private Arguments	m_args;

		public Command( ICommand command, Arguments args, Schedule schedule = Schedule.Immediate  )
		{
			m_command	= command;
			m_schedule	= schedule;
			m_args		= args;
		}

		public Command( ICommand command,  string arg, Schedule schedule = Schedule.Immediate )
		{
			m_command	= command;
			m_schedule	= schedule;
			m_args		= new Arguments( arg );
		}

		public Command( ICommand command, string arg, string arg2, Schedule schedule = Schedule.Immediate )
		{
			m_command	= command;
			m_schedule	= schedule;
			m_args		= new Arguments( arg, arg2 );
		}

		public Schedule GetSchedule()
		{
			return m_schedule;
		}

		public void Execute()
		{
			m_command.Execute( m_args );
		}
	}

}