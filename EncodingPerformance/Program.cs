﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using EncodingPerformance.Encoders;
using LoggingPerformance.TestHarness;

namespace EncodingPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            var encoders = new ILogEncoder[]
            {
                new XmlLogEncoder()
            };

            foreach (var encoder in encoders)
            {
                Console.WriteLine(encoder.ToString());
                TestEncoder(encoder);
            }
        }

        private static void TestEncoder(ILogEncoder encoder)
        {
            var storage = new EncodedLogStorage(encoder);

            TestEncode(storage);

            var log = TestLength(storage);

            TestDecode(encoder, log);
        }

        private static void TestEncode(EncodedLogStorage storage)
        {
            var watch = Stopwatch.StartNew();
            GenerateLotsOfLogs(storage);
            Console.WriteLine("Time to encode:    " + watch.Elapsed);
        }

        private static void GenerateLotsOfLogs(EncodedLogStorage storage)
        {
            var deployment = new TestDeployment(new Log("tasks-1", storage));
            deployment.Deploy();
        }

        private static string TestLength(EncodedLogStorage storage)
        {
            var log = storage.GetCompleteLog();
            Console.WriteLine("Log size:          " + log.Length);
            return log;
        }

        private static void TestDecode(ILogEncoder encoder, string log)
        {
            var watch = Stopwatch.StartNew();
            var read = encoder.DecodeAll(log).Count();
            Console.WriteLine("Time to decode:    " + watch.Elapsed);
            Console.WriteLine("Log entries read:  " + read);
        }
    }
}