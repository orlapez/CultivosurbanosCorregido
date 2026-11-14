
using Microsoft.EntityFrameworkCore;
using Terpy.API.Helpers;
using Terpy.Shared.Entities;

namespace Terpy.API.Data;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;

    public SeedDb(DataContext context, IUserHelper userHelper)
    {
        _context = context;
        _userHelper = userHelper;
    }

    public async Task SeedAsync()
    {
        // Crear roles
        await CheckRolesAsync();

        // Crear usuario administrador
        var admin = await CheckUserAsync(
            "Administrador",
            "Terpy",
            "admin@terpy.local");

        // Crear huerta y zona de demostración
        await CheckHuertaAsync(admin);
    }

    // ========================================
    // 1. CREAR ROLES
    // ========================================

    private async Task CheckRolesAsync()
    {
        await _userHelper.CheckRoleAsync("Administrador");
        await _userHelper.CheckRoleAsync("Usuario");
    }

    // ========================================
    // 2. CREAR USUARIO ADMINISTRADOR
    // ========================================

    private async Task<User> CheckUserAsync(
        string nombre,
        string apellido,
        string email)
    {
        var user = await _userHelper.GetUserAsync(email);

        if (user == null)
        {
            user = new User
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var result = await _userHelper.AddUserAsync(
                user, "123456");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo crear el administrador: " +
                    string.Join(" ",
                        result.Errors.Select(x => x.Description)));
            }
        }

        // Verificar que tenga el rol administrador
        if (!await _userHelper.IsUserInRoleAsync(
            user, "Administrador"))
        {
            await _userHelper.AddUserToRoleAsync(
                user, "Administrador");
        }

        return user;
    }

    // ========================================
    // 3. CREAR HUERTA DE DEMOSTRACIÓN
    // ========================================

    private async Task CheckHuertaAsync(User admin)
    {
        var huerta = await _context.Huertas
            .FirstOrDefaultAsync(x =>
                x.ResponsableId == admin.Id &&
                x.Nombre == "Huerta de demostración");

        if (huerta == null)
        {
            huerta = new Huerta
            {
                Nombre = "Huerta de demostración",
                Ubicacion = "Espacio de práctica del Equipo Dinamita",
                ResponsableId = admin.Id
            };

            _context.Huertas.Add(huerta);
            await _context.SaveChangesAsync();
        }

        // Crear zona principal si no existe
        var existeZona = await _context.Zonas
            .AnyAsync(x =>
                x.HuertaId == huerta.Id &&
                x.Nombre == "Zona principal");

        if (!existeZona)
        {
            var zona = new Zona
            {
                HuertaId = huerta.Id,
                Nombre = "Zona principal",
                Descripcion = "Zona inicial para probar el sistema."
            };

            _context.Zonas.Add(zona);
            await _context.SaveChangesAsync();
        }
    }
}
