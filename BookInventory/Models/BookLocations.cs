namespace BookInventory.Models
{
    public class BookLocations
    {
        public int Almirah { get; set; }
        public int Bed { get; set; }
        public int Box { get; set; }
        public OtherLocation Other { get; set; }
    }

    public class OtherLocation
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
