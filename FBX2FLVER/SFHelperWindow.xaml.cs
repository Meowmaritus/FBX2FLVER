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
using System.Windows.Shapes;

namespace FBX2FLVER
{
    /// <summary>
    /// Interaction logic for SFHelperWindow.xaml
    /// </summary>
    public partial class SFHelperWindow : Window
    {
        public string Result;

        public void SetBNDList(IEnumerable<string> files)
        {
            foreach (var f in files)
            {
                ListViewBNDFiles.Items.Add(new Label() { Content = f });
            }
        }

        public SFHelperWindow()
        {
            InitializeComponent();
        }

        private void ListViewBNDFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Result = (string)(((Label)ListViewBNDFiles.SelectedItem).Content);
            Close();
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            Result = (string)(((Label)ListViewBNDFiles.SelectedItem).Content);
            Close();
        }
    }
}
