﻿namespace Chamran.Deed.Info.Dtos
{
    public class GetCommentLikeForViewDto
    {
        public CommentLikeDto CommentLike { get; set; }

        public string CommentCommentCaption { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int PostId { get; set; }
    }
}