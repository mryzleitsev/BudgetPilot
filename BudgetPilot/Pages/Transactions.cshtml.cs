using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetPilot.Pages
{
    [Authorize]   
    public class Transactions : PageModel
    {
        public void OnGet() { }
    }
}