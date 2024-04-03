// Шифрование и расшифрование по методу RSA


using System.Diagnostics;
using System.Numerics; //BigInteger 
using System.Text;
using System.Windows;

namespace InfoSecLab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            pTextBox.Text = "59785484612127431276145164544135759270791203174447";
            qTextBox.Text = "50200838841393216263704226343579201887653638867703";
            dTextBox.Text = "41742991834606126659659741728014380326573943066393";
            inputTextBox.Text = "555555555555555555555555555552346591364174923733333333333333333333333333333333333333182349236927777777777777777777777777777777777777777777777777774257623952322222222222222222220";
            
            Trace.WriteLine("App started");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            cryptedTextBox.Text = "";
            decryptedTextBox.Text = "";

            RSA rsa = new(this);
            rsa.SetKeys();
            rsa.Encrypt();
            rsa.Decrypt();
        }
    }
}