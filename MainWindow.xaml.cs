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
using System.Xml.Linq;

namespace AhoCorasick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public void AddFile(string name)
        {
            if (FileList.Dispatcher.CheckAccess())
            {
                // We are on the UI thread, directly modify the ListBox
                FileList.Items.Add(name);
            }
            else
            {
                // We are on a different thread, invoke the Dispatcher to modify the ListBox
                FileList.Dispatcher.Invoke(() => FileList.Items.Add(name));
            }
        }

        public void SetStatus(string status)
        {
            StatusBar.Dispatcher.Invoke(() => StatusBar.Content = status);
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
