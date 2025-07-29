namespace ECommerce.Models.ViewModels
{
    public class ProductDetailsVM
    {
        public ShoppingCart ShoppingCart { get; set; }
        public List<Review> Reviews { get; set; } = new();
        public Review NewReview { get; set; } = new();
    }
}
