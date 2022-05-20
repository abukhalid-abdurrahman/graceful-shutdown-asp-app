namespace graceful_shutdown_asp_app.Services
{
    public enum ApplicationStateEnum
    {
        Default = 0,
        Started = 1,
        Stopping = 2,
        Stopped = 3
    }
    
    public class ServerStateService
    {
        private ApplicationStateEnum _applicationState = ApplicationStateEnum.Default;
        public ApplicationStateEnum ApplicationState => _applicationState;

        public void ApplicationStarted()
        {
            _applicationState = ApplicationStateEnum.Started;
        }
        
        public void ApplicationStopping()
        {
            _applicationState = ApplicationStateEnum.Stopping;
        }
        
        public void ApplicationStopped()
        {
            _applicationState = ApplicationStateEnum.Stopped;
        }
    }
}