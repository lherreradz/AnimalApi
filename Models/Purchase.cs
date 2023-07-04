namespace AnimalAPI.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public float FreightPrice { get; set; }
        public int DiscountPercentage { get; set; } 
        public float TotalPrice { get; set; }
    }
}
