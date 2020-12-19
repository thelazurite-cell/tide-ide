using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FontAwesome.WPF;
using Tamir.SharpSsh.java.lang;
using TAF.AutomationTool.Ui.Activities;
using TAF.AutomationTool.Ui.CustomElements;
using TAF.AutomationTool.Ui.ViewModels;
using TestAutomation.SolutionHandler.Core;
using TestAutomation.Tide.DataBase;
using TestAutomation.Tide.Ui.Controls;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;
using DataTable = TestAutomation.Tide.DataBase.DataTable;

namespace TAF.AutomationTool.Ui
{
    public partial class SqlRemoteData
    {
        private bool handlingDelete;
        private ProjectEventManager eventBindings;


        public void SetEventBindings(ProjectEventManager eventBindings)
        {
            this.SetupEventBindings(eventBindings);
        }

        public SqlRemoteData()
        {
            this.InitializeComponent();
            this.ResultDataGrid.EnableColumnVirtualization = true;
            this.ResultDataGrid.EnableRowVirtualization = true;
            this.DataContext = new SqlRemoteDataViewModel();
        }

        private void SetupEventBindings(ProjectEventManager eventBindings)
        {
            this.eventBindings = eventBindings;
            this.eventBindings.SqlDataReceived += this.SqlDataReceived;
            this.eventBindings.MainWinowRendered += this.MainWinowRendered;
            this.eventBindings.GetData += this.GetData;
            this.eventBindings.RefreshSqlData += this.RefreshSqlData;
        }

        private void RefreshSqlData(object sender, EventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            vm.Changes.Clear();
            this.InvokeDataFetch(vm);
        }

        private T GetParent<T>(DbView view)
            where T : DbView
        {
            var parent = view.Parent;
            while (parent != null)
            {
                if (parent is T result)
                {
                    return result;
                }

                parent = parent.Parent;
            }

            return null;
        }

        private async void GetData(object sender, EventArgs e)
        {
            Invoke(async () =>
            {
                if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
                if (!(sender is DataTable dataTable)) return;
                if (dataTable.Name != (vm?.Context?.Table?.Name ?? string.Empty))
                    vm.Context.Pagination.CurrentPage = 1;
                var columns = this.GetChildCollection<TableColumn>(dataTable);
                if (!columns.Any()) return;
                var connection = this.GetParent<Connection>(dataTable);
                using (var conn = new SqlDataManager(connection.Name, connection.ConnectionString))
                {
                    var results = await conn.GetTableData(dataTable, columns,
                        vm?.Context?.Pagination ?? new TablePagination());
                    this.eventBindings.SqlDataReceived?.Invoke(results, EventArgs.Empty);
                }
            });
        }

        private void MainWinowRendered(object sender, EventArgs e) =>
            new HorizontalMouseMove(this.ScrollViewer, this.eventBindings.ProjectWindow);

        private void SqlDataReceived(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!(sender is DtoCollection result)) return;
                    if (!result.Dtos.Any())
                    {
                        MessageBox.Show($"There are no records in {result.Table.FriendlyName}.");
                        return;
                    }

                    var listType = typeof(ObservableCollection<>);
                    SqlRemoteDataViewModel vm = null;

                    Invoke(() =>
                    {
                        if (!(this.DataContext is SqlRemoteDataViewModel vma)) return;
                        vma.Changes.Clear();
                        vma.IsClean = true;
                        vm = vma;
                        vm.Context = result;
                    });

                    var constructedListType = listType.MakeGenericType(vm.Context.DtoType);

                    var listInstance = Activator.CreateInstance(constructedListType);
                    Invoke(() =>
                    {
                        this.ResultDataGrid.DataContext = listInstance;
                        var bind = new Binding {IsAsync = true};
                        this.ResultDataGrid.SetBinding(ItemsControl.ItemsSourceProperty, bind);

                        vm.Context.Pagination.CurrentPageRowCount = result.Dtos.Count;
                        vm.Context.Pagination.CalculatePageNavigation(vm.Context.Pagination.CurrentPage);
                        this.DataContext = vm;
                        ((SqlRemoteDataViewModel) this.DataContext).Context.TimeTaken = vm.Context.TimeTaken;
                    });

