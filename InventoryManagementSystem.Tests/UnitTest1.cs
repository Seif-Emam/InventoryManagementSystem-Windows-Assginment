using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Forms;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InventoryManagementSystem.Tests
{
    [TestClass]
    public class BusinessRulesAndModelTests
    {
        [TestMethod]
        public void StockStatus_Rule1_QuantityZero_ReturnsOutOfStock()
        {
            var product = new Product(1, "Monitor", 4500m, 0, 2);
            Assert.AreEqual("Out of Stock", product.StockStatus);
        }

        [TestMethod]
        public void StockStatus_Rule2_QuantityLessThanOrEqualToMinStock_ReturnsLowStock()
        {
            var product1 = new Product(2, "Mouse", 250m, 3, 5);
            Assert.AreEqual("Low Stock", product1.StockStatus);

            var product2 = new Product(3, "USB Cable", 15m, 5, 5);
            Assert.AreEqual("Low Stock", product2.StockStatus);
        }

        [TestMethod]
        public void StockStatus_Rule3_QuantityGreaterThanMinStock_ReturnsAvailable()
        {
            var product = new Product(4, "Keyboard", 500m, 15, 5);
            Assert.AreEqual("Available", product.StockStatus);
        }

        [TestMethod]
        public void StockStatus_CalculatesDynamically_WhenQuantityChanges()
        {
            var product = new Product(5, "Desk Lamp", 40m, 20, 5);
            Assert.AreEqual("Available", product.StockStatus);

            // Decrease to threshold
            product.Quantity = 5;
            Assert.AreEqual("Low Stock", product.StockStatus);

            // Decrease below threshold
            product.Quantity = 2;
            Assert.AreEqual("Low Stock", product.StockStatus);

            // Depleted
            product.Quantity = 0;
            Assert.AreEqual("Out of Stock", product.StockStatus);

            // Restocked
            product.Quantity = 10;
            Assert.AreEqual("Available", product.StockStatus);
        }

        [TestMethod]
        public void Product_Clone_CreatesExactIndependentCopy()
        {
            var original = new Product(10, "Graphic Tablet", 350m, 8, 3);
            var clone = original.Clone();

            Assert.AreEqual(original.Id, clone.Id);
            Assert.AreEqual(original.Name, clone.Name);
            Assert.AreEqual(original.Price, clone.Price);
            Assert.AreEqual(original.Quantity, clone.Quantity);
            Assert.AreEqual(original.MinStock, clone.MinStock);
            Assert.AreEqual(original.StockStatus, clone.StockStatus);

            // Modify clone; original must remain unchanged
            clone.Name = "Modified Tablet";
            clone.Quantity = 0;

            Assert.AreEqual("Graphic Tablet", original.Name);
            Assert.AreEqual(8, original.Quantity);
            Assert.AreEqual("Available", original.StockStatus);
            Assert.AreEqual("Out of Stock", clone.StockStatus);
        }

        [TestMethod]
        public void DesignSystem_Colors_MatchSpecifications()
        {
            var (availFg, availBg, availBar) = DesignSystem.GetStockColors("Available");
            Assert.AreEqual(ColorTranslator.FromHtml("#166534"), availFg);
            Assert.AreEqual(ColorTranslator.FromHtml("#DCFCE7"), availBg);
            Assert.AreEqual(ColorTranslator.FromHtml("#16A34A"), availBar);

            var (lowFg, lowBg, lowBar) = DesignSystem.GetStockColors("Low Stock");
            Assert.AreEqual(ColorTranslator.FromHtml("#92400E"), lowFg);
            Assert.AreEqual(ColorTranslator.FromHtml("#FEF3C7"), lowBg);
            Assert.AreEqual(ColorTranslator.FromHtml("#D97706"), lowBar);

            var (outFg, outBg, outBar) = DesignSystem.GetStockColors("Out of Stock");
            Assert.AreEqual(ColorTranslator.FromHtml("#991B1B"), outFg);
            Assert.AreEqual(ColorTranslator.FromHtml("#FEE2E2"), outBg);
            Assert.AreEqual(ColorTranslator.FromHtml("#DC2626"), outBar);
        }

        [TestMethod]
        public void GDIChart_RendersSuccessfully_WithVariousDataSets()
        {
            var chart = new StockChartPanel
            {
                Size = new Size(600, 300)
            };

            using var bmp = new Bitmap(600, 300);

            // 1. Empty dataset test
            chart.UpdateData(new List<Product>());
            chart.DrawToBitmap(bmp, new Rectangle(0, 0, 600, 300));

            // 2. Realistic dataset test
            var sample = DatabaseInitializer.GetSeedProducts().Take(5).ToList();
            chart.UpdateData(sample);
            chart.DrawToBitmap(bmp, new Rectangle(0, 0, 600, 300));

            // 3. Single zero item test
            chart.UpdateData(new List<Product> { new Product(1, "ZeroItem", 10m, 0, 5) });
            chart.DrawToBitmap(bmp, new Rectangle(0, 0, 600, 300));

            Assert.IsNotNull(bmp);
        }

        [TestMethod]
        public void GDIChart_Interactivity_ClickToEditTriggersEvent()
        {
            var chart = new StockChartPanel
            {
                Size = new Size(800, 400)
            };

            var testProducts = new List<Product>
            {
                new Product(1, "Interactive Mouse", 50m, 10, 2),
                new Product(2, "Interactive Keyboard", 80m, 5, 2)
            };

            chart.UpdateData(testProducts);

            // Force layout and paint
            using var bmp = new Bitmap(800, 400);
            chart.DrawToBitmap(bmp, new Rectangle(0, 0, 800, 400));

            Product? clickedProduct = null;
            chart.ProductEditRequested += (s, p) => clickedProduct = p;

            // Trigger mouse click on the middle of the first bar area
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var hitTargets = (List<(Rectangle Bounds, Product Product)>)chart.GetType().GetField("_hitTargets", flags)!.GetValue(chart)!;

            Assert.IsTrue(hitTargets.Count >= 2, "Hit targets must be recorded for interactive chart bars.");

            // Simulate clicking on the first product's hit target
            var firstTarget = hitTargets[0];
            var centerPt = new Point(firstTarget.Bounds.X + firstTarget.Bounds.Width / 2, firstTarget.Bounds.Y + firstTarget.Bounds.Height / 2);

            // Invoke OnMouseClick via reflection
            var onMouseClick = chart.GetType().GetMethod("OnMouseClick", flags);
            onMouseClick!.Invoke(chart, new object[] { new MouseEventArgs(MouseButtons.Left, 1, centerPt.X, centerPt.Y, 0) });

            Assert.IsNotNull(clickedProduct, "Clicking an interactive bar must trigger ProductEditRequested event.");
            Assert.AreEqual(firstTarget.Product.Id, clickedProduct.Id);
        }

        [TestMethod]
        public void RoundedRectanglePath_CalculatesValidPath()
        {
            var rect = new Rectangle(0, 0, 100, 50);
            using var path = DesignSystem.CreateRoundedRectanglePath(rect, 8);
            Assert.IsNotNull(path);
            Assert.IsTrue(path.PointCount > 0);
        }

        [TestMethod]
        public void HelpForm_InitializesAndDisplaysAllGuideSections()
        {
            using var help = new HelpForm();
            Assert.IsNotNull(help);
            Assert.IsTrue(help.Controls.Count >= 3);
            Assert.AreEqual("User Guide & Keyboard Shortcuts — InventoryHub", help.Text);
        }

        [TestMethod]
        public void MainForm_DashboardView_LayoutAndScrollbarAreProperlyConfigured()
        {
            using var form = new MainForm();
            form.Size = new Size(1320, 860);
            form.Show();

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var header = (Panel)form.GetType().GetField("_headerPanel", flags)!.GetValue(form)!;
            var views = (Panel)form.GetType().GetField("_viewsContainer", flags)!.GetValue(form)!;
            var dash = (Panel)form.GetType().GetField("_dashboardView", flags)!.GetValue(form)!;
            var dgvDash = (DataGridView)form.GetType().GetField("_dgvDashboardProducts", flags)!.GetValue(form)!;
            var dgvProd = (DataGridView)form.GetType().GetField("_dgvProducts", flags)!.GetValue(form)!;

            // 1. Header must be docked at Top (Y == 0, Height == 84)
            Assert.AreEqual(0, header.Location.Y);
            Assert.AreEqual(84, header.Height);

            // 2. Views container must start below Header (Y == 84), NOT overlapping at Y == 0!
            Assert.AreEqual(84, views.Location.Y);

            // 3. DashboardView must have AutoScroll enabled
            Assert.IsTrue(dash.AutoScroll);

            // 4. Content height in DashboardView exceeds standard window height, activating vertical scrollbar
            Assert.IsTrue(dash.DisplayRectangle.Height >= 900);

            // 5. DataGridViews must include the inline "EditAction" button column
            Assert.IsTrue(dgvDash.Columns.Contains("EditAction"), "Dashboard table must have inline Edit button column.");
            Assert.IsTrue(dgvProd.Columns.Contains("EditAction"), "Products catalog table must have inline Edit button column.");

            form.Close();
        }

        [TestMethod]
        public void NotificationDialog_Confirmation_InitializesWithCorrectControlsAndProductChip()
        {
            var testProduct = new Product(42, "Logitech MX Master 3S", 99.99m, 18, 5);

            using var dialog = new NotificationDialog(
                NotificationType.Danger,
                "Delete Confirmation",
                "Are you sure you want to permanently delete this product? This action cannot be undone.",
                isConfirmation: true,
                confirmText: "Yes, Delete",
                cancelText: "No, Cancel",
                product: testProduct
            );

            // Dialog configuration checks
            Assert.AreEqual("Delete Confirmation", dialog.Text);
            Assert.AreEqual(FormStartPosition.CenterParent, dialog.StartPosition);
            Assert.IsFalse(dialog.Confirmed);
            Assert.IsNotNull(dialog.AcceptButton, "Confirmation modal must have an AcceptButton.");
            Assert.IsNotNull(dialog.CancelButton, "Confirmation modal must have a CancelButton.");

            // Confirm button properties
            var confirmBtn = dialog.AcceptButton as ModernButton;
            var cancelBtn = dialog.CancelButton as ModernButton;
            Assert.IsNotNull(confirmBtn);
            Assert.IsNotNull(cancelBtn);
            Assert.AreEqual("Yes, Delete", confirmBtn.Text);
            Assert.AreEqual("No, Cancel", cancelBtn.Text);
            Assert.AreEqual(ModernButton.ButtonType.Danger, confirmBtn.Type);
            Assert.AreEqual(ModernButton.ButtonType.Secondary, cancelBtn.Type);

            // Verify product details chip is created
            bool foundProductLabel = false;
            foreach (Control c in dialog.Controls)
            {
                if (c is Panel p && p.Controls.Count > 0)
                {
                    foreach (Control sub in p.Controls)
                    {
                        if (sub is Label lbl && lbl.Text.Contains("Logitech MX Master 3S"))
                        {
                            foundProductLabel = true;
                            break;
                        }
                    }
                }
            }
            Assert.IsTrue(foundProductLabel, "Product details chip card must be present when product is supplied.");
        }

        [TestMethod]
        public void NotificationDialog_Success_InitializesWithSingleButton()
        {
            var testProduct = new Product(10, "Samsung 990 EVO SSD", 89.99m, 15, 5);

            using var dialog = new NotificationDialog(
                NotificationType.Success,
                "Product Added Successfully",
                "The product has been saved to the SQLite database.",
                isConfirmation: false,
                confirmText: "Continue",
                product: testProduct
            );

            Assert.AreEqual("Product Added Successfully", dialog.Text);
            Assert.IsNotNull(dialog.AcceptButton);
            Assert.IsNull(dialog.CancelButton, "Informational success modal must have only a single action button.");

            var okBtn = dialog.AcceptButton as ModernButton;
            Assert.IsNotNull(okBtn);
            Assert.AreEqual("Continue", okBtn.Text);
            Assert.AreEqual(ModernButton.ButtonType.Primary, okBtn.Type);
        }

        [TestMethod]
        public void NotificationDialog_WarningAndError_InitializesProperly()
        {
            using var warnDialog = new NotificationDialog(
                NotificationType.Warning,
                "Input Validation",
                "Product name cannot be empty.",
                isConfirmation: false,
                confirmText: "Understood"
            );

            Assert.AreEqual("Input Validation", warnDialog.Text);
            var warnBtn = warnDialog.AcceptButton as ModernButton;
            Assert.IsNotNull(warnBtn);
            Assert.AreEqual("Understood", warnBtn.Text);

            using var errDialog = new NotificationDialog(
                NotificationType.Danger,
                "Database Error",
                "An unexpected database exception occurred.",
                isConfirmation: false,
                confirmText: "Dismiss"
            );

            Assert.AreEqual("Database Error", errDialog.Text);
            var errBtn = errDialog.AcceptButton as ModernButton;
            Assert.IsNotNull(errBtn);
            Assert.AreEqual("Dismiss", errBtn.Text);
            Assert.AreEqual(ModernButton.ButtonType.Danger, errBtn.Type);
        }
    }

    /// <summary>
    /// Comprehensive automated tests verifying SQLite database creation, schema,
    /// idempotent seeding, persistence across restarts, and full CRUD operations.
    /// Strictly operates on isolated temporary SQLite databases to protect user data.
    /// </summary>
    [TestClass]
    public class SqlitePersistenceTests
    {
        private string _testDbPath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_inv_{Guid.NewGuid():N}.db");
        }

        [TestCleanup]
        public void Teardown()
        {
            try
            {
                // Force garbage collection to release any lingering SQLite file handles
                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup locks on temp files
            }
        }

        [TestMethod]
        public void Requirement1_DatabaseCreation_CreatesDatabaseFileAndSchema()
        {
            Assert.IsFalse(File.Exists(_testDbPath), "Test database file should not exist before init.");

            DatabaseInitializer.Initialize(_testDbPath);

            Assert.IsTrue(File.Exists(_testDbPath), "SQLite database file must be created.");
            var repo = new ProductRepository(_testDbPath);
            var products = repo.GetAll();
            Assert.IsNotNull(products);
        }

        [TestMethod]
        public void Requirement2_InitialSeeding_SeedsRealisticInventoryDataset()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            var products = repo.GetAll();

            // 25 realistic demonstration products seeded
            Assert.AreEqual(25, products.Count);
            Assert.IsTrue(products.Any(p => p.Name.Contains("Logitech MX Master 3S")));
            Assert.IsTrue(products.Any(p => p.Name.Contains("Dell 24-inch FHD Monitor")));
            Assert.IsTrue(products.Any(p => p.Name.Contains("Samsung 990 EVO SSD")));

            // Covers all three stock conditions
            Assert.IsTrue(products.Any(p => p.StockStatus == "Available"));
            Assert.IsTrue(products.Any(p => p.StockStatus == "Low Stock"));
            Assert.IsTrue(products.Any(p => p.StockStatus == "Out of Stock"));
        }

        [TestMethod]
        public void Requirement3_SeedingDoesNotDuplicateRecords_WhenReinitialized()
        {
            // First initialization seeds the database
            DatabaseInitializer.Initialize(_testDbPath);
            var repo1 = new ProductRepository(_testDbPath);
            int countAfterFirstInit = repo1.GetAll().Count;
            Assert.AreEqual(25, countAfterFirstInit);

            // Second initialization must be idempotent
            DatabaseInitializer.Initialize(_testDbPath);
            var repo2 = new ProductRepository(_testDbPath);
            int countAfterSecondInit = repo2.GetAll().Count;

            Assert.AreEqual(25, countAfterSecondInit, "Database reinitialization must not duplicate records.");
        }

        [TestMethod]
        public void Requirement4_ExistingDataSurvivesDatabaseReinitialization()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            // Add custom user product
            var customProduct = new Product(0, "User Added Mechanical Keyboard", 149.99m, 7, 2);
            repo.Add(customProduct);

            int totalBefore = repo.GetAll().Count;
            Assert.AreEqual(26, totalBefore);

            // Re-run initialization (simulating subsequent application launches)
            DatabaseInitializer.Initialize(_testDbPath);

            var freshRepo = new ProductRepository(_testDbPath);
            var productsAfter = freshRepo.GetAll();

            Assert.AreEqual(26, productsAfter.Count);
            var found = freshRepo.GetById(customProduct.Id);
            Assert.IsNotNull(found);
            Assert.AreEqual("User Added Mechanical Keyboard", found.Name);
        }

        [TestMethod]
        public void Requirement5_AddProduct_PersistsCorrectlyToSqlite()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            var newProd = new Product(0, "Razer DeathAdder V3", 69.99m, 14, 3);
            repo.Add(newProd);

            Assert.IsTrue(newProd.Id > 0, "SQLite must assign auto-incremented primary key.");

            // Verify persistence by querying through a separate repository instance
            var verifyRepo = new ProductRepository(_testDbPath);
            var fetched = verifyRepo.GetById(newProd.Id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual("Razer DeathAdder V3", fetched.Name);
            Assert.AreEqual(69.99m, fetched.Price);
            Assert.AreEqual(14, fetched.Quantity);
            Assert.AreEqual(3, fetched.MinStock);
            Assert.AreEqual("Available", fetched.StockStatus);
        }

        [TestMethod]
        public void Requirement6_UpdateProduct_PersistsCorrectlyToSqlite()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            var prod = new Product(0, "Original Wireless Earbuds", 59.99m, 20, 5);
            repo.Add(prod);

            // Modify properties
            prod.Name = "Updated Pro Earbuds";
            prod.Price = 89.99m;
            prod.Quantity = 3; // Now Low Stock (3 <= 5)
            prod.MinStock = 5;

            bool success = repo.Update(prod);
            Assert.IsTrue(success);

            // Verify with independent reader
            var verifyRepo = new ProductRepository(_testDbPath);
            var updated = verifyRepo.GetById(prod.Id);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Pro Earbuds", updated.Name);
            Assert.AreEqual(89.99m, updated.Price);
            Assert.AreEqual(3, updated.Quantity);
            Assert.AreEqual("Low Stock", updated.StockStatus);
        }

        [TestMethod]
        public void Requirement7_DeleteProduct_PersistsCorrectlyToSqlite()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            var prod = new Product(0, "Product to Delete", 19.99m, 5, 2);
            repo.Add(prod);

            int id = prod.Id;
            Assert.IsNotNull(repo.GetById(id));

            bool deleted = repo.Delete(id);
            Assert.IsTrue(deleted);

            // Verify deletion across fresh instance
            var verifyRepo = new ProductRepository(_testDbPath);
            Assert.IsNull(verifyRepo.GetById(id), "Deleted product must not exist in SQLite.");
        }

        [TestMethod]
        public void Requirement8_Search_WorksByNameAndId()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            // Search by partial Name (case-insensitive)
            var logitechResults = repo.Search("logitech");
            Assert.IsTrue(logitechResults.Count >= 3);
            Assert.IsTrue(logitechResults.All(p => p.Name.Contains("Logitech", StringComparison.OrdinalIgnoreCase)));

            // Search by specific ID
            var firstProduct = repo.GetAll().First();
            var idResults = repo.Search(firstProduct.Id.ToString());
            Assert.IsTrue(idResults.Any(p => p.Id == firstProduct.Id));

            // Search empty query returns all
            var allResults = repo.Search("");
            Assert.AreEqual(25, allResults.Count);
        }

        [TestMethod]
        public void Requirement9_DashboardStatistics_MatchDatabaseRecords()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            var (total, lowStock, outOfStock) = repo.GetDashboardStatistics();
            var allProducts = repo.GetAll();

            Assert.AreEqual(allProducts.Count, total);
            Assert.AreEqual(allProducts.Count(p => p.StockStatus == "Low Stock"), lowStock);
            Assert.AreEqual(allProducts.Count(p => p.StockStatus == "Out of Stock"), outOfStock);

            // Verify non-zero counts reflecting the seeded distribution
            Assert.AreEqual(25, total);
            Assert.AreEqual(4, lowStock);
            Assert.AreEqual(4, outOfStock);
        }

        [TestMethod]
        public void Requirement10_StockStatus_CalculatedCorrectlyInSqliteContext()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            var repo = new ProductRepository(_testDbPath);

            // Out of stock product
            var outOfStockProd = repo.GetAll().First(p => p.Quantity == 0);
            Assert.AreEqual("Out of Stock", outOfStockProd.StockStatus);

            // Low stock product
            var lowStockProd = repo.GetAll().First(p => p.Quantity > 0 && p.Quantity <= p.MinStock);
            Assert.AreEqual("Low Stock", lowStockProd.StockStatus);

            // Available product
            var availableProd = repo.GetAll().First(p => p.Quantity > p.MinStock);
            Assert.AreEqual("Available", availableProd.StockStatus);
        }

        [TestMethod]
        public void Section9_ExactFullLifecycleVerificationScenario()
        {
            DatabaseInitializer.Initialize(_testDbPath);
            Assert.IsTrue(File.Exists(_testDbPath));

            var session1Repo = new ProductRepository(_testDbPath);
            var initialProducts = session1Repo.GetAll();
            Assert.AreEqual(25, initialProducts.Count);

            var addedProd = new Product(0, "Custom Audio Interface", 199.99m, 12, 3);
            session1Repo.Add(addedProd);
            Assert.IsTrue(addedProd.Id > 0);
            int newId = addedProd.Id;

            var productToEdit = session1Repo.GetAll().First(p => p.Name.Contains("Logitech MX Master 3S"));
            int editedId = productToEdit.Id;
            productToEdit.Name = "Logitech MX Master 3S Mouse - Graphite Edition";
            productToEdit.Price = 109.99m;
            productToEdit.Quantity = 2;
            session1Repo.Update(productToEdit);

            var productToDelete = session1Repo.GetAll().First(p => p.Name.Contains("SteelSeries Rival 3"));
            int deletedId = productToDelete.Id;
            session1Repo.Delete(deletedId);

            Assert.AreEqual(25, session1Repo.GetAll().Count);

            session1Repo = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            DatabaseInitializer.Initialize(_testDbPath);

            var session2Repo = new ProductRepository(_testDbPath);
            var session2Products = session2Repo.GetAll();

            var verifiedAdded = session2Repo.GetById(newId);
            Assert.IsNotNull(verifiedAdded);
            Assert.AreEqual("Custom Audio Interface", verifiedAdded.Name);

            var verifiedEdited = session2Repo.GetById(editedId);
            Assert.IsNotNull(verifiedEdited);
            Assert.AreEqual("Logitech MX Master 3S Mouse - Graphite Edition", verifiedEdited.Name);
            Assert.AreEqual("Low Stock", verifiedEdited.StockStatus);

            var verifiedDeleted = session2Repo.GetById(deletedId);
            Assert.IsNull(verifiedDeleted);

            Assert.AreEqual(25, session2Products.Count);

            var (total, lowStock, outOfStock) = session2Repo.GetDashboardStatistics();
            Assert.AreEqual(25, total);
            Assert.AreEqual(session2Products.Count(p => p.StockStatus == "Low Stock"), lowStock);
            Assert.AreEqual(session2Products.Count(p => p.StockStatus == "Out of Stock"), outOfStock);
        }
    }

    [TestClass]
    public class ApplicationCommandsSystemTests
    {
        private string _testDbPath = null!;

        [TestInitialize]
        public void Setup()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"ims_cmds_test_{Guid.NewGuid():N}.db");
            DatabaseInitializer.Initialize(_testDbPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (File.Exists(_testDbPath))
            {
                try { File.Delete(_testDbPath); } catch { }
            }
        }

        [TestMethod]
        public void Requirement1_CommandRegistry_ContainsAtLeast10FunctionalCommands()
        {
            var repo = new ProductRepository(_testDbPath);
            using var form = new MainForm(repo);

            var registry = form.Commands;
            Assert.IsNotNull(registry);
            Assert.IsTrue(registry.Commands.Count >= 10, $"Must contain at least 10 commands; found {registry.Commands.Count}.");

            // Verify unique Command IDs
            var ids = registry.Commands.Select(c => c.Id).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count(), "All Command IDs must be unique.");

            // Verify all required metadata fields are populated with real content
            foreach (var cmd in registry.Commands)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.Id), "Command ID must not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.Name), $"Command {cmd.Id} name must not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.Purpose), $"Command {cmd.Id} purpose must not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.ExpectedAction), $"Command {cmd.Id} expected action must not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.HandlerName), $"Command {cmd.Id} handler name must not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.AffectedComponents), $"Command {cmd.Id} affected components must not be empty.");
                Assert.IsNotNull(cmd.Action, $"Command {cmd.Id} must have an executable Action delegate.");
            }
        }

        [TestMethod]
        public void Requirement2_Commands_PerformActualFunctionality()
        {
            var repo = new ProductRepository(_testDbPath);
            using var form = new MainForm(repo);
            form.Size = new Size(1320, 860);
            form.Show();

            var registry = form.Commands;

            // 1. Test Refresh Data
            var refreshCmd = registry.FindById("CMD_DATA_REFRESH");
            Assert.IsNotNull(refreshCmd);
            var refreshResult = refreshCmd.Execute(form);
            Assert.IsTrue(refreshResult.Success);
            Assert.AreEqual(1, refreshCmd.ExecutionCount);
            Assert.IsNotNull(refreshCmd.LastExecutedAt);

            // 2. Test Navigation to Products Catalog & Dashboard
            var navCatalogCmd = registry.FindById("CMD_NAV_CATALOG");
            Assert.IsNotNull(navCatalogCmd);
            var navCatalogResult = navCatalogCmd.Execute(form);
            Assert.IsTrue(navCatalogResult.Success);

            var navDashCmd = registry.FindById("CMD_NAV_DASHBOARD");
            Assert.IsNotNull(navDashCmd);
            var navDashResult = navDashCmd.Execute(form);
            Assert.IsTrue(navDashResult.Success);

            // 3. Test Filters
            var filterLowCmd = registry.FindById("CMD_FILTER_LOW_STOCK");
            Assert.IsNotNull(filterLowCmd);
            var filterLowResult = filterLowCmd.Execute(form);
            Assert.IsTrue(filterLowResult.Success);

            var filterOutCmd = registry.FindById("CMD_FILTER_OUT_OF_STOCK");
            Assert.IsNotNull(filterOutCmd);
            var filterOutResult = filterOutCmd.Execute(form);
            Assert.IsTrue(filterOutResult.Success);

            var filterResetCmd = registry.FindById("CMD_FILTER_ALL");
            Assert.IsNotNull(filterResetCmd);
            var filterResetResult = filterResetCmd.Execute(form);
            Assert.IsTrue(filterResetResult.Success);

            // 4. Test Search Focus & Clear
            var focusSearchCmd = registry.FindById("CMD_SEARCH_FOCUS");
            Assert.IsNotNull(focusSearchCmd);
            var focusSearchResult = focusSearchCmd.Execute(form);
            Assert.IsTrue(focusSearchResult.Success);

            var clearSearchCmd = registry.FindById("CMD_SEARCH_CLEAR");
            Assert.IsNotNull(clearSearchCmd);
            var clearSearchResult = clearSearchCmd.Execute(form);
            Assert.IsTrue(clearSearchResult.Success);

            form.Close();
        }

        [TestMethod]
        public void Requirement3_CommandsMenu_ExistsInMainMenuStrip()
        {
            var repo = new ProductRepository(_testDbPath);
            using var form = new MainForm(repo);

            Assert.IsNotNull(form.MainMenuStrip, "MainForm must have a MainMenuStrip.");

            // Look for "Commands" top-level menu
            ToolStripMenuItem? commandsMenu = null;
            if (form.MainMenuStrip != null)
            {
                foreach (ToolStripItem item in form.MainMenuStrip.Items)
                {
                    if (item is ToolStripMenuItem mi && mi.Text != null && mi.Text.Contains("Commands"))
                    {
                        commandsMenu = mi;
                        break;
                    }
                }
            }

            Assert.IsNotNull(commandsMenu, "MainMenuStrip must contain a 'Commands' menu.");
            Assert.IsTrue(commandsMenu.DropDownItems.Count >= 5, "Commands menu must have sub-items.");

            // Verify Commands Explorer item is present with shortcut
            ToolStripMenuItem? explorerItem = null;
            foreach (ToolStripItem sub in commandsMenu.DropDownItems)
            {
                if (sub is ToolStripMenuItem subMi && subMi.Text != null && subMi.Text.Contains("Commands Explorer"))
                {
                    explorerItem = subMi;
                    break;
                }
            }

            Assert.IsNotNull(explorerItem, "Commands menu must have 'Commands Explorer...' item.");
            Assert.AreEqual(Keys.Control | Keys.Shift | Keys.C, explorerItem.ShortcutKeys);
        }

        [TestMethod]
        public void Requirements4To6_CommandsExplorerForm_InitializesWithDataGridViewAndDetails()
        {
            var repo = new ProductRepository(_testDbPath);
            using var mainForm = new MainForm(repo);
            using var explorer = new CommandsExplorerForm(mainForm, mainForm.Commands);

            Assert.IsNotNull(explorer);
            Assert.AreEqual("Application Commands Explorer — InventoryHub", explorer.Text);

            // Find DataGridView
            var dgv = FindControl<DataGridView>(explorer);

            Assert.IsNotNull(dgv, "CommandsExplorerForm must contain a DataGridView.");
            Assert.IsTrue(dgv.Columns.Count >= 7, "DataGridView must have all command detail columns.");

            // Verify required columns exist
            Assert.IsTrue(dgv.Columns.Contains("colId"), "Must have Command ID column.");
            Assert.IsTrue(dgv.Columns.Contains("colName"), "Must have Command Name column.");
            Assert.IsTrue(dgv.Columns.Contains("colPurpose"), "Must have Purpose column.");
            Assert.IsTrue(dgv.Columns.Contains("colExpectedAction"), "Must have Expected Action column.");
            Assert.IsTrue(dgv.Columns.Contains("colHandler"), "Must have Associated Handler column.");
            Assert.IsTrue(dgv.Columns.Contains("colShortcut"), "Must have Shortcut column.");
            Assert.IsTrue(dgv.Columns.Contains("colAffected"), "Must have Affected Components column.");
            Assert.IsTrue(dgv.Columns.Contains("colExecute"), "Must have Execute Action button column.");

            // Verify rows match registered commands
            Assert.AreEqual(mainForm.Commands.Commands.Count, dgv.Rows.Count);
        }

        [TestMethod]
        public void Requirements_CommandsExplorer_AllCommandsIncludeSyntaxUsageAndEffects()
        {
            var repo = new ProductRepository(_testDbPath);
            using var mainForm = new MainForm(repo);
            using var explorer = new CommandsExplorerForm(mainForm, mainForm.Commands);

            // Verify all registered commands have non-empty Purpose, ExpectedAction (How to use), and AffectedComponents (What it effects)
            foreach (var cmd in mainForm.Commands.Commands)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.Purpose), $"Command {cmd.Id} must have Purpose.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.ExpectedAction), $"Command {cmd.Id} must have ExpectedAction (How to Use).");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cmd.AffectedComponents), $"Command {cmd.Id} must have AffectedComponents (What it Effects).");
            }

            // Verify the 8 terminal CLI commands are all present and categorized as Terminal CLI
            var terminalCommands = mainForm.Commands.Commands.Where(c => c.Category == "Terminal CLI").ToList();
            Assert.AreEqual(8, terminalCommands.Count, "Must have 8 Terminal CLI commands registered.");

            var cliNames = terminalCommands.Select(c => c.Id).ToList();
            CollectionAssert.Contains(cliNames, "CMD_CLI_ADD");
            CollectionAssert.Contains(cliNames, "CMD_CLI_EDIT");
            CollectionAssert.Contains(cliNames, "CMD_CLI_DELETE");
            CollectionAssert.Contains(cliNames, "CMD_CLI_LIST");
            CollectionAssert.Contains(cliNames, "CMD_CLI_SEARCH");
            CollectionAssert.Contains(cliNames, "CMD_CLI_INFO");
            CollectionAssert.Contains(cliNames, "CMD_CLI_HELP");
            CollectionAssert.Contains(cliNames, "CMD_CLI_CLEAR");
        }

        private static T? FindControl<T>(Control parent) where T : Control
        {
            if (parent is T match) return match;
            foreach (Control child in parent.Controls)
            {
                var found = FindControl<T>(child);
                if (found != null) return found;
            }
            return null;
        }
    }

    [TestClass]
    public class TerminalCommandProcessorTests
    {
        private string _testDbPath = null!;

        [TestInitialize]
        public void Setup()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"ims_term_test_{Guid.NewGuid():N}.db");
            DatabaseInitializer.Initialize(_testDbPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (File.Exists(_testDbPath))
            {
                try { File.Delete(_testDbPath); } catch { }
            }
        }

        [TestMethod]
        public void Terminal_Add_WithFlags_InsertsProductDirectlyWithoutWindow()
        {
            var repo = new ProductRepository(_testDbPath);
            bool dataModifiedCalled = false;
            var processor = new TerminalCommandProcessor(repo, () => dataModifiedCalled = true);

            var output = processor.Process("add --name \"Razer Basilisk V3\" --price 69.99 --stock 25 --min 5");

            Assert.IsTrue(output.Success, $"Expected success but got: {output.Message}");
            Assert.IsTrue(dataModifiedCalled, "onDataModified callback must be invoked.");
            Assert.IsNotNull(output.AffectedProduct);
            Assert.IsTrue(output.AffectedProduct.Id > 0);

            // Verify in SQLite database
            var saved = repo.GetById(output.AffectedProduct.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("Razer Basilisk V3", saved.Name);
            Assert.AreEqual(69.99m, saved.Price);
            Assert.AreEqual(25, saved.Quantity);
            Assert.AreEqual(5, saved.MinStock);
            Assert.AreEqual("Available", saved.StockStatus);
        }

        [TestMethod]
        public void Terminal_Add_PositionalArguments_InsertsProductDirectly()
        {
            var repo = new ProductRepository(_testDbPath);
            var processor = new TerminalCommandProcessor(repo);

            var output = processor.Process("add \"HyperX Cloud II Headset\" 89.50 14 3");

            Assert.IsTrue(output.Success, $"Expected success but got: {output.Message}");
            Assert.IsNotNull(output.AffectedProduct);

            var saved = repo.GetById(output.AffectedProduct.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("HyperX Cloud II Headset", saved.Name);
            Assert.AreEqual(89.50m, saved.Price);
            Assert.AreEqual(14, saved.Quantity);
            Assert.AreEqual(3, saved.MinStock);
        }

        [TestMethod]
        public void Terminal_Add_Validation_RejectsMissingOrNegativeFields()
        {
            var repo = new ProductRepository(_testDbPath);
            var processor = new TerminalCommandProcessor(repo);

            // Empty name
            var out1 = processor.Process("add");
            Assert.IsFalse(out1.Success);

            // Negative price
            var out2 = processor.Process("add --name \"Item\" --price -10 --stock 5");
            Assert.IsFalse(out2.Success);

            // Negative stock
            var out3 = processor.Process("add --name \"Item\" --price 10 --stock -5");
            Assert.IsFalse(out3.Success);

            // Invalid numeric price
            var out4 = processor.Process("add --name \"Item\" --price abc --stock 5");
            Assert.IsFalse(out4.Success);
        }

        [TestMethod]
        public void Terminal_Edit_PartialUpdate_PreservesUnchangedFields()
        {
            var repo = new ProductRepository(_testDbPath);
            var initial = repo.GetAll().First();
            int id = initial.Id;
            string originalName = initial.Name;
            int originalMin = initial.MinStock;

            bool dataModifiedCalled = false;
            var processor = new TerminalCommandProcessor(repo, () => dataModifiedCalled = true);

            // Only update price and stock
            var output = processor.Process($"edit {id} --price 199.99 --stock 30");

            Assert.IsTrue(output.Success, $"Expected success but got: {output.Message}");
            Assert.IsTrue(dataModifiedCalled);

            var updated = repo.GetById(id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(originalName, updated.Name, "Name should be preserved.");
            Assert.AreEqual(199.99m, updated.Price, "Price should be updated.");
            Assert.AreEqual(30, updated.Quantity, "Stock should be updated.");
            Assert.AreEqual(originalMin, updated.MinStock, "MinStock should be preserved.");
        }

        [TestMethod]
        public void Terminal_Edit_DepleteStockToZero_RecalculatesOutOfStock()
        {
            var repo = new ProductRepository(_testDbPath);
            var initial = repo.GetAll().First(p => p.Quantity > 0);
            int id = initial.Id;

            var processor = new TerminalCommandProcessor(repo);
            var output = processor.Process($"edit {id} --stock 0");

            Assert.IsTrue(output.Success);
            var updated = repo.GetById(id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(0, updated.Quantity);
            Assert.AreEqual("Out of Stock", updated.StockStatus);
        }

        [TestMethod]
        public void Terminal_Delete_RemovesProductFromDatabase()
        {
            var repo = new ProductRepository(_testDbPath);
            var initial = repo.GetAll().First();
            int id = initial.Id;

            bool dataModifiedCalled = false;
            var processor = new TerminalCommandProcessor(repo, () => dataModifiedCalled = true);

            var output = processor.Process($"delete {id}");

            Assert.IsTrue(output.Success, $"Expected success but got: {output.Message}");
            Assert.IsTrue(dataModifiedCalled);

            Assert.IsNull(repo.GetById(id), "Deleted product must not exist in SQLite.");
        }

        [TestMethod]
        public void Terminal_Delete_NonExistentId_ReturnsError()
        {
            var repo = new ProductRepository(_testDbPath);
            var processor = new TerminalCommandProcessor(repo);

            var output = processor.Process("delete 99999");
            Assert.IsFalse(output.Success);
            Assert.IsTrue(output.Message.Contains("not exist") || output.Message.Contains("not found"));
        }

        [TestMethod]
        public void Terminal_List_And_Search_ReturnFormattedOutputs()
        {
            var repo = new ProductRepository(_testDbPath);
            var processor = new TerminalCommandProcessor(repo);

            var listOut = processor.Process("list");
            Assert.IsTrue(listOut.Success);
            Assert.IsTrue(listOut.Message.Contains("PRODUCT NAME"));
            Assert.IsTrue(listOut.Message.Contains("PRICE"));

            var searchOut = processor.Process("search logitech");
            Assert.IsTrue(searchOut.Success);
            Assert.IsTrue(searchOut.Message.Contains("Logitech"));

            var helpOut = processor.Process("help");
            Assert.IsTrue(helpOut.Success);
            Assert.IsTrue(helpOut.Message.Contains("add"));
            Assert.IsTrue(helpOut.Message.Contains("edit"));
            Assert.IsTrue(helpOut.Message.Contains("delete"));
        }
    }
}
