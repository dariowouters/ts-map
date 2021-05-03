using System;
using System.Threading.Tasks;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Job {
    public abstract class ThreadJob {
        public Task t { get; set; }

        protected abstract void Do();

        public string JobName() => this.GetType().Name;

        protected abstract void OnEnd();

        public void Run() {
            this.t = Task.Factory.StartNew( () => this.Do(), TaskCreationOptions.AttachedToParent );
            this.t.ContinueWith( t => HandleException( t.Exception ), TaskContinuationOptions.OnlyOnFaulted );
            this.t.ContinueWith( i => this.OnEnd() );
        }

        public void RunAndWait() {
            this.Run();
            this.t.Wait();
        }

        public StoreHelper Store() => StoreHelper.Instance;

        private static void HandleException( Exception e ) {
            if ( e.GetBaseException().GetType() != typeof( JobException ) ) return;

            var ex = (JobException) e.GetBaseException();

            Log.Error( "Job Exception ({0}): {1} | Stack: {2}", ex.JobName, ex.Message, ex.StackTrace );
        }
    }
}