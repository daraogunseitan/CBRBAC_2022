using AuthAttempt2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthAttempt2.Core;
using System.Net;
using System.Security.Claims;

namespace AuthAttempt2.Services
{
    public class RecordAuthorisationService : IRecordAuthorisationService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IJSONAccessoriesService _jsonAccessories;
        private readonly ILogger<RecordAuthorisationService> _logger;
        private readonly List<string> avaliable_actions = new List<string> { "delete", "read", "create", "update"};
        public RecordAuthorisationService(ILogger<RecordAuthorisationService> logger, UserManager<User> userManager, RoleManager<Roles> roleManager, IJSONAccessoriesService JSONAccessories)
            => (_logger,_userManager, _roleManager, _jsonAccessories) = (logger, userManager, roleManager, JSONAccessories);
        public async Task<HttpResult<bool>> CheckAccess(string username, string hierarchy, string filePosition)
        {

            //Get the user from database 
            User? user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                //check if the user is in the same hierarchy
                var chckr = claims.FirstOrDefault(c => c.Value.Contains(hierarchy));

                //load hierarchy from file
                (bool success, List<Node>? nodes, string? error)  = _jsonAccessories.loadjsonfromdisk(hierarchy.ToLower());
                if (!success)
                {
                    return Result.Fail<bool>(HttpStatusCode.NotFound, hierarchy + " hierarchy was not found");
                }
                if (chckr != null)
                {
                    //returns hierarchy and location in hierarchy for user
                    string[] hclaims = chckr.Value.Split('/');

                    //Find postcode of user and file
                    string? userPostCode = nodes?.Find(u => u.Name.Equals(hclaims[1]))?.PostCode;
                    string? filePostCode = nodes?.Find(f => f.Name.Equals(filePosition))?.PostCode;

                    if(filePostCode == null)
                    {
                        return Result.Fail<bool>(HttpStatusCode.Unauthorized, "File position " + filePosition + " not found in hiearchy " + hierarchy);
                    }
                    
                    //Determine access
                    int result = Math.Min(userPostCode.Length, filePostCode.Length);
                    if (userPostCode.Substring(0,result) == filePostCode.Substring(0, result))
                    {
                        return Result.Ok<bool>(HttpStatusCode.OK, true);
                    }
                    else
                    {
                        return Result.Fail<bool>(HttpStatusCode.Unauthorized,"You don't have access to this file because of position in " + hierarchy + " hierarchy");
                    }

                }
                else
                {
                    return Result.Fail<bool>(HttpStatusCode.NotFound,"User " + username + " does not belong to the same hierarchy as file");
                }
            }
            else
            {
                return Result.Fail<bool>(HttpStatusCode.NotFound,"User " + username + " can not be found.");
            }


        }

        public async Task<HttpResult<bool>> CheckPremissions(string username, string action, string fileowner) //Realistic input would be finding and user
        {
            
            //Get the user from database 
            var user = await _userManager.FindByNameAsync(username);

            var role = await _userManager.GetRolesAsync(user);

            if (!avaliable_actions.Contains(action))
            {
                return Result.Fail<bool>(HttpStatusCode.NotFound, "The action " + action + " is not a valid action");
            }

            if (role == null)
            {
                return Result.Fail<bool>(HttpStatusCode.NotFound,"Role for user " + user.UserName + " can't be found.");
            }

            Roles rtrn = await _roleManager.FindByNameAsync(role.First());
            IList<Claim>? roleClaims = await _roleManager.GetClaimsAsync(rtrn);
            //Owner check 
            if (!roleClaims.FirstOrDefault(c => c.Type.Equals("OwnerPermissions")).Equals(null))
            {
                if (fileowner.ToLower().Equals(user.UserName))
                {
                    if (roleClaims.FirstOrDefault(c => c.Type.Equals("OwnerPermissions")).Value.Contains(action.ToLower().First()))
                    {
                        return Result.Ok(HttpStatusCode.OK,true);
                    }
                } 
            }

            //Future work: System check
            //if(rtrn.SystemPermissions != null)
            //{

            //}

            //Default check 
            if (!roleClaims.FirstOrDefault(c => c.Type.Equals("DefaultPermissions")).Equals(null))
            {
                if (roleClaims.FirstOrDefault(c => c.Type.Equals("DefaultPermissions")).Value.Contains(action.ToLower().First()))
                {
                    return Result.Ok(HttpStatusCode.OK, true);
                } 
            }



            return Result.Fail<bool>(HttpStatusCode.Unauthorized, "You do not have needed permissions to access this file");



        }
    }
}
