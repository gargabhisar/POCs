namespace BookInventory.Models
{
    public class ChangePasswordVM
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
