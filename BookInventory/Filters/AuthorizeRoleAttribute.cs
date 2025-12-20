using BookInventory.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookInventory.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = SessionHelper.GetUser(context.HttpContext);

            if (user == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
                return;
            }

            if (!string.IsNullOrEmpty(_role) && user.Role != _role)
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
