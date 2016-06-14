using OptimalScheduling.Infrastructure;
using OptimalScheduling.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace OptimalScheduling.Controllers
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
				ModelState.AddModelError("", "Fill all fields to continue.");
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
			var fastAlgorithmSchedule = Schedule<MachineSchedule>.BuildSchedule(tasks, machines);
			sw1.Stop();
			var fastAlgorithmTime = sw1.ElapsedMilliseconds;


			var scheduleModel = new ScheduleViewModel
			{
				FastAlgorithmSchedule = fastAlgorithmSchedule,
				FastAlgorithmTime = fastAlgorithmTime,
				IsOptimal = fastAlgorithmSchedule.OptimalityCriterion
			};

			if (model.BBMethod)
			{
				var sw2 = new Stopwatch();
				sw2.Start();
				var accurateAlgorithmSchedule = Schedule<MachineSchedule>.BuildOptimalSchedule(tasks, machines);
				sw2.Stop();
				var accurateAlgorithmTime = sw2.ElapsedMilliseconds;

				scheduleModel.AccurateAlgorithmSchedule = accurateAlgorithmSchedule;
				scheduleModel.AccurateAlgorithmTime = accurateAlgorithmTime;
				scheduleModel.AccurateAlgorithm = true;

				if (!scheduleModel.IsOptimal)
				{
					var fastAlgTimes = fastAlgorithmSchedule.Select(x => x.StartTime).ToArray();
					var accAlgTimes = accurateAlgorithmSchedule.Select(x => x.StartTime).ToArray();
					var optimal = !(Schedule<MachineSchedule>.Compare(fastAlgTimes, accAlgTimes, fastAlgTimes.Length) < 0);
					scheduleModel.IsOptimal = optimal;
				}
			}

			return View("Schedule", scheduleModel);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}