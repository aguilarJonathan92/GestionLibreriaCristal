using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using GestionLibreriaCristal.Views;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ClientesViewModel : ObservableObject
    {
        private List<Cliente> _todosLosClientes = new();

        public ObservableCollection<Cliente> ClientesFiltrados { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Cliente? clienteSeleccionado;

        [ObservableProperty]
        private bool mostrarSoloConDeuda = false;

        public ClientesViewModel()
        {
            CargarClientes();
        }

        partial void OnTextoBusquedaChanged(string value)
        {
            AplicarFiltro();
        }

        partial void OnMostrarSoloConDeudaChanged(bool value)
        {
            AplicarFiltro();
        }

        public void CargarClientes()
        {
            using var db = new LibreriaDbContext();
            _todosLosClientes = db.Clientes
                .OrderBy(c => c.Nombre)
                .ToList();

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            IEnumerable<Cliente> resultado = _todosLosClientes;

            if (!string.IsNullOrEmpty(filtro))
            {
                resultado = resultado.Where(c =>
                    c.Nombre.ToLower().Contains(filtro) ||
                    (c.Telefono != null && c.Telefono.ToLower().Contains(filtro)) ||
                    (c.DniCuit != null && c.DniCuit.ToLower().Contains(filtro)));
            }

            if (MostrarSoloConDeuda)
            {
                resultado = resultado.Where(c => c.Saldo > 0);
            }

            ClientesFiltrados.Clear();
            foreach (var c in resultado)
                ClientesFiltrados.Add(c);
        }

        [RelayCommand]
        private void Agregar()
        {
            var vm = new ClienteEditViewModel();
            var ventana = new ClienteEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarClientes();
        }

        [RelayCommand]
        private void Editar()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un cliente para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new ClienteEditViewModel(ClienteSeleccionado);
            var ventana = new ClienteEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarClientes();
        }

        [RelayCommand]
        private void Eliminar()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un cliente para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ClienteSeleccionado.Saldo != 0)
            {
                MessageBox.Show(
                    "No se puede eliminar un cliente con saldo pendiente.\n\n" +
                    "Primero regularizá su cuenta.",
                    "No se puede eliminar",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Seguro que querés eliminar a \"{ClienteSeleccionado.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            using var db = new LibreriaDbContext();
            var cliDb = db.Clientes.FirstOrDefault(c => c.Id == ClienteSeleccionado.Id);
            if (cliDb != null)
            {
                db.Clientes.Remove(cliDb);
                db.SaveChanges();
            }

            CargarClientes();
        }
    }
}