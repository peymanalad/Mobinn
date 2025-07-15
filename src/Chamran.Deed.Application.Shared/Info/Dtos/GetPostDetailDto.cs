public class GetPostDetailDto
{
    public int Id { get; set; }
    public string PostTitle { get; set; }
    public string PostCaption { get; set; }
    public string[] AttachmentUrls { get; set; }  // full file URLs
    public bool IsVideo { get; set; }
}