using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chamran.Deed.Mobile.MAUI.Models.Deed;
using SQLite;
namespace Chamran.Deed.Mobile.MAUI.Services.Deed
{
    public class GetExploreDataService
    {
        readonly string _dbPath;
        private SQLiteAsyncConnection conn;
        public GetExploreDataService(string dbPath)
        {
            _dbPath = dbPath;
        }

        private async Task InitAsync()
        {
            try
            {
                // Don't Create database if it exists
                if (conn != null)
                    return;
                // Create database and ExploreDataModel Table
                conn = new SQLiteAsyncConnection(_dbPath);
                await conn.CreateTableAsync<ExploreDataModel>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<ExploreDataModel>> GetExploreDataAsync()
        {
            await InitAsync();
            return await conn.Table<ExploreDataModel>().ToListAsync();
        }
        public async Task<ExploreDataModel> CreateOrUpdateExploreDataAsync(
            ExploreDataModel paramExploreDataModel)
        {
            var _category = await conn.FindAsync<ExploreDataModel>(c => c.Id == paramExploreDataModel.Id);

            if (_category != null)
                // Insert
                await conn.UpdateAsync(paramExploreDataModel);
            else
            {
                await conn.InsertAsync(paramExploreDataModel);

            }
            // return the object with the
            // auto incremented Id populated
            return paramExploreDataModel;
        }
        public async Task<ExploreDataModel> UpdateExploreDataAsync(
            ExploreDataModel paramExploreDataModel)
        {
            // Update
            await conn.UpdateAsync(paramExploreDataModel);
            // Return the updated object
            return paramExploreDataModel;
        }
        public async Task<ExploreDataModel> DeleteExploreDataAsync(
            ExploreDataModel paramExploreDataModel)
        {
            // Delete
            await conn.DeleteAsync(paramExploreDataModel);
            return paramExploreDataModel;
        }
    }


}
