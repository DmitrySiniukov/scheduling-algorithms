using OptimalSchedulingLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Web.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index()	
		{
			return View(new InputViewModel());
		}

		[HttpPost]
		public ActionResult Index(InputViewModel model)
		{
			if (model.TasksFile == null || model.MachinesFile == null)
			{
				ModelState.AddModelError("", "Specify input files to continue.");
				return View();
			}

			var tasks = new List<Task>();
			var taskStream = new StreamReader(model.TasksFile.InputStream);
			var baseDeadline = DateTime.Now.Date;
			try
			{
				var taskNumber = int.Parse(taskStream.ReadLine());
				for (var i = 0; i < taskNumber; ++i)
				{
					var values = taskStream.ReadLine().Split(' ');
					tasks.Add(new Task(int.Parse(values[0]), values[1], double.Parse(values[2]), baseDeadline.AddMinutes(double.Parse(values[3]))));
				}
			}
			catch (Exception)
			{
				ModelState.AddModelError("", "The tasks file has incorrect format.");
				return View();
			}

			var machines = new List<Machine>();
			var machinesStream = new StreamReader(model.MachinesFile.InputStream);
			try
			{
				var machinesNumber = int.Parse(machinesStream.ReadLine());
				for (var i = 0; i < machinesNumber; ++i)
				{
					var values = machinesStream.ReadLine().Split(' ');
					machines.Add(new Machine(int.Parse(values[0]), values[1]));
				}
			}
			catch (Exception)
			{
				ModelState.AddModelError("", "The machines file has incorrect format.");
				return View();
			}
			
			var sw1 = new Stopwatch();
			sw1.Start();
			var fastAlgorithmSchedule = Schedule.BuildWithPavlovAlgorithm(tasks, machines);
			sw1.Stop();
			var fastAlgorithmTime = sw1.ElapsedMilliseconds;

			var scheduleModel = new ScheduleViewModel
			{
				FastAlgorithmSchedule = fastAlgorithmSchedule,
				FastAlgorithmTime = fastAlgorithmTime,
				IsOptimal = fastAlgorithmSchedule.OptimalityCriterion
			};

            Schedule accurateAlgorithmSchedule = null;
            if (model.BBMethod)
            {
                var sw2 = new Stopwatch();
                sw2.Start();
                accurateAlgorithmSchedule = Schedule.BuildOptimalSchedule(tasks, machines);
                sw2.Stop();
                var accurateAlgorithmTime = sw2.ElapsedMilliseconds;

                if (accurateAlgorithmSchedule != null)
                {
                    scheduleModel.AccurateAlgorithmSchedule = accurateAlgorithmSchedule;
                    scheduleModel.AccurateAlgorithmTime = accurateAlgorithmTime;
                    scheduleModel.AccurateAlgorithm = true;

                    if (!scheduleModel.IsOptimal)
                    {
                        var optimal = fastAlgorithmSchedule.CompareTo(accurateAlgorithmSchedule) == 0;
                        scheduleModel.IsOptimal = optimal;
                    }
                }
            }

            var fileName = Path.GetTempFileName();
			using (var fileStream = new StreamWriter(fileName))
			{
				fileStream.WriteLine("n = {0}, m = {1}", tasks.Count, machines.Count);
				fileStream.WriteLine();

			    if (model.BBMethod)
			    {
                    fileStream.WriteLine("By the fast algorithm:");
                }

			    printSchedule(fastAlgorithmSchedule, fileStream, baseDeadline);

				if (model.BBMethod)
				{
					fileStream.WriteLine();
					fileStream.WriteLine("By the accurate algorithm:");

				    if (accurateAlgorithmSchedule == null)
                    {
                        fileStream.WriteLine("An error has occured...");
                    }
				    else
                    {
                        printSchedule(accurateAlgorithmSchedule, fileStream, baseDeadline);
                    }
				}
			}

			scheduleModel.Visualization = model.Visualization;
			scheduleModel.FileId = Path.GetFileNameWithoutExtension(fileName);

			return View("Schedule", scheduleModel);
		}

		public ActionResult ResultFile(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return Redirect("~/Shared/Error.cshtml");
			}

			byte[] fileBytes;
			try
			{
				var tempPath = Path.GetTempPath();
				var path = string.Format(@"{0}/{1}.tmp", tempPath, id);
				fileBytes = System.IO.File.ReadAllBytes(path);
				System.IO.File.Delete(path);
			}
			catch (Exception)
			{
				return Redirect("~/Shared/Error.cshtml");
			}
			
			var fileName = "Result.txt";
			return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
		}

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Contact()
		{
			return View();
		}


		private void printSchedule(Schedule schedule, StreamWriter streamWriter, DateTime baseDeadline)
		{
			foreach (var machineSchedule in schedule)
			{
				var st = (machineSchedule.StartTime - baseDeadline).TotalMinutes;
				streamWriter.WriteLine("({0}) \"{1}\" (r{0} = {2})", machineSchedule.Machine.Id, machineSchedule.Machine.Name, st);
				var startTime = (machineSchedule.StartTime - baseDeadline).TotalMinutes;
				foreach (var task in machineSchedule.Tasks)
				{
					var d = (task.Deadline - baseDeadline).TotalMinutes;
					streamWriter.WriteLine("\t({0}) \"{1}\"\t{2}\t{3}\t{4}\t{5}", task.Id, task.Name, task.Duration, d, startTime,
						startTime + task.Duration);
					startTime += task.Duration;
				}
			}
		}
	}
}