using System.Configuration;
using System.Data;
using System.Windows;
using GestionLibreriaCristal.Data;

namespace GestionLibreriaCristal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Crea la base de datos y las tablas si no existen
            using var db = new LibreriaDbContext();
            db.Database.EnsureCreated();
        }
    }

}
