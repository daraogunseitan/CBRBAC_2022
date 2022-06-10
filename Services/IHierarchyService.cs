using AuthAttempt2.Core;

namespace AuthAttempt2.Services
{
    public interface IHierarchyService
    {
        public Task<HttpResult<bool>> CreateHierarchy(string title);
        public Task<HttpResult<bool>> AddToHierarchy(string title, string parentnode, string newNodeName);
    }
}
