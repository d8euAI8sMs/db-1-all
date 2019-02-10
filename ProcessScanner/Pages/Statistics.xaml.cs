using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Presentation;

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
    public partial class StatisticsPage : UserControl
    {

        private bool firstLoad = true;
        private StatisticsModel vm = new StatisticsModel();

        public StatisticsPage()
        {
            InitializeComponent();
            this.DataContext = vm;
            MainModel.Instance.StartCallback += StartedEventHandler;
            MainModel.Instance.ExitCallback += TerminatedEventHandler;
        }

        private void StartedEventHandler(System.Diagnostics.Process p, string username)
        {
            Refresh_Click(null, null);
        }
        private void TerminatedEventHandler(System.Diagnostics.Process p, string username)
        {
            Refresh_Click(null, null);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.Master.Items = MainModel.Instance.GetSummary(vm.Filter.ProcessName, vm.Filter.UserName, vm.Filter.Start, vm.Filter.End);
                vm.Slave.Items = MainModel.Instance.GetSessions(vm.Filter.ProcessName, vm.Filter.UserName, vm.Filter.Start, vm.Filter.End);
                vm.Message = "Loaded";
            } catch (Exception ex)
            {
                vm.Message = ex.Message;
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            Refresh_Click(null, null);
        }

        private void FilterBy_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Master.Selected == null) return;
            vm.Filter.ProcessName = vm.Master.Selected.Process.ProcessName;
            Refresh_Click(null, null);
        }
        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            vm.Filter.ProcessName = null;
            vm.Filter.UserName = null;
            vm.Filter.Start = null;
            vm.Filter.End = null;
            Refresh_Click(null, null);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (firstLoad) Refresh_Click(sender, e);
            firstLoad = false;
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {
            var selected = sender as ListViewItem;
            ProcessSummary content = selected.Content as ProcessSummary;
            foreach(Session s in vm.Slave.Items)
            {
                if (s.Process.Equals(content.Process))
                {
                    vm.Slave.Selected = s;
                    break;
                }
            }
        }
    }

    public class Filter : NotifyPropertyChanged
    {
        string processName;
        string userName;
        DateTime? start;
        DateTime? end;
        public string ProcessName
        {
            get
            {
                return processName;
            }
            set
            {
                processName = value;
                OnPropertyChanged("ProcessName");
            }
        }
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }
        public DateTime? Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
                OnPropertyChanged("Start");
            }
        }
        public DateTime? End
        {
            get
            {
                return end;
            }
            set
            {
                end = value;
                OnPropertyChanged("End");
            }
        }

    }

    public class StatisticsModel : TablesViewModel<ProcessSummary, Session>
    {

        Filter filter = new Filter();

        public Filter Filter { get { return filter; } }
    }
}
