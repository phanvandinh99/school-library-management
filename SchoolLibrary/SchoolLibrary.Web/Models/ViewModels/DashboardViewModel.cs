using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBorrowedBooks { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        
        // Recent activity
        public List<BorrowRecord>? RecentBorrows { get; set; }
        public List<User>? RecentUsers { get; set; }
    }
}

