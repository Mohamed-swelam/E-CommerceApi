namespace Infrastructure.Seeders.SeedingHelpers
{
    public class SeedUserDto
    {
        public required string UserName { get; set; }

        public required string FullName { get; set; }

        public required string Email { get; set; }

        public required bool EmailConfirmed { get; set; }
        public required string Password { get; set; }

        public required string Role { get; set; }
    }
}
