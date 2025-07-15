using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Chamran.Deed.Posts;

namespace Chamran.Deed.Web.Host.Controllers.Migration
{
    [Route("api/services/app/PostMigration")]
    public class PostMigrationController : AbpController
    {
        private readonly PostMigrationAppService _migrationService;

        public PostMigrationController(PostMigrationAppService migrationService)
        {
            _migrationService = migrationService;
        }

        [HttpPost("Run")]
        public async Task<IActionResult> RunMigrationAsync()
        {
            await _migrationService.ProcessExistingPostsAsync();
            return Ok("Migration completed successfully.");
        }
    }
}