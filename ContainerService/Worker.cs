using ContainerService.Logic;
using ContainerService.Models;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerService
{
    public class Worker : BackgroundService
    {
        internal bool loopRunning = false;
        internal List<Step> currentSteplist = null;
        internal bool amIAsleep = false;

        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await this.Run();
                await Task.Delay((int)RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.TimeInterval.TotalMilliseconds, stoppingToken);
            }
        }

        private async Task Run()
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

            if (this.IsInNightMode())
            {
                this.EndLoop();
                return;
            }

#if SKIPALLSTEPS
            return;
#endif

            Queue<Step> steps = this.GetAllSteps();

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
                        this.EndLoop();
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
                    this.EndLoop();
                    return;
                }

                if (s.Ex != null)
                {
                    Log.Error(s.Ex, $"Error in step {s.Id} ({s.Name}) continue...");
                }
            }

            this.EndLoop(true);
        }

        private bool IsInNightMode()
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
        private Queue<Step> GetAllSteps()
        {
            currentSteplist ??= [];
            currentSteplist.Clear();

            IEnumerable<Type> allSteps = typeof(Worker).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Step)));

            foreach (Type s in allSteps)
            {
                currentSteplist.Add((Step)Activator.CreateInstance(s));
            }

            return new(currentSteplist.Where(x => x.IsActive && !(x.RunOnlyOnceADay && x.RunDay != DateTime.Now.Day)).OrderBy(x => x.Id));
        }

        private void EndLoop(bool success = false)
        {
            if (currentSteplist != null)
            {
                foreach (Step s in currentSteplist)
                {
                    s.Dispose();
                }
            }

            if (success)
            {
                RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.Runday = DateTime.Now.Day;
            }

            loopRunning = false;

            RuntimeStorage.ConfigurationHandler.Save();
        }
    }
}
