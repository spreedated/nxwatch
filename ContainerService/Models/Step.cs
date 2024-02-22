using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ContainerService.Models
{
    public abstract class Step : IDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; private set; }
        public Exception Ex { get; private set; }
        public bool ContinueOnError { get; set; }
        public bool RunOnlyOnceADay { get; set; }
        /// <summary>
        /// Will be only set on successful run
        /// </summary>
        public int RunDay { get; private set; }
        public bool IsActive { get; set; }

        public event EventHandler StepFinished;

        public async Task Execute()
        {
            Stopwatch sw = Stopwatch.StartNew();

            this.Ex = null;
            this.Duration = TimeSpan.Zero;

            try
            {
                await this.Processor();
            }
            catch (Exception ex)
            {
                this.Ex = ex;
            }

            sw.Stop();
            this.OnStepFinished(sw.Elapsed);
        }

        public abstract Task Processor();

        public virtual bool CanExecute()
        {
            return true;
        }

        protected void OnStepFinished(TimeSpan duration)
        {
            this.Duration = duration;
            this.RunDay = DateTime.Now.Day;
            StepFinished?.Invoke(this, EventArgs.Empty);
        }

        protected void SetError(Exception ex)
        {
            this.Ex = ex;
        }

        #region Dispose
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
        #endregion
    }
}
