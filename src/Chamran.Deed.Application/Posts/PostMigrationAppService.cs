using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chamran.Deed.Info;

namespace Chamran.Deed.Posts
{
    public class PostMigrationAppService : ApplicationService
    {
        private readonly IRepository<Post> _postRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PostMigrationAppService(IRepository<Post> postRepository, IWebHostEnvironment hostingEnvironment)
        {
            _postRepository = postRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task ProcessExistingPostsAsync()
        {
            var webRoot = _hostingEnvironment.WebRootPath;

            var posts = await _postRepository.GetAll()
                .Include(x => x.AppBinaryObjectFk)
                .Where(x => !x.IsDeleted && x.AppBinaryObjectFk != null)
                .ToListAsync();

            foreach (var post in posts)
            {
                var file = post.AppBinaryObjectFk;
                var ext = Path.GetExtension(file.Description)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || file.Bytes == null)
                    continue;

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    post.PostFileThumb = await GenerateThumbnailAsync(file.Bytes, post.Id, webRoot);
                }
                else if (ext == ".mp4" || ext == ".mov")
                {
                    var videoPath = Path.Combine(webRoot, "videos", $"{post.Id}{ext}");
                    Directory.CreateDirectory(Path.GetDirectoryName(videoPath));
                    await File.WriteAllBytesAsync(videoPath, file.Bytes);

                    //post.PostVideoPreview = await GenerateVideoPreviewAsync(videoPath, webRoot, post.Id);
                }

                await _postRepository.UpdateAsync(post);
            }
        }

        private async Task<string> GenerateThumbnailAsync(byte[] fileBytes, int postId, string webRoot)
        {
            using var image = Image.Load(fileBytes);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(400, 400)
            }));

            var fileName = $"thumb_{postId}.jpg";
            var path = Path.Combine(webRoot, "thumbs", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await image.SaveAsJpegAsync(path, new JpegEncoder { Quality = 80 });

            return $"/thumbs/{fileName}";
        }

    }
}
