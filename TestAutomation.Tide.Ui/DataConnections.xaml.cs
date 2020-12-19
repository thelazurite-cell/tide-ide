using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FontAwesome.WPF;
using TAF.AutomationTool.Ui.Activities;
using TestAutomation.SolutionHandler.Core;
using TestAutomation.Tide.DataBase;
using TestAutomation.Tide.Ui.Controls;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;
using TableColumn = TestAutomation.Tide.DataBase.TableColumn;

namespace TAF.AutomationTool.Ui
{
    public partial class DataConnections
    {
        private readonly ProjectEventManager eventBindings;
        public ObservableCollection<DbView> Connections = new ObservableCollection<DbView>();

        public DataConnections(ObservableCollection<Connection> connections, ProjectEventManager eventBindings)
        {
            this.InitializeComponent();
            this.eventBindings = eventBindings;
            connections.ToList().ForEach(this.Connections.Add);
            this.eventBindings.MainWinowRendered += this.MainWinowRendered;
            this.DataList.ItemsSource = this.Connections;
        }

        private void MainWinowRendered(object sender, EventArgs e)
        {
            new HorizontalMouseMove(this.ScrollViewer, this.eventBindings.ProjectWindow);
        }

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer) sender;
            viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private async void DataList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(this.DataList.SelectedItem is DbView view)) return;
            try
            {
                switch (view.Type)
                {
                    case DbType.Connection:
                        await this.GetDatabases(view);
                        break;
                    case DbType.Database:
                        await this.GetTables(view);
                        break;
                    case DbType.Table:
                        var table = (DbView) this.DataList.SelectedItem;
                        await this.CreateTableInfoHierarchy(table);
                        break;
                    case DbType.Column:
                        break;
                    case DbType.Primary:
                        break;
                    case DbType.Secondary:
                        break;
                    case DbType.Folder:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                var dlg = new SharpDialog(title: "Error", message: $"Couldn't connect to the server {ex.Message}",
                    parent: this.eventBindings.ProjectWindow);
                dlg.ShowDialog();
            }
        }

        private async Task<bool> CreateTableInfoHierarchy(DbView table)
        {
            if (table.Children.Any()) return true;
            var connection = this.GetParent<Connection>(table);
            var database = this.GetParent<Database>(table);
            using (var manager = new SqlDataManager(connection.Name, connection.ConnectionString))
            {
                if (!(table is DataTable tableView)) return false;
                var columns = await manager.GetTableColumns(tableView);
                var pks = await manager.GetPrimaryKeys(database.Name, table.Name);
                var fks = await manager.GetForeignKeys(database.Name, table.Name);
                var cnsts = await manager.GetConstraints(database.Name, table.Name);

                var columnsFolder = new Folder {Name = "Columns", Parent = table};

                foreach (var column in columns)
                {
                    var isKey = pks.FirstOrDefault(itm =>
                        itm.IndexColumnsNames.Any(key => key == column.Name));
                    if (isKey != null)
                    {
                        column.IsGenerated =
                            await manager.GetAutoIncrementCheck(database.Name, column.Name, table.Name);
                        column.Icon = FontAwesomeIcon.Key;
                        column.IsPrimaryKey = true;
                    }

                    columnsFolder.Children.Add(column);
                }

                table.Children.Add(columnsFolder);


                if (tableView.TableType != DataTableType.View)
                {
                    var pksFolder = new Folder {Name = "Primary Keys", Parent = table};

                    foreach (var primaryKey in pks)
                    {
                        pksFolder.Children.Add(primaryKey);
                    }

                    table.Children.Add(pksFolder);
                }

                if (tableView.TableType != DataTableType.View)
                {
                    var fksFolder = new Folder {Name = "Foreign Keys", Parent = table};

                    foreach (var foreignKey in fks)
                    {
                        fksFolder.Children.Add(foreignKey);
                    }

                    table.Children.Add(fksFolder);
                }

                var cnstsFolder = new Folder {Name = "Constraints", Parent = table};
                foreach (var constraint in cnsts)
                {
                    cnstsFolder.Children.Add(constraint);
                }

                table.Children.Add(cnstsFolder);
            }

            return false;
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


        private async Task<bool> GetTables(DbView view)
        {
            if (view.Children.Any()) return false;
            if (!(view is Database databaseView)) return false;
            if (!(view.Parent is Connection connDatabase)) return false;
            using (var managerDatabase = new SqlDataManager(connDatabase.Name, connDatabase.ConnectionString))
            {
                var dbConnIndex = this.Connections.IndexOf(connDatabase);

                var dbView = this.Connections[dbConnIndex].Children;
                var dbViewChild = dbView[dbView.IndexOf(databaseView)];
                var databaseTables = await managerDatabase.GetDatabaseTables(view.Name);

                var tableFolder = new Folder {Name = "Tables", Parent = dbViewChild};
                foreach (var table in databaseTables.Where(itm => itm.TableType == DataTableType.Table))
                {
                    table.Parent = tableFolder;
                    tableFolder.Children.Add(table);
                }

                var viewFolder = new Folder {Name = "Views", Parent = dbViewChild};
                foreach (var viewType in databaseTables.Where(itm => itm.TableType == DataTableType.View))
                {
                    viewType.Parent = viewFolder;
                    viewFolder.Children.Add(viewType);
                }

                dbViewChild.Children.Add(tableFolder);
                dbViewChild.Children.Add(viewFolder);
                return true;
            }
        }

        private async Task<bool> GetDatabases(DbView view1)
        {
            if (view1.Children.Any()) return false;
            var index = this.Connections.IndexOf(view1);

            if (!(view1 is Connection connRoot)) return true;
            using (var connectionName = new SqlDataManager(connRoot.Name, connRoot.ConnectionString))
            {
                foreach (var database in await connectionName.GetDatabases())
                {
                    database.Parent = connRoot;
                    Invoke(() => this.Connections[index].Children.Add(database));
                }

                return true;
            }
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataList.SelectedItem is DataTable table)) return;
            await this.CreateTableInfoHierarchy(table).ContinueWith(_ =>
                this.eventBindings.GetData?.Invoke(table, EventArgs.Empty));
        }
    }
}