using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostCategoriesForViewDto
    {
        public int Id { get; set; }
        public string PostGroupDescription { get; set; }
        public Guid? PostGroupLatestPicFile { get; set; }
        public Guid? PostGroupHeaderPicFile { get; set; }

    }
}
