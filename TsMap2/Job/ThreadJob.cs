using System.Threading.Tasks;

namespace TsMap2.Job {
    public abstract class ThreadJob {
        public             Task t { get; set; }
        protected abstract void Do();

        protected abstract string JobName();

        public abstract void OnEnd();


        public void Run() {
            this.t = Task.Run( this.Do );
            this.t.ContinueWith( i => this.OnEnd() );
        }
    }
}