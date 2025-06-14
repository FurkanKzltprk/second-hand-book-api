using IkincielKitapApi.Data;
using IkıncielKitapApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

public static class SeedData
{

    #region Neden SeedData sınıfına ihtiyaç duydum FKT 6/3/25
    /*
    SeedData.cs dosyası, uygulamanın ilk çalıştırılması sırasında veritabanını
    otomatik olarak başlangıç verileriyle doldurmak için oluşturulmuştur.

    Bu dosyanın temel amacı:
    - Veritabanında henüz hiç kullanıcı (özellikle admin) yoksa, otomatik olarak
      bir admin kullanıcısı oluşturmak.
    - Böylece sistem ilk açıldığında yönetici olmadan çalışmaz ve admin
      kullanıcı manuel olarak eklenmek zorunda kalmaz.
    - Bu, güvenlik ve yönetim kolaylığı açısından kritik bir adımdır.

    Seed işlemi, veritabanı migrations tamamlandıktan sonra tetiklenir ve
    sadece kullanıcı tablosu boşsa çalışır, böylece tekrar tekrar admin eklenmez.

    Özetle, SeedData.cs:
    - Veritabanı başlangıç ayarlarını yapar,
    - İlk admin kullanıcısını oluşturur,
    - Uygulamanın sorunsuz ve güvenli bir şekilde başlamasını sağlar.
*/

    #endregion
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        // Veritabanı yoksa oluştur (opsiyonel)
        context.Database.Migrate();

        // Kullanıcı var mı kontrol et
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Role = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}
