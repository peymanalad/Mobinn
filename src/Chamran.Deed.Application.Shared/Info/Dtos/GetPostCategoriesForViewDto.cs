using System;

public class GetPostCategoriesForViewDto
{
    public int Id { get; set; }
    public string PostGroupDescription { get; set; }

    public string PostGroupLatestPicFile { get; set; } 
    public Guid? PostGroupHeaderPicFile { get; set; }

    public bool? HasSubGroups { get; set; }
}