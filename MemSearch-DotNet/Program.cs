using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MemSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
            {
                Console.WriteLine("MemSearch [process name regex] [config ascii base64 encoded string]");
                Console.WriteLine($"If you meant to specify a process filter of {args[0]}, try something else");
                Console.WriteLine();
                Environment.Exit(0);
            }

            String processFilter = args.Length > 0 ? args[0] : "chrome";
            Console.WriteLine($"Looking for process with filter '{processFilter}'");

            String configBase64 = null;
            if (args.Length > 1)
            {
                Console.WriteLine("Loading config from stdin");
                configBase64 = args[1];
            }
            IEnumerable<Input> inputs = Config.Load(configBase64);

            long totalBytes = 0;
            DateTime start = DateTime.Now;

            var processes = Process.GetProcesses().Where((p) => System.Text.RegularExpressions.Regex.IsMatch(p.ProcessName, processFilter));
            processes.ToList().ForEach(x => Console.WriteLine($"Matched process {x.ProcessName}"));

            foreach (var proc in processes)
            {
                foreach (var chunk in ProcessReader.ReadMemory((IntPtr)proc.Id))
                {
                    totalBytes += chunk.Length;

                    foreach (var input in inputs)
                    {
                        var res = TextSearcher.Find(chunk, input.Marker, input.Regex);
                        foreach (var str in res)
                        {
                            Console.WriteLine($"Found match for {input.Title}: {input.Decode(str)}");
                        }
                    }
                }
            }

            DateTime stop = DateTime.Now;
            Console.WriteLine($"Done, scanned {totalBytes} bytes in {stop - start}");

            long bytesPerSecond = totalBytes / (long)Math.Ceiling((stop - start).TotalSeconds);
            Console.WriteLine($"That's a rate of {bytesPerSecond} bytes/second");

            if (totalBytes == 0)
                Console.WriteLine("Did not scan any bytes! Are you sure the process is running?");

            Console.WriteLine("Done");
        }
    }
}
