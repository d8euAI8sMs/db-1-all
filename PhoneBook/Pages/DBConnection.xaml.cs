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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class DBConnectionPage : UserControl
    {

        private bool firstLoad = true;

        public DBConnectionPage()
        {
            InitializeComponent();

            this.DataContext = new DBConnectionViewModel();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!((DBConnectionViewModel)this.DataContext).Test()) return;
            ((DBConnectionViewModel)this.DataContext).Apply();
            NavigationWindow nav = this.Parent as NavigationWindow;
            NavigationCommands.GoToPage.Execute("/Pages/Cities.xaml", this);
        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            ((DBConnectionViewModel)this.DataContext).Test();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (firstLoad) Apply_Click(sender, e);
            firstLoad = false;
        }

        private void CreateSchema_Click(object sender, RoutedEventArgs e)
        {
            ((DBConnectionViewModel)this.DataContext).CreateSchema();
        }

        private void RunSql_Click(object sender, RoutedEventArgs e)
        {
            ((DBConnectionViewModel)this.DataContext).RunSql();
        }
    }
}
