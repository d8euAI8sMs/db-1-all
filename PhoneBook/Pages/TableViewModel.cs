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
}
