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
using TTP_EA.Specimen.Factories;

namespace TTP_EA.EA
{
    public static class ProgramEA
    {
        private static readonly string baseDataPath = "./../../../Data";
        private static readonly string baseLogPath = "./../../../../../Wyniki";
        public static void Run_EA(TTP_Data data, string logPath)
        {
            IMutator<TTP_Specimen> mutator = new SwapMutator<TTP_Specimen>(0.01);
            ICrossover<TTP_Specimen> crosover = new OrderedCrossover<TTP_Specimen>(0.5);
            ISelector<TTP_Specimen> selector = new TournamentSelection<TTP_Specimen>(10, false);

            //IMutator mutator = new InversionMutator(0.1);
            //ICrossover crosover = new PartiallyMatchedCrossover(0.4);
            //ISelector selector = new RouletteSelection(true);

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);

            uint populationSize = 100;
            uint generations = 100;

            EvolutionaryAlgorithm<TTP_Specimen> ea = new EvolutionaryAlgorithm<TTP_Specimen>(data, mutator, crosover, selector, factory, populationSize, generations, logger);
            ea.InitializePopulation();
            ea.Execute();
            logger.Dispose();
        }

        public static void GetTenEAResults(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            string currentLogFileName = fileNameNoExtension + "_0.csv";
            var logPath = Path.Combine(baseLogPath, currentLogFileName);

            IMutator<TTP_Specimen> mutator = new SwapMutator<TTP_Specimen>(0.01);
            //ICrossover crosover = new OrderedCrossover(0.2);
            ISelector<TTP_Specimen> selector = new TournamentSelection<TTP_Specimen>(10, false);

            //IMutator mutator = new InversionMutator(0.25);
            ICrossover<TTP_Specimen> crosover = new PartiallyMatchedCrossover<TTP_Specimen>(0.5);
            //ISelector selector = new RouletteSelection(false);

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);

            uint populationSize = 100;
            uint generations = 100;

