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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            this.imageList.ItemsSource = filePaths.Select(filePath => {
                var marks = RecognizeImiage(filePath);

                string text = "";
                marks.All(m => { text += m; return true; });

                return new RecognizedImage
                {
                    Text = text,
                    ImagePath = filePath,
                };
            });
        }



        private IEnumerable<string> RecognizeImiage(string filePath)
        {
            // TODO ここに、画像認識コードを入れてください。
            yield return filePath + "\n" + WashTagRecognize.Recognize(filePath);
        }
    }
}
