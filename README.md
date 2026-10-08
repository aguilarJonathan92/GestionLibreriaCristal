# 📚 Gestión Librería Cristal

Sistema de gestión de escritorio para una librería cristiana. Desarrollado en **.NET 10** con **WPF**, aplicando el patrón **MVVM** y **Entity Framework Core** con **SQLite** como base de datos local.

> **Estado del proyecto:** En desarrollo activo 🚧  
> Proyecto real, desarrollado para un cliente con necesidades concretas de gestión comercial.

---

## 🎯 Sobre el proyecto

Este sistema nace para resolver las necesidades diarias de una librería cristiana que vende biblias, himnarios, libros, mantillas, libretas, agendas y regalos. Reemplaza el uso de planillas de Excel por una aplicación de escritorio con base de datos, interfaz moderna y funcionalidades pensadas para el día a día del negocio.

El objetivo principal es que la dueña del negocio pueda gestionar su inventario, sus clientes, sus proveedores y sus ventas de forma simple, rápida y sin depender de conocimientos técnicos.

---

## ✨ Funcionalidades

### ✅ Implementadas

- **Gestión de Productos**: alta, edición, eliminación y búsqueda en tiempo real.
  - Nombre, descripción, categoría, proveedor.
  - Costo de compra, precio de venta.
  - Stock actual, stock mínimo.
  - Margen unitario y porcentual calculados automáticamente.
  - Estado del stock (Sin stock, Reponer, Bajo, Óptimo).

- **Gestión de Categorías**: categorías configurables con color personalizado para identificación visual.
  - Validación: no se puede eliminar una categoría con productos asociados.

- **Gestión de Proveedores**: alta, edición, eliminación y búsqueda.
  - Nombre, teléfono, email, dirección y notas.
  - Advertencia al eliminar proveedores con productos asociados.

- **Gestión de Clientes**: alta, edición, eliminación y búsqueda.
  - Nombre, teléfono, email, dirección, DNI/CUIT, notas.
  - Saldo de cuenta corriente (positivo = debe, negativo = a favor).
  - Filtro "solo con deuda".
  - Estado de cuenta automático (Al día, Debe, A favor).
  - Validación: no se puede eliminar un cliente con deuda pendiente.

- **Interfaz moderna**: diseño con sidebar de navegación, paleta de colores personalizada y estilos consistentes en toda la aplicación.

- **Base de datos local**: SQLite con Entity Framework Core, almacenada en `%AppData%` para preservar los datos entre actualizaciones.

### 🚧 En desarrollo

- **Módulo de Pedidos/Ventas**: registro de ventas con múltiples items, descuentos, formas de pago (efectivo, transferencia, tarjeta, cuenta corriente) y descuento automático de stock.
- **Módulo de Caja**: registro de ingresos y egresos, balance diario y mensual.
- **Módulo de Cuentas Corrientes**: gestión detallada de deudas por cliente.
- **Dashboard con gráficos**: ventas del mes, productos más vendidos, stock bajo, deudas pendientes.
- **Combos de productos**: agrupación de productos en kits/packs.
- **Reportes en PDF**: facturas, recibos y estados de cuenta.

---

## 🛠️ Stack tecnológico

| Tecnología | Uso |
|:---|:---|
| **.NET 10** | Framework base |
| **WPF** | Interfaz de escritorio |
| **C# 13** | Lenguaje de programación |
| **MVVM** | Patrón de arquitectura |
| **CommunityToolkit.Mvvm** | Implementación de MVVM (ObservableProperty, RelayCommand) |
| **Entity Framework Core 10** | ORM para acceso a datos |
| **SQLite** | Base de datos local |
| **XAML** | Definición de interfaces |

---

## 🏗️ Arquitectura

El proyecto sigue el patrón **MVVM (Model-View-ViewModel)** con una separación clara de responsabilidades:

```
GestionLibreriaCristal/
│
├── Models/              # Entidades del dominio
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Proveedor.cs
│   ├── Cliente.cs
│   ├── Pedido.cs
│   └── DetallePedido.cs
│
├── Data/                # Acceso a datos
│   └── LibreriaDbContext.cs
│
├── Services/            # Lógica de negocio (en desarrollo)
│
├── ViewModels/          # Lógica de presentación
│   ├── MainViewModel.cs
│   ├── ProductosViewModel.cs
│   ├── ProductoEditViewModel.cs
│   ├── CategoriasViewModel.cs
│   ├── CategoriaEditViewModel.cs
│   ├── ProveedoresViewModel.cs
│   ├── ProveedorEditViewModel.cs
│   ├── ClientesViewModel.cs
│   └── ClienteEditViewModel.cs
│
├── Views/               # Interfaz de usuario
│   ├── MainWindow.xaml
│   ├── ProductosView.xaml
│   ├── ProductoEditWindow.xaml
│   ├── CategoriasView.xaml
│   ├── CategoriaEditWindow.xaml
│   ├── ProveedoresView.xaml
│   ├── ProveedorEditWindow.xaml
│   ├── ClientesView.xaml
│   └── ClienteEditWindow.xaml
│
├── Resources/           # Estilos y recursos
│
└── Assets/              # Imágenes y logos
```

**Principios aplicados:**

- **Separación de responsabilidades**: la lógica de negocio no está en la UI.
- **Data Binding bidireccional**: comunicación fluida entre View y ViewModel.
- **Comandos en lugar de eventos**: los botones invocan comandos del ViewModel, sin código en el code-behind.
- **Colecciones observables**: la UI se actualiza automáticamente al modificar datos.
- **Validaciones en el ViewModel**: no en la UI ni en la base de datos.
- **Reglas de negocio centralizadas**: por ejemplo, no se puede eliminar una categoría con productos asociados.

---

## 🚀 Cómo ejecutar el proyecto

### Requisitos previos

- **Visual Studio 2022 o superior** con la carga de trabajo **".NET desktop development"**.
- **.NET 10 SDK** instalado.
- **Windows 10/11**.

### Pasos

1. Cloná el repositorio:
   ```bash
   git clone https://github.com/tu-usuario/GestionLibreriaCristal.git
   ```

2. Abrí la solución `GestionLibreriaCristal.sln` en Visual Studio.

3. Restaurá los paquetes NuGet (Visual Studio lo hace automáticamente).

4. Compilá y ejecutá con **F5**.

La base de datos se crea automáticamente la primera vez que se ejecuta la aplicación, en:
```
%AppData%\GestionLibreriaCristal\libreria.db
```

---

## 📸 Capturas de pantalla

> *(Próximamente: capturas de la pantalla de Productos, Categorías, Proveedores y Clientes)*

---

## 🗺️ Roadmap

- [x] Módulo de Productos
- [x] Módulo de Categorías
- [x] Módulo de Proveedores
- [x] Módulo de Clientes
- [ ] Módulo de Pedidos/Ventas
- [ ] Módulo de Caja
- [ ] Módulo de Cuentas Corrientes
- [ ] Dashboard con gráficos
- [ ] Combos de productos
- [ ] Reportes en PDF
- [ ] Empaquetado e instalador

---

## 👨‍💻 Sobre el desarrollador

Proyecto desarrollado como parte de mi portafolio profesional, aplicando buenas prácticas de desarrollo de software, patrones de diseño y tecnologías modernas del ecosistema .NET.

**Objetivo:** demostrar habilidades en desarrollo de aplicaciones de escritorio, arquitectura MVVM, acceso a datos con Entity Framework Core y diseño de interfaces con WPF.

---

## 📝 Licencia

Este proyecto es de uso privado para el cliente. El código se comparte con fines de demostración profesional.
