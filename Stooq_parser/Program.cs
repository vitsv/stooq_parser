using System;

namespace Stooq_parser
{
    class Program
    {
        /// <summary>
        /// How often stocks will be checked. Miliseconds
        /// </summary>
        public static int Interval { get; set; }
        /// <summary>
        /// Url for retrieving data
        /// </summary>
        public static string Url { get; set; }
        /// <summary>
        /// Timer to keep app alive
        /// </summary>
        private static System.Timers.Timer _timer;

        static void Main(string[] args)
        {
            //A litle bit configuration
            Interval = 5000; //5 sekonds
            Url = "http://s.stooq.pl/pp/g.js"; //GPW 

            //Create grabber
            var grabber = new StocksGrabber(Url, new StooqGpwParser(), new ConsolePersister());

            //Run timer
            _timer = new System.Timers.Timer(Interval);

            _timer.Elapsed += grabber.Update;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            GC.KeepAlive(_timer);

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();
        }
    }
}
