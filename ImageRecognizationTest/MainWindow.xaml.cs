using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ImageRecognizationTest
{

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDebug = false;
        private String tmpTitle;


        public MainWindow()
        {
            InitializeComponent();
        }



        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F12:
                    if (this.tmpTitle == null)
                    {
                        this.tmpTitle = this.Title;
                    }

                    // デバッグモード切替
                    this.isDebug = !this.isDebug;

                    this.Title = this.tmpTitle + (this.isDebug ? " (debug)" : "");
                    break;
                default:
                    break;
            }   
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
            yield return filePath + "\n" + WashTagRecognize.Recognize(filePath, this.isDebug);
        }

    }
}