            EvolutionaryAlgorithm<TTP_Specimen> ea = new EvolutionaryAlgorithm<TTP_Specimen>(data, mutator, crosover, selector, factory, populationSize, generations, logger);
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
            var path = Path.Combine(baseDataPath, fileName);
            uint count = 10000;
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
            var path = Path.Combine(baseDataPath, fileName);

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
                GreedyCreator<TTP_Specimen>.FillGreedyKnapsack(specimen);
                var score = specimen.Score();
                resulsts.Add(score);
                Console.WriteLine($"Starting City: {city.Index}, score: {score}");
            }
            return resulsts;
        }

        public static void Run()
        {
            string filename = "hard_0.ttp";

            GenerateGreedySpecimens(filename);
        }

        //Task based ea run code starts here
        public static TTP_Specimen RunSingleEATestAsync(TTP_Data problemData, IMutator<TTP_Specimen> mutator, ICrossover<TTP_Specimen> crossover, ISelector<TTP_Specimen> selector, ISpecimenFactory<TTP_Specimen> factory, uint populationSize, uint generationsToCheck, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);
            EvolutionaryAlgorithm<TTP_Specimen> ea = new EvolutionaryAlgorithm<TTP_Specimen>(problemData, mutator, crossover, selector, factory, populationSize, generationsToCheck, logger);
            var bestFoundSpecimen = ea.Execute();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TTP_Specimen_Gendered RunSingleEAGenderedTestAsync(TTP_Data problemData, IMutator<TTP_Specimen_Gendered> mutator, ICrossover<TTP_Specimen_Gendered> crossover, ISelector<TTP_Specimen_Gendered> selector, ISpecimenFactory<TTP_Specimen_Gendered> factory, uint populationSize, uint generationsToCheck, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);
            EvolutionaryAlgorithm<TTP_Specimen_Gendered> ea = new EvolutionaryAlgorithm<TTP_Specimen_Gendered>(problemData, mutator, crossover, selector, factory, populationSize, generationsToCheck, logger);
            var bestFoundSpecimen = ea.Execute();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TTP_Specimen RunSingleEATwoMutationsTestAsync(TTP_Data problemData, IMutator<TTP_Specimen> mutatorOne, IMutator<TTP_Specimen> mutatorTwo, ICrossover<TTP_Specimen> crossover, ISelector<TTP_Specimen> selector, ISpecimenFactory<TTP_Specimen> factory, float mutationOneProbability, uint populationSize, uint generationsToCheck, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);
            EvolutionaryAlgorithmTwoMutations<TTP_Specimen> ea = new EvolutionaryAlgorithmTwoMutations<TTP_Specimen>(problemData, mutatorOne, mutatorTwo, crossover, selector, factory, mutationOneProbability, populationSize, generationsToCheck, logger);
            var bestFoundSpecimen = ea.Execute();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TTP_Specimen_Gendered RunSingleEATwoMutationsGenderedTestAsync(TTP_Data problemData, IMutator<TTP_Specimen_Gendered> mutatorOne, IMutator<TTP_Specimen_Gendered> mutatorTwo, ICrossover<TTP_Specimen_Gendered> crossover, ISelector<TTP_Specimen_Gendered> selector, ISpecimenFactory<TTP_Specimen_Gendered> factory, float mutationOneProbability, uint populationSize, uint generationsToCheck, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<EARecord> logger = new CSV_Logger<EARecord>(logPath);
            EvolutionaryAlgorithmTwoMutations<TTP_Specimen_Gendered> ea = new EvolutionaryAlgorithmTwoMutations<TTP_Specimen_Gendered>(problemData, mutatorOne, mutatorTwo, crossover, selector, factory, mutationOneProbability, populationSize, generationsToCheck, logger);
            var bestFoundSpecimen = ea.Execute();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TestInstanceData RunMutationEATestsAsync(TTP_Data data, ISpecimenFactory<TTP_Specimen> factory, IMutator<TTP_Specimen> mutator, string mutatorName, string fileNameNoExtension, string baseLogPath)
        {
            ICrossover<TTP_Specimen> crossover = new PartiallyMatchedCrossover<TTP_Specimen>(1);
            ISelector<TTP_Specimen> tournamentSelector = new TournamentSelection<TTP_Specimen>(1, false);

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
                                ((TournamentSelection<TTP_Specimen>)tournamentSelector).SpecimenCount = ((int)(populationSizes[ps] * tournamentPartSizes[tps]));

                                for (int c = 0; c < 2; c++)
                                {
                                    if (c == 1)
                                    {
                                        crossover = new OrderedCrossover<TTP_Specimen>(1);
                                        crossoverName = "Ordered";
                                    }
                                    else if (c == 0)
                                    {
                                        crossover = new PartiallyMatchedCrossover<TTP_Specimen>(1);
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
                                                    taskArray[t] = Task.Factory.StartNew(() => RunSingleEATestAsync(data, mutator, crossover, tournamentSelector, factory, populationSizes[ps], generations[g], currentLogFileName, baseLogPath, j.ToString()));
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
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            IMutator<TTP_Specimen> inverseMutator = new InversionMutator<TTP_Specimen>(1);
            IMutator<TTP_Specimen> swapMutator = new SwapMutator<TTP_Specimen>(1);

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

            Task<TestInstanceData> inverseInstance = Task.Factory.StartNew(() => RunMutationEATestsAsync(data, factory, inverseMutator, "Inverse", fileNameNoExtension, baseLogPath));
            Task<TestInstanceData> swapInstance = Task.Factory.StartNew(() => RunMutationEATestsAsync(data, factory, swapMutator, "Swap", fileNameNoExtension, baseLogPath));

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


        public static TestInstanceData RunEATestsAsync(TTP_Data data, ISpecimenFactory<TTP_Specimen> factory, IMutator<TTP_Specimen> mutator, string mutatorName, string fileNameNoExtension, string baseLogPath)
        {
            ICrossover<TTP_Specimen> crossover = new OrderedCrossover<TTP_Specimen>(1);
            ISelector<TTP_Specimen> tournamentSelector = new TournamentSelection<TTP_Specimen>(1, false);

            string crossoverName = "Ordered";

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
            double[] crossoverProbabilities = new double[] { 0.2, 0.4, 0.6, 0.8 };

            for (int g = 0; g < generations.Length; g++)
            {
                for (int ps = 0; ps < populationSizes.Length; ps++)
                {
                    if (generations[g] * populationSizes[ps] == searchSize)
                    {
                        for (int tps = 0; tps < tournamentPartSizes.Length; tps++)
                        {
                            if ((int)(populationSizes[ps] * tournamentPartSizes[tps]) > 1)
                            {
                                ((TournamentSelection<TTP_Specimen>)tournamentSelector).SpecimenCount = ((int)(populationSizes[ps] * tournamentPartSizes[tps]));

                                for (int cp = 0; cp < crossoverProbabilities.Length; cp++)
                                {
                                    crossover.CrossoverProbability = crossoverProbabilities[cp];

                                    string currentLogFileName = fileNameNoExtension +
                                            "_Mutation_" + mutatorName +
                                            "_prob_" + mutator.MutationProbability.ToString() +
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
                                        taskArray[t] = Task.Factory.StartNew(() => RunSingleEATestAsync(data, mutator, crossover, tournamentSelector, factory, populationSizes[ps], generations[g], currentLogFileName, baseLogPath, j.ToString()));
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

            return bestData;
        }

        public static void GetOneFixedEAResultAsynchronously(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            IMutator<TTP_Specimen> mutator = new InversionMutator<TTP_Specimen>(0.5);
            ICrossover<TTP_Specimen> crossover = new OrderedCrossover<TTP_Specimen>(0.2);
            ISelector<TTP_Specimen> tournamentSelector = new TournamentSelection<TTP_Specimen>(5, false);
            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);
            uint generations = 5000;
            uint population = 10;

            string currentLogFileName = fileNameNoExtension +
                                            "_Mutation_" + "Inverse" +
                                            "_prob_" + mutator.MutationProbability.ToString() +
                                            "_crossover_" + "Ordered" +
                                            "_prob_" + crossover.CrossoverProbability.ToString() +
                                            "_tournament_percentage_" + 0.5 +
                                            "_population_" + 10 +
                                            "_generations_" + 5000;
            Console.WriteLine(currentLogFileName);

            List<double> results = new List<double>();

            Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[5];
            DateTime startTime = DateTime.Now;
            for (int t = 0; t < taskArray.Length; t++)
            {
                int j = t;
                taskArray[t] = Task.Factory.StartNew(() => RunSingleEATestAsync(data, mutator, crossover, tournamentSelector, factory, population, generations, currentLogFileName, baseLogPath, j.ToString()));
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

                Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
                Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
                Console.WriteLine($"Average instance single test run time: {avgExecutionTime}");
            }
        }

        public static void GetHardEAResultsAsynchronously(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            IMutator<TTP_Specimen> inverseMutator03 = new InversionMutator<TTP_Specimen>(0.3);
            IMutator<TTP_Specimen> inverseMutator05 = new InversionMutator<TTP_Specimen>(0.5);

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

            Task<TestInstanceData> inverseInstance03 = Task.Factory.StartNew(() => RunEATestsAsync(data, factory, inverseMutator03, "Inverse", fileNameNoExtension, baseLogPath));
            Task<TestInstanceData> inverseInstance05 = Task.Factory.StartNew(() => RunEATestsAsync(data, factory, inverseMutator05, "Inverse", fileNameNoExtension, baseLogPath));

            Task.WaitAll(inverseInstance03, inverseInstance05);

            var inverse03Result = inverseInstance03.Result;
            var inverse05Result = inverseInstance05.Result;

            if (inverse03Result.AvgScore > inverse05Result.AvgScore)
            {
                Console.WriteLine($"\nFile: {inverse03Result.InstanceName} Inverse 0,3");
                Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
                Console.WriteLine($"{inverse03Result.MaxScore}\t{inverse03Result.MinScore}\t{inverse03Result.AvgScore}\t{inverse03Result.StdDeviation}");
                Console.WriteLine($"Average instance single test run time: {inverse03Result.AvgSingleRunSeconds}");
            }
            else
            {
                Console.WriteLine($"\nFile: {inverse05Result.InstanceName} Inverse 0,5");
                Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
                Console.WriteLine($"{inverse05Result.MaxScore}\t{inverse05Result.MinScore}\t{inverse05Result.AvgScore}\t{inverse05Result.StdDeviation}");
                Console.WriteLine($"Average instance single test run time: {inverse05Result.AvgSingleRunSeconds}");
            }
        }

        public static void RunSingleEATest(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            IMutator<TTP_Specimen> inverseMutator015 = new InversionMutator<TTP_Specimen>(0.15);
            IMutator<TTP_Specimen> inverseMutator03 = new InversionMutator<TTP_Specimen>(0.3);
            IMutator<TTP_Specimen> inverseMutator05 = new InversionMutator<TTP_Specimen>(0.5);

            IMutator<TTP_Specimen>[] mutatorTab = new IMutator<TTP_Specimen>[] { inverseMutator015, inverseMutator03, inverseMutator05 };

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);
            ISpecimenFactory<TTP_Specimen> factory = new TTP_SpecimenFactory(data, creator);

            ICrossover<TTP_Specimen> crossover = new OrderedCrossover<TTP_Specimen>(0.4);
            ISelector<TTP_Specimen> tournamentSelector = new TournamentSelection<TTP_Specimen>(30, false);

            Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[3];
            for (int t = 0; t < taskArray.Length; t++)
            {
                int j = t;
                
                taskArray[t] = Task.Factory.StartNew(() => RunSingleEATestAsync(data, mutatorTab[j], crossover, tournamentSelector, factory, 100, 500, fileNameNoExtension, baseLogPath, j.ToString()));
            }
            Task.WaitAll(taskArray);
        }

        public static void RunSingleEAGenderedTest(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);

            IMutator<TTP_Specimen_Gendered> inverseMutator = new InversionMutator<TTP_Specimen_Gendered>(0.15);
            ISpecimenCreator<TTP_Specimen_Gendered> creator = new RandomCreator<TTP_Specimen_Gendered>(data, 0.3);
            ISpecimenFactory<TTP_Specimen_Gendered> factory = new TTP_GenderedSpecimenFactory(data, creator);
            ICrossover<TTP_Specimen_Gendered> crossover = new PartiallyMatchedCrossover<TTP_Specimen_Gendered>(0.5);
            ISelector<TTP_Specimen_Gendered> tournamentSelector = new TournamentSelection<TTP_Specimen_Gendered>(30, false);

            RunSingleEAGenderedTestAsync(data, inverseMutator, crossover, tournamentSelector, factory, 100, 100, "gender_test_" + fileName, baseLogPath, "1");
        }
    }
}
