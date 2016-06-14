using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using OptimalScheduling.Infrastructure;

namespace OptimalScheduling.Models
{
	public class ScheduleViewModel
	{
		public Schedule<MachineSchedule> FastAlgorithmSchedule { get; set; }

		public long FastAlgorithmTime { get; set; }

		public Schedule<MachineSchedule> AccurateAlgorithmSchedule { get; set; }

		public long AccurateAlgorithmTime { get; set; }

		public bool AccurateAlgorithm { get; set; }

		public bool IsOptimal { get; set; }


		public string GetJson()
		{
			var data = new List<object>();
			var links = new List<object>();
			var currentId = 3;
			var linkId = 10;

			List<object> fastAlgorithmData;
			List<object> fastAlgorithmLinks;
			DateTime? fastAlgorithmMinStartTime;
			double? fastAlgorithmDuration;
			var commonId = AccurateAlgorithm ? (int?)1 : null;
			getData(FastAlgorithmSchedule, commonId, ref currentId, ref linkId, out fastAlgorithmData, out fastAlgorithmLinks,
				out fastAlgorithmMinStartTime, out fastAlgorithmDuration);
			if (fastAlgorithmMinStartTime != null)
			{
				if (AccurateAlgorithm)
				{
					var mst = (DateTime) fastAlgorithmMinStartTime;
					data.Add(new
					{
						id = 1,
						text = string.Format("Fast algorithm schedule (builded in {0} ms)", FastAlgorithmTime),
						start_date = dateString(mst),
						duration = string.Format("{0}", fastAlgorithmDuration),
						progress = 0,
						open = true
					});
				}

				data.AddRange(fastAlgorithmData);
				links.AddRange(fastAlgorithmLinks);
			}

			if (AccurateAlgorithm)
			{
				List<object> accAlgorithmData;
				List<object> accAlgorithmLinks;
				DateTime? accAlgorithmMinStartTime;
				double? accAlgorithmDuration;
				getData(AccurateAlgorithmSchedule, 2, ref currentId, ref linkId, out accAlgorithmData, out accAlgorithmLinks,
					out accAlgorithmMinStartTime, out accAlgorithmDuration);
				if (accAlgorithmMinStartTime != null)
				{
					var mst = (DateTime) accAlgorithmMinStartTime;
					data.Add(new
					{
						id = 2,
						text = string.Format("Accurate algorithm schedule (builded in {0} ms)", AccurateAlgorithmTime),
						start_date = dateString(mst),
						duration = string.Format("{0}", accAlgorithmDuration),
						progress = 0,
						open = true
					});

					data.AddRange(accAlgorithmData);
					links.AddRange(accAlgorithmLinks);
				}
			}

			return new JavaScriptSerializer().Serialize(new { data = data, links = links });
		}


		private static void getData(Schedule<MachineSchedule> schedule, int? commonId, ref int startId, ref int startLinkId,
			out List<object> data, out List<object> links, out DateTime? minStartTime, out double? totalDuration)
		{
			data = new List<object>();
			links = new List<object>();

			totalDuration = null;
			minStartTime = null;
			DateTime? maxEndTime = null;
			foreach (var machine in schedule.OrderBy(x => x.StartTime))
			{
				if (machine.Tasks.Count != 0)
				{
					var parentId = startId++;
					var startTime = machine.StartTime;
					var previousId = 0;
					var duration = 0d;

					if (minStartTime == null || startTime < minStartTime)
					{
						minStartTime = startTime;
					}

					foreach (var task in machine.Tasks)
					{
						data.Add(new
						{
							id = startId,
							text = string.Format("{0} ({1})", task.Name, task.Id),
							start_date = dateString(startTime),
							duration = string.Format("{0}", task.Duration),
							parent = parentId.ToString(),
							progress = 0,
							open = true
						});
						startTime = startTime.AddMinutes(task.Duration);

						if (previousId != 0)
						{
							links.Add(new
							{
								id = startLinkId.ToString(),
								source = previousId.ToString(),
								target = startId.ToString(),
								type = "0"
							});
							++startLinkId;
						}
						previousId = startId;
						duration += task.Duration;
						startId++;
					}
					
					data.Add(new
					{
						id = parentId,
						text = string.Format("{0} ({1})", machine.Machine.Name, machine.Machine.Id),
						start_date = dateString(machine.StartTime),
						duration = string.Format("{0}", duration),
						parent = commonId == null ? string.Empty : commonId.ToString(),
						progress = 1,
						open = false
					});

					var endTime = startTime;
					if (maxEndTime == null || maxEndTime < endTime)
					{
						maxEndTime = endTime;
					}
				}
			}

			if (minStartTime != null)
			{
				totalDuration = ((DateTime) maxEndTime - (DateTime) minStartTime).TotalMinutes;
			}
		}

		private static string dateString(DateTime date)
		{
			return string.Format("{0:D2}-{1:D2}-{2} {3:D2}:{4:D2}", date.Day, date.Month, date.Year, date.Hour, date.Minute);
		}
	}
}