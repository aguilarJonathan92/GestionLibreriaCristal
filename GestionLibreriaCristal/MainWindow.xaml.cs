using GestionLibreriaCristal.Views;
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

namespace GestionLibreriaCristal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Productos_Click(object sender, RoutedEventArgs e)
        {
            ContenidoPrincipal.Content = new ProductosView();
        }
        private void Categorias_Click(object sender, RoutedEventArgs e)
        {
            ContenidoPrincipal.Content = new CategoriasView();
        }

        private void Proveedores_Click(object sender, RoutedEventArgs e)
        {
            ContenidoPrincipal.Content = new ProveedoresView();
        }

        private void Clientes_Click(object sender, RoutedEventArgs e)
        {
            ContenidoPrincipal.Content = new ClientesView();
        }
    }
}