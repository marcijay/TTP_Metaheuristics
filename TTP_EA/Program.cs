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
using TTP_EA.Specimen.Factories;
using TTP_EA.TS;
using TTP_EA.TS.Neighbors;


void GenerateRandomSpecimens(string fileName)
{
    string basePath = "./../../../Data";
    var path = Path.Combine(basePath, fileName);
    uint count = 50000;
    double maxScore = double.MinValue;
    double minScore = double.MaxValue;

    var data = TTP_Reader.Load(path);

    ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
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
    string basePath = "./../../../Data";
    var path = Path.Combine(basePath, fileName);

    var data = TTP_Reader.Load(path);

    ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
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
        GreedyCreator<TTP_Specimen>.FillGreedyKnapsack(specimen);
        //GreedyCreator.FillGreedyKnapsackEasy(specimen);
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

string filename = "easy_0.ttp";

//ProgramEA.RunSingleEAGenderedTest(filename);

//GenerateRandomSpecimens(filename);
//GenerateGreedySpecimens(filename);
//GetTenSAResults(filename);

//ProgramTS.GetTenTSResultsDividedAsynchronously(filename);
//ProgramSA.GetTenSAResultsDividedAsynchronously(filename);
//ProgramEA.GetTenEAResultsDividedAsynchronously(filename);
//ProgramEA.GetHardEAResultsAsynchronously(filename);
//ProgramEA.GetOneFixedEAResultAsynchronously(filename);
//ProgramEA.RunSingleEATest(filename);

string baseDataPath = "./../../../Data";
string baseLogPath = "./../../../../../Wyniki";
var dataPath = Path.Combine(baseDataPath, filename);

var data = TTP_Reader.Load(dataPath);
var fileNameNoExtension = filename.Substring(0, filename.IndexOf("."));

float mutatorOneProbability = 0.3f;
float mutatorOneProbability_1 = 0.7f;
float mutatorOneProbability_2 = 0.5f;

uint generations = 5000;
uint population = 100;

double swapMutationProbability = 0.03;
double inversionMutationProbability = 0.3;
double orderedCrossoverProbability = 0.5;
double pmCrossoverProbability = 0.5;

double tournamentPart = 0.2;

int tournamentPopulation = (int)(tournamentPart * population);


IMutator<TTP_Specimen> inverseMutator = new InversionMutator<TTP_Specimen>(inversionMutationProbability);
IMutator<TTP_Specimen> swapMutator = new SwapMutator<TTP_Specimen>(swapMutationProbability);

ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

ICrossover<TTP_Specimen> orderedCrossover = new OrderedCrossover<TTP_Specimen>(orderedCrossoverProbability);
ICrossover<TTP_Specimen> pmCrossover = new PartiallyMatchedCrossover<TTP_Specimen>(pmCrossoverProbability);
ISelector<TTP_Specimen> tournamentSelector = new TournamentSelection<TTP_Specimen>(tournamentPopulation, false);



IMutator<TTP_Specimen_Gendered> inverseMutatorGendered = new InversionMutator<TTP_Specimen_Gendered>(inversionMutationProbability);
IMutator<TTP_Specimen_Gendered> swapMutatorGendered = new SwapMutator<TTP_Specimen_Gendered>(swapMutationProbability);

ISpecimenCreator<TTP_Specimen_Gendered> creatorGendered = new RandomCreator<TTP_Specimen_Gendered>(data, 0.3);
ISpecimenFactory<TTP_Specimen_Gendered> factoryGendered = new TTP_GenderedSpecimenFactory(data, creatorGendered);

ICrossover<TTP_Specimen_Gendered> orderedCrossoverGendered = new OrderedCrossover<TTP_Specimen_Gendered>(orderedCrossoverProbability);
ICrossover<TTP_Specimen_Gendered> pmCrossoverGendered = new PartiallyMatchedCrossover<TTP_Specimen_Gendered>(pmCrossoverProbability);
ISelector<TTP_Specimen_Gendered> tournamentSelectorGendered = new TournamentSelection<TTP_Specimen_Gendered>(tournamentPopulation, false);

string testNameStandardInversion = filename + "_standard_Inversion";
string testNameStandardSwap = filename + "_standard_Swap";
string testNameGenderedInversion = filename + "_gendered_Inversion";
string testNameGenderedSwap = filename + "_gendered_Swap";
string testNameTwoMutations = filename + "_twoMutations";
string testNameTwoMutationsGendered = filename + "_twoMutationsGendered";

Console.WriteLine("\n" + testNameStandardInversion);
List<double> results = new List<double>();

Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[5];
DateTime startTime = DateTime.Now;
for (int t = 0; t < taskArray.Length; t++)
{
    int j = t;
    taskArray[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATestAsync(data, inverseMutator, orderedCrossover, tournamentSelector, factory, population, generations, testNameStandardInversion, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArray);

double executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArray.Length; t++)
{
    results.Add(taskArray[t].Result.Score());
}

double standardDeviation = 0d;
double avgScore = 0d;
double maxScore = 0d;
double minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

Console.WriteLine("\n" + testNameStandardSwap);

taskArray = new Task<TTP_Specimen>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArray.Length; t++)
{
    int j = t;
    taskArray[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATestAsync(data, swapMutator, orderedCrossover, tournamentSelector, factory, population, generations, testNameStandardSwap, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArray);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArray.Length; t++)
{
    results.Add(taskArray[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}
string currentTitle = testNameTwoMutations + "_" + mutatorOneProbability;
Console.WriteLine("\n" + currentTitle);
taskArray = new Task<TTP_Specimen>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArray.Length; t++)
{
    int j = t;
    taskArray[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsTestAsync(data, inverseMutator, swapMutator, orderedCrossover, tournamentSelector, factory, mutatorOneProbability, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArray);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArray.Length; t++)
{
    results.Add(taskArray[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}
currentTitle = testNameTwoMutations + "_" + mutatorOneProbability_1;
Console.WriteLine("\n" + currentTitle);
taskArray = new Task<TTP_Specimen>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArray.Length; t++)
{
    int j = t;
    taskArray[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsTestAsync(data, inverseMutator, swapMutator, orderedCrossover, tournamentSelector, factory, mutatorOneProbability_1, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArray);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArray.Length; t++)
{
    results.Add(taskArray[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

currentTitle = testNameTwoMutations + "_" + mutatorOneProbability_2;
Console.WriteLine("\n" + currentTitle);
taskArray = new Task<TTP_Specimen>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArray.Length; t++)
{
    int j = t;
    taskArray[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsTestAsync(data, inverseMutator, swapMutator, orderedCrossover, tournamentSelector, factory, mutatorOneProbability_2, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArray);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArray.Length; t++)
{
    results.Add(taskArray[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

Console.WriteLine("\n" + testNameGenderedInversion);
Task<TTP_Specimen_Gendered>[] taskArrayGendered = new Task<TTP_Specimen_Gendered>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArrayGendered.Length; t++)
{
    int j = t;
    taskArrayGendered[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEAGenderedTestAsync(data, inverseMutatorGendered, orderedCrossoverGendered, tournamentSelectorGendered, factoryGendered, population, generations, testNameGenderedInversion, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArrayGendered);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArrayGendered.Length; t++)
{
    results.Add(taskArrayGendered[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

Console.WriteLine("\n" + testNameGenderedSwap);
taskArrayGendered = new Task<TTP_Specimen_Gendered>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArrayGendered.Length; t++)
{
    int j = t;
    taskArrayGendered[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEAGenderedTestAsync(data, swapMutatorGendered, orderedCrossoverGendered, tournamentSelectorGendered, factoryGendered, population, generations, testNameGenderedSwap, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArrayGendered);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArrayGendered.Length; t++)
{
    results.Add(taskArrayGendered[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

currentTitle = testNameTwoMutationsGendered + "_" + mutatorOneProbability;
Console.WriteLine("\n" + currentTitle);
taskArrayGendered = new Task<TTP_Specimen_Gendered>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArrayGendered.Length; t++)
{
    int j = t;
    taskArrayGendered[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsGenderedTestAsync(data, swapMutatorGendered, inverseMutatorGendered, orderedCrossoverGendered, tournamentSelectorGendered, factoryGendered, mutatorOneProbability, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArrayGendered);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArrayGendered.Length; t++)
{
    results.Add(taskArrayGendered[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

currentTitle = testNameTwoMutationsGendered + "_" + mutatorOneProbability_1;
Console.WriteLine("\n" + currentTitle);
taskArrayGendered = new Task<TTP_Specimen_Gendered>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArrayGendered.Length; t++)
{
    int j = t;
    taskArrayGendered[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsGenderedTestAsync(data, swapMutatorGendered, inverseMutatorGendered, orderedCrossoverGendered, tournamentSelectorGendered, factoryGendered, mutatorOneProbability_1, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArrayGendered);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArrayGendered.Length; t++)
{
    results.Add(taskArrayGendered[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

currentTitle = testNameTwoMutationsGendered + "_" + mutatorOneProbability_2;
Console.WriteLine("\n" + currentTitle);
taskArrayGendered = new Task<TTP_Specimen_Gendered>[5];
startTime = DateTime.Now;
for (int t = 0; t < taskArrayGendered.Length; t++)
{
    int j = t;
    taskArrayGendered[t] = Task.Factory.StartNew(() => ProgramEA.RunSingleEATwoMutationsGenderedTestAsync(data, swapMutatorGendered, inverseMutatorGendered, orderedCrossoverGendered, tournamentSelectorGendered, factoryGendered, mutatorOneProbability_2, population, generations, currentTitle, baseLogPath, j.ToString()));
}
Task.WaitAll(taskArrayGendered);

executionTime = (DateTime.Now - startTime).TotalSeconds;

for (int t = 0; t < taskArrayGendered.Length; t++)
{
    results.Add(taskArrayGendered[t].Result.Score());
}

standardDeviation = 0d;
avgScore = 0d;
maxScore = 0d;
minScore = 0d;

if (results.Any())
{
    avgScore = results.Average();
    maxScore = results.Max();
    minScore = results.Min();
    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));

    Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
    Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
    Console.WriteLine($"Average instance single test run time: {executionTime}");

    results.Clear();
}

