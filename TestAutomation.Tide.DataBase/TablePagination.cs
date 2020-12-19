using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestAutomation.Tide.DataBase.Properties;

namespace TestAutomation.Tide.DataBase
{
    public class TablePagination : INotifyPropertyChanged
    {
        private bool isEnabled = true;
        private int pageSize = 250;
        private int currentPage = 1;
        private int startingIndex = 0;
        private int rowsCount;
        private int currentPageRowCount;
        private int totalPages;
        private bool hasNextPage = true;
        private bool hasPrevPage = false;


        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                this.isEnabled = value;
                this.OnPropertyChanged(nameof(this.IsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        public int PageSize
        {
            get => this.pageSize;
            set
            {
                this.pageSize = value;
                this.OnPropertyChanged(nameof(this.PageSize));
                this.CalculateTotalPages();
            }
        }

        public int CurrentPage
        {
            get => this.currentPage;
            set
            {
                this.CalculatePageNavigation(value);
                this.currentPage = value;
                this.OnPropertyChanged(nameof(this.CurrentPage));
            }
        }

        public int RowsCount
        {
            get => this.rowsCount;
            set
            {
                this.rowsCount = value;
                this.OnPropertyChanged(nameof(this.RowsCount));
                this.CalculateTotalPages();
            }
        }

        public int TotalPages
        {
            get => this.totalPages;
            set
            {
                this.totalPages = value;
                this.OnPropertyChanged(nameof(this.TotalPages));
            }
        }

        public int StartingIndex
        {
            get => this.startingIndex;
            set
            {
                this.startingIndex = value;
                this.OnPropertyChanged(nameof(this.StartingIndex));
            }
        }

        public int EndingIndex => this.startingIndex + this.pageSize;

        public int CurrentPageRowCount
        {
            get => this.currentPageRowCount;
            set
            {
                this.currentPageRowCount = value;
                this.OnPropertyChanged(nameof(this.CurrentPageRowCount));
            }
        }

        public bool HasNextPage
        {
            get => this.hasNextPage;
            set
            {
                this.hasNextPage = value;
                this.OnPropertyChanged(nameof(this.HasNextPage));
            }
        }

        public bool HasPreviousPage
        {
            get => this.hasPrevPage;
            set
            {
                this.hasPrevPage = value;
                this.OnPropertyChanged(nameof(this.HasPreviousPage));
            }
        }

        public void CalculateTotalPages()
        {
            var pages = this.rowsCount / this.pageSize;
            if (this.rowsCount % this.pageSize > 0) ++pages;
            this.TotalPages = pages;
        }

        public void CalculatePageNavigation(int currPage)
        {
            this.HasNextPage = this.CurrentPageRowCount == this.PageSize && this.EndingIndex < this.RowsCount;
            this.startingIndex = (currPage - 1) * this.PageSize;
            this.HasPreviousPage = this.CurrentPage > 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}