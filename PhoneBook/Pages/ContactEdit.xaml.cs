using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Presentation;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoneBook.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class ContactEditPage : UserControl
    {

        public ContactEditPage(ContactEditViewModel contactVM)
        {
            InitializeComponent();
            this.DataContext = contactVM;
        }

        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {
            ((ContactEditViewModel)this.DataContext).HideAndShow();
        }
    }

    public class ContactEditViewModel : NotifyPropertyChanged
    {
        private static IEnumerable<ContactType> contactTypes = Enum.GetValues(typeof(ContactType)).Cast<ContactType>();

        public ContactEditViewModel(Contact contact)
        {
            Contact = contact;
        }

        public Contact Contact { get; set; }

        public ICollection<City> Cities { get; set; }

        public ICollection<Person> Persons { get; set; }

        public IEnumerable<ContactType> ContactTypes { get { return contactTypes; } }

        public bool IsCityPhone {
            get
            {
                return Contact.Type != ContactType.Mobile;
            }
        }
        public void HideAndShow()
        {
            OnPropertyChanged("IsCityPhone");
        }
    }
}
