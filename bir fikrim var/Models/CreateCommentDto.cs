namespace bir_fikrim_var.Models
{
    public class CreateCommentDTO
    {
        public int IdeaId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

}