                    result.Dtos.ForEach(itm =>
                    {
                        Thread.Sleep(10);
                        Invoke(() => constructedListType.GetMethod("Add")
                            ?.Invoke(listInstance, new[] {itm.CastToReflected(vm.Context.DtoType)}));
                        itm.PropertyChanged += this.ItmOnPropertyChanged;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private void ItmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.handlingDelete) return;
            var coll = this.ResultDataGrid.DataContext.GetType();
            var indexMethod = coll.GetMethod("IndexOf");
            if (indexMethod == null) return;
            var dataContext = this.ResultDataGrid.DataContext.CastToReflected(coll);

            var resultIndex = indexMethod.Invoke(dataContext,
                new[] {sender.CastToReflected(sender.GetType())});

            if (!(resultIndex is int indexOf)) return;
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            var obj = vm.Context.Dtos[indexOf];
            var dtoType = obj.GetType();
            var editProperty = dtoType.GetProperty(e.PropertyName);

            if (editProperty?.GetCustomAttribute<FieldNameAttribute>() == null) return;

            var originalProperty = dtoType.GetProperty($"Original{e.PropertyName}");
            var editValue = editProperty.GetValue(obj);
            var originalValue = originalProperty.GetValue(obj);
            var itms = vm.Changes
                .Where(i => i is SqlDataUpdate itm && itm.Index == indexOf && itm.Property == e.PropertyName)
                .ToList();

            foreach (var sqlDataChange in itms)
            {
                vm.Changes.Remove(sqlDataChange);
            }

            if (editValue != originalValue)
            {
                vm.Changes.Add(new SqlDataUpdate()
                {
                    Index = indexOf,
                    Property = e.PropertyName,
                    ColumnOriginalValue = originalValue,
                    ColumnValue = editValue
                });
            }

            Invoke(() =>
            {
                vm.IsClean = !vm.Changes.Any();
                this.DataContext = vm;
            });
        }

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer) sender;
            viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ResultDataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            if (vm.Context.DtoType.GetProperty(e.PropertyName).GetCustomAttribute(typeof(FieldNameAttribute))
                is FieldNameAttribute attrib)
            {
                this.ProcessBitType(e, attrib);
                e.Column.Header = attrib.FieldName;
            }
            else e.Column.Visibility = Visibility.Collapsed;
        }

        private void ProcessBitType(DataGridAutoGeneratingColumnEventArgs e,
            FieldNameAttribute fieldNameAttribute)
        {
            if (fieldNameAttribute.DataType == DataType.bit)
            {
                var checkBoxColumn = new DataGridCheckBoxColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName),
                    IsThreeState = fieldNameAttribute.IsNullable
                };

                e.Column = checkBoxColumn;
            }
            else
            {
                var type = e.Column.GetType();
                var dataGridTextColumn = new DataGridTextColumn()
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) {TargetNullValue = "[null]"}
                };
                e.Column = dataGridTextColumn;
            }
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            this.eventBindings.RefreshSqlData?.Invoke(vm.Context.Table, EventArgs.Empty);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            if (!(this.ResultDataGrid.DataContext is IList collection)) return;
            vm.AffectedRows.Clear();

            var modified = new UnpersistedChangesViewModel() {Context = vm.Context};
            foreach (var obj in collection)
            {
                var indexOf = collection.IndexOf(obj);
                if (vm.Changes.All(itm => itm.Index != indexOf)) continue;

                if (!(obj is DataTransferObject dto)) continue;

                var items = dto.GetType().GetProperties()
                    .Where(itm => itm.GetCustomAttribute<FieldNameAttribute>() != null).ToList();
                var affected = new List<string>();
                foreach (var propertyInfo in items)
                {
                    var attr = propertyInfo.GetCustomAttribute<FieldNameAttribute>().FieldName;
                    var val = dto.GetType()
                                  .GetProperty(dto.IsInserting ? propertyInfo.Name : $"Original{propertyInfo.Name}")
                                  ?.GetValue(dto) ?? "<null>";
                    affected.Add($"{attr} : {val}");
                }

                var dataChanges = vm.Changes.Where(itm => itm.Index == indexOf);
                foreach (var dataChange in dataChanges.OfType<SqlDataInsert>())
                {
                    dataChange.Source = collection[indexOf] as DataTransferObject;
                }

                modified.Modifications.Add(new AffectedRowChangePair
                {
                    IsDeleting = dto.IsDeleting,
                    IsInserting = dto.IsInserting,
                    IsUpdating = dataChanges.All(itm => itm is SqlDataUpdate) && (!dto.IsDeleting && !dto.IsInserting),
                    Affected = affected,
                    Changes = dataChanges.ToList()
                });
            }

            var win = new ConfirmSqlChanges(this.eventBindings)
            {
                DataContext = modified,
                Owner = this.eventBindings.ProjectWindow
            };
            win.ShowDialog();
        }

        private void ResultDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            if (e.Key != Key.Delete) return;
            this.HandleDeletion(vm);
            e.Handled = true;
        }

        public static TContainer GetContainerFromIndex<TContainer>
            (ItemsControl itemsControl, int index)
            where TContainer : DependencyObject
        {
            return (TContainer)
                itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        }

        public static DataGridRow GetEditingRow(DataGrid dataGrid)
        {
            var sIndex = dataGrid.SelectedIndex;
            if (sIndex >= 0)
            {
                var selected = GetContainerFromIndex<DataGridRow>(dataGrid, sIndex);
                if (selected.IsEditing) return selected;
            }

            for (var i = 0; i < dataGrid.Items.Count; i++)
            {
                if (i == sIndex) continue;
                var item = GetContainerFromIndex<DataGridRow>(dataGrid, i);
                if (item.IsEditing) return item;
            }

            return null;
        }

        private ObservableCollection<DbView> GetChildCollection<T>(DbView view)
            where T : DbView
        {
            if (view.Children.Any() && view.Children.All(itm => itm is T))
            {
                return view.Children;
            }

            foreach (var child in view.Children)
            {
                var observableCollection = this.GetChildCollection<T>(child);

                if (observableCollection != null)
                {
                    return observableCollection;
                }
            }

            return null;
        }

        private void HandleDeletion(SqlRemoteDataViewModel vm)
        {
            // If we're editing we wanna delete the current value of a cell instead. 
            var dataGridRow = GetEditingRow(this.ResultDataGrid);
            if (dataGridRow != null)
            {
                var current = this.ResultDataGrid.CurrentCell;
                var index = this.ResultDataGrid.SelectedIndex;
                if (index == -1)
                    return;
                var dc = this.ResultDataGrid.DataContext as IList;
                var dto = dc[index] as DataTransferObject;
                var head = current.Column.Header.ToString();
                dto?.GetType().GetProperty(head).SetValue(dto, null);
                return;
            }

            // otherwise we've gotta handle deletion of a row.
            this.handlingDelete = true;

            var list = (this.ResultDataGrid.DataContext as IList);

            foreach (var selectedItem in this.ResultDataGrid.SelectedItems.OfType<DataTransferObject>().ToList())
            {
                if (!(selectedItem is DataTransferObject dto)) continue;
                var indexOf = list.IndexOf(dto);
                var dtoFromList = (list[indexOf] as DataTransferObject);
                var currentValue = dtoFromList.IsDeleting;

                // if it was a row pending addition, we'll disregard it.
                if (dtoFromList.IsInserting)
                {
                    list.RemoveAt(indexOf);
                }
                // otherwise mark the item as pending deletion.
                else
                {
                    dtoFromList.IsDeleting = !currentValue;

                    var changes = vm.Changes.Where(itm => itm.Index == indexOf).OfType<SqlDataUpdate>()
                        .ToList();

                    if (changes.Any())
                    {
                        foreach (var change in changes)
                        {
                            dtoFromList.GetType().GetProperty(change.Property)?.SetValue(dtoFromList,
                                dtoFromList.IsDeleting ? change.ColumnOriginalValue : change.ColumnValue);
                        }
                    }

                    if (dtoFromList.IsDeleting)
                    {
                        vm.Changes.Add(new SqlDataDelete() {Index = indexOf});
                    }
                    else
                    {
                        var deletes = vm.Changes.Where(itm => itm.Index == indexOf).OfType<SqlDataDelete>()
                            .ToList();
                        if (!deletes.Any()) continue;
                        foreach (var delete in deletes)
                        {
                            vm.Changes.Remove(delete);
                        }
                    }
                }
            }

            vm.IsClean = !vm.Changes.Any();
            this.handlingDelete = false;
            this.ResultDataGrid.DataContext = list;
            this.DataContext = vm;
        }

        private void ResultDataGrid_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            if (!(this.ResultDataGrid.DataContext is IList list)) return;
            vm.IsClean = false;
            if (Activator.CreateInstance(vm.Context.DtoType) is DataTransferObject dto)
            {
                dto.IsInserting = true;
                list.Add(dto);
                this.ResultDataGrid.DataContext = list;
                var index = list.IndexOf(dto);
                vm.Changes.Add(new SqlDataInsert {Index = index});
            }

            this.DataContext = vm;
        }

        private void ResultDataGrid_OnInitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (this.ResultDataGrid.DataContext is IList list)
            {
                list.Remove(e.NewItem);
                this.ResultDataGrid.SelectedIndex = -1;
            }
        }

        private void SortStates_OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (!(sender is Grid grid) || !(grid.FindName("SortIcon") is ImageAwesome ico)) return;

            const string sortascending = "SortAscending";
            const string sortdescending = "SortDescending";

            switch (e.NewState.Name)
            {
                case sortascending:
                    ico.Icon = FontAwesomeIcon.CaretUp;
                    break;
                case sortdescending:
                    ico.Icon = FontAwesomeIcon.CaretDown;
                    break;
                default:
                    ico.Icon = FontAwesomeIcon.None;
                    break;
            }
        }

        private void PART_NULL_PLACEHOLDER_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Label textBlock)) return;
            if (textBlock.IsMouseOver && e.LeftButton == MouseButtonState.Pressed)
            {
                var grid = textBlock.FindControlAbove<Grid>();
                var presenter = grid.FindName("PART_PRESENTER") as ContentPresenter;
                textBlock.Visibility = Visibility.Collapsed;
                presenter.Visibility = Visibility.Visible;
                (presenter.TemplatedParent as DataGridCell).Focus();
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            // experimental code. hopefully there's a better way of doing this rubbish.
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;

            DataGridSelectionUnit original = DataGridSelectionUnit.FullRow;
            if (!(sender is ContentPresenter presenter)) return;
            var tp = (presenter.TemplatedParent as DataGridCell);
            Invoke(() =>
            {
                if (!(presenter.Content is FrameworkElement fe)) return;
                original = this.ResultDataGrid.SelectionUnit;
                this.ResultDataGrid.SelectionUnit = DataGridSelectionUnit.Cell;
                this.ResultDataGrid.SelectedCells.Clear();
            });

            tp.IsSelected = true;
            var list = this.ResultDataGrid.DataContext as IList;
            var dataGridCellInfo = this.ResultDataGrid.SelectedCells.First();
            var row = list.IndexOf(dataGridCellInfo.Item);
            var grid = presenter.FindControlAbove<Grid>();
            if (!(grid.FindName("PART_NULL_PLACEHOLDER") is FrameworkElement textBlock)) return;

            if (row == -1)
            {
                this.SetPresenterVisibility(textBlock, presenter);
                return;
            }

            try
            {
                var dcRow = vm.Context.Dtos[row];
                if (!dcRow.IsInserting)
                {
                    this.SetPresenterVisibility(textBlock, presenter);
                    return;
                }
            }
            catch (Exception)
            {
            }

            var col = dataGridCellInfo.Column.Header;
            var propertyInfo = list[row].GetType().GetProperty(col.ToString());
            var value = propertyInfo?.GetValue(list[row]);
            var fieldAttribute = propertyInfo.GetCustomAttribute<FieldNameAttribute>();
            if (fieldAttribute == null)
            {
                this.SetPresenterVisibility(textBlock, presenter);
                return;
            }

            if (value == null && fieldAttribute.DataType != DataType.bit)
            {
                var column = vm.Context.Columns.FirstOrDefault(itm => itm.Name.ToString() == col.ToString());
                switch (column)
                {
                    case TableColumn tableColumn when tableColumn.IsGenerated:
                        textBlock.SetValue(ContentProperty, "[Generated]");
                        textBlock.SetValue(IsEnabledProperty, false);
                        this.SetPresenterVisibility(textBlock, presenter, false);
                        break;
                    case TableColumn tableColumn when tableColumn.HasDefaultValue:
                        textBlock.SetValue(ContentProperty, "[Default]");
                        this.SetPresenterVisibility(textBlock, presenter, false);
                        break;
                }
            }
            else
            {
                this.SetPresenterVisibility(textBlock, presenter);
            }

            tp.Focus();
            this.ResultDataGrid.SelectedCells.Clear();
            this.ResultDataGrid.SelectionUnit = original;
        }

        private void SetPresenterVisibility(FrameworkElement frameworkElement, ContentPresenter contentPresenter,
            bool visible = true, DataGridSelectionUnit original = DataGridSelectionUnit.FullRow)
        {
            frameworkElement.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
            contentPresenter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            this.ResultDataGrid.SelectedCells.Clear();
            this.ResultDataGrid.SelectionUnit = original;
            return;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1 || !(this.DataContext is SqlRemoteDataViewModel vm)) return;
            var first = e.AddedItems[0];
            if (!(first is int f) || f == vm.Context.Pagination.PageSize) return;

            vm.Context.Pagination.PageSize = f;
            vm.Context.Pagination.CurrentPage = 1;
            this.eventBindings.GetData?.Invoke(vm.Context.Table, EventArgs.Empty);
        }

        private void SqlRemoteData_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            vm.Context.Pagination.CurrentPage++;
            this.InvokeDataFetch(vm);
        }

        private void InvokeDataFetch(SqlRemoteDataViewModel sqlRemoteDataViewModel)
        {
            this.eventBindings.GetData?.Invoke(sqlRemoteDataViewModel.Context.Table, EventArgs.Empty);
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;

                if (sender is TextBox tb)
                {
                    int.TryParse(tb.Text, out var page);

                    if (vm.Context.Pagination.TotalPages < page)
                    {
                        new SharpDialog("Value is out of range", type: SharpDialog.SharpDialogType.Asterisk,
                                parent: this.eventBindings.ProjectWindow)
                            .ShowDialog();
                        return;
                    }

                    vm.Context.Pagination.CurrentPage = page == 0 ? 1 : page;
                }

                this.InvokeDataFetch(vm);
            }
        }

        private void FinalPage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;

            vm.Context.Pagination.CurrentPage = vm.Context.Pagination.TotalPages;
            this.InvokeDataFetch(vm);
        }

        private void PreviousPage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;

            vm.Context.Pagination.CurrentPage--;
            this.InvokeDataFetch(vm);
        }

        private void FirstPage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;

            vm.Context.Pagination.CurrentPage = 1;
            this.InvokeDataFetch(vm);
        }

        private void TogglePagination_OnLMBDown(object sender, MouseButtonEventArgs e)
        {
            if (!(this.DataContext is SqlRemoteDataViewModel vm)) return;
            if (!vm.Context.Pagination.IsEnabled || e.LeftButton != MouseButtonState.Pressed ||
                !(sender as CheckBox).IsMouseOver) return;
            var dlg = new SharpDialog(
                "Data operations on large tables may take a long time. Are you sure you want to disable paging?",
                type: SharpDialog.SharpDialogType.Question, buttons: SharpDialog.SharpDialogButton.YesNo,
                parent: this.eventBindings.ProjectWindow);
            dlg.ShowDialog();
            if (dlg.Result != SharpDialog.SharpDialogResult.Yes)
            {
                e.Handled = true;
            }
            else
            {
                this.TogglePagination.IsChecked = false;
            }
        }
    }
}