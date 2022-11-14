using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.EA.Crossovers;
using TTP_EA.EA.Mutators;
using TTP_EA.EA.Selectors;
using TTP_EA.Logger;
using TTP_EA.Reader;
using TTP_EA.Specimen;
using TTP_EA.Specimen.Creators;

namespace TTP_EA.EA
{
    public static class ProgramEA
    {
        public static void Run_EA(TTP_Data data, string logPath)
        {
            IMutator mutator = new SwapMutator(0.01);
            ICrossover crosover = new OrderedCrossover(0.5);
            ISelector selector = new TournamentSelection(10, false);

            //IMutator mutator = new InversionMutator(0.1);
            //ICrossover crosover = new PartiallyMatchedCrossover(0.4);
            //ISelector selector = new RouletteSelection(true);

            ISpecimenCreator creator = new RandomCreator(data, 0.3);

            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);

            uint populationSize = 100;
            uint generations = 100;

            EvolutionaryAlgorithm ea = new EvolutionaryAlgorithm(data, mutator, crosover, selector, creator, populationSize, generations, logger);
            ea.InitializePopulation();
            ea.Execute();
            logger.Dispose();
        }

        public static void GetTenEAResults(string fileName)
        {
            string baseDataPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Dane/";
            string baseLogPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Wyniki/";
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            string currentLogFileName = fileNameNoExtension + "_0.csv";
            var logPath = Path.Combine(baseLogPath, currentLogFileName);

            IMutator mutator = new SwapMutator(0.01);
            //ICrossover crosover = new OrderedCrossover(0.2);
            ISelector selector = new TournamentSelection(10, false);

            //IMutator mutator = new InversionMutator(0.25);
            ICrossover crosover = new PartiallyMatchedCrossover(0.5);
            //ISelector selector = new RouletteSelection(false);

            ISpecimenCreator creator = new RandomCreator(data, 0.3);

            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);

            uint populationSize = 100;
            uint generations = 100;

            EvolutionaryAlgorithm ea = new EvolutionaryAlgorithm(data, mutator, crosover, selector, creator, populationSize, generations, logger);
            ea.InitializePopulation();
            ea.Execute();
            logger.Dispose();

