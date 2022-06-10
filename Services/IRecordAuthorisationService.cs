using AuthAttempt2.Core;

namespace AuthAttempt2.Services
{
    public interface IRecordAuthorisationService
    {
        public Task<HttpResult<bool>> CheckAccess(string username, string hierarchy, string recordPosition);
        public Task<HttpResult<bool>> CheckPremissions(string username, string action, string recordOwner);
    }
}
