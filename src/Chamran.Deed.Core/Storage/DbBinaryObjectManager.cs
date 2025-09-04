//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Abp.Dependency;
//using Abp.Domain.Repositories;

//namespace Chamran.Deed.Storage
//{
//    public class DbBinaryObjectManager : IBinaryObjectManager, ITransientDependency
//    {
//        private readonly string _baseFolder;

//        public DbBinaryObjectManager()
//        {
//            _baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BinaryObjects");
//            if (!Directory.Exists(_baseFolder))
//            {
//                Directory.CreateDirectory(_baseFolder);
//            }
//        }
     
//        public async Task<BinaryObject> GetOrNullAsync(Guid id)
//        {
//            var directoryPath = GetDirectoryPath(id);
//            if (Directory.Exists(directoryPath))
//            {
//                var files = Directory.GetFiles(directoryPath);
//                if (files.Length > 0)
//                {
//                    var fullFilePath= files[0];
//                    var bytes = await File.ReadAllBytesAsync(fullFilePath);
//                    return new BinaryObject { Id = id, Bytes = bytes, Description = Path.GetFileName(files[0]) };
//                }
//            }
//            return null;
//        }


//        public async Task SaveAsync(BinaryObject file)
//        {
//            var directoryPath = GetDirectoryPath(file.Id);
//            if (!Directory.Exists(directoryPath))
//            {
//                Directory.CreateDirectory(directoryPath);
//            }

//            var filePath = GetFilePath(file.Id, file.Description);
//            await File.WriteAllBytesAsync(filePath, file.Bytes);
//        }

//        public async Task DeleteAsync(Guid id)
//        {
//            var directoryPath = GetDirectoryPath(id);
//            if (Directory.Exists(directoryPath))
//            {
//                var files = Directory.GetFiles(directoryPath);
//                if (files.Length > 0)
//                {
//                    var fileName = Path.GetFileNameWithoutExtension(files[0]);
//                    var filePath = GetFilePath(id, fileName);
//                    if (File.Exists(filePath))
//                    {
//                        File.Delete(filePath);
//                    }
//                }
//            }
//        }

//        private string GetDirectoryPath(Guid id)
//        {
//            return Path.Combine(_baseFolder, id.ToString()); 
//        }

//        private string GetFilePath(Guid id, string description)
//        {
//            var extension = Path.GetExtension(description);
//            var fileNameWithoutExtension = id.ToString();
//            return Path.Combine(GetDirectoryPath(id), $"{fileNameWithoutExtension}{extension}");
//        }
//    }
//}
