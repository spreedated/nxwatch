﻿using System.ServiceProcess;
using System.Threading;

namespace NxBrewWindowsServiceReporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            ServiceEntry o = new();
            o.StartDebugging();
            Thread.Sleep(Timeout.Infinite);
#endif
            ServiceBase.Run(new ServiceEntry());
        }
    }
}