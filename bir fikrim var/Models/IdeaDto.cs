namespace bir_fikrim_var.Models
{
    public class IdeaDto
    {
        public int IdeaId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
        public string AuthorName { get; set; }

        public int? LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
