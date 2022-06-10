using AuthAttempt2.Core;

namespace AuthAttempt2.Services
{
    public interface IAuthorisationService
    {
        public Task<HttpResult<bool>> Authorise(string username, string hierarchy, string action, string recordPosition, string recordOwner, string record, string systemTags);
    }
}
