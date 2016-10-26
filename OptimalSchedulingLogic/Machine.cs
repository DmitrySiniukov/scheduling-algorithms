namespace OptimalSchedulingLogic
{
    public class Machine
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public Machine(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}