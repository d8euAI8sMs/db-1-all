using FirstFloor.ModernUI.Windows.Controls;

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

namespace PhoneBook.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class CitiesPage : UserControl
    {

        private bool firstLoad = true;
        private TableViewModel<City> vm = new TableViewModel<City>();

        public CitiesPage()
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.Items = MainModel.Instance.GetCities();
                vm.Message = "Loaded";
            } catch (Exception ex)
            {
                vm.Message = ex.Message;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            City city = new City();
            ModernDialog dialog = new ModernDialog
            {
                Content = new CityEditPage(city),
                Title = "New City"
            };
            dialog.Buttons = new[] { dialog.OkButton, dialog.CancelButton };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MainModel.Instance.Persist(city);
                    vm.Items = MainModel.Instance.GetCities();
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
            City selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            City city = new City(selected);
            ModernDialog dialog = new ModernDialog
            {
                Content = new CityEditPage(city),
                Title = "Edit City"
            };
            dialog.Buttons = new[] { dialog.OkButton, dialog.CancelButton };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MainModel.Instance.Persist(city);
                    vm.Items = MainModel.Instance.GetCities();
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
            City selected = vm.Selected;
            if (selected == null)
            {
                return;
            }
            try
            {
                MainModel.Instance.Delete(selected);
                vm.Items = MainModel.Instance.GetCities();
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
    }
}
