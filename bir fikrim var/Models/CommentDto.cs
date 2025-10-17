namespace bir_fikrim_var.Models
{
    public class CommentDto
    {
        public int CommentId { get; set; }

        public int IdeaId { get; set; }

        public int UserId { get; set; }

        public string Content { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public string authorName { get; set; } = null!;


    }
}
