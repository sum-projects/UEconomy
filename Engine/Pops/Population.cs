namespace UEconomy.Engine.Pops;

public class Population
{
    public int TotalAmount => LowerClass.Amount + MiddleClass.Amount + UpperClass.Amount;

    public PopulationSegment LowerClass { get; private set; }
    public PopulationSegment MiddleClass { get; private set; }
    public PopulationSegment UpperClass { get; private set; }

    public Dictionary<string, int> Employment { get; private set; } = new();

    public Population(int initialPopulation)
    {
        LowerClass = new PopulationSegment(SocialClass.LowerClass, initialPopulation, 100);
        MiddleClass = new PopulationSegment(SocialClass.LowerClass, 0, 500);
        UpperClass = new PopulationSegment(SocialClass.LowerClass, 0, 2000);


        Employment["unemployed"] = initialPopulation;
    }

    public PopulationSegment GetSegmentForNeed(string needType)
    {
        return needType switch
        {
            "glass" or "iron" or "coal" when UpperClass.Amount > 0 => UpperClass,
            "glass" or "iron" or "coal" when MiddleClass.Amount > 0 => MiddleClass,
            "tool" or "furniture" when MiddleClass.Amount > 0 => MiddleClass,
            _ => LowerClass
        };
    }

    public Dictionary<string, int> CalculateDailyConsumption()
    {
        var lowerClassNeeds = LowerClass.CalculateSegmentConsumption();
        var middleClassNeeds = MiddleClass.CalculateSegmentConsumption();
        var upperClassNeeds = UpperClass.CalculateSegmentConsumption();

        var totalNeeds = new Dictionary<string, int>();

        foreach (var item in lowerClassNeeds)
        {
            totalNeeds[item.Key] = item.Value;
        }

        foreach (var item in middleClassNeeds)
        {
            if (totalNeeds.ContainsKey(item.Key))
                totalNeeds[item.Key] += item.Value;
            else
                totalNeeds[item.Key] = item.Value;
        }

        foreach (var item in upperClassNeeds)
        {
            if (totalNeeds.ContainsKey(item.Key))
                totalNeeds[item.Key] += item.Value;
            else
                totalNeeds[item.Key] = item.Value;
        }

        return totalNeeds;
    }

    public int GetBuildingEmploymentPriority(string buildingId)
    {
        return buildingId switch
        {
            // Najwyższy priorytet dla produkcji żywności
            "WheatFarm" => 100,
            "RiceFarm" => 95,
            "MaizeFarm" => 90,

            // Wysoki priorytet dla wydobycia surowców podstawowych
            "LoggingCamp" => 85,
            "IronMine" => 80,
            "CoalMine" => 75,

            // Średni priorytet dla produkcji materiałów
            "TextileMill" => 70,
            "LumberMill" => 65,

            // Niższy priorytet dla produkcji dóbr konsumpcyjnych
            "FurnitureFactory" => 60,
            "Glassworks" => 55,
            "Workshop" => 50,

            // Podstawowe gospodarstwa subsystencyjne na końcu
            "subsistenceFarm" => 40,

            // Domyślny priorytet dla nieznanych typów budynków
            _ => 10
        };
    }

    public void AllocateWorkers(List<Building> buildings)
    {
        // Resetujemy zatrudnienie we wszystkich budynkach
        foreach (var building in buildings)
        {
            building.CurrentEmployees = 0;
        }

        // Sortujemy budynki według priorytetu zatrudnienia
        var sortedBuildings = buildings.OrderByDescending(b => GetBuildingEmploymentPriority(b.Id)).ToList();

        // Obliczamy całkowite zapotrzebowanie na pracowników
        var totalDemand = sortedBuildings.Sum(b => b.MaxEmployeesPerLevel * b.Level);

        // Dostępna siła robocza (niższa klasa + część średniej)
        var availableWorkers = LowerClass.Amount + (int)(MiddleClass.Amount * 0.7);

        // Współczynnik zatrudnienia (jeśli mamy mniej pracowników niż potrzeba)
        var employmentRatio = Math.Min(1.0, availableWorkers / (double)Math.Max(1, totalDemand));

        // Przydzielamy pracowników do budynków według priorytetu
        var remainingWorkers = availableWorkers;

        foreach (var building in sortedBuildings)
        {
            var desiredWorkers = building.MaxEmployeesPerLevel * building.Level;
            var allocatedWorkers = (int)(desiredWorkers * employmentRatio);

            // Nie przydzielamy więcej niż jest dostępne
            allocatedWorkers = Math.Min(allocatedWorkers, remainingWorkers);

            building.CurrentEmployees = allocatedWorkers;
            remainingWorkers -= allocatedWorkers;

            if (remainingWorkers <= 0) break;
        }

        // Aktualizujemy statystyki zatrudnienia
        Employment["unemployed"] = availableWorkers - (availableWorkers - remainingWorkers);

        // Aktualizujemy szczegółowe statystyki zatrudnienia według typu budynku
        foreach (var building in buildings)
        {
            var jobType = building.Id;
            if (!Employment.ContainsKey(jobType))
                Employment[jobType] = 0;

            Employment[jobType] += building.CurrentEmployees;
        }
    }

    public void UpdateSocialMobility()
    {
        const double promotionThreshold = 0.8;
        const double degradationThreshold = 0.3;

        // Obliczamy migrację między klasami społecznymi
        var lowerToMiddle = (int)(
            LowerClass.Amount *
            Math.Max(0, (LowerClass.LifeQualityIndex - promotionThreshold) / (1 - promotionThreshold)) * 0.01
        );

        var middleToUpper = (int)(
            MiddleClass.Amount *
            Math.Max(0, (MiddleClass.LifeQualityIndex - promotionThreshold) / (1 - promotionThreshold)) * 0.005
        );

        // Obliczenie degradacji ze średniej do niższej klasy
        var middleToLower = (int)(
            MiddleClass.Amount *
            Math.Max(0, (degradationThreshold - MiddleClass.LifeQualityIndex) / degradationThreshold) * 0.02
        );

        var upperToMiddle = (int)(
            UpperClass.Amount *
            Math.Max(0, (degradationThreshold - UpperClass.LifeQualityIndex) / degradationThreshold) * 0.01
        );

        // Aktualizujemy liczby populacji w segmentach
        LowerClass.AdjustAmount(middleToLower - lowerToMiddle);
        MiddleClass.AdjustAmount(lowerToMiddle + upperToMiddle - middleToLower - middleToUpper);
        UpperClass.AdjustAmount(middleToUpper - upperToMiddle);

        // Logujemy zmiany społeczne
        Console.WriteLine($"Social Mobility: Lower->Middle: {lowerToMiddle}, Middle->Upper: {middleToUpper}, " +
                          $"Upper->Middle: {upperToMiddle}, Middle->Lower: {middleToLower}");
    }
}