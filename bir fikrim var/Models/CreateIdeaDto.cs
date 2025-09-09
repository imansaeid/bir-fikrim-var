namespace bir_fikrim_var.Models
{
    public class CreateIdeaDto
    {
            public int UserId { get; set; }  // hangi kullanıcı yazdı
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime? CreatedDate { get; set; }
    }

    }

