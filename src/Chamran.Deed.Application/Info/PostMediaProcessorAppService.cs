using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Chamran.Deed.Info;
using Chamran.Deed;
using SixLabors.ImageSharp.Formats.Jpeg;
using Chamran.Deed.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class PostMediaProcessorAppService : DeedAppServiceBase, IPostMediaProcessorAppService, ITransientDependency
{
    private readonly IRepository<Post> _postRepository;
    private readonly IBinaryObjectManager _binaryObjectManager;
    //private readonly IWebHostEnvironment _env;
    private readonly ILogger<PostMediaProcessorAppService> _logger;

    public PostMediaProcessorAppService(
        IRepository<Post> postRepository,
        IBinaryObjectManager binaryObjectManager,
        //IWebHostEnvironment env,
        ILogger<PostMediaProcessorAppService> logger)
    {
        _postRepository = postRepository;
        _binaryObjectManager = binaryObjectManager;
        //_env = env;
        _logger = logger;
    }

    public async Task RegenerateAllThumbnailsAndPreviewsAsync()
    {
        var allPosts = await _postRepository.GetAllListAsync();
        //var webRoot = _env.WebRootPath;

        //Directory.CreateDirectory(Path.Combine(webRoot, "thumbs"));
        //Directory.CreateDirectory(Path.Combine(webRoot, "previews"));
        //Directory.CreateDirectory(Path.Combine(webRoot, "temp"));

        foreach (var post in allPosts)
        {
            var fileIds = new List<Guid?> {
                post.PostFile, post.PostFile2, post.PostFile3, post.PostFile4, post.PostFile5,
                post.PostFile6, post.PostFile7, post.PostFile8, post.PostFile9, post.PostFile10
            };

            foreach (var fileId in fileIds)
            {
                if (!fileId.HasValue) continue;

                var binary = await _binaryObjectManager.GetOrNullAsync(fileId.Value);
                if (binary == null || string.IsNullOrWhiteSpace(binary.Description)) continue;

                var ext = Path.GetExtension(binary.Description).ToLower();
                //var fileName = $"{fileId}{ext}";
                //var tempPath = Path.Combine(webRoot, "temp", fileName);
                //await File.WriteAllBytesAsync(tempPath, binary.Bytes);

                try
                {
                    if (IsImage(ext))
                    {
                        post.PostFileThumb = await GenerateImageThumbnailAsync(post.Id, binary.Bytes);
                    }
                    //await GenerateImageThumbnailAsync(post.Id, fileId.Value, tempPath);
                    else if (IsVideo(ext))
                    {
                        post.PostVideoPreview = await GenerateVideoPreviewAsync(post.Id, binary.Bytes, ext);
                    }
                    //await GenerateVideoPreviewAsync(post.Id, fileId.Value, tempPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[WARN] PostId={post.Id} FileId={fileId}: {ex.Message}");
                }
                //finally
                //{
                //    File.Delete(tempPath);
                //}
            }
            await _postRepository.UpdateAsync(post);
        }
    }

    private static bool IsImage(string ext) => ext is ".jpg" or ".jpeg" or ".png";
    private static bool IsVideo(string ext) => ext is ".mp4" or ".mov" or ".ts";

    //private async Task GenerateImageThumbnailAsync(long postId, Guid fileId, string sourcePath)
    private async Task<string> GenerateImageThumbnailAsync(long postId, byte[] sourceBytes)
    {
        //var thumbPath = Path.Combine(_env.WebRootPath, "thumbs", $"thumb_{postId}.jpg");
        //if (File.Exists(thumbPath)) return;
        using var ms = new MemoryStream(sourceBytes);
        using var img = Image.Load(ms);

        //var args = $"-y \"{sourcePath}\" -resize 400x400 \"{thumbPath}\"";
        img.Mutate(x => x.AutoOrient());
        img.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(300, 300) }));

        //var startInfo = new ProcessStartInfo
        //{
        //    FileName = "convert",
        //    Arguments = args,
        //    RedirectStandardOutput = true,
        //    RedirectStandardError = true,
        //    UseShellExecute = false,
        //    CreateNoWindow = true
        //};
        using var outStream = new MemoryStream();
        await img.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 80 });

        //await RunProcessAsync(startInfo);
        var binary = new BinaryObject(AbpSession.TenantId, outStream.ToArray(), BinarySourceType.Post, $"thumb_{postId}.jpg");
        await _binaryObjectManager.SaveAsync(binary);

        return binary.Id.ToString();
    }

    //private async Task GenerateVideoPreviewAsync(long postId, Guid fileId, string sourcePath)
    private async Task<string> GenerateVideoPreviewAsync(long postId, byte[] sourceBytes, string ext)
    {
        //var previewPath = Path.Combine(_env.WebRootPath, "previews", $"preview_{postId}.mp4");
        //if (File.Exists(previewPath)) return;
        var tempDir = Path.GetTempPath();
        var inputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}{ext}");
        var outputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.gif");

        //var args = $"-y -i \"{sourcePath}\" -ss 00:00:01.000 -t 00:00:02.000 -c:v libx264 -preset ultrafast -an \"{previewPath}\"";
        await File.WriteAllBytesAsync(inputPath, sourceBytes);

        //var startInfo = new ProcessStartInfo
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            //Arguments = args,
            Arguments =
                "-y -hide_banner -loglevel error -ss 0 -t 5 " +
                $"-i \"{inputPath}\" " +
                "-filter_complex \"fps=10,scale=320:-1:flags=lanczos,split[s0][s1];[s0]palettegen=stats_mode=full[p];[s1][p]paletteuse=new=1\" " +
                "-f gif -loop 0 " +
                $"\"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        //    await RunProcessAsync(startInfo);
        //}

        //private async Task RunProcessAsync(ProcessStartInfo startInfo)
        //{
        //    using var process = new Process { StartInfo = startInfo };
        //    process.Start();
        using var proc = Process.Start(psi);
        if (proc != null)
        {
            await proc.WaitForExitAsync();
            if (proc.ExitCode != 0)
            {
                var err = await proc.StandardError.ReadToEndAsync();
                throw new UserFriendlyException($"Thumbnail/Preview generation failed:\n{err}");
            }
        }

        //string stderr = await process.StandardError.ReadToEndAsync();
        //string stdout = await process.StandardOutput.ReadToEndAsync();
        var bytes = await File.ReadAllBytesAsync(outputPath);
        File.Delete(inputPath);
        File.Delete(outputPath);

        //await process.WaitForExitAsync();
        var binary = new BinaryObject(AbpSession.TenantId, bytes, BinarySourceType.Post, $"preview_{postId}.gif");
        await _binaryObjectManager.SaveAsync(binary);

        //if (process.ExitCode != 0)
        //{
        //    throw new UserFriendlyException($"Thumbnail/Preview generation failed:\n{stderr}");
        //}
        return binary.Id.ToString();
    }
}
