using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RectExtractDotnet
{
    public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = state;
        }

        public State state = new State();

        string AddSuffix(string filename, string suffix)
        {
            string fDir = System.IO.Path.GetDirectoryName(filename);
            string fName = System.IO.Path.GetFileNameWithoutExtension(filename);
            string fExt = System.IO.Path.GetExtension(filename);
            return System.IO.Path.Combine(fDir, String.Concat(fName, suffix, fExt));
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.png, *.jpeg, *.jpg)|*.jpeg;*.png;*.jpg"
            };

            if (dlg.ShowDialog() == true)
            {
                
                try
                {
                    var filename = dlg.FileName;
                    var image = await Task<Mat>.Factory.StartNew(() => new Mat(filename));
                    DrawCanvas.Image = image;
                    state.Filename = System.IO.Path.ChangeExtension(filename, "png");
                    state.Counter = 1;
                }
                catch (Exception)
                {
                    MessageBox.Show("Open image file failed.");
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Reset();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (var img = DrawCanvas.Crop())
            {
                if (img != null)
                {
                    var p = AddSuffix(state.Filename, "_" + state.Counter);
                    img.ImWrite(p);
                    state.Counter += 1;
                    DrawCanvas.Reset();
                }
            }

        }
    }
}
