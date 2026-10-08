using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GestionLibreriaCristal.ViewModels;

namespace GestionLibreriaCristal.Views
{
    public partial class ProductoEditWindow : Window
    {
        public ProductoEditViewModel ViewModel { get; }

        public ProductoEditWindow(ProductoEditViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;

            TituloVentana.Text = ViewModel.EsEdicion ? "Editar producto" : "Nuevo producto";
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Guardar())
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
