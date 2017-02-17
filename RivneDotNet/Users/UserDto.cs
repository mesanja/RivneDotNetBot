using System;

namespace RivneDotNet.Users
{
    public class UserDto
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public Gender Gender { get; set; }

        public DateTime Date { get; set; }
    }
}