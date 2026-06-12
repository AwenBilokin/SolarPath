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

        if (await db.Routes.AnyAsync())
        {
            // Базові маршрути вже існують — додаємо лише нові (якщо ще не додані)
            await SeedAdditionalRoutesAsync(services);
            return;
        }

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

        var r7 = new RouteModel { Title="Синевир — Перлина Карпат", Description="Найбільше та найкрасивіше озеро Українських Карпат, оточене смерековими лісами. Легкий прогулянковий маршрут до озера з оглядовими майданчиками та можливістю покататись на човні.", Difficulty=DifficultyLevel.Easy, DistanceKm=5.0, DurationMinutes=150, MaxParticipants=18, AvailableSlots=18, PricePerPerson=400, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,15), RouteStatus=RouteStatus.Published, CategoryId=5, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1518495973542-4542c06a5843?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-8) };
        var r8 = new RouteModel { Title="Хотинська фортеця — Сторожа Дністра", Description="Одна з найкраще збережених фортець України, місце зйомок десятків історичних фільмів. Прогулянка стінами, баштами та підземеллями з оглядом на каньйон Дністра.", Difficulty=DifficultyLevel.Easy, DistanceKm=4.0, DurationMinutes=150, MaxParticipants=22, AvailableSlots=22, PricePerPerson=350, SeasonStart=new DateTime(2025,3,1), SeasonEnd=new DateTime(2025,11,30), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1559521783-1d1599583485?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-7) };
        var r9 = new RouteModel { Title="Велотур Південним Бугом — Гранітні каньйони", Description="Маршрут вздовж порогів і гранітних скель Південного Бугу через Мигію та Гард. Краєвиди, що нагадують Скандинавію — ідеально для любителів активного відпочинку на природі.", Difficulty=DifficultyLevel.Medium, DistanceKm=42.0, DurationMinutes=300, MaxParticipants=12, AvailableSlots=12, PricePerPerson=750, SeasonStart=new DateTime(2025,4,15), SeasonEnd=new DateTime(2025,10,15), RouteStatus=RouteStatus.Published, CategoryId=2, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1486870591958-9b9d0d1dda99?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-6) };
        var r10 = new RouteModel { Title="Сплав по Стрию — Карпатська течія", Description="Захоплюючий рафтинг по гірській річці Стрий з порогами різної складності. Інструктаж, спорядження та незабутні емоції серед карпатських лісів включені.", Difficulty=DifficultyLevel.Hard, DistanceKm=16.0, DurationMinutes=240, MaxParticipants=8, AvailableSlots=8, PricePerPerson=950, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,9,15), RouteStatus=RouteStatus.Published, CategoryId=4, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1530866495561-507c9faab2ed?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-5) };
        var r11 = new RouteModel { Title="Тунель кохання — Клевань", Description="Знаменита залізнична гілка, оточена живоплотом з дерев, що утворює природний тунель. Романтична легка прогулянка, особливо красива влітку та восени.", Difficulty=DifficultyLevel.Easy, DistanceKm=3.0, DurationMinutes=90, MaxParticipants=20, AvailableSlots=20, PricePerPerson=280, SeasonStart=new DateTime(2025,4,1), SeasonEnd=new DateTime(2025,10,31), RouteStatus=RouteStatus.Published, CategoryId=1, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-4) };
        var r12 = new RouteModel { Title="Львів — Підземними легендами старого міста", Description="Пішохідна екскурсія старовинними вуличками, площами та внутрішніми двориками Львова з відвідуванням підземель та оглядом архітектури різних епох.", Difficulty=DifficultyLevel.Easy, DistanceKm=5.5, DurationMinutes=210, MaxParticipants=20, AvailableSlots=20, PricePerPerson=420, SeasonStart=new DateTime(2025,1,1), SeasonEnd=new DateTime(2025,12,31), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1599394020900-19c4843a8a93?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-3) };
        var r13 = new RouteModel { Title="Бескиди — Хребтова стежка Парашки", Description="Класичний гірський маршрут по гребеню Сколівських Бескидів з мальовничими видами на полонини та сусідні хребти. Чудовий варіант для новачків у горах.", Difficulty=DifficultyLevel.Medium, DistanceKm=14.0, DurationMinutes=360, MaxParticipants=14, AvailableSlots=14, PricePerPerson=600, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,20), RouteStatus=RouteStatus.Published, CategoryId=5, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1454496522488-7a8e488e8606?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-2) };
        var r14 = new RouteModel { Title="Велотур Буковиною — Дерев'яні церкви та полонини", Description="Маршрут через мальовничі села Буковини з відвідуванням дерев'яних церков ЮНЕСКО, сирних ферм та гірських полонин з традиційними стравами.", Difficulty=DifficultyLevel.Medium, DistanceKm=58.0, DurationMinutes=420, MaxParticipants=10, AvailableSlots=10, PricePerPerson=1100, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,1), RouteStatus=RouteStatus.Published, CategoryId=2, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1601122700611-369bc1a96b71?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-1) };
        var r15 = new RouteModel { Title="Каякінг по Десні — Тиха річкова подорож", Description="Спокійний сплав на каяках по чистих водах Десни серед заплавних лісів і пляжів. Підходить для початківців, включає зупинки для відпочинку та купання.", Difficulty=DifficultyLevel.Easy, DistanceKm=20.0, DurationMinutes=270, MaxParticipants=14, AvailableSlots=14, PricePerPerson=550, SeasonStart=new DateTime(2025,5,15), SeasonEnd=new DateTime(2025,9,15), RouteStatus=RouteStatus.Published, CategoryId=4, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=800&q=80", CreatedAt=DateTime.UtcNow };

        db.Routes.AddRange(r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15);
        await db.SaveChangesAsync();

        // Точки маршруту Говерла
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r1.Id, Title="Старт — с. Лазещина",    Latitude=48.2102, Longitude=24.3856, PointType=PointType.Start,      OrderIndex=0 },
            new RoutePoint { RouteId=r1.Id, Title="Кордон заповідника",      Latitude=48.1980, Longitude=24.4210, PointType=PointType.Checkpoint, OrderIndex=1 },
            new RoutePoint { RouteId=r1.Id, Title="Альпійські луки",         Latitude=48.1820, Longitude=24.4580, PointType=PointType.Highlight,  OrderIndex=2 },
            new RoutePoint { RouteId=r1.Id, Title="Вершина Говерла 2061м",   Latitude=48.1622, Longitude=24.5007, PointType=PointType.Finish,     OrderIndex=3 }
        );

        // Точки маршруту Дністер
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r3.Id, Title="Старт — Заліщики",       Latitude=48.6377, Longitude=25.7268, PointType=PointType.Start,      OrderIndex=0 },
            new RoutePoint { RouteId=r3.Id, Title="Скеля Дівич-гора",        Latitude=48.6290, Longitude=25.7580, PointType=PointType.Highlight,  OrderIndex=1 },
            new RoutePoint { RouteId=r3.Id, Title="Вигин Дністра",           Latitude=48.6180, Longitude=25.7820, PointType=PointType.Checkpoint, OrderIndex=2 },
            new RoutePoint { RouteId=r3.Id, Title="Фініш — Нирків",          Latitude=48.6050, Longitude=25.8150, PointType=PointType.Finish,     OrderIndex=3 }
        );

        // Точки для Софіївського парку (Умань)
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r2.Id, Title="Вхід до парку",    Latitude=48.7538, Longitude=30.2246, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r2.Id, Title="Острів Анти-Цирці",Latitude=48.7558, Longitude=30.2198, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r2.Id, Title="Великий каскад",   Latitude=48.7545, Longitude=30.2230, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Велотуру Закарпаттям
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r4.Id, Title="Старт — Берегово", Latitude=48.2042, Longitude=22.6411, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r4.Id, Title="Замок Мукачево",   Latitude=48.4412, Longitude=22.7163, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r4.Id, Title="Фініш — Ужгород",  Latitude=48.6239, Longitude=22.2983, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Шляху Довбуша
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r5.Id, Title="Старт — Яремче",  Latitude=48.4568, Longitude=24.5536, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r5.Id, Title="Скелі Довбуша",   Latitude=48.7220, Longitude=24.0890, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r5.Id, Title="Фініш — Бубнище", Latitude=48.7260, Longitude=24.0930, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Кам'янець-Подільського
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r6.Id, Title="Вхідні ворота",   Latitude=48.6714, Longitude=26.5631, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r6.Id, Title="Стара фортеця",   Latitude=48.6712, Longitude=26.5628, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r6.Id, Title="Старе місто",     Latitude=48.6730, Longitude=26.5590, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Синевиру
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r7.Id, Title="Старт — стоянка біля КПП", Latitude=48.6088, Longitude=23.6850, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r7.Id, Title="Оглядовий майданчик",      Latitude=48.6105, Longitude=23.6875, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r7.Id, Title="Озеро Синевир",            Latitude=48.6120, Longitude=23.6890, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Хотинської фортеці
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r8.Id, Title="Головна брама",   Latitude=48.5125, Longitude=26.4925, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r8.Id, Title="Дозорна вежа",    Latitude=48.5130, Longitude=26.4930, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r8.Id, Title="Оглядовий майданчик над Дністром", Latitude=48.5118, Longitude=26.4940, PointType=PointType.Finish, OrderIndex=2 }
        );
        // Точки для Південного Бугу
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r9.Id, Title="Старт — Мигія",     Latitude=47.7270, Longitude=31.0190, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r9.Id, Title="Гранітні пороги",   Latitude=47.7510, Longitude=31.0420, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r9.Id, Title="Фініш — урочище Гард", Latitude=47.7825, Longitude=31.0680, PointType=PointType.Finish, OrderIndex=2 }
        );
        // Точки для сплаву по Стрию
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r10.Id, Title="Старт — Верхнє Синьовидне", Latitude=49.0640, Longitude=23.4870, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r10.Id, Title="Найскладніший порід",       Latitude=49.0780, Longitude=23.4990, PointType=PointType.Checkpoint, OrderIndex=1 },
            new RoutePoint { RouteId=r10.Id, Title="Фініш — Нижнє Синьовидне",  Latitude=49.0920, Longitude=23.5110, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Тунелю кохання
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r11.Id, Title="Початок тунелю", Latitude=50.7295, Longitude=26.0125, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r11.Id, Title="Середина тунелю — найгустіша зелень", Latitude=50.7310, Longitude=26.0145, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r11.Id, Title="Кінець тунелю", Latitude=50.7325, Longitude=26.0165, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Львова
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r12.Id, Title="Площа Ринок",        Latitude=49.8419, Longitude=24.0315, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r12.Id, Title="Латинський собор",   Latitude=49.8425, Longitude=24.0330, PointType=PointType.Checkpoint, OrderIndex=1 },
            new RoutePoint { RouteId=r12.Id, Title="Підземелля бернардинського монастиря", Latitude=49.8400, Longitude=24.0345, PointType=PointType.Highlight, OrderIndex=2 },
            new RoutePoint { RouteId=r12.Id, Title="Високий Замок",      Latitude=49.8470, Longitude=24.0395, PointType=PointType.Finish,    OrderIndex=3 }
        );
        // Точки для Бескидів (Парашка)
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r13.Id, Title="Старт — Сколе",        Latitude=49.0530, Longitude=23.5780, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r13.Id, Title="Полонина",              Latitude=49.0680, Longitude=23.5920, PointType=PointType.Checkpoint, OrderIndex=1 },
            new RoutePoint { RouteId=r13.Id, Title="Вершина Парашка 1268м", Latitude=49.0750, Longitude=23.6010, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Буковини
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r14.Id, Title="Старт — Чернівці",         Latitude=48.2917, Longitude=25.9352, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r14.Id, Title="Дерев'яна церква ЮНЕСКО",  Latitude=48.1850, Longitude=25.7600, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r14.Id, Title="Фініш — полонина Руська",  Latitude=48.0950, Longitude=25.5400, PointType=PointType.Finish,    OrderIndex=2 }
        );
        // Точки для Десни
        db.RoutePoints.AddRange(
            new RoutePoint { RouteId=r15.Id, Title="Старт — Новгород-Сіверський", Latitude=52.0010, Longitude=33.2630, PointType=PointType.Start,     OrderIndex=0 },
            new RoutePoint { RouteId=r15.Id, Title="Піщаний пляж",                Latitude=51.9700, Longitude=33.2200, PointType=PointType.Highlight, OrderIndex=1 },
            new RoutePoint { RouteId=r15.Id, Title="Фініш — с. Леньків",          Latitude=51.9400, Longitude=33.1700, PointType=PointType.Finish,    OrderIndex=2 }
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
            new Review { RouteId=r6.Id, TouristId=tourist.Id, Rating=4, Text="Фортеця вражає масштабом. Підземні ходи — окремий атракціон.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddDays(-1) },
            new Review { RouteId=r7.Id, TouristId=tourist.Id, Rating=5, Text="Озеро неймовірне, вода кристально чиста. Дуже легка прогулянка, підійде для всієї сім'ї.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-20) },
            new Review { RouteId=r11.Id, TouristId=tourist.Id, Rating=5, Text="Дуже романтичне місце! Радимо приїжджати на світанку, поки немає натовпу.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-10) },
            new Review { RouteId=r12.Id, TouristId=tourist.Id, Rating=5, Text="Гід знає неймовірну кількість історій про кожен будинок. Львів закохав у себе ще більше.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-5) }
        );

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Додає 9 нових маршрутів до вже існуючої бази (ідемпотентно — перевіряє за Title).
    /// Викликається, якщо в базі вже є маршрути (тобто базовий seed вже виконано раніше).
    /// </summary>
    private static async Task SeedAdditionalRoutesAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var um = services.GetRequiredService<UserManager<ApplicationUser>>();

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

        var newRoutes = new (string Title, RouteModel Route, RoutePoint[] Points, Review? Review)[]
        {
            ("Синевир — Перлина Карпат",
             new RouteModel { Title="Синевир — Перлина Карпат", Description="Найбільше та найкрасивіше озеро Українських Карпат, оточене смерековими лісами. Легкий прогулянковий маршрут до озера з оглядовими майданчиками та можливістю покататись на човні.", Difficulty=DifficultyLevel.Easy, DistanceKm=5.0, DurationMinutes=150, MaxParticipants=18, AvailableSlots=18, PricePerPerson=400, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,15), RouteStatus=RouteStatus.Published, CategoryId=5, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1518495973542-4542c06a5843?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-8) },
             new[] {
                new RoutePoint { Title="Старт — стоянка біля КПП", Latitude=48.6088, Longitude=23.6850, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Оглядовий майданчик",      Latitude=48.6105, Longitude=23.6875, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Озеро Синевир",            Latitude=48.6120, Longitude=23.6890, PointType=PointType.Finish,    OrderIndex=2 }
             },
             new Review { Rating=5, Text="Озеро неймовірне, вода кристально чиста. Дуже легка прогулянка, підійде для всієї сім'ї.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-20) }),

            ("Хотинська фортеця — Сторожа Дністра",
             new RouteModel { Title="Хотинська фортеця — Сторожа Дністра", Description="Одна з найкраще збережених фортець України, місце зйомок десятків історичних фільмів. Прогулянка стінами, баштами та підземеллями з оглядом на каньйон Дністра.", Difficulty=DifficultyLevel.Easy, DistanceKm=4.0, DurationMinutes=150, MaxParticipants=22, AvailableSlots=22, PricePerPerson=350, SeasonStart=new DateTime(2025,3,1), SeasonEnd=new DateTime(2025,11,30), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1559521783-1d1599583485?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-7) },
             new[] {
                new RoutePoint { Title="Головна брама",   Latitude=48.5125, Longitude=26.4925, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Дозорна вежа",    Latitude=48.5130, Longitude=26.4930, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Оглядовий майданчик над Дністром", Latitude=48.5118, Longitude=26.4940, PointType=PointType.Finish, OrderIndex=2 }
             },
             null),

            ("Велотур Південним Бугом — Гранітні каньйони",
             new RouteModel { Title="Велотур Південним Бугом — Гранітні каньйони", Description="Маршрут вздовж порогів і гранітних скель Південного Бугу через Мигію та Гард. Краєвиди, що нагадують Скандинавію — ідеально для любителів активного відпочинку на природі.", Difficulty=DifficultyLevel.Medium, DistanceKm=42.0, DurationMinutes=300, MaxParticipants=12, AvailableSlots=12, PricePerPerson=750, SeasonStart=new DateTime(2025,4,15), SeasonEnd=new DateTime(2025,10,15), RouteStatus=RouteStatus.Published, CategoryId=2, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1486870591958-9b9d0d1dda99?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-6) },
             new[] {
                new RoutePoint { Title="Старт — Мигія",     Latitude=47.7270, Longitude=31.0190, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Гранітні пороги",   Latitude=47.7510, Longitude=31.0420, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Фініш — урочище Гард", Latitude=47.7825, Longitude=31.0680, PointType=PointType.Finish, OrderIndex=2 }
             },
             null),

            ("Сплав по Стрию — Карпатська течія",
             new RouteModel { Title="Сплав по Стрию — Карпатська течія", Description="Захоплюючий рафтинг по гірській річці Стрий з порогами різної складності. Інструктаж, спорядження та незабутні емоції серед карпатських лісів включені.", Difficulty=DifficultyLevel.Hard, DistanceKm=16.0, DurationMinutes=240, MaxParticipants=8, AvailableSlots=8, PricePerPerson=950, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,9,15), RouteStatus=RouteStatus.Published, CategoryId=4, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1530866495561-507c9faab2ed?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-5) },
             new[] {
                new RoutePoint { Title="Старт — Верхнє Синьовидне", Latitude=49.0640, Longitude=23.4870, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Найскладніший порід",       Latitude=49.0780, Longitude=23.4990, PointType=PointType.Checkpoint, OrderIndex=1 },
                new RoutePoint { Title="Фініш — Нижнє Синьовидне",  Latitude=49.0920, Longitude=23.5110, PointType=PointType.Finish,    OrderIndex=2 }
             },
             null),

            ("Тунель кохання — Клевань",
             new RouteModel { Title="Тунель кохання — Клевань", Description="Знаменита залізнична гілка, оточена живоплотом з дерев, що утворює природний тунель. Романтична легка прогулянка, особливо красива влітку та восени.", Difficulty=DifficultyLevel.Easy, DistanceKm=3.0, DurationMinutes=90, MaxParticipants=20, AvailableSlots=20, PricePerPerson=280, SeasonStart=new DateTime(2025,4,1), SeasonEnd=new DateTime(2025,10,31), RouteStatus=RouteStatus.Published, CategoryId=1, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-4) },
             new[] {
                new RoutePoint { Title="Початок тунелю", Latitude=50.7295, Longitude=26.0125, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Середина тунелю — найгустіша зелень", Latitude=50.7310, Longitude=26.0145, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Кінець тунелю", Latitude=50.7325, Longitude=26.0165, PointType=PointType.Finish,    OrderIndex=2 }
             },
             new Review { Rating=5, Text="Дуже романтичне місце! Радимо приїжджати на світанку, поки немає натовпу.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-10) }),

            ("Львів — Підземними легендами старого міста",
             new RouteModel { Title="Львів — Підземними легендами старого міста", Description="Пішохідна екскурсія старовинними вуличками, площами та внутрішніми двориками Львова з відвідуванням підземель та оглядом архітектури різних епох.", Difficulty=DifficultyLevel.Easy, DistanceKm=5.5, DurationMinutes=210, MaxParticipants=20, AvailableSlots=20, PricePerPerson=420, SeasonStart=new DateTime(2025,1,1), SeasonEnd=new DateTime(2025,12,31), RouteStatus=RouteStatus.Published, CategoryId=3, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1599394020900-19c4843a8a93?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-3) },
             new[] {
                new RoutePoint { Title="Площа Ринок",        Latitude=49.8419, Longitude=24.0315, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Латинський собор",   Latitude=49.8425, Longitude=24.0330, PointType=PointType.Checkpoint, OrderIndex=1 },
                new RoutePoint { Title="Підземелля бернардинського монастиря", Latitude=49.8400, Longitude=24.0345, PointType=PointType.Highlight, OrderIndex=2 },
                new RoutePoint { Title="Високий Замок",      Latitude=49.8470, Longitude=24.0395, PointType=PointType.Finish,    OrderIndex=3 }
             },
             new Review { Rating=5, Text="Гід знає неймовірну кількість історій про кожен будинок. Львів закохав у себе ще більше.", IsVerified=true, CreatedAt=DateTime.UtcNow.AddHours(-5) }),

            ("Бескиди — Хребтова стежка Парашки",
             new RouteModel { Title="Бескиди — Хребтова стежка Парашки", Description="Класичний гірський маршрут по гребеню Сколівських Бескидів з мальовничими видами на полонини та сусідні хребти. Чудовий варіант для новачків у горах.", Difficulty=DifficultyLevel.Medium, DistanceKm=14.0, DurationMinutes=360, MaxParticipants=14, AvailableSlots=14, PricePerPerson=600, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,20), RouteStatus=RouteStatus.Published, CategoryId=5, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1454496522488-7a8e488e8606?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-2) },
             new[] {
                new RoutePoint { Title="Старт — Сколе",        Latitude=49.0530, Longitude=23.5780, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Полонина",              Latitude=49.0680, Longitude=23.5920, PointType=PointType.Checkpoint, OrderIndex=1 },
                new RoutePoint { Title="Вершина Парашка 1268м", Latitude=49.0750, Longitude=23.6010, PointType=PointType.Finish,    OrderIndex=2 }
             },
             null),

            ("Велотур Буковиною — Дерев'яні церкви та полонини",
             new RouteModel { Title="Велотур Буковиною — Дерев'яні церкви та полонини", Description="Маршрут через мальовничі села Буковини з відвідуванням дерев'яних церков ЮНЕСКО, сирних ферм та гірських полонин з традиційними стравами.", Difficulty=DifficultyLevel.Medium, DistanceKm=58.0, DurationMinutes=420, MaxParticipants=10, AvailableSlots=10, PricePerPerson=1100, SeasonStart=new DateTime(2025,5,1), SeasonEnd=new DateTime(2025,10,1), RouteStatus=RouteStatus.Published, CategoryId=2, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1601122700611-369bc1a96b71?w=800&q=80", CreatedAt=DateTime.UtcNow.AddDays(-1) },
             new[] {
                new RoutePoint { Title="Старт — Чернівці",         Latitude=48.2917, Longitude=25.9352, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Дерев'яна церква ЮНЕСКО",  Latitude=48.1850, Longitude=25.7600, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Фініш — полонина Руська",  Latitude=48.0950, Longitude=25.5400, PointType=PointType.Finish,    OrderIndex=2 }
             },
             null),

            ("Каякінг по Десні — Тиха річкова подорож",
             new RouteModel { Title="Каякінг по Десні — Тиха річкова подорож", Description="Спокійний сплав на каяках по чистих водах Десни серед заплавних лісів і пляжів. Підходить для початківців, включає зупинки для відпочинку та купання.", Difficulty=DifficultyLevel.Easy, DistanceKm=20.0, DurationMinutes=270, MaxParticipants=14, AvailableSlots=14, PricePerPerson=550, SeasonStart=new DateTime(2025,5,15), SeasonEnd=new DateTime(2025,9,15), RouteStatus=RouteStatus.Published, CategoryId=4, GuideId=guide.Id, ImageUrl="https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=800&q=80", CreatedAt=DateTime.UtcNow },
             new[] {
                new RoutePoint { Title="Старт — Новгород-Сіверський", Latitude=52.0010, Longitude=33.2630, PointType=PointType.Start,     OrderIndex=0 },
                new RoutePoint { Title="Піщаний пляж",                Latitude=51.9700, Longitude=33.2200, PointType=PointType.Highlight, OrderIndex=1 },
                new RoutePoint { Title="Фініш — с. Леньків",          Latitude=51.9400, Longitude=33.1700, PointType=PointType.Finish,    OrderIndex=2 }
             },
             null),
        };

        var tourist = await um.FindByEmailAsync("tourist@solarpath.ua");

        bool addedAny = false;
        foreach (var (title, route, points, review) in newRoutes)
        {
            if (await db.Routes.AnyAsync(r => r.Title == title)) continue;

            db.Routes.Add(route);
            await db.SaveChangesAsync(); // потрібен Id для точок/відгуків

            foreach (var p in points)
            {
                p.RouteId = route.Id;
                db.RoutePoints.Add(p);
            }

            if (review != null && tourist != null)
            {
                review.RouteId = route.Id;
                review.TouristId = tourist.Id;
                db.Reviews.Add(review);
            }

            addedAny = true;
        }

        if (addedAny) await db.SaveChangesAsync();
    }
}
