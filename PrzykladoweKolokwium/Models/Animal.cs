namespace PrzykladoweKolokwium.Models
{
    public class Animal
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public int Owner_ID { get; set; }
    }
}
