namespace AnimalAPI.Models
{
    public class Animal
    {
        public int AnimalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Sex { get; set; } = string.Empty;
        public float Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
