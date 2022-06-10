using Newtonsoft.Json;
using AuthAttempt2.Models;
namespace AuthAttempt2.Services
{
    public class JSONAccessoryService : IJSONAccessoriesService
    {
        //Furture work: 
        //File paths to save Json have to change to desired locations 

        public List<Node>? nodes = null;
        public JSONAccessoryService()
        {

        }
        public (bool success, List<Node>? nodes, string? error) loadjsonfromdisk(string filename)
        {
            string readfile = "C:/Users/darac/source/repos/AuthAttempt2/HierarchyJsons/" + filename.ToLower() + ".json";
            if (System.IO.File.Exists(readfile))
            {
                List<Node>? list = JsonConvert.DeserializeObject<List<Node>>(System.IO.File.ReadAllText(readfile));
                if(list != null)
                    nodes = list;
                else
                    nodes = new List<Node>();
            } else
            {
                return (false, null, "A hierarchy with the name " + filename + "could not be found");
            }
        
            return (true, nodes, null);
        }

        public bool createJsonfile(string filename)
        {
            try
            {
                string writefile = "C:/Users/darac/source/repos/AuthAttempt2/HierarchyJsons/" + filename.ToLower() + ".json";
                var json = JsonConvert.SerializeObject("", Formatting.Indented);
                System.IO.File.WriteAllText(writefile, json);
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }
        
        public bool savejsontodisk(string filename, List<Node>? nodes)
        {
            try
            {
                string writefile = "C:/Users/darac/source/repos/AuthAttempt2/HierarchyJsons/" + filename.ToLower() + ".json";
                var json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
        
                if (System.IO.File.Exists(writefile))
                {
                    System.IO.File.Delete(writefile);
                }
        
                System.IO.File.WriteAllText(writefile, json);
            }
            catch (Exception)
            {
        
                return false;
            }
        
        
            return true;
        }
        
        public (bool success, string? errors) JsonFileInsert(string filename, Node newNode)
        {
            string errMsg = "";
            loadjsonfromdisk(filename.ToLower());
            if (nodes == null)
            {
                nodes = new List<Node>();
            }
        
            //check if the node already exists in the hierarchy
            var check = nodes.FirstOrDefault(n => n.PostCode == newNode.PostCode);
        
            if (check != null)
            {
                //Add new node to hierarchy
                nodes.Add(newNode); 
        
                //save list to disk
                bool savesuccess = savejsontodisk(filename.ToLower(), nodes);
                if (!savesuccess)
                {
                    //construct error message
                    errMsg = "Error writing JSON to disk\n" + newNode.PostCode + " already exosts in hierarchy";
                }
                return (false, errMsg);
        
        
            }
        return (true, null);
        }
    }
}
