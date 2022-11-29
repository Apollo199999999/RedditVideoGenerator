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
            List<string> words = TitleText.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();



            //calculate number of words to highlight
            int NumberOfStartWordsToHighlight = 5;

            if (words.Count <= 5)
            {
                NumberOfStartWordsToHighlight = words.Count - 1;
            }
            else
            {
                NumberOfStartWordsToHighlight = 5;
            }

            List<string> StartWordsToHighlight = words.Take(NumberOfStartWordsToHighlight).ToList();

            //split titletext by the startwordstohighlihght, keeping delimiters

            //insert delimiters to split the text at
            foreach (string word in StartWordsToHighlight)
            {
                if (StartWordsToHighlight.IndexOf(word) == 0)
                {
                    //if word is first word of titletext.text, adjust split delimiters accordingly
                    //add additional delimiter in front in case in titletext there is punctuation before the start of the word
                    TitleText.Text = TitleText.Text.ReplaceFirst(word + " ", word + "| ");
                }
                else
                {
                    TitleText.Text = TitleText.Text.ReplaceFirst(" " + word + " ", " |" + word + "| ");
                }
            }

            List<string> LastWordsToHighlight = new List<string>();

            //next, highlight the last 4 words if the title text has more than or equal to 12 words
            if (words.Count >= 12)
            {
                LastWordsToHighlight = Enumerable.Reverse(words).Take(4).Reverse().ToList();

                foreach (string word in LastWordsToHighlight)
                {
                    if (LastWordsToHighlight.IndexOf(word) == LastWordsToHighlight.Count() - 1)
                    {
                        //if word is last word of titletext.text, adjust split delimiters accordingly
                        //add additional delimiter at the back in case in titletext there is punctuation at the end
                        TitleText.Text = TitleText.Text.ReplaceLast(" " + word, " |" + word);
                    }
                    else
                    {
                        TitleText.Text = TitleText.Text.ReplaceLast(" " + word + " ", " |" + word + "| ");
                    }
                }
            }

            List<string> TitleTextSplitWords = TitleText.Text.Split(new char[] { '|' }, StringSplitOptions.None).ToList();

            //now set the respective colours of the words using run element
            TitleText.Text = String.Empty;
            TitleText.Inlines.Clear();
            foreach (string phrase in TitleTextSplitWords)
            {
                if (StartWordsToHighlight.Contains(phrase) || LastWordsToHighlight.Contains(phrase))
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
