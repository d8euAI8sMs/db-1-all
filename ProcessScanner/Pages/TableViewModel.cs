using FirstFloor.ModernUI.Presentation;

using System;
using PhoneBook;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace PhoneBook.Pages
{

    public class TableViewModel<T>
        : NotifyPropertyChanged
    {

        string message = "";

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                this.message = value;
                OnPropertyChanged("Message");
            }
        }

        ICollection<T> items = null;
        T selected;

        public ICollection<T> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
                OnPropertyChanged("Items");
            }
        }

        public T Selected {
            get { return this.selected; }
            set { this.selected = value; OnPropertyChanged("Selected"); }
        }
    }

    public class TablesViewModel<T1, T2>
        : NotifyPropertyChanged
    {

        TableViewModel<T1> master = new TableViewModel<T1>();
        TableViewModel<T2> slave = new TableViewModel<T2>();

        public string Message
        {
            get
            {
                return master.Message;
            }
            set
            {
                master.Message = value;
                OnPropertyChanged("Message");
            }
        }
        public TableViewModel<T1> Master
        {
            get { return master; }
        }
        public TableViewModel<T2> Slave
        {
            get { return slave; }
        }
    }
}
