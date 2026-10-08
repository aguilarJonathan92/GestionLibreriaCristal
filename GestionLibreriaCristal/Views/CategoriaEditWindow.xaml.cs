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
    /// <summary>
    /// Lógica de interacción para CategoriaEditWindow.xaml
    /// </summary>
    public partial class CategoriaEditWindow : Window
    {
        public CategoriaEditViewModel ViewModel { get; }

        public CategoriaEditWindow(CategoriaEditViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;

            TituloVentana.Text = ViewModel.EsEdicion ? "Editar categoría" : "Nueva categoría";
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
