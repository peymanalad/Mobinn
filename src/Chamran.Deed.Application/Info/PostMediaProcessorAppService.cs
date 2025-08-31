using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Chamran.Deed.Info;
using Chamran.Deed;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Chamran.Deed.Common;

public class PostMediaProcessorAppService : DeedAppServiceBase, IPostMediaProcessorAppService, ITransientDependency
{
    private readonly IRepository<Post> _postRepository;
    private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly ILogger<PostMediaProcessorAppService> _logger;

    public PostMediaProcessorAppService(
        IRepository<Post> postRepository,
        IBinaryObjectManager binaryObjectManager,
        ILogger<PostMediaProcessorAppService> logger)
    {
        _postRepository = postRepository;
        _binaryObjectManager = binaryObjectManager;
        _logger = logger;
    }

    public async Task RegenerateAllThumbnailsAndPreviewsAsync()
    {
        var allPosts = await _postRepository.GetAllListAsync();

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

                try
                {
                    if (IsImage(ext))
                    {
                        post.PostFileThumb = await GenerateImageThumbnailAsync(post.Id, binary.Bytes);
                    }
                    else if (IsVideo(ext))
                    {
                        post.PostVideoPreview = await GenerateVideoPreviewAsync(post.Id, binary.Bytes, ext);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[WARN] PostId={post.Id} FileId={fileId}: {ex.Message}");
                }
            }

            await _postRepository.UpdateAsync(post);
        }
    }

    private static bool IsImage(string ext) => ext is ".jpg" or ".jpeg" or ".png";
    private static bool IsVideo(string ext) => ext is ".mp4" or ".mov" or ".ts";

    private async Task<string> GenerateImageThumbnailAsync(long postId, byte[] sourceBytes)
    {
        using var ms = new MemoryStream(sourceBytes);
        using var img = Image.Load(ms);

        img.Mutate(x => x.AutoOrient());
        img.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(300, 300) }));

        using var outStream = new MemoryStream();
        await img.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 80 });

        var binary = new BinaryObject(AbpSession.TenantId, outStream.ToArray(), BinarySourceType.Post, $"thumb_{postId}.jpg");
        await _binaryObjectManager.SaveAsync(binary);

        return binary.Id.ToString();
    }

    private async Task<string> GenerateVideoPreviewAsync(long postId, byte[] sourceBytes, string ext)
    {
        var tempDir = Path.GetTempPath();
        var inputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}{ext}");
        var outputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.gif");

        await File.WriteAllBytesAsync(inputPath, sourceBytes);

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
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

        var bytes = await File.ReadAllBytesAsync(outputPath);
        File.Delete(inputPath);
        File.Delete(outputPath);

        var binary = new BinaryObject(AbpSession.TenantId, bytes, BinarySourceType.Post, $"preview_{postId}.gif");
        await _binaryObjectManager.SaveAsync(binary);

        return binary.Id.ToString();
    }
}
