namespace Infrastructure.Seeders.SeedingHelpers
{
    public class SellerProfileSeedDto
    {
        public string StoreName { get; set; }

        public byte[] Logo { get; set; }

        public string Description { get; set; }

        public string UserEmail { get; set; }

        public bool IsApproved { get; set; }

        public decimal TotalEarnings { get; set; }
    }
}
