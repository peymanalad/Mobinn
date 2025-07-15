using System.Threading.Tasks;

public interface IPostMediaProcessorAppService
{
    Task RegenerateAllThumbnailsAndPreviewsAsync();
}