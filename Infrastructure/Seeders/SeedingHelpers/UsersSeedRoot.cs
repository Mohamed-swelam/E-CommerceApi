namespace Infrastructure.Seeders.SeedingHelpers
{
    public class UsersSeedRoot
    {
        public List<SeedUserDto> Sellers { get; set; }

        public List<SeedUserDto> Customers { get; set; }

        public SeedUserDto Admin { get; set; }
    }
}
