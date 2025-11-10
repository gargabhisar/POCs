namespace DelhiveryOne.Models
{
    public class DelhiveryPayload
    {
        // 👇 Automatically initialize default pickup location
        public DelhiveryPayload()
        {
            pickup_location = new PickupLocation();
        }
        
        public List<Shipment> shipments { get; set; }
        public PickupLocation pickup_location { get; set; }
    }
}
