namespace BookInventory.Models
{
    public class WhatsAppSendResult
    {
        public int HttpStatus { get; set; }
        public string RawResponse { get; set; }
        public string WaMessageId { get; set; }
    }
}
