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
        Game newgame;
        
        public StartUpWIndow()
        {
            InitializeComponent();
            
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            (newgame, bool isValid) = Gamestart.HandleNewGame(newgame, (int)GridWidthSlider.Value, (int)GridHeightSlider.Value, (int)AntCountSlider.Value, (int)FoodCountSlider.Value);

            if (isValid)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Too many ants and food for the grid size. Please adjust the values.");
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
