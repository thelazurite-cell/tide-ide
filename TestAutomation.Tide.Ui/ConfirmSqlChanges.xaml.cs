using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TAF.AutomationTool.Ui.Activities;
using TAF.AutomationTool.Ui.ViewModels;
using TestAutomation.Tide.DataBase;
using TestAutomation.Tide.Ui.Controls;
using DtoException = TestAutomation.Tide.DataBase.DtoException;

namespace TAF.AutomationTool.Ui
{
    /// <summary>
    /// Interaction logic for ConfirmSqlChanges.xaml
    /// </summary>
    public partial class ConfirmSqlChanges
    {
        private readonly ProjectEventManager eventBindings;

        public ConfirmSqlChanges(ProjectEventManager eventBindings)
        {
            this.eventBindings = eventBindings;
            this.InitializeComponent();
        }

        private async void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is UnpersistedChangesViewModel vm)) return;
            foreach (var rowIndex in vm.Modifications)
            {
                if (rowIndex.IsUpdating)
                {
                    var changesToRow = rowIndex.Changes
                        .OfType<SqlDataUpdate>().ToList();
                    if (!changesToRow.Any()) continue;
                    var row = vm.Context.Dtos[changesToRow.FirstOrDefault().Index];
                    row.UpdateFailed += this.Row_UpdateFailed;
                    await row.Update(changesToRow, vm.Context);
                }
                else if (rowIndex.IsInserting)
                {
                    var insertRow = rowIndex.Changes.OfType<SqlDataInsert>().FirstOrDefault();
                    if (insertRow == null) continue;
                    insertRow.Source.InsertFailed += this.Row_InsertFailed;
                    await insertRow.Source.Insert(vm.Context);
                }
                else if (rowIndex.IsDeleting)
                {
                    var deleteRow = rowIndex.Changes.OfType<SqlDataDelete>().FirstOrDefault();
                    if(deleteRow == null) continue;
                    var row = vm.Context.Dtos[deleteRow.Index];
                    row.DeleteFailed += this.Row_DeleteFailed;
                    await row.Delete(deleteRow, vm.Context);
                }
            }

            this.eventBindings.RefreshSqlData?.Invoke(vm.Context.Table, EventArgs.Empty);
            this.Close();
        }

        private void Row_DeleteFailed(object sender, EventArgs e)
        {
            if (!this.IsDtoException(sender, out var error)) return;
        }

        private void Row_InsertFailed(object sender, EventArgs e)
        {
            if (!this.IsDtoException(sender, out var error)) return;
        }

        private void Row_UpdateFailed(object sender, EventArgs e)
        {
            if (!this.IsDtoException(sender, out var error)) return;
        }

        private bool IsDtoException(object sender, out DtoException dtoException)
        {
            if ((sender is DtoException error))
            {
                var dlg = new SharpDialog($"Action failed because:\n{this.generateExceptionString(error.Exception)}");dlg.ShowDialog();
                dtoException = error;
                return true;
            }

            dtoException = null;
            return true;
        }

        private string generateExceptionString(Exception e) => e == null 
            ? string.Empty 
            : $"({e.GetType().Name}):{e.Message}\r\n{this.generateExceptionString(e.InnerException)}";

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer) sender;
            viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfirmSqlChanges_OnContentRendered(object sender, EventArgs e)
        {
            new HorizontalMouseMove(this.ScrollViewer, this);
        }
    }
}