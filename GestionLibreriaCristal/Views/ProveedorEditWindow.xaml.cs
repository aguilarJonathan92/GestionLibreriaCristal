using GestionLibreriaCristal.ViewModels;
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

namespace GestionLibreriaCristal.Views
{
    public partial class ProveedorEditWindow : Window
    {
        public ProveedorEditViewModel ViewModel { get; }

        public ProveedorEditWindow(ProveedorEditViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;

            TituloVentana.Text = ViewModel.EsEdicion ? "Editar proveedor" : "Nuevo proveedor";
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