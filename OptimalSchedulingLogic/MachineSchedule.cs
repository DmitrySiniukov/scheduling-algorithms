using System;
using System.Collections.Generic;

namespace OptimalSchedulingLogic
{
    public class MachineSchedule : ICloneable, IComparable<MachineSchedule>
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

        #region Constructors

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

        #endregion

        #region Implementations

        public object Clone()
        {
            return new MachineSchedule(Machine, StartTime, new LinkedList<Task>(Tasks));
        }

        public int CompareTo(MachineSchedule other)
        {
            if (other == null)
            {
                return 1;
            }

            return StartTime == other.StartTime
                ? Machine.Id.CompareTo(other.Machine.Id)
                : StartTime.CompareTo(other.StartTime);
        }

        #endregion
    }
}