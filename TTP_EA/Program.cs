using TTP_EA.Data;
using TTP_EA.EA;
using TTP_EA.EA.Crossovers;
using TTP_EA.EA.Mutators;
using TTP_EA.EA.Selectors;
using TTP_EA.Logger;
using TTP_EA.Reader;
using TTP_EA.SA;
using TTP_EA.Specimen;
using TTP_EA.Specimen.Creators;
using TTP_EA.TS;
using TTP_EA.TS.Neighbors;


void GenerateRandomSpecimens(string fileName)
{
    string basePath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Dane/";
    var path = Path.Combine(basePath, fileName);
    uint count = 10000;
    double maxScore = double.MinValue;
    double minScore = double.MaxValue;

    var data = TTP_Reader.Load(path);

    ISpecimenCreator creator = new RandomCreator(data, 0.3);
    TTP_Specimen specimen = new TTP_Specimen(data, creator);
    List<double> results = new List<double>();

    for (int i = 0; i < count; i++)
    {
        specimen.VisitedCities.Clear();
        specimen.FitnessScore = null;
        specimen.Init();
        var score = specimen.Score();
        results.Add(score);

        if(score > maxScore)
        {
            maxScore = score;
        }
        if(score < minScore)
        {
            minScore = score;
        }
    }

    double standardDeviation = 0d;
    double avgScore = 0d;

    if (results.Any())
    {
        avgScore = results.Average();
        double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
        standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));
    }

    Console.WriteLine($"\nFile: {fileName} RANDOM");
    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
}

void GenerateGreedySpecimens(string fileName)
{
    string basePath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Dane/";
    var path = Path.Combine(basePath, fileName);

    var data = TTP_Reader.Load(path);

    ISpecimenCreator creator = new RandomCreator(data, 0.3);
    TTP_Specimen specimen = new TTP_Specimen(data, creator);
    List<double> results = GetAllGreedySpecimens(specimen, data);

    double maxScore = results.Max();
    double minScore = results.Min();

    double standardDeviation = 0d;
    double avgScore = 0d;

    if (results.Any())
    {
        avgScore = results.Average();
        double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
        standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));
    }

    Console.WriteLine($"\nFile: {fileName} GREEDY");
    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
}

List<double> GetAllGreedySpecimens(TTP_Specimen specimen, TTP_Data data)
{
    var distances = data.GetDistanceMatrix();
    List<double> resulsts = new List<double>();

    foreach (var city in data.Cities)
    {
        specimen.VisitedCities.Clear();
        specimen.FitnessScore = null;
        HashSet<City> currentCities = data.Cities.ToHashSet();
        var currentCity = city;
        currentCities.Remove(currentCity);
        specimen.VisitedCities.Add(currentCity);

        while (currentCities.Count > 0)
        {
            var paths = distances[currentCity.Index - 1];
            double maxDistance = double.MaxValue;
            var selectedPath = paths[0];

            foreach (var path in paths)
            {
                if (path.From != path.To && currentCities.Contains(path.To) && maxDistance > path.Distance)
                {
                    selectedPath = path;
                    maxDistance = path.Distance;
                }
            }
            currentCity = selectedPath.To;
            currentCities.Remove(currentCity);
            specimen.VisitedCities.Add(currentCity);
        }
        GreedyCreator.FillGreedyKnapsack(specimen);
        var score = specimen.Score();
        resulsts.Add(score);
        Console.WriteLine($"Starting City: {city.Index}, score: {score}");
    }
    return resulsts;

    //var distances = data.GetDistances();
    //List<double> resulsts = new List<double>();

    //foreach (var city in data.Cities)
    //{
    //    specimen.VisitedCities.Clear();
    //    specimen.FitnessScore = null;
    //    List<City> currentCities = data.Cities.ToList();
    //    var currentCity = city;
    //    currentCities.Remove(currentCity);
    //    specimen.VisitedCities.Add(currentCity);

    //    while (currentCities.Count > 0)
    //    {
    //        var distancesFrom = distances.Where(x => x.Key.from == currentCity && currentCities.Contains(x.Key.to)
    //            && x.Key.from != x.Key.to);
    //        var selected = distances.Where(x => x.Key.from == currentCity
    //            && currentCities.Contains(x.Key.to)
    //            && x.Key.from != x.Key.to
    //            ).MinBy(x => x.Value);
    //        currentCity = selected.Key.to;
    //        currentCities.Remove(currentCity);
    //        specimen.VisitedCities.Add(currentCity);
    //    }
    //    GreedyCreator.FillGreedyKnapsack(specimen);
    //    var score = specimen.Score();
    //    resulsts.Add(score);
    //    Console.WriteLine($"Starting City: {city.Index}, score: {score}");
    //}
    //return resulsts;
}

string filename = "medium_0.ttp";


//GenerateRandomSpecimens(filename);
//GenerateGreedySpecimens(filename);
//GetTenSAResults(filename);

//ProgramTS.GetTenTSResultsDividedAsynchronously(filename);
//ProgramSA.GetTenSAResultsDividedAsynchronously(filename);
ProgramEA.GetTenEAResultsDividedAsynchronously(filename);
