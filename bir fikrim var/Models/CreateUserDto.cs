﻿namespace bir_fikrim_var.Models
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
