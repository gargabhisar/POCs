using BookInventory.Models;
using BookInventory.Repositories;

namespace BookInventory.Services
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _repo;

        public InvoiceService(InvoiceRepository repo)
        {
            _repo = repo;
        }

        public void Save(Invoice invoice)
        {
            _repo.Save(invoice);
        }
    }

}
