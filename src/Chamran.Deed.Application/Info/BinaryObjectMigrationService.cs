using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chamran.Deed.Storage
{
    public class BinaryObjectMigrationService : DeedAppServiceBase, ITransientDependency
    {
        private readonly IRepository<BinaryObject, Guid> _binaryObjectRepository;
        private readonly DbBinaryObjectManager _dbBinaryObjectManager;
        private readonly ILogger<BinaryObjectMigrationService> _logger;

        public BinaryObjectMigrationService(
            IRepository<BinaryObject, Guid> binaryObjectRepository,
            DbBinaryObjectManager dbBinaryObjectManager,
            ILogger<BinaryObjectMigrationService> logger)
        {
            _binaryObjectRepository = binaryObjectRepository ?? throw new ArgumentNullException(nameof(binaryObjectRepository));
            _dbBinaryObjectManager = dbBinaryObjectManager ?? throw new ArgumentNullException(nameof(dbBinaryObjectManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task MigrateAsync()
        {
            _logger.LogInformation("Starting binary object migration...");

            try
            {
                int pageSize = 20;
                int pageNumber = 1;
                bool hasMoreData = true;

                while (hasMoreData)
                {
                    var binaryObjects = await _binaryObjectRepository.GetAll()
                        .OrderBy(x => x.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (binaryObjects.Any())
                    {
                        foreach (var binaryObject in binaryObjects)
                        {
                            try
                            {
                                await _dbBinaryObjectManager.SaveAsync(binaryObject);
                                await _binaryObjectRepository.DeleteAsync(binaryObject);
                                _logger.LogInformation($"Successfully migrated binary object with ID: {binaryObject.Id}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error migrating binary object with ID: {binaryObject.Id}");
                            }
                        }
                        pageNumber++;
                    }
                    else
                    {
                        hasMoreData = false;
                    }
                }

                _logger.LogInformation("Binary object migration completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during binary object migration.");
            }
        }
    }
}
