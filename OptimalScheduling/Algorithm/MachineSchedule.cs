using System;
using System.Collections.Generic;

namespace OptimalScheduling.Algorithm
{
	public class MachineSchedule : ICloneable
	{
		/// <summary>
		/// Machine
		/// </summary>
		public Machine Machine { get; }

		/// <summary>
		/// The time of launch
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Tasks
		/// </summary>
		public LinkedList<Task> Tasks { get; }


		/// <summary>
		/// Full constructor
		/// </summary>
		/// <param name="machine"></param>
		/// <param name="startTime"></param>
		/// <param name="tasks"></param>
		public MachineSchedule(Machine machine, DateTime startTime, LinkedList<Task> tasks)
		{
			Machine = machine;
			StartTime = startTime;
			Tasks = tasks;
		}

		/// <summary>
		/// Machine constructor
		/// </summary>
		/// <param name="machine"></param>
		public MachineSchedule(Machine machine)
		: this(machine, DateTime.MaxValue, new LinkedList<Task>())
		{
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public MachineSchedule()
		: this(null, DateTime.MaxValue, new LinkedList<Task>())
		{
		}

	    public object Clone()
	    {
	        return new MachineSchedule(Machine, StartTime, new LinkedList<Task>(Tasks));
	    }
	}
}