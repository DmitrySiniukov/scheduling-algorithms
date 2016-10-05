using System.Web;

namespace OptimalScheduling.Models
{
	public class InputViewModel
	{
		/// <summary>
		/// Tasks file
		/// </summary>
		public HttpPostedFileBase TasksFile { get; set; }

		/// <summary>
		/// Machines file
		/// </summary>
		public HttpPostedFileBase MachinesFile { get; set; }

		/// <summary>
		/// Use branch & bounds method
		/// </summary>
		public bool BBMethod { get; set; }

        /// <summary>
        /// Build diagram
        /// </summary>
        public bool Visualization { get; set; }
	}
}