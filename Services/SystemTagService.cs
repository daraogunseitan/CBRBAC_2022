using AuthAttempt2.Core;
using AuthAttempt2.Models;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace AuthAttempt2.Services
{
    public class SystemTagService : ISystemTagService
    {
        private readonly ILogger<SystemTagService> _logger;
        private readonly UserManager<User> _userManager;
        public SystemTagService(ILogger<SystemTagService> logger, UserManager<User> userManager)
            => (_logger, _userManager) = (logger, userManager);

        /// <summary>
        /// Add System Tags to a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="attributes"></param>
        /// <returns>True if successful</returns>
        public async Task<HttpResult<bool>> AddUserSystemTags(string username, List<string> attributes)
        {
            username = username.Trim().ToLower();
            //Get user from database
            User user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Result.Fail<bool>(HttpStatusCode.NotFound, "User " + username + " can not be found in the database");
            }
            else
            {
                //Get the system tags.
                var claims = await _userManager.GetClaimsAsync(user);
                Claim? SystagClaim = claims.FirstOrDefault(c => c.Type.Equals("SystemTags"));
                List<string>? tags = SystagClaim?.Value.Split("/").ToList();
                //Add all the tags to the claim
                if (tags != null)
                {
                    foreach (var attribute in attributes)
                    {
                        if (!tags.Contains(attribute))
                        {
                            tags.Add(attribute);
                        }

                    }

                }
                else
                {
                    tags = attributes;
                }

                string attributesToString = string.Join("/", tags);

                //Remove old claims and add new claims
                await _userManager.RemoveClaimAsync(user, SystagClaim);
                await _userManager.AddClaimAsync(user, new Claim("SystemTags", attributesToString));

                return Result.Ok<bool>(HttpStatusCode.OK, true);

            }
        }

        public async Task<HttpResult<List<string>>> GetUserSystemTags(string username)
        {
            username = username.Trim().ToLower();
            //get user from database
            User user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Result.Fail<List<string>>(HttpStatusCode.NotFound,"User " + username + " can not be found in the database");
            }
            else
            {
                //get the system tags and return them.
                var claims = await _userManager.GetClaimsAsync(user);
                var tagclaims = claims.FirstOrDefault(c => c.Type.Equals("SystemTags"));
                if (tagclaims != null)
                {
                    var rtrn = tagclaims.Value.Split("/");
                    return Result.Ok<List<string>>(HttpStatusCode.OK,rtrn.ToList());
                }

                return Result.Fail<List<string>>(HttpStatusCode.NotFound, "Can't find " + user.UserName + "'s system claims");
            }
        }

        public async Task<HttpResult<bool>> CheckSystemTagMatchAsync(string username, string record, string systemTags)
        {
            List<string> filetags = systemTags.Split("/").ToList();
            HttpResult<List<string>>? result = await GetUserSystemTags(username.ToLower());
            if (result.IsSuccess) { 
                List<string>? tags = result?.Value;
                //intersect both lists
                var finalList = filetags.Intersect(tags);

                if (finalList.Any())
                {
                    return Result.Ok<bool>(HttpStatusCode.OK, true);
                }
            }

            return Result.Fail<bool>(HttpStatusCode.NotFound, "No SystemTag match was found");
        }
    }
}