            for (int i = 1; i < 10; i++)
            {
                currentLogFileName = fileNameNoExtension + "_" + i.ToString() + ".csv";
                logPath = Path.Combine(baseLogPath, currentLogFileName);
                logger = new CSV_Logger<EARecord>(logPath);

                ea.Logger = logger;
                ea.InitializePopulation();
                ea.Execute();
                logger.Dispose();
            }
        }

        public static void GenerateRandomSpecimens(string fileName)
        {
            string basePath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Dane/";
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

                if (score > maxScore)
                {
                    maxScore = score;
                }
                if (score < minScore)
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

        public static void GenerateGreedySpecimens(string fileName)
        {
            string basePath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Dane/";
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

        public static List<double> GetAllGreedySpecimens(TTP_Specimen specimen, TTP_Data data)
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

                    foreach(var path in paths)
                    {
                        if(path.From != path.To && currentCities.Contains(path.To) && maxDistance > path.Distance)
                        {
                            selectedPath = path;
                            maxDistance = path.Distance;
                        }
                    }
                    currentCity = selectedPath.To;
                    currentCities.Remove(currentCity);
                    specimen.VisitedCities.Add(currentCity);
                    //var distancesFrom = distances.Where(x => x.Key.from == currentCity && currentCities.Contains(x.Key.to)
                    //    && x.Key.from != x.Key.to);
                    //var selected = distances.Where(x => x.Key.from == currentCity
                    //    && currentCities.Contains(x.Key.to)
                    //    && x.Key.from != x.Key.to
                    //    ).MinBy(x => x.Value);
                    //currentCity = selected.Key.to;
                    //currentCities.Remove(currentCity);
                    //specimen.VisitedCities.Add(currentCity);
                }
                GreedyCreator.FillGreedyKnapsack(specimen);
                var score = specimen.Score();
                resulsts.Add(score);
                Console.WriteLine($"Starting City: {city.Index}, score: {score}");
            }
            return resulsts;
        }

        public static void Run()
        {
            string filename = "hard_0.ttp";

            //string baseDataPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Dane/";
            //string baseLogPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab_1/Wyniki/";
            //var dataPath = Path.Combine(baseDataPath, filename);

            //var data = TTP_Reader.Load(dataPath);
            //var fileNameNoExtension = filename.Substring(0, filename.IndexOf("."));

            //string currentLogFileName = fileNameNoExtension + "_0.csv";
            //var logPath = Path.Combine(baseLogPath, currentLogFileName);

            //Run_EA(data, logPath);

            //GenerateRandomSpecimens(filename);
            GenerateGreedySpecimens(filename);
            //GetTenEAResults(filename);
        }

        //Task based ea run code starts here
        public static TTP_Specimen RunSingleEATestAsync(TTP_Data problemData, IMutator mutator, ICrossover crossover, ISelector selector, ISpecimenCreator creator, uint populationSize, uint generationsToCheck, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);
            EvolutionaryAlgorithm ea = new EvolutionaryAlgorithm(problemData, mutator, crossover, selector, creator, populationSize, generationsToCheck, logger);
            var bestFoundSpecimen = ea.Execute();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TestInstanceData RunMutationEATestsAsync(TTP_Data data, ISpecimenCreator creator, IMutator mutator, string mutatorName, string fileNameNoExtension, string baseLogPath)
        {
            ICrossover crossover = new PartiallyMatchedCrossover(1);
            ISelector tournamentSelector = new TournamentSelection(1, false);

            string crossoverName = "PartiallyMatched";

            TestInstanceData bestData = new TestInstanceData
            {
                InstanceName = "",
                MaxScore = -1d,
                MinScore = -1d,
                AvgScore = double.MinValue,
                StdDeviation = -1d,
                AvgSingleRunSeconds = -1d
            };

            int searchSize = 50000;
            uint[] generations = new uint[] { 100, 500, 1000, 5000 };
            uint[] populationSizes = new uint[] { 500, 100, 50, 10 };
            double[] tournamentPartSizes = new double[] { 0.05, 0.15, 0.3, 0.5 };
            double[] mutationProbabilities = new double[] { 0.01, 0.05, 0.1, 0.15, 0.3, 0.5 };
            double[] crossoverProbabilities = new double[] { 0.2, 0.4, 0.6, 0.8 };

            for(int g = 0; g < generations.Length; g++)
            {
                for(int ps = 0; ps < populationSizes.Length; ps++)
                {
                    if (generations[g] * populationSizes[ps] == searchSize)
                    {
                        for(int tps = 0; tps < tournamentPartSizes.Length; tps++)
                        {
                            if((int)(populationSizes[ps] * tournamentPartSizes[tps]) > 1)
                            {
                                ((TournamentSelection)tournamentSelector).SpecimenCount = ((int)(populationSizes[ps] * tournamentPartSizes[tps]));

                                for (int c = 0; c < 2; c++)
                                {
                                    if (c == 1)
                                    {
                                        crossover = new OrderedCrossover(1);
                                        crossoverName = "Ordered";
                                    }
                                    else if (c == 0)
                                    {
                                        crossover = new PartiallyMatchedCrossover(1);
                                        crossoverName = "PartiallyMatched";
                                    }

                                    for (int cp = 0; cp < crossoverProbabilities.Length; cp++)
                                    {
                                        crossover.CrossoverProbability = crossoverProbabilities[cp];

                                        for (int mp = 0; mp < mutationProbabilities.Length; mp++)
                                        {
                                            if ((mutatorName == "Inverse" && mutationProbabilities[mp] > 0.12) || (mutatorName == "Swap" && mutationProbabilities[mp] < 0.12))
                                            {
                                                mutator.MutationProbability = mutationProbabilities[mp];

                                                string currentLogFileName = fileNameNoExtension +
                                                "_Mutation_" + mutatorName +
                                                "_prob_" + mutationProbabilities[mp].ToString() +
                                                "_crossover_" + crossoverName +
                                                "_prob_" + crossoverProbabilities[cp].ToString() +
                                                "_tournament_percentage_" + tournamentPartSizes[tps].ToString() +
                                                "_population_" + populationSizes[ps].ToString() +
                                                "_generations_" + generations[g].ToString();
                                                Console.WriteLine(currentLogFileName);

                                                List<double> results = new List<double>();

                                                Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[5];
                                                DateTime startTime = DateTime.Now;
                                                for (int t = 0; t < taskArray.Length; t++)
                                                {
                                                    int j = t;
                                                    taskArray[t] = Task.Factory.StartNew(() => RunSingleEATestAsync(data, mutator, crossover, tournamentSelector, creator, populationSizes[ps], generations[g], currentLogFileName, baseLogPath, j.ToString()));
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
                                                double avgExecutionTime = 0d;

                                                if (results.Any())
                                                {
                                                    avgScore = results.Average();
                                                    maxScore = results.Max();
                                                    minScore = results.Min();
                                                    double sum = results.Sum(d => Math.Pow(d - avgScore, 2));
                                                    standardDeviation = Math.Sqrt((sum) / (results.Count() - 1));
                                                    avgExecutionTime = executionTime / taskArray.Length;
                                                }

                                                if (avgScore > bestData.AvgScore)
                                                {
                                                    bestData.AvgScore = avgScore;
                                                    bestData.MinScore = minScore;
                                                    bestData.MaxScore = maxScore;
                                                    bestData.StdDeviation = standardDeviation;
                                                    bestData.InstanceName = currentLogFileName;
                                                    bestData.AvgSingleRunSeconds = avgExecutionTime;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bestData;
        }

        public static void GetTenEAResultsDividedAsynchronously(string fileName)
        {
            string baseDataPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Dane/";
            string baseLogPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Wyniki/";
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            IMutator inverseMutator = new InversionMutator(1);
            IMutator swapMutator = new SwapMutator(1);

            ISpecimenCreator creator = new RandomCreator(data, 0.3);

            Task<TestInstanceData> inverseInstance = Task.Factory.StartNew(() => RunMutationEATestsAsync(data, creator, inverseMutator, "Inverse", fileNameNoExtension, baseLogPath));
            Task<TestInstanceData> swapInstance = Task.Factory.StartNew(() => RunMutationEATestsAsync(data, creator, swapMutator, "Swap", fileNameNoExtension, baseLogPath));

            Task.WaitAll(inverseInstance, swapInstance);

            var inverseResult = inverseInstance.Result;
            var swapResult = swapInstance.Result;

            if (inverseResult.AvgScore > swapResult.AvgScore)
            {
                Console.WriteLine($"\nFile: {inverseResult.InstanceName} Inverse");
                Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
                Console.WriteLine($"{inverseResult.MaxScore}\t{inverseResult.MinScore}\t{inverseResult.AvgScore}\t{inverseResult.StdDeviation}");
                Console.WriteLine($"Average instance single test run time: {inverseResult.AvgSingleRunSeconds}");
            }
            else
            {
                Console.WriteLine($"\nFile: {swapResult.InstanceName} Swap");
                Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
                Console.WriteLine($"{swapResult.MaxScore}\t{swapResult.MinScore}\t{swapResult.AvgScore}\t{swapResult.StdDeviation}");
                Console.WriteLine($"Average instance single test run time: {swapResult.AvgSingleRunSeconds}");
            }
        }
    }
}
