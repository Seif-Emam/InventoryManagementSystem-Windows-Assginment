using System;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Forms
{
    public partial class MainForm
    {
        // =================================================================
        // TOPIC 1 & 5: EVENTS, MESSAGES & CRUD HANDLERS WITH SQLITE
        // =================================================================

        /// <summary>
        /// Handles Add Product command: Saves new product to SQLite database.
        /// </summary>
        public void HandleAddProduct()
        {
            using ProductForm form = new ProductForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Product newProduct = form.CurrentProduct;
                try
                {
                    // Insert into SQLite database; repository assigns generated Id
                    _repository.Add(newProduct);

                    RefreshAllUI();

                    NotificationDialog.ShowSuccess(
                        this,
                        "Product Added Successfully",
                        $"Product '{newProduct.Name}' has been successfully registered and saved to the SQLite database.",
                        newProduct,
                        "Continue"
                    );
                }
                catch (Exception ex)
                {
                    NotificationDialog.ShowError(
                        this,
                        "[Product Creation Error]",
                        $"Failed to save product '{newProduct.Name}' to SQLite database:\n\n{ex.Message}"
                    );
                }
            }
        }

        /// <summary>
        /// Handles Edit Product command for active selection.
        /// </summary>
        public void HandleEditProduct()
        {
            Product? selected = GetActiveSelectedProduct();
            if (selected == null)
            {
                NotificationDialog.ShowWarning(
                    this,
                    "[Product Editing Selection]",
                    "Please select a product from the table to edit, or click '✏ Edit' directly in any product row."
                );
                return;
            }

            OpenEditDialog(selected);
        }

        /// <summary>
        /// Unified Edit method: Opens ProductForm modal for any given Product instance.
        /// Reused by table button clicks, row double-clicks, menu actions, and interactive chart bar clicks!
        /// </summary>
        public void OpenEditDialog(Product product)
        {
            if (product == null) return;

            using ProductForm form = new ProductForm(product);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    // Update in SQLite database
                    _repository.Update(form.CurrentProduct);

                    RefreshAllUI();

                    NotificationDialog.ShowSuccess(
                        this,
                        "Product Updated Successfully",
                        $"Product '{form.CurrentProduct.Name}' has been successfully updated in the SQLite database.",
                        form.CurrentProduct,
                        "Continue"
                    );
                }
                catch (Exception ex)
                {
                    NotificationDialog.ShowError(
                        this,
                        "[Product Editing Error]",
                        $"Failed to update product #{form.CurrentProduct.Id} ('{form.CurrentProduct.Name}') in SQLite database:\n\n{ex.Message}"
                    );
                }
            }
        }

        /// <summary>
        /// Handles Delete Product command: Shows modern modal confirmation and removes record from SQLite.
        /// </summary>
        public void HandleDeleteProduct()
        {
            Product? selected = GetActiveSelectedProduct();
            if (selected == null)
            {
                NotificationDialog.ShowWarning(
                    this,
                    "[Product Deletion Selection]",
                    "Please select a product from the table to delete."
                );
                return;
            }

            // Pro Modal confirmation dialog with Yes / No
            bool confirmed = NotificationDialog.ShowConfirmation(
                this,
                "Delete Product Confirmation",
                $"Are you sure you want to delete this product from the inventory database?\n\nThis action cannot be undone and will permanently remove all stock records for this item.",
                confirmText: "Yes, Delete",
                cancelText: "No, Keep Product",
                product: selected
            );

            if (confirmed)
            {
                try
                {
                    _repository.Delete(selected.Id);
                    RefreshAllUI();

                    NotificationDialog.ShowSuccess(
                        this,
                        "Product Deleted Successfully",
                        $"Product '{selected.Name}' (ID: #{selected.Id}) was permanently removed from the SQLite database.",
                        null,
                        "Done"
                    );
                }
                catch (Exception ex)
                {
                    NotificationDialog.ShowError(
                        this,
                        "[Product Deletion Error]",
                        $"Failed to delete product #{selected.Id} ('{selected.Name}') from SQLite database:\n\n{ex.Message}"
                    );
                }
            }
        }

        /// <summary>
        /// Handles Refresh Data command: Queries SQLite database and refreshes all UI components.
        /// </summary>
        public void HandleRefreshData()
        {
            RefreshAllUI();
        }

        /// <summary>
        /// Handles Focus Search Box command: Focuses and selects search input.
        /// </summary>
        public void HandleFocusSearch()
        {
            FocusSearchBox();
        }

        /// <summary>
        /// Handles Clear Search Filter command: Clears search query and restores full catalog.
        /// </summary>
        public void HandleClearSearch()
        {
            _txtSearch.Text = string.Empty;
            FocusSearchBox();
        }

        /// <summary>
        /// Handles Navigation to Dashboard View.
        /// </summary>
        public void HandleNavDashboard()
        {
            ShowView(isDashboard: true);
        }

        /// <summary>
        /// Handles Navigation to Products Catalog View.
        /// </summary>
        public void HandleNavProducts()
        {
            ShowView(isDashboard: false);
        }

        /// <summary>
        /// Filters catalog to items with Low Stock status.
        /// </summary>
        public void HandleFilterLowStock()
        {
            try
            {
                var all = _repository.GetAll();
                _displayedProducts = all.Where(p => p.StockStatus == "Low Stock").ToList();
                BindTableData(_dgvDashboardProducts);
                BindTableData(_dgvProducts);
                string countText = $"Showing {_displayedProducts.Count} of {all.Count} products (Filter: Low Stock)";
                _lblDashboardProductsCount.Text = countText;
                _lblProductsCount.Text = countText;
            }
            catch (Exception ex)
            {
                NotificationDialog.ShowError(this, "[Inventory Filter Error]", $"Failed to filter products by Low Stock status:\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Filters catalog to items with Out of Stock status.
        /// </summary>
        public void HandleFilterOutOfStock()
        {
            try
            {
                var all = _repository.GetAll();
                _displayedProducts = all.Where(p => p.StockStatus == "Out of Stock").ToList();
                BindTableData(_dgvDashboardProducts);
                BindTableData(_dgvProducts);
                string countText = $"Showing {_displayedProducts.Count} of {all.Count} products (Filter: Out of Stock)";
                _lblDashboardProductsCount.Text = countText;
                _lblProductsCount.Text = countText;
            }
            catch (Exception ex)
            {
                NotificationDialog.ShowError(this, "[Inventory Filter Error]", $"Failed to filter products by Out of Stock status:\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Resets all filters and displays the complete product catalog.
        /// </summary>
        public void HandleResetFilter()
        {
            _txtSearch.Text = string.Empty;
            RefreshAllUI();
        }

        /// <summary>
        /// Handles opening the Commands Explorer form.
        /// </summary>
        public void HandleOpenCommandsExplorer()
        {
            using CommandsExplorerForm explorer = new CommandsExplorerForm(this, Commands);
            explorer.ShowDialog(this);
        }

        /// <summary>
        /// Toggles the interactive Direct Command Terminal drawer.
        /// Allows direct CRUD command execution (add, edit, delete) without opening dialog windows.
        /// </summary>
        public void ToggleTerminal(bool? forceState = null)
        {
            _isTerminalVisible = forceState ?? !_isTerminalVisible;
            _terminalPanel.Visible = _isTerminalVisible;
            if (_btnNavTerminal != null)
            {
                _btnNavTerminal.IsActiveNav = _isTerminalVisible;
            }
            if (_isTerminalVisible)
            {
                _terminalPanel.FocusInput();
            }
        }

        /// <summary>
        /// Displays the User Guide and Keyboard Shortcuts instructions dialog.
        /// </summary>
        public void HandleUserGuide()
        {
            using HelpForm help = new HelpForm();
            help.ShowDialog(this);
        }

        public void HandleAbout()
        {
            using AboutForm about = new AboutForm();
            about.ShowDialog(this);
        }

        public void HandleExit()
        {
            Close();
        }
    }
}

