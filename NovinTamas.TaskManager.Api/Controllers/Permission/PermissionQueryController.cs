using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Permission;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Permission;

namespace NovinTamas.TaskManager.Api.Controllers.Permission
{
    public class PermissionQueryController : BaseQueryController
    {
        public PermissionQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("GetMemberPermissions")]
        public async Task<ActionResult<List<MemberPermissionResult>>> GetMemberPermissions([FromBody] GetMemberPermissionsQuery query)
            => Ok(await Bus.Dispatch<GetMemberPermissionsQuery, List<MemberPermissionResult>>(query));
    }
}
