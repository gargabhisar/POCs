using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class PublishingInvoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Invoice identity
        public int InvoiceNo { get; set; }              // Shared counter
        public DateTime InvoiceDate { get; set; }

        // Author details
        public string AuthorName { get; set; }
        public string AuthorMobile { get; set; }
        public string AuthorAddress { get; set; }

        // Tax
        public string TaxType { get; set; }              // IGST / CGST_SGST

        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }

        // Services
        public List<PublishingInvoiceItem> Services { get; set; }

        // Totals
        public decimal SubTotal { get; set; }            // Sum of taxable amounts
        public decimal GrandTotal { get; set; }
    }
}
