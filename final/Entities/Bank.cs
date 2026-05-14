using static System.Runtime.InteropServices.JavaScript.JSType;

namespace final.Entities
{
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // CIB, NBE, QNB...
        public string LogoUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<VisaCard> VisaCards { get; set; } = new List<VisaCard>();
    }
}