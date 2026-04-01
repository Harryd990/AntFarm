using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AntFarm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void createNdestroybut_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelCreateDestroy);
        }

        private void SettingsBut_click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelSettings);
        }

        private void Dev_tools_But_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelDevTools);
        }

        private void ShowActionsPanel(StackPanel panelToShow)
        {
            PanelCreateDestroy.Visibility = Visibility.Collapsed;
            PanelSettings.Visibility = Visibility.Collapsed;
            PanelDevTools.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
        }
        // Create and Destroy
        private void BuildFarm_Click(object sender, RoutedEventArgs e) { }
        private void BuildFoodStore_Click(object sender, RoutedEventArgs e) { }
        private void Dig_Click(object sender, RoutedEventArgs e) { }
        private void RemoveBuilding_Click(object sender, RoutedEventArgs e) { }

        // Settings
        private void IdealPop_Click(object sender, RoutedEventArgs e) { }
        private void ExitGame_Click(object sender, RoutedEventArgs e) { }
        private void SaveGame_Click(object sender, RoutedEventArgs e) { }
        private void GetStats_Click(object sender, RoutedEventArgs e) { }

        // Dev Tools
        private void SpawnEntity_Click(object sender, RoutedEventArgs e) { }
        private void RemoveArea_Click(object sender, RoutedEventArgs e) { }
        private void GetCellDetails_Click(object sender, RoutedEventArgs e) { }
        private void EditEntity_Click(object sender, RoutedEventArgs e) { }

    }  
}     