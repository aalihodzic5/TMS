using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TMS.Models;
using TMS.Models.Enums;

namespace TMS.Data
{
    public static class DatabaseSeeder
    {
        private const string SeedPassword = "Test123!";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();
            await EnsureRolesAsync(roleManager);
            var users = await EnsureUsersAsync(userManager);
            await EnsureDomainDataAsync(context, users);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Broker", "Dispatcher" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<SeededUsers> EnsureUsersAsync(UserManager<User> userManager)
        {
            var seeded = new List<SeedUserDefinition>
            {
                new("broker1@tms.test", "Broker", "Marko", "Petrovic", "+381601234567"),
                new("dispatcher1@tms.test", "Dispatcher", "Ana", "Kovacevic", "+381611234567"),
                new("carrier1@tms.test", null, "Luka", "Jankovic", "+381621234567"),
                new("driver1@tms.test", null, "Petar", "Milosevic", "+381631234567"),
                new("logistics1@tms.test", null, "Ivana", "Jovanovic", "+381641234567")
            };

            var result = new SeededUsers();

            foreach (var seed in seeded)
            {
                var user = await userManager.FindByEmailAsync(seed.Email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = seed.Email,
                        Email = seed.Email,
                        EmailConfirmed = true,
                        Ime = seed.FirstName,
                        Prezime = seed.LastName,
                        PhoneNumber = seed.PhoneNumber
                    };

                    var createResult = await userManager.CreateAsync(user, SeedPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create seed user {seed.Email}: {errors}");
                    }
                }

                if (!string.IsNullOrEmpty(seed.Role) && !await userManager.IsInRoleAsync(user, seed.Role))
                {
                    await userManager.AddToRoleAsync(user, seed.Role);
                }

                result.Add(seed.Email, user);
            }

            return result;
        }

