namespace BookInventory.Models
{
    public class BookSales
    {
        public int InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string BookName { get; set; }
        public int Quantity { get; set; }
        public int MRP { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal SoldAt { get; set; }
        public string PaymentMode { get; set; }
        public string Remark { get; set; }
    }
}
