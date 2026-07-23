using Microsoft.AspNetCore.Identity;

namespace ZaatMarket.Data
{
    public class ApplicationUser : IdentityUser
    {
      
        public string? DisplayName { get; set; }

        public string? Location { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? SellerDescription { get; set; }

        public string AccountType { get; set; } = "Individual";
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

        public string? ProfilePictureUrl { get; set; }
        public bool IsPremiumAgent { get; set; } = false;
        public DateTime? PremiumExpirationUtc { get; set; }
    }
}