        private static async Task EnsureDomainDataAsync(ApplicationDbContext context, SeededUsers users)
        {
            var faker = new Faker("en");

            if (!await context.Truck.AnyAsync())
            {
                var trucks = new List<Truck>
                {
                    new()
                    {
                        brand = "Volvo",
                        model = "FH16 750",
                        licensePlate = "BG-123-TR",
                        specification = "Euro 6 long haul tractor, climate package and 78 t towing capacity.",
                        registration = new DateTime(2021, 3, 15),
                        nextServiceDate = DateTime.UtcNow.AddMonths(4),
                        UserID = users.Driver.Id
                    },
                    new()
                    {
                        brand = "Scania",
                        model = "S520",
                        licensePlate = "SK-456-DL",
                        specification = "High roof sleeper cab for international food transport.",
                        registration = new DateTime(2022, 6, 5),
                        nextServiceDate = DateTime.UtcNow.AddMonths(5),
                        UserID = users.Logistics.Id
                    },
                    new()
                    {
                        brand = "Mercedes-Benz",
                        model = "Actros 2545",
                        licensePlate = "NS-789-CM",
                        specification = "Efficient long-distance tractor for refrigerated and dry cargo.",
                        registration = new DateTime(2020, 11, 20),
                        nextServiceDate = DateTime.UtcNow.AddMonths(6),
                        UserID = users.Carrier.Id
                    },
                    new()
                    {
                        brand = "DAF",
                        model = "XF 530",
                        licensePlate = "ZAG-234-TL",
                        specification = "Heavy duty tractor used for oversized loads and construction equipment.",
                        registration = new DateTime(2023, 1, 10),
                        nextServiceDate = DateTime.UtcNow.AddMonths(3),
                        UserID = users.Broker.Id
                    }
                };

                context.Truck.AddRange(trucks);
                await context.SaveChangesAsync();

                var trailers = new List<Trailer>
                {
                    new()
                    {
                        TruckId = trucks[0].Id,
                        brand = "Krone",
                        trailerType = TrailerTypes.REEFER_TRAILER,
                        licensePlate = "TR-001-REF",
                        registration = new DateTime(2022, 2, 1),
                        nextServiceDate = DateTime.UtcNow.AddMonths(5),
                        specification = "Reefer trailer for chilled food, pharma and grocery distribution."
                    },
                    new()
                    {
                        TruckId = trucks[1].Id,
                        brand = "Schmitz",
                        trailerType = TrailerTypes.FLATBED_TRAILER,
                        licensePlate = "TR-002-FB",
                        registration = new DateTime(2021, 9, 12),
                        nextServiceDate = DateTime.UtcNow.AddMonths(6),
                        specification = "Flatbed trailer for steel pipes, construction materials, and project cargo."
                    },
                    new()
                    {
                        TruckId = trucks[2].Id,
                        brand = "Nooteboom",
                        trailerType = TrailerTypes.TANKER,
                        licensePlate = "TR-003-TK",
                        registration = new DateTime(2023, 4, 22),
                        nextServiceDate = DateTime.UtcNow.AddMonths(4),
                        specification = "Fuel tanker trailer certified for petrol, diesel and chemical deliveries."
                    },
                    new()
                    {
                        TruckId = trucks[3].Id,
                        brand = "Broshuis",
                        trailerType = TrailerTypes.LOWBOY_TRAILER,
                        licensePlate = "TR-004-LB",
                        registration = new DateTime(2022, 12, 10),
                        nextServiceDate = DateTime.UtcNow.AddMonths(8),
                        specification = "Lowboy trailer for heavy machinery, mining equipment and oversized cargo."
                    }
                };

                context.Trailer.AddRange(trailers);
                await context.SaveChangesAsync();
            }

            if (!await context.Driver.AnyAsync())
            {
                var truck = await context.Truck.FirstAsync();
                var driver = new Driver
                {
                    UserID = users.Driver.Id,
                    TruckId = truck.Id,
                    FirstName = "Petar",
                    LastName = "Milosavljevic",
                    PhoneNumber = "+381638765432",
                    DateOfBirth = new DateTime(1987, 5, 16),
                    DriverStatus = DriverStatus.AVAILABLE,
                    DriverLicences = new List<DriverLicence>
                    {
                        new() { Licence = Licence.ATP },
                        new() { Licence = Licence.TANKER }
                    }
                };

                context.Driver.Add(driver);
                await context.SaveChangesAsync();
            }

            if (!await context.Job.AnyAsync())
            {
                var driver = await context.Driver.FirstAsync();
                var today = DateTime.UtcNow.Date;
                var jobs = new List<Job>
                {
                    new()
                    {
                        UserId = users.Broker.Id,
                        DriverId = driver.Id,
                        loadDate = today.AddDays(7),
                        TrailerTypes = TrailerTypes.REEFER_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 120,
                        distanceDestination = 980,
                        locationOrigin = "Belgrade, Serbia",
                        locationDestination = "Munich, Germany",
                        companyName = "Adriatic Fresh Logistics",
                        loadWeight = 24.5,
                        loadLength = 14.6,
                        price = 6600m,
                        comments = faker.Lorem.Sentence(10),
                        postingDate = today.AddDays(-2)
                    },
                    new()
                    {
                        UserId = users.Dispatcher.Id,
                        DriverId = driver.Id,
                        loadDate = today.AddDays(10),
                        TrailerTypes = TrailerTypes.FLATBED_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 80,
                        distanceDestination = 1420,
                        locationOrigin = "Zagreb, Croatia",
                        locationDestination = "Lyon, France",
                        companyName = "Danube Heavy Cargo",
                        loadWeight = 18.0,
                        loadLength = 12.0,
                        price = 11400m,
                        comments = "Over-dimensional load with escort vehicle required for French highway transit.",
                        postingDate = today.AddDays(-1)
                    },
                    new()
                    {
                        UserId = users.Broker.Id,
                        DriverId = null,
                        loadDate = today.AddDays(14),
                        TrailerTypes = TrailerTypes.TANKER,
                        LoadType = LoadType.PARTIAL,
                        distanceOrigin = 290,
                        distanceDestination = 1400,
                        locationOrigin = "Sofia, Bulgaria",
                        locationDestination = "Vienna, Austria",
                        companyName = "Balkan Fuel Transport",
                        loadWeight = 28.0,
                        loadLength = 14.0,
                        price = 9800m,
                        comments = "Chemical tanker cargo with ADR certification required.",
                        postingDate = today.AddDays(-3)
                    },
                    new()
                    {
                        UserId = users.Dispatcher.Id,
                        DriverId = null,
                        loadDate = today.AddDays(18),
                        TrailerTypes = TrailerTypes.LOWBOY_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 640,
                        distanceDestination = 1220,
                        locationOrigin = "Pristina, Kosovo",
                        locationDestination = "Munich, Germany",
                        companyName = "Euro Project Movers",
                        loadWeight = 46.0,
                        loadLength = 20.0,
                        price = 18750m,
                        comments = "Heavy machinery transport with permit paperwork and scheduled border crossings.",
                        postingDate = today.AddDays(-4)
                    },
                    new()
                    {
                        UserId = users.Broker.Id,
                        DriverId = driver.Id,
                        loadDate = today.AddDays(21),
                        TrailerTypes = TrailerTypes.REEFER_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 320,
                        distanceDestination = 725,
                        locationOrigin = "Tirane, Albania",
                        locationDestination = "Athens, Greece",
                        companyName = "Mediterranean Cargo Partners",
                        loadWeight = 16.0,
                        loadLength = 13.5,
                        price = 7400m,
                        comments = "Perishable goods bound for Greek retail warehouses with night delivery.",
                        postingDate = today.AddDays(-5)
                    }
                };

                context.Job.AddRange(jobs);
                await context.SaveChangesAsync();
            }

            if (!await context.Offer.AnyAsync())
            {
                var jobs = await context.Job.Take(4).ToListAsync();
                var offers = new List<Offer>
                {
                    new()
                    {
                        JobId = jobs[0].Id,
                        UserId = users.Carrier.Id,
                        report = "Competitive offer with secure refrigerated transport and real-time route tracking.",
                        offerDate = DateTime.UtcNow.AddDays(-1),
                        price = 6300m,
                        offerState = OfferState.PENDING
                    },
                    new()
                    {
                        JobId = jobs[1].Id,
                        UserId = users.Driver.Id,
                        report = "Experienced crew on ready tractor-trailer for France delivery with full escort.",
                        offerDate = DateTime.UtcNow.AddDays(-2),
                        price = 11000m,
                        offerState = OfferState.ACCEPTED
                    },
                    new()
                    {
                        JobId = jobs[2].Id,
                        UserId = users.Carrier.Id,
                        report = "ADR-certified tanker solution for hazardous fuel with transit permit support.",
                        offerDate = DateTime.UtcNow.AddDays(-1),
                        price = 10050m,
                        offerState = OfferState.REJECTED
                    },
                    new()
                    {
                        JobId = jobs[3].Id,
                        UserId = users.Carrier.Id,
                        report = "Lowboy transport available for oversized construction equipment on Balkan-EU corridor.",
                        offerDate = DateTime.UtcNow,
                        price = 18500m,
                        offerState = OfferState.PENDING
                    }
                };

                context.Offer.AddRange(offers);
                await context.SaveChangesAsync();
            }

            if (!await context.Payment.AnyAsync())
            {
                var acceptedOffer = await context.Offer.FirstOrDefaultAsync(o => o.offerState == OfferState.ACCEPTED);
                if (acceptedOffer != null)
                {
                    context.Payment.Add(new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        OfferId = acceptedOffer.Id,
                        UserId = users.Broker.Id,
                        paymentStatus = PaymentStatus.COMPLETED,
                        paymentDate = DateTime.UtcNow.AddDays(-1)
                    });
                }

                context.Payment.Add(new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    OfferId = null,
                    UserId = users.Dispatcher.Id,
                    paymentStatus = PaymentStatus.PENDING,
                    paymentDate = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            if (!await context.Subscription.AnyAsync())
            {
                var payment = await context.Payment.FirstOrDefaultAsync(p => p.UserId == users.Dispatcher.Id && p.paymentStatus == PaymentStatus.PENDING);
                if (payment != null)
                {
                    context.Subscription.Add(new Subscription
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = users.Dispatcher.Id,
                        PaymentId = payment.Id,
                        date = DateTime.UtcNow.Date,
                        expirationDate = DateTime.UtcNow.Date.AddMonths(6),
                        subscriptionStatus = SubscriptionStatus.SUBSCRIBED,
                        subPrice = 2500m
                    });

                    await context.SaveChangesAsync();
                }
            }

            if (!await context.SavedJob.AnyAsync())
            {
                var job = await context.Job.FirstAsync();
                context.SavedJob.Add(new SavedJob
                {
                    UserId = users.Dispatcher.Id,
                    JobId = job.Id,
                    savedDate = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            if (!await context.Notification.AnyAsync())
            {
                var notifications = new List<Notification>
                {
                    new()
                    {
                        UserId = users.Broker.Id,
                        Message = "New offer received for your refrigerated load.",
                        NotificationDate = DateTime.UtcNow.AddDays(1),
                        status = NotificationStatus.UNREAD,
                        Link = "/Offer/Index"
                    },
                    new()
                    {
                        UserId = users.Dispatcher.Id,
                        Message = "Subscription payment is pending approval.",
                        NotificationDate = DateTime.UtcNow.AddDays(2),
                        status = NotificationStatus.UNREAD,
                        Link = "/Subscription/Details"
                    },
                    new()
                    {
                        UserId = users.Carrier.Id,
                        Message = "Driver licence verification completed.",
                        NotificationDate = DateTime.UtcNow.AddDays(1),
                        status = NotificationStatus.READ,
                        Link = "/Driver/Index"
                    }
                };

                context.Notification.AddRange(notifications);
                await context.SaveChangesAsync();
            }

            if (!await context.OfferUsers.AnyAsync())
            {
                var acceptedOffer = await context.Offer.FirstOrDefaultAsync(o => o.offerState == OfferState.ACCEPTED);
                var pendingOffer = await context.Offer.FirstOrDefaultAsync(o => o.offerState == OfferState.PENDING);

                if (acceptedOffer != null)
                {
                    context.OfferUsers.Add(new OfferUser
                    {
                        OfferId = acceptedOffer.Id,
                        UserId = users.Dispatcher.Id
                    });
                }

                if (pendingOffer != null)
                {
                    context.OfferUsers.Add(new OfferUser
                    {
                        OfferId = pendingOffer.Id,
                        UserId = users.Broker.Id
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private record SeedUserDefinition(string Email, string? Role, string FirstName, string LastName, string PhoneNumber);

        private class SeededUsers : Dictionary<string, User>
        {
            public User Broker => this[nameof(Broker)];
            public User Dispatcher => this[nameof(Dispatcher)];
            public User Carrier => this[nameof(Carrier)];
            public User Driver => this[nameof(Driver)];
            public User Logistics => this[nameof(Logistics)];

            public new void Add(string email, User user)
            {
                if (email.StartsWith("broker", StringComparison.OrdinalIgnoreCase))
                    base[nameof(Broker)] = user;
                else if (email.StartsWith("dispatcher", StringComparison.OrdinalIgnoreCase))
                    base[nameof(Dispatcher)] = user;
                else if (email.StartsWith("carrier", StringComparison.OrdinalIgnoreCase))
                    base[nameof(Carrier)] = user;
                else if (email.StartsWith("driver", StringComparison.OrdinalIgnoreCase))
                    base[nameof(Driver)] = user;
                else if (email.StartsWith("logistics", StringComparison.OrdinalIgnoreCase))
                    base[nameof(Logistics)] = user;
                else
                    base[email] = user;
            }
        }
    }
}
