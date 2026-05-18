using MVC_CRUD.Models;
using System.Collections.Generic;

namespace MVC_CRUD.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalJobs { get; set; }
        public int TotalUsers { get; set; }
        public int JobsToday { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();

        public List<string> Last7Labels { get; set; } = new();
        public List<int> Last7Data { get; set; } = new();
        public List<string> JobTypeLabels { get; set; } = new();
        public List<int> JobTypeData { get; set; } = new();
    }
}