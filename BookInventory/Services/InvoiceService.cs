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

        public List<Invoice> GetAll() => _repo.GetAll();

        public Invoice Get(string id) => _repo.GetById(id);
    }

}
