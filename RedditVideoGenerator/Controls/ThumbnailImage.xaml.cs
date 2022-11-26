using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Wpf.Ui.Controls;

namespace RedditVideoGenerator.Controls
{
    /// <summary>
    /// Interaction logic for ThumbnailImage.xaml
    /// </summary>
    public partial class ThumbnailImage : UserControl
    {
        public ThumbnailImage()
        {
            InitializeComponent();

            //force measure and arrange for quick access of actual width and height of titletext
            this.Measure(new Size(Width, Height));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));
        }

        public void SetAccentColor(Color color)
        {
            //set control accents
            SolidColorBrush AccentBrush = new SolidColorBrush(color);
            ThumbnailBorder.BorderBrush = AccentBrush;

        }

        public void SetVariableTitleFontSize()
        {
            UpdateLayout();

            //keep decreasing fontsize while titletext height > 800
            while (TitleText.ActualHeight > 800)
            {
                TitleText.FontSize -= 1;
                TitleText.UpdateLayout();
            }

            //keep increasing fontsize while titletext height < 540
            while (TitleText.ActualHeight < 540)
            {
                TitleText.FontSize += 1;
                TitleText.UpdateLayout();
            }
        }

        
    }
}
