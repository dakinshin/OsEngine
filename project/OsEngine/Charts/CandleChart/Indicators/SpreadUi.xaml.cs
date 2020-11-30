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

namespace OsEngine.Charts.CandleChart.Indicators
{
    /// <summary>
    /// Логика взаимодействия для SpreadUi.xaml
    /// </summary>
    public partial class SpreadUi : Window
    {
        public SpreadUi(Spread spread)
        {
            //InitializeComponent();
            Changed = false;
        }

        public bool Changed { get; set; }
    }
}
