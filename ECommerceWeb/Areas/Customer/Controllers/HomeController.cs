using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower();
                products = products.Where(p => p.Title.ToLower().Contains(searchLower) || p.Author.ToLower().Contains(searchLower));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price100 >= (double)minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price100 <= (double)maxPrice.Value);
            }

            ViewBag.Categories = _unitOfWork.Category.GetAll(); // For dropdown
            return View(products);
        }


        public IActionResult Details(int productId)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages");

            var reviews = _unitOfWork.Review.GetAll(r => r.ProductId == productId, includeProperties: "ApplicationUser").ToList();

            var cart = new ShoppingCart
            {
                Product = product,
                Count = 1,
                ProductId = productId
            };

            var viewModel = new ProductDetailsVM
            {
                ShoppingCart = cart,
                Reviews = reviews,
                NewReview = new Review { ProductId = productId }
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);
            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";



            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingReview = _unitOfWork.Review.Get(r => r.ProductId == review.ProductId && r.UserId == userId);
            if (existingReview != null)
            {
                TempData["error"] = "You have already submitted a review for this product.";
                return RedirectToAction("Details", new { productId = review.ProductId });
            }

            review.UserId = userId;

            _unitOfWork.Review.Add(review);
            _unitOfWork.Save();

            TempData["success"] = "Review submitted successfully.";
            return RedirectToAction("Details", new { productId = review.ProductId });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}