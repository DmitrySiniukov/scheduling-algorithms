using System;

namespace OptimalScheduling.Algorithm
{
	/// <summary>
	/// Represents a task for processing
	/// </summary>
	public class Task
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; set; }

		/// <summary>
		/// Task name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Duration
		/// </summary>
		public double Duration { get; set; }
		
        /// <summary>
        /// Deadline
        /// </summary>
        public DateTime Deadline { get; set; }

		/// <summary>
		/// Extreme start time
		/// </summary>
		public DateTime ExtremeTime
		{
			get { return Deadline.AddMinutes(-Duration); }
		}


		public Task(int id, string name, double duration, DateTime deadline)
        {
	        Id = id;
			Name = name;
			Duration = duration;
            Deadline = deadline;
        }
    }
}