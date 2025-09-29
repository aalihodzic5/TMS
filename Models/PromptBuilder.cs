using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMS.Models; // da koristi tvoj Job model

namespace GeminiPromptGenerator
{
    public static class PromptBuilder
    {
        public static string BuildGeminiPrompt(IEnumerable<Job> jobs, string preferredCurrency = "EUR", int maxResults = 5)
        {
            var sb = new StringBuilder();

            // Upute za Gemini
            sb.AppendLine("Ti si stručnjak za logistiku i optimizaciju transporta. Analizirat ćeš listu poslova i predložiti najisplativije poslove.");
            sb.AppendLine();
            sb.AppendLine("Ulazne informacije za svaki posao (obavezno):");
            sb.AppendLine("- Id posla");
            sb.AppendLine("- Težina robe (kg)");
            sb.AppendLine("- Početna lokacija");
            sb.AppendLine("- Odredišna lokacija");
            sb.AppendLine("- Udaljenost (km)");
            sb.AppendLine("- Cijena posla (" + preferredCurrency + ")");
            sb.AppendLine("- Trajanje/Dužina posla (u satima ili danima)");
            sb.AppendLine("- Dodatne napomene");
            sb.AppendLine();
            sb.AppendLine("Zadatak:");
            sb.AppendLine($"1) Rangiraj sve poslove od najisplativijeg do najmanje isplativog i vrati top {maxResults}.");
            sb.AppendLine("2) Za svaki posao izračunaj i prikaži:");
            sb.AppendLine(" - Cijena po kilometru");
            sb.AppendLine(" - Cijena po kilogramu");
            sb.AppendLine(" - Cijena po satu/danu");
            sb.AppendLine(" - Profitabilni indeks (objasni formulu)");
            sb.AppendLine("3) Kratko obrazloženje (1-2 rečenice).");
            sb.AppendLine("4) Predloži optimalni posao.");
            sb.AppendLine("5) Označi neisplative poslove.");
            sb.AppendLine();
            sb.AppendLine("Format izlaza:");
            sb.AppendLine("- Tabela (CSV ili markdown).");
            sb.AppendLine("- Sažetak (4-6 rečenica).");
            sb.AppendLine();
            sb.AppendLine("Podaci poslova:");
            sb.AppendLine("ID,WeightKg,Origin,Destination,DistanceKm,Price,Currency,DurationHours,Notes");

            foreach (var j in jobs)
            {
                // Ako nema trajanje, možeš ga ostaviti prazno ili procijeniti
                string durationHours = "24"; // primjer: default 1 dan (24h), možeš promijeniti

                string originSafe = (j.locationOrigin ?? "").Replace(",", ";");
                string destSafe = (j.locationDestination ?? "").Replace(",", ";");
                string notesSafe = (j.comments ?? "").Replace(",", ";");

                sb.AppendLine(string.Join(",",
                    j.Id,
                    j.loadWeight.ToString("0.##", CultureInfo.InvariantCulture),
                    originSafe,
                    destSafe,
                    j.distanceDestination.ToString("0.##", CultureInfo.InvariantCulture),
                    j.price.ToString("0.##", CultureInfo.InvariantCulture),
                    preferredCurrency,
                    durationHours,
                    notesSafe));
            }

            sb.AppendLine();
            sb.AppendLine("Kraj prompta. Odgovori u tabeli + sažetku.");

            return sb.ToString();
        }
    }
}
