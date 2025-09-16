namespace bir_fikrim_var.Models
{
    public class IdeasDetailsViewModel
    {
        public IdeaDto Idea { get; set; }
        public List<CommentDto> Comments { get; set; }
        public int LikeCount { get; set; }
        public bool UserLiked { get; set; }
    }
}
