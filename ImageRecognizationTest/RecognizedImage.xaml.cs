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

namespace ImageRecognizationTest
{
    /// <summary>
    /// RecognizedImage.xaml の相互作用ロジック
    /// </summary>
    public partial class RecognizedImage : UserControl
    {
        public RecognizedImage()
        {
            InitializeComponent();
        }

        private string _uriSource = null;

        public string ImagePath
        {
            get { return _uriSource; }
            set
            {
                if (value == null) return;

                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(value);
                    image.EndInit();

                    _image.Source = image;
                    _uriSource = value;
                }
                catch (NotSupportedException)
                {
                }

            }
        }

        public string Text
        {
            get { return (string)_label.Content; }
            set { _label.Content = value; }
        }
    }
}
