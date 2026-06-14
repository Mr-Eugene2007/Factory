using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class ProductionOrderPage : Page
    {
        // ── Модели для отображения ───────────────────────────

        public class RecipeItem
        {
            public int LineNumber { get; set; }
            public string MaterialName { get; set; }
            public string Unit { get; set; }
            public decimal QuantityPerUnit { get; set; }
            public decimal PlannedQty { get; set; }
            public decimal TotalRequired => QuantityPerUnit * PlannedQty;
            public decimal CurrentStock { get; set; }
            public string StockStatus =>
                CurrentStock >= TotalRequired ? "✅ Достаточно" : "⚠️ Нехватка";
        }

        public class OrderViewModel
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public string ProductName { get; set; }
            public string ProductType { get; set; }
            public string Unit { get; set; }
            public decimal PlannedQuantity { get; set; }
            public decimal? ActualQuantity { get; set; }
            public string Status { get; set; }
            public int? Priority { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? CreatedDate { get; set; }
            public List<RecipeItem> RecipeItems { get; set; }
        }

        private List<OrderViewModel> _orders = new List<OrderViewModel>();

        public ProductionOrderPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        // ── Загрузка заказов через EDMX контекст ────────────
        private void LoadOrders()
        {
            try
            {
                var db = BeverageFactoryEntities.GetContext();

                _orders = db.ProductionOrders
                    .Include("Product")
                    .Include("Product.ProductType")
                    .Include("Product.UnitType")
                    .Include("StatusType")
                    .ToList()
                    .Select(po => new OrderViewModel
                    {
                        Id = po.id,
                        PlannedQuantity = po.planned_quantity,
                        ActualQuantity = po.actual_quantity,
                        Priority = po.priority,
                        StartDate = po.start_date,
                        EndDate = po.end_date,
                        CreatedDate = po.created_date,
                        ProductName = po.Product.name,
                        ProductType = po.Product.ProductType.name,
                        Unit = po.Product.UnitType.name,
                        Status = po.StatusType.name,
                        DisplayName = $"№{po.id} — {po.Product.name}",

                        // Рецепт: ингредиенты для данного продукта
                        RecipeItems = db.Recipes
                            .Where(r => r.product_id == po.product_id)
                            .Include("RawMaterial")
                            .Include("RawMaterial.UnitType")
                            .ToList()
                            .Select((r, i) => new RecipeItem
                            {
                                LineNumber = i + 1,
                                MaterialName = r.RawMaterial.name,
                                Unit = r.RawMaterial.UnitType.name,
                                QuantityPerUnit = r.quantity,
                                CurrentStock = r.RawMaterial.current_stock ?? 0,
                                PlannedQty = po.planned_quantity
                            }).ToList()
                    }).ToList();

                cmbOrders.ItemsSource = _orders;
                cmbOrders.DisplayMemberPath = "DisplayName";

                if (_orders.Any())
                    cmbOrders.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        // ── Выбор заказа → заполнение документа ─────────────
        private void cmbOrders_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            if (cmbOrders.SelectedItem is OrderViewModel order)
                FillDocument(order);
        }

        private void FillDocument(OrderViewModel o)
        {
            txtOrderId.Text = $"№{o.Id}";
            txtCreatedDate.Text = o.CreatedDate?.ToString("dd.MM.yyyy") ?? "—";
            txtProductName.Text = o.ProductName;
            txtProductType.Text = o.ProductType;
            txtPlannedQty.Text = $"{o.PlannedQuantity:N2} {o.Unit}";
            txtActualQty.Text = o.ActualQuantity.HasValue
                                    ? $"{o.ActualQuantity:N2} {o.Unit}" : "—";
            txtStatus.Text = o.Status;
            txtPriority.Text = o.Priority.HasValue
                                    ? o.Priority.ToString() : "—";
            txtStartDate.Text = o.StartDate?.ToString("dd.MM.yyyy") ?? "—";
            txtEndDate.Text = o.EndDate?.ToString("dd.MM.yyyy") ?? "—";

            dgRecipe.ItemsSource = o.RecipeItems;

            // Проверка готовности производства
            bool ready = o.RecipeItems.All(r => r.CurrentStock >= r.TotalRequired);
            txtReadiness.Text = ready
                ? "✅ Все материалы в наличии"
                : "⚠️ Нехватка некоторых материалов";
            txtReadiness.Foreground = ready
                ? Brushes.Green
                : Brushes.OrangeRed;
        }

        // ── Сохранение в PDF ─────────────────────────────────
        private void SaveToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = flowDocumentReader.Document;

            if (doc == null)
            {
                MessageBox.Show("Документ не найден.");
                return;
            }

            MessageBox.Show(
                "В диалоге выбора принтера выберите «Microsoft Print to PDF»\n" +
                "и укажите папку для сохранения файла.",
                "Сохранение в PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                IDocumentPaginatorSource src = doc;
                dlg.PrintDocument(
                    src.DocumentPaginator,
                    $"Производственный заказ {txtOrderId.Text}");
            }
        }
    }
}