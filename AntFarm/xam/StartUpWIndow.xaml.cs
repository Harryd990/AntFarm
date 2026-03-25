using AntFarm.handelers;
using AntFarm.main;
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

namespace AntFarm
{
    /// <summary>
    /// Interaction logic for StartUpWIndow.xaml
    /// </summary>
    public partial class StartUpWIndow : Window
    {
        
        public StartUpWIndow()
        {
            InitializeComponent();
            
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            Gamestart newgame = new Gamestart((int)GridWidthSlider.Value, (int)GridHeightSlider.Value, (int)AntCountSlider.Value, (int)FoodCountSlider.Value);
            if(newgame!= null )
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            // do later but is just loading grid and variables from a file then opening main window with those settings

        }

        private void FoodCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void GridHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void GridWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void AntCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
