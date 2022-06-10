using AuthAttempt2.Core;
using AuthAttempt2.Models;
using System.Net;

namespace AuthAttempt2.Services
{
    public class HierarchyService : IHierarchyService
    {
        private readonly IJSONAccessoriesService _JSONAccessories;

        public HierarchyService(IJSONAccessoriesService JSONAccessories)
            => (_JSONAccessories) = (JSONAccessories);


        public async Task<HttpResult<bool>> CreateHierarchy(string title)
        {
            title = title.ToLower();
            bool rtrn = _JSONAccessories.createJsonfile(title);
            //Future Work: If encountered return the option to delete if the log in person has the permission
            if (rtrn)
            {
                return Result.Ok<bool>(HttpStatusCode.Created,true);
            }
            return Result.Fail<bool>(HttpStatusCode.BadRequest,title + ".json could not be created");

        }
        public async Task<HttpResult<bool>> AddToHierarchy(string title, string parentnode, string newNodeName)
        {
            title = title.ToLower();
            parentnode = parentnode.ToLower();
            newNodeName = newNodeName.ToLower();

            //Future work: Find better ways to generate the postcodes.
            (bool? loaded, List<Node>? nodes, string? errors) = _JSONAccessories.loadjsonfromdisk(title);
            Console.WriteLine(nodes?.Count);
            var parentcheck = nodes?.FirstOrDefault(n => n.Name == parentnode);
            var newnodecheck = nodes?.FirstOrDefault(n => n.Name == newNodeName);

            //If first node create head node.
            if (nodes?.Count == 0)
            {
                Node node = new Node();
                node.Name = newNodeName;
                node.ParentPostCode = "None";
                node.PostCode = "1";
                node.ChildrenNumber = 0;

                nodes?.Add(node);
            }
            else if (parentcheck != null && newnodecheck == null)
            {
                Node? pNode = nodes?.Find(n => n.Name == parentnode);
                Node node = new Node();
                node.Name = newNodeName;
                node.ParentPostCode = pNode.PostCode;
                pNode.ChildrenNumber++;
                node.PostCode = pNode.PostCode + "." + pNode.ChildrenNumber;
                node.ChildrenNumber = 0;

                nodes?.Add(node);
            }
            else
            {
                return Result.Fail<bool>(HttpStatusCode.BadRequest,"Could not add " + newNodeName);
            }

            if (_JSONAccessories.savejsontodisk(title, nodes))
            {
                return Result.Ok(HttpStatusCode.OK,true);
            }

            return Result.Fail<bool>(HttpStatusCode.BadRequest,"Could not add " + newNodeName);

        }
    }
}
