using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Models;
using RouteModel = SolarPath.Web.Models.Route;

namespace SolarPath.Web.Data;

public static class SeedData
{
    public static async Task SeedRoutesAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var um = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Routes.AnyAsync()) return;

        // Гід-демо
        const string guideEmail = "guide@solarpath.ua";
        var guide = await um.FindByEmailAsync(guideEmail);
        if (guide == null)
        {
            guide = new ApplicationUser
            {
                FirstName = "Олексій", LastName = "Петренко",
                Email = guideEmail, UserName = guideEmail
            };
            await um.CreateAsync(guide, "Guide123!");
            await um.AddToRoleAsync(guide, "Guide");
        }

        var r1 = new RouteModel { Title="Говерла — Корона Карпат", Description="Найвища вершина України (2061 м) — культовий маршрут для кожного, хто хоче підкорити Карпати. Шлях проходить через букові ліси, альпійські луки та кам'янисті схили. З вершини відкривається панорама на три держави в ясну погоду.", Difficulty=DifficultyLevel.Hard, DistanceKm=18.5, DurationMinutes=480, MaxParticipants=12, AvailableSlots=12, PricePerPerson=850, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,15), RouteStatus=RouteStatus.Published, CategoryId=5, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-30) };
        var r2 = new RouteModel { Title="Софіївський парк — Садова казка Умані", Description="Один з найкрасивіших ландшафтних парків Європи, закладений на початку XIX ст. Тінисті алеї, острови на озерах, гроти, каскади та античні павільйони. Ідеально для сімейних прогулянок та романтичних побачень.", Difficulty=DifficultyLevel.Easy, DistanceKm=6.0, DurationMinutes=180, MaxParticipants=20, AvailableSlots=20, PricePerPerson=320, SeasonStart=new DateTime(2025,4,1), SeasonEnd=new DateTime(2025,11,1), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1585320806297-9794b3e4eeae?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-25) };
        var r3 = new RouteModel { Title="Каньйон Дністер — Річковий лабіринт", Description="Вражаючий каньйон Дністра з крейдяними скелями та прозорою водою. Сплав на байдарках між скелями заввишки до 100 м — незабутнє відчуття дикої природи у серці Поділля.", Difficulty=DifficultyLevel.Medium, DistanceKm=24.0, DurationMinutes=360, MaxParticipants=8, AvailableSlots=8, PricePerPerson=1200, SeasonStart=new DateTime(2025,5,15), SeasonEnd=new DateTime(2025,9,30), RouteStatus=RouteStatus.Published, CategoryId=4, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1504280390367-361c6d9f38f4?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-20) };
        var r4 = new RouteModel { Title="Велотур Закарпаттям — Виноградні долини", Description="Три дні серед виноградників, замків і угорських традицій. Маршрут проходить через Берегово, Мукачево та Ужгород — міста з багатовіковою історією.", Difficulty=DifficultyLevel.Easy, DistanceKm=95.0, DurationMinutes=1440, MaxParticipants=15, AvailableSlots=15, PricePerPerson=2400, SeasonStart=new DateTime(2025,4,15), SeasonEnd=new DateTime(2025,10,31), RouteStatus=RouteStatus.Published, CategoryId=2, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1541625602330-2277a4c46182?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-18) };
        var r5 = new RouteModel { Title="Шлях Довбуша — Скелі легенд", Description="Маршрут печерами і скелями опришка Олекси Довбуша в Яремче та Бубнищі. Гігантські кам'яні брили з природними гротами — атмосфера первісної природи і народних легенд.", Difficulty=DifficultyLevel.Medium, DistanceKm=12.0, DurationMinutes=300, MaxParticipants=10, AvailableSlots=10, PricePerPerson=650, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,31), RouteStatus=RouteStatus.Published, CategoryId=1, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-15) };
        var r6 = new RouteModel { Title="Кам'янець-Подільський — Місто над прірвою", Description="Середньовічна фортеця на скелястому острові серед каньйону Смотрич — одне з семи чудес України. Екскурсія по підземних ходах, баштах та бастіонах фортеці.", Difficulty=DifficultyLevel.Easy, DistanceKm=8.0, DurationMinutes=240, MaxParticipants=25, AvailableSlots=25, PricePerPerson=480, SeasonStart=new DateTime(2025,3,1), SeasonEnd=new DateTime(2025,11,30), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1520209759809-a9bcb6cb3241?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-10) };

        db.Routes.AddRange(r1, r2, r3, r4, r5, r6);
        await db.SaveChangesAsync();

        // Точки маршруту Говерла
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r1.Id, Title="Старт — с. Лазещина",    Latitude=48.1580, Longitude=24.4970, PointType=PointType.Start,      OrderIndex=0 },
            new RoutePoint { RouteId=r1.Id, Title="Альпійські луки",         Latitude=48.1750, Longitude=24.5080, PointType=PointType.Highlight,  OrderIndex=1 },
            new RoutePoint { RouteId=r1.Id, Title="Вершина Говерла 2061м",   Latitude=48.1622, Longitude=24.5007, PointType=PointType.Finish,     OrderIndex=2 }
        );

        // Точки маршруту Дністер
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r3.Id, Title="Старт — Заліщики",       Latitude=48.6380, Longitude=25.7310, PointType=PointType.Start,      OrderIndex=0 },
            new RoutePoint { RouteId=r3.Id, Title="Скеля Дівич-гора",        Latitude=48.6450, Longitude=25.7600, PointType=PointType.Highlight,  OrderIndex=1 },
            new RoutePoint { RouteId=r3.Id, Title="Фініш — Городок",         Latitude=48.6520, Longitude=25.7900, PointType=PointType.Finish,     OrderIndex=2 }
        );

        await db.SaveChangesAsync();

        // Турист-демо
        const string touristEmail = "tourist@solarpath.ua";
        var tourist = await um.FindByEmailAsync(touristEmail);
        if (tourist == null)
        {
            tourist = new ApplicationUser { FirstName="Марія", LastName="Коваленко", Email=touristEmail, UserName=touristEmail };
            await um.CreateAsync(tourist, "Tourist123!");
            await um.AddToRoleAsync(tourist, "Tourist");
        }

        db.Reviews.AddRange(
            new Review { RouteId=r1.Id, TouristId=tourist.Id, Rating=5, Text="Неймовірні враження! Гід Олексій — професіонал. Вид з вершини — незабутній!", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-5) },
            new Review { RouteId=r1.Id, TouristId=tourist.Id, Rating=4, Text="Важкий підйом, але того варто. Рекомендую готуватись фізично.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-3) },
            new Review { RouteId=r2.Id, TouristId=tourist.Id, Rating=5, Text="Казковий парк! Були з дітьми, всі в захваті.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-7) },
            new Review { RouteId=r4.Id, TouristId=tourist.Id, Rating=5, Text="Кращий велотур у моєму житті. Вино, замки і гори!", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-2) },
            new Review { RouteId=r6.Id, TouristId=tourist.Id, Rating=4, Text="Фортеця вражає масштабом. Підземні ходи — окремий атракціон.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-1) }
        );

        await db.SaveChangesAsync();
    }
}
