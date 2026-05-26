using System;
using System.Collections.Generic;

namespace Hevelab2026.Models
{
    public class DashboardViewModel
    {
        public List<MetricCard> Metrics { get; set; } = new();
        public List<QuickAccessModule> QuickAccessModules { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<RecentSale> RecentSales { get; set; } = new();
    }

    public class MetricCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string TrendText { get; set; } = string.Empty;
        public string TrendType { get; set; } = "up"; // "up", "down", "warning"
        public string IconSvg { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = "primary"; // primary, success, danger, warning
    }

    public class QuickAccessModule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public string IconSvg { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string BadgeType { get; set; } = "secondary"; // primary, success, warning, danger
    }

    public class RecentActivity
    {
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // "success", "info", "warning", "danger"
    }

    public class RecentSale
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pagada", "Pendiente", "Vencida"
        public string StatusType { get; set; } = "success"; // "success", "warning", "danger"
        public string Date { get; set; } = string.Empty;
    }
}
