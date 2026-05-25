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
            string[] roles = { "Broker", "Dispatcher", "Administrator" };
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
                new("admin@tms.test", "Administrator", "Admin", "User", "+38761000000"),
                new("broker1@tms.test", "Broker", "Marko", "Petrovic", "+381601234567"),
                new("dispatcher1@tms.test", "Dispatcher", "Ana", "Kovacevic", "+381611234567"),
                new("dispatcher2@tms.test", "Dispatcher", "Maja", "Dragan", "+387612345678"),
                new("carrier1@tms.test", null, "Luka", "Jankovic", "+381621234567"),
                new("driver1@tms.test", null, "Petar", "Milosevic", "+381631234567"),
                new("driver2@tms.test", null, "Marko", "Djordjevic", "+387621234566"),
                new("driver3@tms.test", null, "Milena", "Savic", "+381621234568"),
                new("driver4@tms.test", null, "Stefan", "Radovic", "+387621234569"),
                new("driver5@tms.test", null, "Jasmina", "Ibric", "+381621234570"),
                new("driver6@tms.test", null, "Emir", "Kovacevic", "+387621234571"),
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

            var dispatcher1 = users.GetByEmail("dispatcher1@tms.test");
            var dispatcher2 = users.GetByEmail("dispatcher2@tms.test");

            var truckDefinitions = new[]
            {
                new
                {
                    LicensePlate = "SA-001-TR",
                    Brand = "Volvo",
                    Model = "FH16 750",
                    OwnerId = dispatcher1.Id,
                    Specification = "Long haul tractor for temperature-sensitive Balkan routes with full comfort package.",
                    Registration = new DateTime(2021, 3, 15),
                    NextServiceDate = DateTime.UtcNow.AddMonths(4)
                },
                new
                {
                    LicensePlate = "SK-002-TR",
                    Brand = "Scania",
                    Model = "R500",
                    OwnerId = dispatcher1.Id,
                    Specification = "High-roof sleeper cab for flatbed and mixed cargo on regional European deliveries.",
                    Registration = new DateTime(2022, 6, 5),
                    NextServiceDate = DateTime.UtcNow.AddMonths(5)
                },
                new
                {
                    LicensePlate = "BG-003-TR",
                    Brand = "Mercedes-Benz",
                    Model = "Actros 2545",
                    OwnerId = dispatcher1.Id,
                    Specification = "Efficient tractor for ADR and refrigerated loads across the Balkans and Central Europe.",
                    Registration = new DateTime(2020, 11, 20),
                    NextServiceDate = DateTime.UtcNow.AddMonths(6)
                },
                new
                {
                    LicensePlate = "TU-004-TR",
                    Brand = "MAN",
                    Model = "TGX 18.510",
                    OwnerId = dispatcher2.Id,
                    Specification = "Powerful tractor for lowboy and special cargo on Herzegovina and Dalmatian routes.",
                    Registration = new DateTime(2022, 1, 14),
                    NextServiceDate = DateTime.UtcNow.AddMonths(5)
                },
                new
                {
                    LicensePlate = "ZAG-005-TR",
                    Brand = "DAF",
                    Model = "XF 530",
                    OwnerId = dispatcher2.Id,
                    Specification = "Reliable container tractor for Adriatic imports and Central European lanes.",
                    Registration = new DateTime(2023, 1, 10),
                    NextServiceDate = DateTime.UtcNow.AddMonths(3)
                },
                new
                {
                    LicensePlate = "NS-006-TR",
                    Brand = "Renault",
                    Model = "T High 520",
                    OwnerId = dispatcher2.Id,
                    Specification = "Comfortable long-haul tractor for international fuel, retail and project transport.",
                    Registration = new DateTime(2022, 9, 2),
                    NextServiceDate = DateTime.UtcNow.AddMonths(4)
                }
            };

            var trucks = new List<Truck>();
            foreach (var truckDef in truckDefinitions)
            {
                var truck = await context.Truck.FirstOrDefaultAsync(t => t.licensePlate == truckDef.LicensePlate);
                if (truck == null)
                {
                    truck = new Truck
                    {
                        brand = truckDef.Brand,
                        model = truckDef.Model,
                        licensePlate = truckDef.LicensePlate,
                        specification = truckDef.Specification,
                        registration = truckDef.Registration,
                        nextServiceDate = truckDef.NextServiceDate,
                        UserID = truckDef.OwnerId
                    };
                    context.Truck.Add(truck);
                }
                else
                {
                    truck.UserID = truckDef.OwnerId;
                    truck.brand = truckDef.Brand;
                    truck.model = truckDef.Model;
                    truck.specification = truckDef.Specification;
                    truck.registration = truckDef.Registration;
                    truck.nextServiceDate = truckDef.NextServiceDate;
                }

                trucks.Add(truck);
            }

            await context.SaveChangesAsync();

            var trailerDefinitions = new[]
            {
                new
                {
                    LicensePlate = "TR-101-REF",
                    Brand = "Schmitz Cargobull",
                    Type = TrailerTypes.REEFER_TRAILER,
                    TruckLicense = "SA-001-TR",
                    Specification = "Reefer trailer for chilled cargo on Sarajevo to Zagreb and Central Europe runs.",
                    Registration = new DateTime(2022, 2, 1),
                    NextServiceDate = DateTime.UtcNow.AddMonths(5)
                },
                new
                {
                    LicensePlate = "TR-102-CT",
                    Brand = "Krone Profi Liner",
                    Type = TrailerTypes.CONTAINER_TRAILER,
                    TruckLicense = "SK-002-TR",
                    Specification = "Container trailer for fast port-to-inland transfers and secure palletised loads.",
                    Registration = new DateTime(2021, 9, 12),
                    NextServiceDate = DateTime.UtcNow.AddMonths(6)
                },
                new
                {
                    LicensePlate = "TR-103-FB",
                    Brand = "Kögel Cargo",
                    Type = TrailerTypes.FLATBED_TRAILER,
                    TruckLicense = "BG-003-TR",
                    Specification = "Flatbed trailer for steel sections, timber and construction materials.",
                    Registration = new DateTime(2023, 4, 22),
                    NextServiceDate = DateTime.UtcNow.AddMonths(4)
                },
                new
                {
                    LicensePlate = "TR-104-SD",
                    Brand = "Wielton Curtain Master",
                    Type = TrailerTypes.STEP_DECK_TRAILER,
                    TruckLicense = "TU-004-TR",
                    Specification = "Step deck trailer for oversized project cargo on Balkan and Adriatic routes.",
                    Registration = new DateTime(2022, 12, 10),
                    NextServiceDate = DateTime.UtcNow.AddMonths(8)
                },
                new
                {
                    LicensePlate = "TR-105-PL",
                    Brand = "Schwarzmüller Platform",
                    Type = TrailerTypes.FLATBED_TRAILER,
                    TruckLicense = "ZAG-005-TR",
                    Specification = "Platform trailer for cargo that requires easy loading and secured cross-border transit.",
                    Registration = new DateTime(2023, 5, 6),
                    NextServiceDate = DateTime.UtcNow.AddMonths(5)
                },
                new
                {
                    LicensePlate = "TR-106-HP",
                    Brand = "Krone Profi Liner",
                    Type = TrailerTypes.REEFER_TRAILER,
                    TruckLicense = "NS-006-TR",
                    Specification = "Reefer trailer for food and pharmaceutical loads into Austria and Germany.",
                    Registration = new DateTime(2022, 11, 18),
                    NextServiceDate = DateTime.UtcNow.AddMonths(6)
                }
            };

            foreach (var trailerDef in trailerDefinitions)
            {
                var trailer = await context.Trailer.FirstOrDefaultAsync(t => t.licensePlate == trailerDef.LicensePlate);
                var truck = trucks.First(t => t.licensePlate == trailerDef.TruckLicense);

                if (trailer == null)
                {
                    trailer = new Trailer
                    {
                        TruckId = truck.Id,
                        brand = trailerDef.Brand,
                        trailerType = trailerDef.Type,
                        licensePlate = trailerDef.LicensePlate,
                        registration = trailerDef.Registration,
                        nextServiceDate = trailerDef.NextServiceDate,
                        specification = trailerDef.Specification
                    };
                    context.Trailer.Add(trailer);
                }
                else
                {
                    trailer.TruckId = truck.Id;
                    trailer.brand = trailerDef.Brand;
                    trailer.trailerType = trailerDef.Type;
                    trailer.registration = trailerDef.Registration;
                    trailer.nextServiceDate = trailerDef.NextServiceDate;
                    trailer.specification = trailerDef.Specification;
                }
            }

            await context.SaveChangesAsync();

            var driverTrucks = trucks;
            var driverTemplates = new[]
            {
                (FirstName: "Petar", LastName: "Milosavljevic", Phone: "+381638765432", Dob: new DateTime(1987, 5, 16), Licences: new[] { Licence.ATP }, TruckIndex: 0, DispatcherId: dispatcher1.Id),
                (FirstName: "Marko", LastName: "Djordjevic", Phone: "+387621234566", Dob: new DateTime(1989, 2, 20), Licences: new[] { Licence.SPECIAL_TRANSPORT }, TruckIndex: 1, DispatcherId: dispatcher1.Id),
                (FirstName: "Milena", LastName: "Savic", Phone: "+381621234568", Dob: new DateTime(1990, 11, 3), Licences: new[] { Licence.ADR, Licence.TANKER }, TruckIndex: 2, DispatcherId: dispatcher1.Id),
                (FirstName: "Stefan", LastName: "Radovic", Phone: "+387621234569", Dob: new DateTime(1985, 7, 9), Licences: new[] { Licence.SPECIAL_TRANSPORT }, TruckIndex: 3, DispatcherId: dispatcher2.Id),
                (FirstName: "Jasmina", LastName: "Ibric", Phone: "+381621234570", Dob: new DateTime(1992, 4, 18), Licences: new[] { Licence.ATP }, TruckIndex: 4, DispatcherId: dispatcher2.Id),
                (FirstName: "Emir", LastName: "Kovacevic", Phone: "+387621234571", Dob: new DateTime(1988, 12, 27), Licences: new[] { Licence.ATP }, TruckIndex: 5, DispatcherId: dispatcher2.Id)
            };

            foreach (var driverInfo in driverTemplates)
            {
                if (driverInfo.TruckIndex >= driverTrucks.Count)
                    break;

                var existingDriver = await context.Driver.FirstOrDefaultAsync(d => d.FirstName == driverInfo.FirstName && d.LastName == driverInfo.LastName);
                if (existingDriver != null)
                {
                    existingDriver.UserID = driverInfo.DispatcherId;
                    existingDriver.TruckId = driverTrucks[driverInfo.TruckIndex].Id;
                    existingDriver.PhoneNumber = driverInfo.Phone;
                    existingDriver.DateOfBirth = driverInfo.Dob;
                    existingDriver.DriverStatus = DriverStatus.AVAILABLE;
                    existingDriver.DriverLicences = driverInfo.Licences.Select(l => new DriverLicence { Licence = l }).ToList();
                    continue;
                }

                context.Driver.Add(new Driver
                {
                    UserID = driverInfo.DispatcherId,
                    TruckId = driverTrucks[driverInfo.TruckIndex].Id,
                    FirstName = driverInfo.FirstName,
                    LastName = driverInfo.LastName,
                    PhoneNumber = driverInfo.Phone,
                    DateOfBirth = driverInfo.Dob,
                    DriverStatus = DriverStatus.AVAILABLE,
                    DriverLicences = driverInfo.Licences.Select(l => new DriverLicence { Licence = l }).ToList()
                });
            }

            await context.SaveChangesAsync();

            if (!await context.Job.AnyAsync())
            {
                var driver1 = await context.Driver.FirstAsync(d => d.FirstName == "Petar" && d.LastName == "Milosavljevic");
                var driver2 = await context.Driver.FirstAsync(d => d.FirstName == "Marko" && d.LastName == "Djordjevic");
                var driver3 = await context.Driver.FirstAsync(d => d.FirstName == "Milena" && d.LastName == "Savic");
                var driver4 = await context.Driver.FirstAsync(d => d.FirstName == "Stefan" && d.LastName == "Radovic");
                var driver5 = await context.Driver.FirstAsync(d => d.FirstName == "Jasmina" && d.LastName == "Ibric");
                var driver6 = await context.Driver.FirstAsync(d => d.FirstName == "Emir" && d.LastName == "Kovacevic");
                var today = DateTime.UtcNow.Date;

                var jobs = new List<Job>
                {
                    new()
                    {
                        UserId = dispatcher1.Id,
                        DriverId = driver1.Id,
                        loadDate = today.AddDays(7),
                        TrailerTypes = TrailerTypes.REEFER_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 90,
                        distanceDestination = 410,
                        locationOrigin = "Sarajevo, Bosnia and Herzegovina",
                        locationDestination = "Zagreb, Croatia",
                        companyName = "Sarajevo Fresh Logistics",
                        loadWeight = 22.0,
                        loadLength = 14.0,
                        price = 4200m,
                        comments = "Refrigerated fruit and vegetable shipment with daily temperature checks.",
                        postingDate = today.AddDays(-2)
                    },
                    new()
                    {
                        UserId = dispatcher1.Id,
                        DriverId = driver2.Id,
                        loadDate = today.AddDays(10),
                        TrailerTypes = TrailerTypes.FLATBED_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 180,
                        distanceDestination = 750,
                        locationOrigin = "Tuzla, Bosnia and Herzegovina",
                        locationDestination = "Ljubljana, Slovenia",
                        companyName = "Balkan Steel Carriers",
                        loadWeight = 24.5,
                        loadLength = 13.0,
                        price = 8500m,
                        comments = "Open flatbed transport for steel beams, with bridge height checks on Slovenian corridor.",
                        postingDate = today.AddDays(-1)
                    },
                    new()
                    {
                        UserId = dispatcher1.Id,
                        DriverId = driver4.Id,
                        loadDate = today.AddDays(12),
                        TrailerTypes = TrailerTypes.LOWBOY_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 85,
                        distanceDestination = 150,
                        locationOrigin = "Mostar, Bosnia and Herzegovina",
                        locationDestination = "Split, Croatia",
                        companyName = "Hercegovina Project Movers",
                        loadWeight = 40.0,
                        loadLength = 18.5,
                        price = 3200m,
                        comments = "Heavy construction machinery transfer for coastal infrastructure works.",
                        postingDate = today.AddDays(-3)
                    },
                    new()
                    {
                        UserId = dispatcher2.Id,
                        DriverId = driver3.Id,
                        loadDate = today.AddDays(8),
                        TrailerTypes = TrailerTypes.TANKER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 115,
                        distanceDestination = 1115,
                        locationOrigin = "Belgrade, Serbia",
                        locationDestination = "Munich, Germany",
                        companyName = "Danube Fuel Express",
                        loadWeight = 28.0,
                        loadLength = 13.5,
                        price = 11200m,
                        comments = "ADR-rated fuel run for a retail depot with German permit ready.",
                        postingDate = today.AddDays(-2)
                    },
                    new()
                    {
                        UserId = dispatcher2.Id,
                        DriverId = driver5.Id,
                        loadDate = today.AddDays(11),
                        TrailerTypes = TrailerTypes.CONTAINER_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 245,
                        distanceDestination = 930,
                        locationOrigin = "Sofia, Bulgaria",
                        locationDestination = "Vienna, Austria",
                        companyName = "Black Sea Container Lines",
                        loadWeight = 22.0,
                        loadLength = 13.7,
                        price = 9600m,
                        comments = "Container transport for retail equipment with secure loading and port transfer.",
                        postingDate = today.AddDays(-1)
                    },
                    new()
                    {
                        UserId = dispatcher2.Id,
                        DriverId = driver6.Id,
                        loadDate = today.AddDays(14),
                        TrailerTypes = TrailerTypes.STEP_DECK_TRAILER,
                        LoadType = LoadType.FULL,
                        distanceOrigin = 140,
                        distanceDestination = 560,
                        locationOrigin = "Zagreb, Croatia",
                        locationDestination = "Venice, Italy",
                        companyName = "Adriatic Heavy Transport",
                        loadWeight = 30.0,
                        loadLength = 16.0,
                        price = 6100m,
                        comments = "Oversized industrial equipment shipment with step deck trailer and Italian customs coordination.",
                        postingDate = today.AddDays(-4)
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

            public User GetByEmail(string email) => this[email];

            public new void Add(string email, User user)
            {
                base[email] = user;

                if (email.StartsWith("broker", StringComparison.OrdinalIgnoreCase))
                    base.TryAdd(nameof(Broker), user);
                else if (email.StartsWith("dispatcher", StringComparison.OrdinalIgnoreCase))
                    base.TryAdd(nameof(Dispatcher), user);
                else if (email.StartsWith("carrier", StringComparison.OrdinalIgnoreCase))
                    base.TryAdd(nameof(Carrier), user);
                else if (email.StartsWith("driver", StringComparison.OrdinalIgnoreCase))
                    base.TryAdd(nameof(Driver), user);
                else if (email.StartsWith("logistics", StringComparison.OrdinalIgnoreCase))
                    base.TryAdd(nameof(Logistics), user);
            }
        }
    }
}
