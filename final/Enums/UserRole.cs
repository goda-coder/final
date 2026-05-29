using System.Text.Json.Serialization;

namespace final.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        None,
        User,
        Merchant,
        Admin
    }

    public enum MerchantStatus
    {
        Pending,
        Approved,
        Rejected,
        Suspended
    }

    // ✅ أضفنا Gender
    public enum Gender
    {
        Male,
        Female
    }
}