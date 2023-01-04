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

namespace TTP_EA.TS
{
    public static class ProgramTS
    {
        private static readonly string baseDataPath = "./../../../Data";
        private static readonly string baseLogPath = "./../../../../../Wyniki";
        public static void GetTenTSResults(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            string currentLogFileName = fileNameNoExtension + "_0.csv";
            var logPath = Path.Combine(baseLogPath, currentLogFileName);

            INeighbor neighbor = new InverseNeighbor();
            //INeighbor neighbor = new SwapNeighbor();

            NeighborhoodFinder neighborhoodFinder = new NeighborhoodFinder(neighbor);
            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);

            CSV_Logger<TSRecord> logger = new CSV_Logger<TSRecord>(logPath);

            int neighborsCount = 20;
            int iterations = 1000;
            int tabuListCount = 100;

            TabuSearch ts = new TabuSearch(data, creator, iterations, neighborsCount, tabuListCount, neighborhoodFinder, logger);

            var bestFoundSpecimen = ts.RunTabuSearch();

            logger.Dispose();

            for (int i = 1; i < 3; i++)
            {
                currentLogFileName = fileNameNoExtension + "_" + i.ToString() + ".csv";
                logPath = Path.Combine(baseLogPath, currentLogFileName);
                logger = new CSV_Logger<TSRecord>(logPath);

                ts.Logger = logger;
                bestFoundSpecimen = ts.RunTabuSearch();
                logger.Dispose();
            }
        }

        public static TTP_Specimen RunSingleTSTestAsync(TTP_Data data, ISpecimenCreator<TTP_Specimen> creator, int iterations, int neighbors, int tabuSize, NeighborhoodFinder neighborhoodFinder, string currentLogFileName, string baseLogPath, string testNo)
        {
            string currentTestLogFileName = currentLogFileName + "_" + testNo + ".csv";
            string logPath = Path.Combine(baseLogPath, currentTestLogFileName);
            CSV_Logger<TSRecord> logger = new CSV_Logger<TSRecord>(logPath);
            TabuSearch ts = new TabuSearch(data, creator, iterations, neighbors, tabuSize, neighborhoodFinder, logger);
            var bestFoundSpecimen = ts.RunTabuSearch();
            logger.Dispose();

            return bestFoundSpecimen;
        }

        public static TestInstanceData RunNeighborhoodTestsAsync(TTP_Data data, ISpecimenCreator<TTP_Specimen> creator, INeighbor neighbor, string neighborName, string fileNameNoExtension, string baseLogPath)
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
            int[] tabuSizes = new int[] { 50, 100, 200 };

            for (int n = 0; n < neighbors.Length; n++)
            {
                for (int i = 0; i < iterations.Length; i++) 
                {
                    if (neighbors[n] * iterations[i] == searchSize)
                    {
                        for (int ts = 0; ts < tabuSizes.Length; ts++)
                        {
                            string currentLogFileName = fileNameNoExtension + "_Neighborhood_" + neighborName + "_neighbors_" + neighbors[n].ToString() + "_tabu_" + tabuSizes[ts].ToString() + "_iterations_" + iterations[i].ToString();
                            Console.WriteLine(currentLogFileName);

                            List<double> results = new List<double>();

                            Task<TTP_Specimen>[] taskArray = new Task<TTP_Specimen>[5];
                            DateTime startTime = DateTime.Now;
                            for (int t = 0; t < taskArray.Length; t++)
                            {
                                int j = t;
                                taskArray[t] = Task.Factory.StartNew(() => RunSingleTSTestAsync(data, creator, iterations[i], neighbors[n], tabuSizes[ts], neighborhoodFinder, currentLogFileName, baseLogPath, j.ToString()));
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

            return bestData;
        }

        public static void GetTenTSResultsDividedAsynchronously(string fileName)
        {
            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            INeighbor neighborInverse = new InverseNeighbor();
            INeighbor neighborSwap = new SwapNeighbor();

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);

            Task<TestInstanceData> inverseInstance = Task.Factory.StartNew(() => RunNeighborhoodTestsAsync(data, creator, neighborInverse, "Inverse", fileNameNoExtension, baseLogPath));
            Task<TestInstanceData> swapInstance = Task.Factory.StartNew(() => RunNeighborhoodTestsAsync(data, creator, neighborSwap, "Swap", fileNameNoExtension, baseLogPath));

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

        //sequential - do not use!
        public static void RunTabuAnalysis(string fileName)
        {
            double bestAvgScore = double.MinValue;
            double bestMaxScore = -1d;
            double bestMinScore = -1d;
            double beststandardDeviation = -1;

            string bestConfigName = "";

            var dataPath = Path.Combine(baseDataPath, fileName);

            var data = TTP_Reader.Load(dataPath);
            var fileNameNoExtension = fileName.Substring(0, fileName.IndexOf("."));

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(data, 0.3);

            for (int neighborhood = 0; neighborhood < 2; neighborhood++)
            {
                INeighbor neighbor;
                if (neighborhood == 0)
                {
                    neighbor = new InverseNeighbor();
                }
                else
                {
                    neighbor = new SwapNeighbor();
                }
                NeighborhoodFinder neighborhoodFinder = new NeighborhoodFinder(neighbor);

                for (int neighbors = 20; neighbors <= 60; neighbors += 10)
                {
                    for (int tabuSize = 100; tabuSize <= 500; tabuSize += 100)
                    {
                        for (int iterations = 2000; iterations <= 10000; iterations += 2000)
                        {
                            List<double> results = new List<double>();

                            string currentLogFileName = fileNameNoExtension + "_Neighborhood_" + neighborhood.ToString() + "_neighbors_" + neighbors.ToString() + "_tabu_" + tabuSize.ToString() + "_iterations_" + iterations.ToString();
                            Console.WriteLine(currentLogFileName);
                            string currentTestLogFileName = fileNameNoExtension + "_Neighborhood_" + neighborhood.ToString() + "_neighbors_" + neighbors.ToString() + "_tabu_" + tabuSize.ToString() + "_iterations_" + iterations.ToString() + "_0.csv";
                            var logPath = Path.Combine(baseLogPath, currentTestLogFileName);

                            CSV_Logger<TSRecord> logger = new CSV_Logger<TSRecord>(logPath);

                            TabuSearch ts = new TabuSearch(data, creator, iterations, neighbors, tabuSize, neighborhoodFinder, logger);

                            var bestFoundSpecimen = ts.RunTabuSearch();

                            results.Add(bestFoundSpecimen.Score());

                            logger.Dispose();

                            for (int i = 1; i < 5; i++)
                            {
                                currentTestLogFileName = currentLogFileName + "_" + i.ToString() + ".csv";
                                logPath = Path.Combine(baseLogPath, currentTestLogFileName);
                                logger = new CSV_Logger<TSRecord>(logPath);

                                ts.Logger = logger;
                                bestFoundSpecimen = ts.RunTabuSearch();
                                results.Add(bestFoundSpecimen.Score());
                                logger.Dispose();
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
                            if (avgScore > bestAvgScore)
                            {
                                bestAvgScore = avgScore;
                                bestMaxScore = maxScore;
                                bestMinScore = minScore;
                                beststandardDeviation = standardDeviation;
                                bestConfigName = currentLogFileName;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"\nFile: {fileName} TS");
            Console.WriteLine($"Max Score\t\tMin Score\t\tAVG Score\t\tStd deviation");
            Console.WriteLine($"{bestMaxScore}\t{bestMinScore}\t{bestAvgScore}\t{beststandardDeviation}");
            Console.WriteLine(bestConfigName);
        }
    }
}
