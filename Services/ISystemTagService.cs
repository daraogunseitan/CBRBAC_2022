using AuthAttempt2.Core;

namespace AuthAttempt2.Services
{
    public interface ISystemTagService
    {
        public Task<HttpResult<bool>> AddUserSystemTags(string username, List<string> attributes);
        public Task<HttpResult<List<string>>> GetUserSystemTags(string username);

        public Task<HttpResult<bool>> CheckSystemTagMatchAsync(string username, string record, string systemTags);
    }
}
