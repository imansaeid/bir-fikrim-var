namespace bir_fikrim_var.Models
{
    public class UpdateUserDTO
    {
        public string FullName { get; set; } // İsim değiştirilebilir
        public string Email { get; set; }    // Mail değiştirilebilir
        public string Password { get; set; } // Şifre değiştirilebilir
    }

}
