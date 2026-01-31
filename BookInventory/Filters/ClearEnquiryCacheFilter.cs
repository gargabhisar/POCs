using Microsoft.AspNetCore.Mvc.Filters;

namespace BookInventory.Filters
{
    public class ClearEnquiryCacheFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller =
                context.RouteData.Values["controller"]?.ToString();

            var action =
                context.RouteData.Values["action"]?.ToString();

            // Cache should exist ONLY for Enquiry/List
            if (!(controller == "Enquiry" && action == "List"))
            {
                context.HttpContext.Session.Remove("ENQUIRY_LIST_CACHE");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}