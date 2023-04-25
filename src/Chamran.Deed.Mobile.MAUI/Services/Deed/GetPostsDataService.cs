using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chamran.Deed.Mobile.MAUI.Models.Deed;
using SQLite;
namespace Chamran.Deed.Mobile.MAUI.Services.Deed
{
    public class GetPostsDataService
    {
        readonly string _dbPath;
        private SQLiteAsyncConnection conn;
        public GetPostsDataService(string dbPath)
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
                // Create database and PostsDataModel Table
                conn = new SQLiteAsyncConnection(_dbPath);
                await conn.CreateTableAsync<PostsDataModel>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<PostsDataModel>> GetPostsDataAsync(int id)
        {
            await InitAsync();
            if(id > 0) { return await conn.Table<PostsDataModel>().Where(x => x.CategoryId == id).ToListAsync(); }
            return await conn.Table<PostsDataModel>().ToListAsync();
        }
        public async Task<PostsDataModel> CreateOrUpdatePostsDataAsync(
            PostsDataModel paramPostsDataModel)
        {
            var _category = await conn.FindAsync<PostsDataModel>(c => c.Id == paramPostsDataModel.Id);

            if (_category != null)
                // Insert
                await conn.UpdateAsync(paramPostsDataModel);
            else
            {
                await conn.InsertAsync(paramPostsDataModel);

            }
            // return the object with the
            // auto incremented Id populated
            return paramPostsDataModel;
        }
        public async Task<PostsDataModel> UpdatePostsDataAsync(
            PostsDataModel paramPostsDataModel)
        {
            // Update
            await conn.UpdateAsync(paramPostsDataModel);
            // Return the updated object
            return paramPostsDataModel;
        }
        public async Task<PostsDataModel> DeletePostsDataAsync(
            PostsDataModel paramPostsDataModel)
        {
            // Delete
            await conn.DeleteAsync(paramPostsDataModel);
            return paramPostsDataModel;
        }
    }


}
