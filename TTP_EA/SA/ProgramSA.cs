using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Logger;
using TTP_EA.Reader;
using TTP_EA.Specimen;
using TTP_EA.TS.Neighbors;

namespace TTP_EA.SA
{
    public static class ProgramSA
    {
        public static void GetTenSAResults(string fileName)
        {
            string baseDataPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Dane/";
            string baseLogPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Wyniki/";
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            string currentLogFileName = fileNameNoExtension + "_0.csv";
            var logPath = Path.Combine(baseLogPath, currentLogFileName);

            //INeighbor neighbor = new InverseNeighbor();
            INeighbor neighbor = new SwapNeighbor();

            NeighborhoodFinder neighborhoodFinder = new NeighborhoodFinder(neighbor);
            ISpecimenCreator creator = new RandomCreator(data, 0.3);

            CSV_Logger<TSRecord> logger = new CSV_Logger<TSRecord>(logPath);

            List<double> results = new List<double>();

            int neighborsCount = 10;
            int iterations = 1000;
            double startTemperature = 10000; //10000000;
            double targetTemperature = 0.5;
            double coolingRatio = 0.995;

            SimulatedAnnealing sa = new SimulatedAnnealing(data, creator, iterations, neighborsCount, startTemperature, targetTemperature, coolingRatio, neighborhoodFinder, logger);

            var bestFoundSpecimen = sa.RunSimulatedAnnealing();
            results.Add(bestFoundSpecimen.Score());

            logger.Dispose();

            for (int i = 1; i < 10; i++)
            {
                currentLogFileName = fileNameNoExtension + "_" + i.ToString() + ".csv";
                logPath = Path.Combine(baseLogPath, currentLogFileName);
                logger = new CSV_Logger<TSRecord>(logPath);

                sa.Logger = logger;
                bestFoundSpecimen = sa.RunSimulatedAnnealing();
                logger.Dispose();

                results.Add(bestFoundSpecimen.Score());
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
            }

            Console.WriteLine($"\nFile: {fileName} SA");
            Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
            Console.WriteLine($"{maxScore}\t{minScore}\t{avgScore}\t{standardDeviation}");
        }

        public static TTP_Specimen RunSingleSATestAsync(TTP_Data data, ISpecimenCreator creator, int iterations, int neighbors, double startTemperature, double targetTemperature, double coolingRatio, NeighborhoodFinder neighborhoodFinder, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<TSRecord> logger = new CSV_Logger<TSRecord>(logPath);
            SimulatedAnnealing sa = new SimulatedAnnealing(data, creator, iterations, neighbors, startTemperature, targetTemperature, coolingRatio, neighborhoodFinder, logger);
            var bestFoundSpecimen = sa.RunSimulatedAnnealing();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TestInstanceData RunNeighborhoodSATestsAsync(TTP_Data data, ISpecimenCreator creator, INeighbor neighbor, string neighborName, string fileNameNoExtension, string baseLogPath)
        {
            NeighborhoodFinder neighborhoodFinder = new NeighborhoodFinder(neighbor);

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
            int[] iterations = new int[] { 1000, 5000, 10000 };
            int[] neighbors = new int[] { 5, 10, 50 };

            //to calculate & fill
            double[] startingTempeatures = new double[] { 1.15, 5, 35, 2000, 2500, 100000, 10000000000, 100000000000000, 100000000000000000 };
            double[] coolingRatios = new double[] { 0.999, 0.997, 0.99, 0.96 };

            double targetTemparature = 0.5d;

            for (int n = 0; n < neighbors.Length; n++)
            {
                for (int i = 0; i < iterations.Length; i++)
                {
                    if (neighbors[n] * iterations[i] == searchSize)
                    {
                        for (int st = 0; st < startingTempeatures.Length; st++)
                        {
                            for(int cr = 0; cr < coolingRatios.Length; cr++)
                            {
                                if ((startingTempeatures[st] * Math.Pow(coolingRatios[cr], iterations[i] * 0.75)) > targetTemparature && (startingTempeatures[st] * Math.Pow(coolingRatios[cr], iterations[i] * 0.85)) < targetTemparature)
                                {
                                    string currentLogFileName = fileNameNoExtension + "_Neighborhood_" + neighborName + "_neighbors_" + neighbors[n].ToString() + "_start_temperature_" + startingTempeatures[st].ToString() + "_cooling_ratio_" + coolingRatios[cr].ToString() + "_iterations_" + iterations[i].ToString();
                                    Console.WriteLine(currentLogFileName);

                                    List<double> results = new List<double>();

                                    Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[5];
                                    DateTime startTime = DateTime.Now;
                                    for (int t = 0; t < taskArray.Length; t++)
                                    {
                                        int j = t;
                                        taskArray[t] = Task.Factory.StartNew(() => RunSingleSATestAsync(data, creator, iterations[i], neighbors[n], startingTempeatures[st], targetTemparature, coolingRatios[cr], neighborhoodFinder, currentLogFileName, baseLogPath, j.ToString()));
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

        public static void GetTenSAResultsDividedAsynchronously(string fileName)
        {
            string baseDataPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Dane/";
            string baseLogPath = "C:/Files/Studia/Sem_7/Metaheurystyki/Lab/Wyniki/";
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            INeighbor neighborInverse = new InverseNeighbor();
            INeighbor neighborSwap = new SwapNeighbor();

            ISpecimenCreator creator = new RandomCreator(data, 0.3);

            Task<TestInstanceData> inverseInstance = Task.Factory.StartNew(() => RunNeighborhoodSATestsAsync(data, creator, neighborInverse, "Inverse", fileNameNoExtension, baseLogPath));
            Task<TestInstanceData> swapInstance = Task.Factory.StartNew(() => RunNeighborhoodSATestsAsync(data, creator, neighborSwap, "Swap", fileNameNoExtension, baseLogPath));

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
