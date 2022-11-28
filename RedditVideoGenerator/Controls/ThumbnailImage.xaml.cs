using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Net.Mime.MediaTypeNames;

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

            //highlight the first 4 words from titletext
            var punctuation = TitleText.Text.Where(Char.IsPunctuation).Distinct().ToArray();
            List<string> words = TitleText.Text.Split().Select(x => x.Trim(punctuation)).ToList();

            //calculate number of words to highlight
            int NumberOfWordsToHighlight = 5;

            if (words.Count <= 5)
            {
                NumberOfWordsToHighlight = words.Count - 1;
            }
            else
            {
                NumberOfWordsToHighlight = 5;
            }

            List<string> WordsToHighlight = words.Take(NumberOfWordsToHighlight).ToList();

            //split titletext by the random words, keeping delimiters
            foreach (string word in WordsToHighlight)
            {
                if (WordsToHighlight.IndexOf(word) == 0)
                {
                    //if word is first word of titletext.text, adjust split delimiters accordingly
                    Regex regex = new Regex(word + " ");
                    TitleText.Text = regex.Replace(TitleText.Text, word + "| ", 1);
                    
                }
                else
                {
                    Regex regex = new Regex(" " + word + " ");
                    TitleText.Text = regex.Replace(TitleText.Text, " |" + word + "| ", 1);
                }
            }

            List<string> TitleTextSplitWords = TitleText.Text.Split(new char[] { '|' }, StringSplitOptions.None).ToList();

            //now set the respective colours of the words using run element
            TitleText.Text = String.Empty;
            TitleText.Inlines.Clear();
            foreach (string phrase in TitleTextSplitWords)
            {
                if (WordsToHighlight.Contains(phrase))
                {
                    TitleText.Inlines.Add(new Run(phrase) { Foreground = AccentBrush });
                }
                else
                {
                    TitleText.Inlines.Add(new Run(phrase) { Foreground = new SolidColorBrush(Color.FromRgb(215, 218, 220)) });
                }
            }

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

            //keep decreasing fontsize while titletext height > 800 (in case previous while loop increased font size too much)
            while (TitleText.ActualHeight > 800)
            {
                TitleText.FontSize -= 1;
                TitleText.UpdateLayout();
            }
        }

        
    }
}
