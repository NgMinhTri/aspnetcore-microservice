using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory
{
    public class PurchaseProductDto
    {
        public string ItemNo { get; set; }
        public string DocumentNo { get; set; }
        public EDocumentType DocumentType { get; set; }
        public string ExteralDocumentNo { get; set; }
        public int Quantity { get; set; }
    }
}
