using System;

namespace OptimalSchedulingLogic
{
    /// <summary>
    /// Represents a task for processing
    /// </summary>
    public class Task
    {
        #region Properties

        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Task name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Duration
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// Deadline
        /// </summary>
        public DateTime Deadline { get; }

        /// <summary>
        /// Extreme start time
        /// </summary>
        public DateTime ExtremeTime { get; }

        #endregion

        public Task(int id, string name, double duration, DateTime deadline)
        {
            Id = id;
            Name = name;
            Duration = duration;
            Deadline = deadline;
            ExtremeTime = Deadline.AddMinutes(-Duration);
        }
    }
}