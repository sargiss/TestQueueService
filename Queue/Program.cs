using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            var signal = new StopSignal();

            var server = new OperatorSessionServer(signal);
            Task.Run(new Action(server.Do));

            for (var i = 0; i < 2; i++)
            {
                Task.Run(new Action(new Worker().Do));
            }

            for (var i = 0; i < 8; i++)
            {
                Task.Run(new Action(new Client(signal, "session" + i/2).Do));
            }

            char k = 'a';
            do
            {
                k = (char)Console.Read();
            }
            while (k != 'q');
            signal.Stop();
            Console.ReadKey();
        }
    }
}
