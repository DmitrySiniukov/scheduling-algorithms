using OptimalScheduling.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimalScheduling.Models
{
    /// <summary>
    /// Represents a schedule
    /// </summary>
    public class Schedule<T> : List<T> where T : MachineSchedule, new()
	{
		public bool OptimalityCriterion { get; set; }


		/// <summary>
		/// Default constructor
		/// </summary>
		public Schedule()
		{
		}

		/// <summary>
		/// Constructor based on machines
		/// </summary>
		/// <param name="machines"></param>
		public Schedule(IEnumerable<Machine> machines)
		{
			var instance = new T();
			foreach (var machine in machines)
			{
				Add(instance.Create(machine) as T);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineSchedules"></param>
		public Schedule(IEnumerable<T> machineSchedules)
		{
			AddRange(machineSchedules);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="tasks"></param>
		/// <param name="machines"></param>
		/// <returns></returns>
		public static Schedule<MachineSchedule> BuildSchedule(IEnumerable<Task> tasks, IEnumerable<Machine> machines)
		{
			#region Validate arguments

			if (tasks == null)
			{
				throw new ArgumentNullException();
			}

			if (machines == null)
			{
				throw new ArgumentNullException();
			}

			var tasksList = tasks.ToList();
			var machinesList = (machines as List<Machine>) ?? machines.ToList();

			if (tasksList.Count == 0 || machinesList.Count == 0)
			{
				return new Schedule<MachineSchedule>(machinesList);
			}

			#endregion
			
			tasksList.Sort((x, y) =>
			{
				var t = x.ExtremeTime.CompareTo(y.ExtremeTime);
				return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
			});

			var initSchedule = initialSchedule(tasksList, machinesList);

			// Initial schedule has been found
			if (initSchedule != null)
			{
				if (initSchedule.NextTaskIndex == tasksList.Count)
				{
					initSchedule.OptimalityCriterion = true;
					var r = initSchedule.Convert();
					r.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
					return r;
				}

				// Algorithm A1.1
				var success = true;
				var sortedSet = new SortedSet<InitialMachineSchedule>(initSchedule, new EndTimeComparer());
				for (var i = initSchedule.NextTaskIndex; i < tasksList.Count; ++i)
				{
					var firstMachine = sortedSet.Min;
					var currentTask = tasksList[i];
					var newEndTime = firstMachine.EndTime.AddMinutes(currentTask.Duration);
					if (newEndTime > currentTask.Deadline)
					{
						success = false;
						break;
					}

					sortedSet.Remove(firstMachine);
					firstMachine.Tasks.AddLast(currentTask);
					firstMachine.EndTime = firstMachine.EndTime.AddMinutes(currentTask.Duration);
					sortedSet.Add(firstMachine);
				}

				if (success)
				{
					var r = (new InitialSchedule(sortedSet).Convert());
					r.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
					return r;
				}
			}

			// Algorithm A2.1

			// Sort by (d, l)
			tasksList.Sort((x, y) =>
			{
				var t = x.Deadline.CompareTo(y.Deadline);
				return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
			});

			var machineSchedule = new Schedule<MachineSchedule>(machinesList);
			var n = tasksList.Count;

			// Building schedule
			var scheduleSet = new SortedSet<MachineSchedule>(machineSchedule, new StartTimeComparer());
			for (var i = n - 1; i >= 0; --i)
			{
				// Find unallowable with minimal start time
				var currentTask = tasksList[i];
				MachineSchedule targetMachine = null;
				foreach (var schedule in scheduleSet)
				{
					if (!(currentTask.Deadline > schedule.StartTime) &&
						(targetMachine == null || schedule.StartTime < targetMachine.StartTime))
					{
						targetMachine = schedule;
					}
				}

				// If founded
				if (targetMachine != null)
				{
					scheduleSet.Remove(targetMachine);
					targetMachine.Tasks.AddFirst(currentTask);
					targetMachine.StartTime = currentTask.ExtremeTime;
					scheduleSet.Add(targetMachine);
					continue;
				}

				// Else take machine with max start time, find allowable task with max duration
				var lastMachine = scheduleSet.Max;
				var longestTask = currentTask;
				var index = i;
				for (var j = i; j >= 0; --j)
				{
					if (!(tasksList[j].Deadline < lastMachine.StartTime) && tasksList[j].Duration > longestTask.Duration)
					{
						longestTask = tasksList[j];
						index = j;
					}
				}

				scheduleSet.Remove(lastMachine);
				lastMachine.Tasks.AddFirst(longestTask);
				lastMachine.StartTime = lastMachine.StartTime.AddMinutes(-longestTask.Duration);
				scheduleSet.Add(lastMachine);

				// Remove appointed task
				tasksList.RemoveAt(index);
			}

			var result = new Schedule<MachineSchedule>(scheduleSet);
			result.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

			return result;
		}

		/// <summary>
		/// Initial schedule
		/// </summary>
		/// <param name="tasks"></param>
		/// <param name="machines"></param>
		/// <returns></returns>
		private static InitialSchedule initialSchedule(List<Task> tasks, List<Machine> machines)
		{
			var schedule = new InitialSchedule(machines);

			var i = 0;
			var j = 0;
			var n = tasks.Count;
			var m = machines.Count;
			while (i < n && j < m)
			{
				var currentTask = tasks[i];

				int? target = null;
				for (var k = 0; k < j; ++k)
				{
					if (schedule[k].EndTime > currentTask.ExtremeTime)
					{
						continue;
					}

					if (target != null)
					{
						return null;
					}

					target = k;
				}

				++i;

				if (target != null)
				{
					var t = (int)target;
					schedule[t].Tasks.AddLast(currentTask);
					schedule[t].EndTime = schedule[t].EndTime.AddMinutes(currentTask.Duration);
					continue;
				}

				// engage next processor
				schedule[j].StartTime = currentTask.ExtremeTime;
				schedule[j].Tasks.AddLast(currentTask);
				schedule[j].EndTime = currentTask.Deadline;
				++j;
			}

			schedule.NextTaskIndex = i;

			return schedule;
		}

		public static Schedule<MachineSchedule> BuildOptimalSchedule(IEnumerable<Task> tasks, IEnumerable<Machine> machines)
		{
			var tasksList = tasks.ToList();
			var machinesList = machines.ToList();

			if (tasksList.Count == 0 || machinesList.Count == 0)
			{
				return new Schedule<MachineSchedule>();
			}

			tasksList.Sort((x, y) =>
			{
				var res = x.Deadline.CompareTo(y.Deadline);
				return res == 0 ? x.Duration.CompareTo(y.Duration) : res;
			});

			var m = machinesList.Count;
			var n = tasksList.Count;
			var queue = new List<BBNode>();
			BBNode record;

			#region Build initial queue

			var heuristicSchedule = BuildSchedule(tasksList, machinesList);

			// First element
			var startTimes = new DateTime[m];
			for (var i = 0; i < m; ++i)
			{
				startTimes[i] = DateTime.MaxValue;
			}
			var target = 0;
			for (var j = 0; j < heuristicSchedule.Count; ++j)
			{
				if (heuristicSchedule[j].Tasks.Any(x => x.Id == tasksList[n - 1].Id))
				{
					target = j;
					break;
				}
			}
			startTimes[target] = tasksList[n - 1].ExtremeTime;
			queue.Add(new BBNode(n - 1, target, null, startTimes));

			var forBranch = 0;
			for (var i = n - 2; i >= 0; --i)
			{
				var currentTask = tasksList[i];

				var node = queue[forBranch];
				queue.RemoveAt(forBranch);

				var nextMachine = 0;
				for (var j = 0; j < heuristicSchedule.Count; ++j)
				{
					if (heuristicSchedule[j].Tasks.Any(x => x.Id == currentTask.Id))
					{
						nextMachine = j;
						break;
					}
				}

				var engaged = node.StartTimes[nextMachine] == DateTime.MaxValue;
				var newPrevious = new PreviousNode
				{
					MachineIndex = node.MachineIndex,
					Previous = node.PreviousNode
				};

				// Copy the array of start times
				var newStartTimesForBranch = new DateTime[m];
				for (var k = 0; k < m; k++)
				{
					newStartTimesForBranch[k] = node.StartTimes[k];
				}

				newStartTimesForBranch[nextMachine] = !(currentTask.Deadline > newStartTimesForBranch[nextMachine])
					? currentTask.ExtremeTime
					: newStartTimesForBranch[nextMachine].AddMinutes(-currentTask.Duration);
				queue.Add(new BBNode(i, nextMachine, newPrevious, newStartTimesForBranch));
				forBranch = queue.Count - 1;

				for (var j = 0; j < m; ++j)
				{
					if (j != nextMachine)
					{
						if (node.StartTimes[j] == DateTime.MaxValue && engaged)
						{
							continue;
						}

						if (node.StartTimes[j] == DateTime.MaxValue)
						{
							engaged = true;
						}
						
						// Copy the array of start times
						var newStartTimes = new DateTime[m];
						for (var k = 0; k < m; k++)
						{
							newStartTimes[k] = node.StartTimes[k];
						}

						newStartTimes[j] = !(currentTask.Deadline > newStartTimes[j])
							? currentTask.ExtremeTime
							: newStartTimes[j].AddMinutes(-currentTask.Duration);
						queue.Add(new BBNode(i, j, newPrevious, newStartTimes));
					}
				}
			}

			// Find record
			record = queue[forBranch];
			queue.RemoveAt(forBranch);
			for (var i = queue.Count - 1; i >= 0 && queue[i].TaskIndex == 0; --i)
			{
				var current = queue[i];
				var temp = Compare(record.StartTimes, current.StartTimes, m);
				if (temp < 0)
				{
					record = current;
				}
				queue.RemoveAt(i);
			}

			// Clear the queue
			for (var i = queue.Count - 1; i >= 0; --i)
			{
				if (Compare(record.StartTimes, queue[i].StartTimes, m) < 0)
				{
					continue;
				}
				queue.RemoveAt(i);
			}

			#endregion

			while (queue.Count != 0)
			{
				var current = queue[queue.Count - 1];
				queue.RemoveAt(queue.Count - 1);

				var newPrevious = new PreviousNode
				{
					MachineIndex = current.MachineIndex,
					Previous = current.PreviousNode
				};

				var engaged = false;
				var currentTaskIndex = current.TaskIndex - 1;
				var currentTask = tasksList[currentTaskIndex];
				for (var j = 0; j < m; ++j)
				{
					if (current.StartTimes[j] == DateTime.MaxValue && engaged)
					{
						continue;
					}

					if (current.StartTimes[j] == DateTime.MaxValue)
					{
						engaged = true;
					}

					// Copy the array of start times
					var newStartTimes = new DateTime[m];
					for (var k = 0; k < m; k++)
					{
						newStartTimes[k] = current.StartTimes[k];
					}

					newStartTimes[j] = !(currentTask.Deadline > newStartTimes[j])
						? currentTask.ExtremeTime
						: newStartTimes[j].AddMinutes(-currentTask.Duration);

					var next = new BBNode(currentTaskIndex, j, newPrevious, newStartTimes);

					if (Compare(record.StartTimes, newStartTimes, m) >= 0)
					{
						continue;
					}

					// New record
					if (currentTaskIndex == 0)
					{
						record = next;
						// Clear the queue
						for (var i = queue.Count - 1; i >= 0; --i)
						{
							if (Compare(record.StartTimes, queue[i].StartTimes, m) < 0)
							{
								continue;
							}
							queue.RemoveAt(i);
						}
						continue;
					}
					
					queue.Add(next);
				}
			}

			var result = new Schedule<MachineSchedule>(machinesList);
			result[record.MachineIndex].Tasks.AddLast(tasksList[record.TaskIndex]);

			var previous = record.PreviousNode;
			var index = 1;
			while (previous != null)
			{
				result[previous.MachineIndex].Tasks.AddLast(tasksList[index]);
				previous = previous.Previous;
				++index;
			}

			for (var i = 0; i < m; i++)
			{
				result[i].StartTime = record.StartTimes[i];
			}

			result.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

			return result;
		}
		
		public static int Compare(DateTime[] first, DateTime[] second, int count)
		{
			var firstSorted = first.ToList();
			firstSorted.Sort();

			var secondSorted = second.ToList();
			secondSorted.Sort();

			for (var i = 0; i < count; i++)
			{
				if (firstSorted[i] < secondSorted[i])
				{
					return -1;
				}
				if (firstSorted[i] > secondSorted[i])
				{
					return 1;
				}
			}

			return 0;
		}


		/// <summary>
		/// Initial machine schedule
		/// </summary>
		private class InitialMachineSchedule : MachineSchedule
		{
			public DateTime EndTime { get; set; }


			public InitialMachineSchedule()
			{
				EndTime = DateTime.MinValue;
			}

			private InitialMachineSchedule(Machine machine) : base(machine)
			{
				EndTime = DateTime.MinValue;
			}


			public override MachineSchedule Create(Machine machine)
			{
				return new InitialMachineSchedule(machine);
			}
		}

		/// <summary>
		/// Initial schedule
		/// </summary>
		private class InitialSchedule : Schedule<InitialMachineSchedule>
		{
			/// <summary>
			/// 
			/// </summary>
			public int NextTaskIndex { get; set; }


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="machines"></param>
			public InitialSchedule(IEnumerable<Machine> machines) : base(machines)
			{
				OptimalityCriterion = true;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="machineSchedules"></param>
			public InitialSchedule(IEnumerable<Schedule<T>.InitialMachineSchedule> machineSchedules) : base(machineSchedules)
			{
				OptimalityCriterion = true;
			}


			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public Schedule<MachineSchedule> Convert()
			{
				var result = new Schedule<MachineSchedule>();
				result.AddRange(this.Cast<MachineSchedule>());
				result.OptimalityCriterion = OptimalityCriterion;
				return result;
			}
		}

		private class EndTimeComparer : IComparer<InitialMachineSchedule>
		{
			public int Compare(InitialMachineSchedule x, InitialMachineSchedule y)
			{
				return x.EndTime == y.EndTime ? x.Machine.Id.CompareTo(y.Machine.Id) : x.EndTime.CompareTo(y.EndTime);
			}
		}

		private class StartTimeComparer : IComparer<MachineSchedule>
		{
			public int Compare(MachineSchedule x, MachineSchedule y)
			{
				return x.StartTime == y.StartTime ? x.Machine.Id.CompareTo(y.Machine.Id) : x.StartTime.CompareTo(y.StartTime);
			}
		}

		private struct BBNode
		{
			public int TaskIndex { get; }

			public int MachineIndex { get; }

			public PreviousNode PreviousNode { get; }

			public DateTime[] StartTimes { get; }


			public BBNode(int taskIndex, int machineIndex, PreviousNode previous, DateTime[] startTimes)
			{
				TaskIndex = taskIndex;
				MachineIndex = machineIndex;
				PreviousNode = previous;
				StartTimes = startTimes;
			}
		}

		private class PreviousNode
		{
			public int MachineIndex { get; set; }

			public PreviousNode Previous { get; set; }
		}
	}
}