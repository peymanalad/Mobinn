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
using Chamran.Deed.Storage;
using Chamran.Deed.Common;

namespace Chamran.Deed.Posts
{
    public class PostMigrationAppService : ApplicationService
    {
        private readonly IRepository<Post> _postRepository;
        //private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBinaryObjectManager _binaryObjectManager;

        //public PostMigrationAppService(IRepository<Post> postRepository, IWebHostEnvironment hostingEnvironment)
        public PostMigrationAppService(IRepository<Post> postRepository, IBinaryObjectManager binaryObjectManager)
        {
            _postRepository = postRepository;
            //_hostingEnvironment = hostingEnvironment;
            _binaryObjectManager = binaryObjectManager;
        }

        public async Task ProcessExistingPostsAsync()
        {
            //var webRoot = _hostingEnvironment.WebRootPath;

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
                    //post.PostFileThumb = await GenerateThumbnailAsync(file.Bytes, post.Id, webRoot);
                    post.PostFileThumb = await GenerateThumbnailAsync(file.Bytes, post.Id);
                }
                else if (ext == ".mp4" || ext == ".mov")
                {
                    //var videoPath = Path.Combine(webRoot, "videos", $"{post.Id}{ext}");
                    //Directory.CreateDirectory(Path.GetDirectoryName(videoPath));
                    //await File.WriteAllBytesAsync(videoPath, file.Bytes);

                    //post.PostVideoPreview = await GenerateVideoPreviewAsync(videoPath, webRoot, post.Id);
                    post.PostVideoPreview = await GenerateVideoPreviewAsync(file.Bytes, ext, post.Id);
                }

                await _postRepository.UpdateAsync(post);
            }
        }

        //private async Task<string> GenerateThumbnailAsync(byte[] fileBytes, int postId, string webRoot)
        private async Task<string> GenerateThumbnailAsync(byte[] fileBytes, int postId)
        {
            using var image = Image.Load(fileBytes);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(400, 400)
            }));

            //var fileName = $"thumb_{postId}.jpg";
            //var path = Path.Combine(webRoot, "thumbs", fileName);
            //Directory.CreateDirectory(Path.GetDirectoryName(path));
            //await image.SaveAsJpegAsync(path, new JpegEncoder { Quality = 80 });
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 80 });

            var binary = new BinaryObject(AbpSession.TenantId, ms.ToArray(), BinarySourceType.Post, $"thumb_{postId}.jpg");
            await _binaryObjectManager.SaveAsync(binary);

            //return $"/thumbs/{fileName}";
            return binary.Id.ToString();
        }


        private async Task<string> GenerateVideoPreviewAsync(byte[] videoBytes, string ext, int postId)
        {
            //var tempDir = Path.GetTempPath();
            //var inputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}{ext}");
            //var outputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.gif");

            //await File.WriteAllBytesAsync(inputPath, videoBytes);

            //var psi = new System.Diagnostics.ProcessStartInfo
            //{
            //    FileName = "ffmpeg",
            //    Arguments =
            //        "-y -hide_banner -loglevel error -ss 0 -t 5 " +
            //        $"-i \"{inputPath}\" " +
            //        "-filter_complex \"fps=10,scale=320:-1:flags=lanczos,split[s0][s1];[s0]palettegen=stats_mode=full[p];[s1][p]paletteuse=new=1\" " +
            //        "-f gif -loop 0 " +
            //        $"\"{outputPath}\"",
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true
            //};

            //using var proc = System.Diagnostics.Process.Start(psi);
            //if (proc != null)
            //{
            //    await proc.WaitForExitAsync();
            //    if (proc.ExitCode != 0)
            //    {
            //        var err = await proc.StandardError.ReadToEndAsync();
            //        throw new Exception($"ffmpeg failed: {err}");
            //    }
            //}

            //var bytes = await File.ReadAllBytesAsync(outputPath);
            //File.Delete(inputPath);
            //File.Delete(outputPath);

            //var binary = new BinaryObject(AbpSession.TenantId, bytes, BinarySourceType.Post, $"preview_{postId}.gif");
            await Task.Yield();
            using var inputStream = new MemoryStream(videoBytes);
            using var outputStream = new MemoryStream();

            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(new StreamPipeSink(outputStream), options => options
                    .WithVideoCodec("gif")
                    .WithCustomArgument("-vf \"fps=10,scale=320:-1:flags=lanczos\" -t 5 -loop 0")
                    .ForceFormat("gif"))
                .ProcessAsynchronously();

            var binary = new BinaryObject(AbpSession.TenantId, outputStream.ToArray(), BinarySourceType.Post, $"preview_{postId}.gif");
            await _binaryObjectManager.SaveAsync(binary);

            return binary.Id.ToString();
        }

    }
}
