//#define RUNONLYONCEADAY

using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace NxBrewWindowsServiceReporter
{
    internal static class Engine
    {
        public static void Initialize()
        {
            mainLoopTimer.Interval = RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.TimeInterval.TotalMilliseconds;
            mainLoopTimer.Elapsed += MainLoopTimerElapsed;
            mainLoopTimer.Enabled = true;
            mainLoopTimer.Start();

            MainLoopTimerElapsed(null, null); // run once at startup
        }

        #region Main Loop
        internal static readonly System.Timers.Timer mainLoopTimer = new();
        internal static bool loopRunning = false;
        internal static List<Step> currentSteplist = null;
        internal static bool amIAsleep = false;
        internal static long runcount = 0;
        public static async void MainLoopTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (loopRunning)
            {
                return;
            }

            loopRunning = true;

#if RUNONLYONCEADAY
            if (RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.Runday == DateTime.Now.Day)
            {
                return;
            }
#endif

            if (IsInNightMode())
            {
                EndLoop();
                return;
            }

            Queue<Step> steps = GetAllSteps();

            Log.Information($"Executing {steps.Count} active steps");
            int initialStepCount = steps.Count;

            for (int i = 0; i < initialStepCount; i++)
            {
                Step s = steps.Dequeue();

                if (!s.CanExecute())
                {
                    Log.Error($"{s.Id} ({s.Name} cannot execute, CanExecute is not returned false");
                    if (!s.ContinueOnError)
                    {
                        EndLoop();
                        return;
                    }
                    continue;
                }

                try
                {
                    await s.Execute();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"Error in step {s.Id} ({s.Name})");
                }

                if (!s.ContinueOnError && s.Ex != null)
                {
                    Log.Error(s.Ex, $"Error in step {s.Id} ({s.Name}) aborting...");
                    EndLoop();
                    return;
                }

                if (s.Ex != null)
                {
                    Log.Error(s.Ex, $"Error in step {s.Id} ({s.Name}) continue...");
                }
            }

            EndLoop(true);
        }
        #endregion

        private static bool IsInNightMode()
        {
            TimeSpan nowT = DateTime.Now.TimeOfDay;

            if (nowT < RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.StartServiceTime || nowT > RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.EndServiceTime)
            {
                if (!amIAsleep)
                {
                    Log.Information("Entering sleepmode...");
                }

                amIAsleep = true;
                return true;
            }

            if (amIAsleep)
            {
                Log.Information($"Exiting sleepmode...");
                amIAsleep = false;
            }

            return false;
        }

        /// <summary>
        /// Retrieve all steps from the assembly<br/>
        /// filtered out inactive steps and steps that are only allowed to run once a day and already ran today
        /// </summary>
        /// <returns></returns>
        private static Queue<Step> GetAllSteps()
        {
            currentSteplist ??= [];
            currentSteplist.Clear();

            IEnumerable<Type> allSteps = typeof(Engine).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Step)));

            foreach (Type s in allSteps)
            {
                currentSteplist.Add((Step)Activator.CreateInstance(s));
            }

            return new(currentSteplist.Where(x => x.IsActive && !(x.RunOnlyOnceADay && x.RunDay != DateTime.Now.Day)).OrderBy(x => x.Id));
        }

        private static void EndLoop(bool success = false)
        {
            foreach (Step s in currentSteplist)
            {
                s.Dispose();
            }

            if (success)
            {
                RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.Runday = DateTime.Now.Day;
            }

            loopRunning = false;
            runcount++;

            RuntimeStorage.ConfigurationHandler.Save();
        }
    }
}
