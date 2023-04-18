namespace PrzykladoweKolokwium.Models.DTOs
{
    public class AnimalWithOwner
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public Owner Owner { get; set; } = null!;
    }
}
