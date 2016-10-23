using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimalScheduling.Algorithm
{
    /// <summary>
    /// Represents a schedule
    /// </summary>
    public class Schedule: List<MachineSchedule>, IComparable<Schedule>, ICloneable
	{
		public bool OptimalityCriterion { get; private set; }

        #region Constructors

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
			foreach (var machine in machines)
			{
			    Add(new MachineSchedule(machine));
			}
		}

        /// <summary>
        /// Constructor based on machine schedules
        /// </summary>
        /// <param name="machineSchedules"></param>
        public Schedule(IEnumerable<MachineSchedule> machineSchedules)
		{
			AddRange(machineSchedules);
		}

        private Schedule(SortedSet<MachineScheduleExt> machineSchedules)
        {
            foreach (var machineSchedule in machineSchedules)
            {
                Add(machineSchedule.Schedule);
            }
        }

        #endregion

        #region Implementations

        /// <summary>
        /// Compare unsorted schedules
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public int CompareTo(Schedule second)
        {
            if (second == null)
            {
                return 1;
            }

            if (second.Count != Count)
            {
                throw new ArgumentException("The schedules must have the same number of machines.");
            }
            
            var firstComparedIndexes = new List<int>();
            var secondComparedIndexes = new List<int>();
            for (var i = 0; i < Count; i++)
            {
                var firstMinTimeIndex = -1;
                var firstMinTime = DateTime.MaxValue;
                for (var j = 0; j < Count; j++)
                {
                    var current = this[j];
                    if ((firstMinTimeIndex < 0 || current.StartTime < firstMinTime) && !firstComparedIndexes.Contains(j))
                    {
                        firstMinTimeIndex = j;
                        firstMinTime = current.StartTime;
                    }
                }

                var secondMinTimeIndex = -1;
                var secondMinTime = DateTime.MaxValue;
                for (var j = 0; j < Count; j++)
                {
                    var current = second[j];
                    if ((secondMinTimeIndex < 0 || current.StartTime < secondMinTime) && !secondComparedIndexes.Contains(j))
                    {
                        secondMinTimeIndex = j;
                        secondMinTime = current.StartTime;
                    }
                }

                if (firstMinTime < secondMinTime)
                {
                    return -1;
                }

                if (firstMinTime < secondMinTime)
                {
                    return 1;
                }

                firstComparedIndexes.Add(firstMinTimeIndex);
                secondComparedIndexes.Add(secondMinTimeIndex);
            }

            return 0;
        }
        
        public object Clone()
        {
            var newSchedule = new Schedule();
            foreach (var machineSchedule in this)
            {
                var newMachineSchedule = machineSchedule.Clone();
                newSchedule.Add((MachineSchedule)newMachineSchedule);
            }
            newSchedule.OptimalityCriterion = OptimalityCriterion;
            
            return newSchedule;
        }

        #endregion

        /// <summary>
        /// Compare sorted schedules
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public int CompareToSorted(Schedule second)
        {
            if (second == null)
            {
                return 1;
            }

            if (second.Count != Count)
            {
                throw new ArgumentException("The schedules must have the same number of machines.");
            }

            for (var i = 0; i < Count; i++)
            {
                if (this[i].StartTime < second[i].StartTime)
                {
                    return -1;
                }
                if (this[i].StartTime > second[i].StartTime)
                {
                    return 1;
                }
            }

            return 0;
        }

        #region Schedule builders

        public static Schedule BuildSchedule(IEnumerable<Task> tasks, IEnumerable<Machine> machines)
		{
			var tasksList = tasks.ToList();
			var machinesList = (machines as List<Machine>) ?? machines.ToList();

			if (tasksList.Count == 0 || machinesList.Count == 0)
			{
				return new Schedule(machinesList);
			}
            
			// Sort by (d, l)
			tasksList.Sort((x, y) =>
			{
				var t = x.Deadline.CompareTo(y.Deadline);
				return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
			});

			var machineSchedule = new Schedule(machinesList);
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

			var result = new Schedule(scheduleSet);
			result.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

			return result;
		}

        public static Schedule BuildWithPrimaryAlgorithm(IEnumerable<Task> tasks, IEnumerable<Machine> machines)
        {
            var tasksList = new List<Task>(tasks);
            var machinesList = (machines as List<Machine>) ?? machines.ToList();

            if (tasksList.Count == 0 || machinesList.Count == 0)
            {
                return new Schedule(machinesList);
            }

            tasksList.Sort((x, y) =>
            {
                var t = x.ExtremeTime.CompareTo(y.ExtremeTime);
                return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
            });

            var schedule_A11 = new Schedule(machinesList);
            int nextTaskIndex;
            bool firstCriterion;
            var initSchedule = primaryInitialSchedule(schedule_A11, tasksList, out nextTaskIndex, out firstCriterion);

            // Initial schedule has been found
            if (initSchedule != null)
            {
                if (nextTaskIndex == tasksList.Count)
                {
                    schedule_A11.OptimalityCriterion = true;
                    schedule_A11.Sort((x, y) =>
                    {
                        var stResult = x.StartTime.CompareTo(y.StartTime);
                        return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
                    });
                    return schedule_A11;
                }

                var remainingTasks = new List<Task>();
                for (var i = nextTaskIndex; i < tasksList.Count; i++)
                {
                    remainingTasks.Add(tasksList[i]);
                }
                
                var success_A11 = true;
                var success_A12 = false;
                var success_A12a = false;
                var success_A13 = false;

                Schedule schedule_A12 = null;
                Schedule schedule_A12a = null;
                Schedule schedule_A13 = null;

                if (firstCriterion)
                {
                    schedule_A12 = (Schedule) schedule_A11.Clone();
                    schedule_A12a = (Schedule)schedule_A11.Clone();
                    schedule_A13 = (Schedule)schedule_A11.Clone();

                    // build schedules

                }

                #region Algorithm A1.1

                foreach (var currentTask in remainingTasks)
                {
                    var firstMachine = initSchedule.Min;
                    var newEndTime = firstMachine.EndTime.AddMinutes(currentTask.Duration);
                    if (newEndTime > currentTask.Deadline)
                    {
                        success_A11 = false;
                        break;
                    }

                    initSchedule.Remove(firstMachine);
                    firstMachine.Schedule.Tasks.AddLast(currentTask);
                    firstMachine.EndTime = newEndTime;
                    initSchedule.Add(firstMachine);
                }

                #endregion
                
                Schedule optimalSchedule = null;

                if (success_A11)
                {
                    schedule_A11.Sort((x, y) =>
                    {
                        var stResult = x.StartTime.CompareTo(y.StartTime);
                        return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
                    });
                    optimalSchedule = schedule_A11;
                    //TODO: add statistics
                }
                if (success_A12)
                {
                    schedule_A12.Sort((x, y) =>
                    {
                        var stResult = x.StartTime.CompareTo(y.StartTime);
                        return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
                    });
                    optimalSchedule = schedule_A12;
                    //TODO: add statistics
                }
                if (success_A12a)
                {
                    schedule_A12a.Sort((x, y) =>
                    {
                        var stResult = x.StartTime.CompareTo(y.StartTime);
                        return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
                    });
                    optimalSchedule = schedule_A12a;
                    //TODO: add statistics
                }
                if (success_A13)
                {
                    schedule_A13.Sort((x, y) =>
                    {
                        var stResult = x.StartTime.CompareTo(y.StartTime);
                        return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
                    });
                    optimalSchedule = schedule_A13;
                    //TODO: add statistics
                }

                if (optimalSchedule != null)
                {
                    optimalSchedule.OptimalityCriterion = true;
                    return optimalSchedule;
                }
            }

            var schedule_A21 = mainHeuristicAlgorithm(tasksList, machinesList, null);
            var schedule_A21a = mainHeuristicAlgorithm(tasksList, machinesList, new TimeSpan(0, 8, 0));
            var schedule_A22 = secondHeuristicAlgorithm(tasksList, machinesList);
            var schedule_A23 = thirdHeuristicAlgorithm(tasksList, machinesList);
            
            schedule_A21.Sort((x, y) =>
            {
                var stResult = x.StartTime.CompareTo(y.StartTime);
                return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
            });

            schedule_A21a.Sort((x, y) =>
            {
                var stResult = x.StartTime.CompareTo(y.StartTime);
                return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
            });

            schedule_A22.Sort((x, y) =>
            {
                var stResult = x.StartTime.CompareTo(y.StartTime);
                return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
            });

            schedule_A23.Sort((x, y) =>
            {
                var stResult = x.StartTime.CompareTo(y.StartTime);
                return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
            });

            var schedules = new List<Schedule> {schedule_A21, schedule_A21a, schedule_A22, schedule_A23};
            schedules.Sort((x, y) => x.CompareToSorted(y));

            // add info

            return schedules[schedules.Count - 1];
        }

        public static Schedule BuildOptimalSchedule(IEnumerable<Task> tasks, IEnumerable<Machine> machines)
        {
            var tasksList = tasks.ToList();
            var machinesList = machines.ToList();

            if (tasksList.Count == 0 || machinesList.Count == 0)
            {
                return new Schedule();
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
                var temp = compare(record.StartTimes, current.StartTimes, m);
                if (temp < 0)
                {
                    record = current;
                }
                queue.RemoveAt(i);
            }

            // Clear the queue
            for (var i = queue.Count - 1; i >= 0; --i)
            {
                if (compare(record.StartTimes, queue[i].StartTimes, m) < 0)
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

                    if (compare(record.StartTimes, newStartTimes, m) >= 0)
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
                            if (compare(record.StartTimes, queue[i].StartTimes, m) < 0)
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

            var result = new Schedule(machinesList);
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

            result.Sort((x, y) =>
            {
                var stResult = x.StartTime.CompareTo(y.StartTime);
                return stResult == 0 ? x.Machine.Id.CompareTo(y.Machine.Id) : stResult;
            });

            return result;
        }

        #endregion

        /// <summary>
        /// Build initial schedule with the primary algorithm
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="machines"></param>
        /// <param name="nextTaskIndex"></param>
        /// <param name="endTimes"></param>
        /// <returns></returns>
        private static SortedSet<MachineScheduleExt> primaryInitialSchedule(Schedule schedule, List<Task> tasks, out int nextTaskIndex,
            out bool firstCriterion)
        {
            nextTaskIndex = 0;
            firstCriterion = true;
            
            var machineSchedules = new SortedSet<MachineScheduleExt>();

            // Engage one machine and appoint first task
            var firstTask = tasks[0];
            var engagedMachine = new MachineScheduleExt(schedule[0]);
            engagedMachine.Schedule.Tasks.AddLast(firstTask);
            engagedMachine.Schedule.StartTime = firstTask.ExtremeTime;
            engagedMachine.EndTime = firstTask.Deadline;
            machineSchedules.Add(engagedMachine);
            
            var i = 1;
            var j = 1;
            while (j < schedule.Count && i < tasks.Count)
            {
                var currentTask = tasks[i];

                // Check di-Ci < lp
                foreach (var machineSchedule in machineSchedules)
                {
                    var currentEndTime = machineSchedule.Schedule.StartTime;
                    foreach (var task in machineSchedule.Schedule.Tasks)
                    {
                        currentEndTime = currentEndTime.AddMinutes(task.Duration);
                        if (!((task.Deadline - currentEndTime).TotalMinutes < currentTask.Duration))
                        {
                            return null;
                        }
                    }
                }

                ++i;

                var minMachine = machineSchedules.Min;
                if (minMachine.EndTime > currentTask.ExtremeTime)
                {
                    // engage next processor
                    var nextMachine = new MachineScheduleExt(schedule[j]);
                    nextMachine.Schedule.StartTime = currentTask.ExtremeTime;
                    nextMachine.Schedule.Tasks.AddLast(currentTask);
                    nextMachine.EndTime = currentTask.Deadline;
                    machineSchedules.Add(nextMachine);
                    ++j;
                    continue;
                }

                machineSchedules.Remove(minMachine);
                if (machineSchedules.Count > 0 && !(machineSchedules.Min.EndTime > currentTask.ExtremeTime))
                {
                    return null;
                }
                
                firstCriterion = false;
                minMachine.Schedule.Tasks.AddLast(currentTask);
                minMachine.EndTime = minMachine.EndTime.AddMinutes(currentTask.Duration);
                machineSchedules.Add(minMachine);
            }

		    nextTaskIndex = i;

			return machineSchedules;
		}

        private static Schedule initialSchedule(List<Task> tasks, List<Machine> machines, out int nextTaskIndex,
            out Dictionary<int, DateTime> endTimes)
        {
            nextTaskIndex = 1;
            endTimes = new Dictionary<int, DateTime>();
            return new Schedule();
        }

        private static Schedule mainHeuristicAlgorithm(List<Task> tasks, List<Machine> machines, TimeSpan? maxInterval)
        {
            var checkInterval = maxInterval != null;
            var maxInvervalValue = TimeSpan.MinValue;
            if (checkInterval)
            {
                maxInvervalValue = (TimeSpan) maxInterval;
            }
            
            var remTasks = new List<Task>(tasks);
            var schedule = heuristicInitialAppointment(remTasks, machines);

            remTasks.Sort((x, y) =>
            {
                var t = y.Deadline.CompareTo(x.Deadline);
                return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
            });

            while (remTasks.Count != 0)
            {
                // Select feasible tasks for the current iteration
                var feasibleTasks = new List<Task>();
                foreach (var task in remTasks)
                {
                    var feasible = false;
                    for (var j = 0; j < machines.Count; j++)
                    {
                        if (schedule[j].StartTime > task.Deadline)
                        {
                            continue;
                        }
                        feasible = true;
                        break;
                    }

                    if (feasible)
                    {
                        feasibleTasks.Add(task);
                    }
                    else
                    {
                        break;
                    }
                }

                feasibleTasks.Sort((x, y) =>
                {
                    var t = x.Duration.CompareTo(y.Duration);
                    return t == 0 ? y.Deadline.CompareTo(x.Deadline) : t;
                });

                var remMachines = new List<MachineSchedule>(schedule);
                while (remMachines.Count != 0)
                {
                    if (feasibleTasks.Count == 0)
                    {
                        break;
                    }
                    var currentTask = feasibleTasks[0];
                    feasibleTasks.Remove(currentTask);

                    MachineSchedule best = null;
                    foreach (var machine in remMachines)
                    {
                        if (machine.StartTime > currentTask.Deadline)
                        {
                            continue;
                        }

                        if (best == null || best.StartTime < machine.StartTime)
                        {
                            best = machine;
                        }
                    }

                    if (best != null)
                    {
                        best.Tasks.AddFirst(currentTask);
                        best.StartTime = best.StartTime.AddMinutes(-currentTask.Duration);
                        remMachines.Remove(best);
                        remTasks.Remove(currentTask);
                    }
                }

                foreach (var machine in remMachines)
                {
                    if (remTasks.Count == 0)
                    {
                        break;
                    }

                    var currentTask = remTasks[0];
                    machine.Tasks.AddFirst(currentTask);
                    machine.StartTime = currentTask.ExtremeTime;
                    remTasks.Remove(currentTask);
                }

                // Execute adjustment algorithm if needed
                if (checkInterval)
                {
                    var maxStartTime = DateTime.MinValue;
                    var minStartTime = DateTime.MaxValue;
                    foreach (var machine in schedule)
                    {
                        if (machine.StartTime > maxStartTime)
                        {
                            maxStartTime = machine.StartTime;
                        }
                        if (machine.StartTime < minStartTime)
                        {
                            minStartTime = machine.StartTime;
                        }
                    }

                    if (maxStartTime - minStartTime > maxInvervalValue)
                    {
                        startTimesAdjustment(schedule);
                    }
                }
            }

            startTimesAdjustment(schedule);

            return schedule;
        }

        private static Schedule secondHeuristicAlgorithm(List<Task> tasks, List<Machine> machines)
        {
            var remTasks = new List<Task>(tasks);
            var schedule = heuristicInitialAppointment(remTasks, machines);

            remTasks.Sort((x, y) =>
            {
                var t = y.Deadline.CompareTo(x.Deadline);
                return t == 0 ? x.Duration.CompareTo(y.Duration) : t;
            });

            var sortedSet = new SortedSet<MachineSchedule>(schedule, new StartTimeComparer());
            while (remTasks.Count != 0)
            {
                var lastMachine = sortedSet.Max;

                // Searching for a feasible task with minimal duration
                Task best = null;
                var startTime = DateTime.MinValue;
                foreach (var task in remTasks)
                {
                    if (lastMachine.StartTime > task.Deadline)
                    {
                        break;
                    }
                    if (best == null || task.Duration < best.Duration)
                    {
                        best = task;
                    }
                }

                if (best == null)
                {
                    foreach (var task in remTasks)
                    {
                        if (best == null || task.ExtremeTime > best.ExtremeTime)
                        {
                            best = task;
                        }
                    }
                    startTime = best.ExtremeTime;
                }
                else
                {
                    startTime = lastMachine.StartTime.AddMinutes(-best.Duration);
                }

                sortedSet.Remove(lastMachine);
                lastMachine.Tasks.AddFirst(best);
                lastMachine.StartTime = startTime;
                sortedSet.Add(lastMachine);
                remTasks.Remove(best);
            }

            startTimesAdjustment(schedule);

            return schedule;
        }

        private static Schedule thirdHeuristicAlgorithm(List<Task> tasks, List<Machine> machines)
        {
            var schedule = new Schedule(machines);
            var machineSchedules = new SortedSet<MachineScheduleExt>();

            // Engage one machine and appoint first task
            var firstTask = tasks[0];
            var engagedMachine = new MachineScheduleExt(schedule[0]);
            engagedMachine.Schedule.Tasks.AddLast(firstTask);
            engagedMachine.Schedule.StartTime = firstTask.ExtremeTime;
            engagedMachine.EndTime = firstTask.Deadline;
            machineSchedules.Add(engagedMachine);

            var j = 1;
            for (var i = 1; i < tasks.Count; i++)
            {
                var currentTask = tasks[i];
                var minMachine = machineSchedules.Min;

                MachineScheduleExt currentBest = null;
                if (minMachine.EndTime > currentTask.ExtremeTime)
                {
                    // Find best machine (engage next if needed)
                    if (j < schedule.Count)
                    {
                        // Engage next machine
                        currentBest = new MachineScheduleExt(schedule[j]);
                        currentBest.Schedule.StartTime = currentTask.ExtremeTime;
                        currentBest.EndTime = currentTask.Deadline;
                        j++;
                    }
                    else
                    {
                        // Find best (with offset)
                        DateTime[] bestStartTimes = null;
                        var k = 0;
                        var bestOffset = TimeSpan.MinValue;
                        foreach (var machineSchedule in machineSchedules)
                        {
                            var startTimes = machineSchedules.Select(m => m.Schedule.StartTime).ToArray();
                            var offset = currentTask.ExtremeTime - machineSchedule.EndTime;
                            startTimes[k] = startTimes[k].Add(offset);
                            if (bestStartTimes == null || compare(bestStartTimes, startTimes, schedule.Count) < 0)
                            {
                                currentBest = machineSchedule;
                                bestStartTimes = startTimes;
                                bestOffset = offset;
                            }
                            k++;
                        }

                        machineSchedules.Remove(currentBest);
                        currentBest.Schedule.StartTime = currentBest.Schedule.StartTime.Add(bestOffset);
                        currentBest.EndTime = currentTask.Deadline;
                    }
                }
                else
                {
                    // The task is feasible for the current best machine
                    currentBest = minMachine;
                    machineSchedules.Remove(currentBest);
                    currentBest.EndTime = currentBest.EndTime.AddMinutes(currentTask.Duration);
                }

                currentBest.Schedule.Tasks.AddLast(currentTask);
                machineSchedules.Add(currentBest);
            }

            startTimesAdjustment(schedule);

            return schedule;
        }

        private static Schedule heuristicInitialAppointment(List<Task> tasks, List<Machine> machines)
        {
            var schedule = new Schedule(machines);
            foreach (var machineSchedule in schedule)
            {
                if (tasks.Count == 0)
                {
                    break;
                }

                var task = tasks[tasks.Count - 1];    // Last task
                machineSchedule.Tasks.AddLast(task);
                machineSchedule.StartTime = task.ExtremeTime;
                tasks.Remove(task);
            }

            return schedule;
        }

        private static void startTimesAdjustment(Schedule schedule)
        {
            var exit = false;
            while (!exit)
            {
                schedule.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
                exit = true;
                for (var i = 0; i < schedule.Count - 1; i++)
                {
                    var currentMachine = schedule[i];
                    if (currentMachine.Tasks.Count == 0)
                    {
                        continue;
                    }

                    var currentStartTime = currentMachine.StartTime;
                    while (shiftCondition(currentMachine.Tasks))
                    {
                        var currentTaskNode = currentMachine.Tasks.First;
                        var shiftingTask = currentTaskNode.Value;
                        var currentTaskEnd = currentStartTime.AddMinutes(shiftingTask.Duration);
                        var shifted = false;
                        while (currentTaskNode.Next != null &&
                            !(currentTaskEnd.AddMinutes(currentTaskNode.Next.Value.Duration) > shiftingTask.Deadline))
                        {
                            currentTaskNode = currentTaskNode.Next;
                            currentTaskEnd = currentTaskEnd.AddMinutes(currentTaskNode.Value.Duration);
                            shifted = true;
                        }

                        if (!shifted)
                        {
                            break;
                        }

                        currentMachine.Tasks.AddAfter(currentTaskNode, shiftingTask);
                        currentMachine.Tasks.RemoveFirst();
                    }

                    var movingTask = currentMachine.Tasks.First.Value;
                    MachineSchedule targetMachine = null;
                    for (var j = schedule.Count - 1; j > i; j--)
                    {
                        if (targetMachine != null && schedule[j].StartTime < movingTask.Deadline)
                        {
                            break;
                        }
                        targetMachine = schedule[j];
                    }

                    if (targetMachine != null)
                    {
                        var newStartTime = (movingTask.Deadline < targetMachine.StartTime)
                            ? movingTask.ExtremeTime
                            : targetMachine.StartTime.AddMinutes(-movingTask.Duration);
                        if (newStartTime > currentStartTime)
                        {
                            currentMachine.Tasks.RemoveFirst();
                            currentMachine.StartTime = calculateStartTime(currentMachine.Tasks);
                            targetMachine.Tasks.AddFirst(movingTask);
                            targetMachine.StartTime = newStartTime;
                            exit = false;
                            break;
                        }
                    }
                }
            }
        }

        private static bool shiftCondition(LinkedList<Task> tasks)
        {
            var first = tasks.First;
            var second = first.Next;
            return second != null && first.Value.Duration > second.Value.Duration;
        }

        private static DateTime calculateStartTime(LinkedList<Task> tasks)
        {
            if (tasks.Count == 0)
            {
                return DateTime.MaxValue;
            }

            var currentTaskNode = tasks.Last;
            var currentStartTime = currentTaskNode.Value.ExtremeTime;
            while (currentTaskNode.Previous != null)
            {
                currentTaskNode = currentTaskNode.Previous;
                currentStartTime = (currentTaskNode.Value.Deadline < currentStartTime)
                    ? currentTaskNode.Value.ExtremeTime
                    : currentStartTime.AddMinutes(-currentTaskNode.Value.Duration);
            }

            return currentStartTime;
        }

        private static int compare(DateTime[] first, DateTime[] second, int count)
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

        private class MachineScheduleExt : IComparable, ICloneable
        {
            public MachineSchedule Schedule { get; }

            public DateTime EndTime { get; set; }


            public MachineScheduleExt(MachineSchedule schedule) : this(schedule, DateTime.MinValue)
            {
            }

            private MachineScheduleExt(MachineSchedule schedule, DateTime endTime)
            {
                Schedule = schedule;
                EndTime = endTime;
            }


            public int CompareTo(object obj)
            {
                var second = (MachineScheduleExt) obj;
                var endTimeCompResult = EndTime.CompareTo(second.EndTime);
                return endTimeCompResult == 0
                    ? Schedule.Machine.Id.CompareTo(second.Schedule.Machine.Id)
                    : endTimeCompResult;
            }

            public object Clone()
            {
                return new MachineScheduleExt((MachineSchedule) Schedule.Clone(), EndTime);
            }
        }

        #region Comparers

        private class EndTimeComparer : IComparer<MachineSchedule>
        {
            private Dictionary<int, DateTime> _endTimes;

            public EndTimeComparer(Dictionary<int, DateTime> endTimes)
            {
                _endTimes = endTimes;
            }

            public int Compare(MachineSchedule x, MachineSchedule y)
            {
                var xEndTime = _endTimes[x.Machine.Id];
                var yEndTime = _endTimes[y.Machine.Id];
                return xEndTime == yEndTime ? x.Machine.Id.CompareTo(y.Machine.Id) : xEndTime.CompareTo(yEndTime);
            }
		}

		private class StartTimeComparer : IComparer<MachineSchedule>
		{
			public int Compare(MachineSchedule x, MachineSchedule y)
			{
				return x.StartTime == y.StartTime ? x.Machine.Id.CompareTo(y.Machine.Id) : x.StartTime.CompareTo(y.StartTime);
			}
		}

        #endregion

        #region BB-method

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

        #endregion
	}
}