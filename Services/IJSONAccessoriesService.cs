using AuthAttempt2.Models;

namespace AuthAttempt2.Services
{
    public interface IJSONAccessoriesService
    {
        public (bool success, List<Node>? nodes, string? error) loadjsonfromdisk(string filename);
        public bool createJsonfile(string filename);
        public bool savejsontodisk(string filename, List<Node>? nodes);
        public (bool success, string? errors) JsonFileInsert(string filename, Node newNode);
    }
}
