using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace PhoneBook.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class ContactsPage : UserControl
    {

        private bool firstLoad = true;
        private ContactTableViewModel vm = new ContactTableViewModel();

        public ContactsPage()
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.Items = MainModel.Instance.GetContacts();
                vm.Message = "Loaded";
            } catch (Exception ex)
            {
                vm.Message = ex.Message;
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.Items = MainModel.Instance.GetContacts(vm.Filter.Name, vm.Filter.Address, vm.Filter.BirthDate, vm.Filter.Phone);
                vm.Message = "Loaded";
            }
            catch (Exception ex)
            {
                vm.Message = ex.Message;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ContactEditViewModel cvm = new ContactEditViewModel(new Contact());
            cvm.Cities = MainModel.Instance.GetCities();
            cvm.Persons = MainModel.Instance.GetPersons();
            ModernDialog dialog = new ModernDialog
            {
                Content = new ContactEditPage(cvm),
                Title = "New Contact"
            };
            dialog.Buttons = new[] { dialog.OkButton, dialog.CancelButton };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MainModel.Instance.Persist(cvm.Contact);
                    vm.Items = MainModel.Instance.GetContacts();
                    vm.Message = "Item Presisted";
                }
                catch (Exception ex)
                {
                    vm.Message = ex.Message;
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Contact selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            Contact contact = new Contact(selected);
            ContactEditViewModel cvm = new ContactEditViewModel(contact);
            cvm.Cities = MainModel.Instance.GetCities();
            cvm.Persons = MainModel.Instance.GetPersons();
            ModernDialog dialog = new ModernDialog
            {
                Content = new ContactEditPage(cvm),
                Title = "Edit Contact"
            };
            dialog.Buttons = new[] { dialog.OkButton, dialog.CancelButton };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MainModel.Instance.Persist(cvm.Contact);
                    vm.Items = MainModel.Instance.GetContacts();
                    vm.Message = "Item Presisted";
                }
                catch (Exception ex)
                {
                    vm.Message = ex.Message;
                }
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Contact selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            try
            {
                MainModel.Instance.Delete(selected);
                vm.Items = MainModel.Instance.GetContacts();
                vm.Message = "Item Deleted";
            }
            catch (Exception ex)
            {
                vm.Message = ex.Message;
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (firstLoad) Refresh_Click(sender, e);
            firstLoad = false;
        }

        private void ShowPerson_Click(object sender, RoutedEventArgs e)
        {
            Contact selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            ModernDialog dialog = new ModernDialog
            {
                Content = new PersonEditPage(new Person(selected.Person)),
                Title = "Person Info"
            };
            dialog.Buttons = new[] { dialog.CloseButton };
            dialog.ShowDialog();
        }

        private void ShowCity_Click(object sender, RoutedEventArgs e)
        {
            Contact selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            if (selected.City == null)
            {
                vm.Message = "This contact has no city specified.";
                return;
            }
            ModernDialog dialog = new ModernDialog
            {
                Content = new CityEditPage(new City(selected.City)),
                Title = "City Info"
            };
            dialog.Buttons = new[] { dialog.CloseButton };
            dialog.ShowDialog();
        }
    }

    public class ContactTableViewModel : TableViewModel<Contact>
    {
        private Filter filter = new Filter();

        public Filter Filter {
            get
            {
                return this.filter;
            }
        }
    }

    public class Filter
    {
        public string Name {get; set; }
        public string Address {get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
    }
}
