using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Invoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }   // ✅ STRING
        public int InvoiceNo { get; set; }   // ✅ NEW
        public DateTime InvoiceDate { get; set; }
        public string PaymentMode { get; set; }   // Cash / UPI / Card
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class InvoiceItem
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public int MRP { get; set; }
        public int DiscountPercent { get; set; }   // integer only
        public int Quantity { get; set; }
        public int FinalPrice { get; set; }         // rounded value
        public int LineTotal { get; set; }          // FinalPrice * Quantity
        public string Remark { get; set; }
    }
}
