using AuthAttempt2.Core;
using System.Net;

namespace AuthAttempt2.Services
{
    public class AuthorisationService : IAuthorisationService
    {
        private readonly ISystemTagService _tagService;
        private readonly IRecordAuthorisationService _recordAuthService;
        private readonly ILogger<AuthorisationService> _logger;
        public AuthorisationService(ILogger<AuthorisationService> logger, ISystemTagService tagService, IRecordAuthorisationService recordAuthService)
            => (_logger, _tagService, _recordAuthService) = (logger, tagService, recordAuthService);

        public async Task<HttpResult<bool>> Authorise(string username, string hierarchy,string action, string recordPosition,string recordOwner, string record, string systemTags)
        {
            //Stage 1: Check Access
            var checkAccess = await _recordAuthService.CheckAccess(username, hierarchy, recordPosition);
            //Stage 2: Check System Tags
            if (checkAccess.Value)
            {
                var checksystags = await _tagService.CheckSystemTagMatchAsync(username, record, systemTags);
                if (checksystags.Value)
                {
                    //Stage 3: Check Permissions
                    var checkpermissions = await _recordAuthService.CheckPremissions(username, action, recordOwner);
                    if (checkpermissions.Value)
                    {
                        return Result.Ok(HttpStatusCode.OK, true);
                    } else
                    {
                        return Result.Fail<bool>(HttpStatusCode.Unauthorized, checkpermissions.Error);
                    }
                } else
                {
                    return Result.Fail<bool>(HttpStatusCode.Unauthorized, checksystags.Error);
                }
            } else
            {
                return Result.Fail<bool>(HttpStatusCode.Unauthorized, checkAccess.Error);
            }

        }

    }
}
