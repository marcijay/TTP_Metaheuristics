using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Logger;
using TTP_EA.Specimen;
using TTP_EA.TS.Neighbors;

namespace TTP_EA.SA
{
    public  class SimulatedAnnealing
    {
        public TTP_Data ProblemData { get; set; }
        public ISpecimenCreator Creator { get; }
        public int Iterations { get; set; }
        public int NeighborhoodSize { get; set; }
        public double StartTemperature { get; set; }
        public double TargetTemperature { get; set; }
        public double CoolingRatio { get; set; }

        public NeighborhoodFinder NeighborhoodFinder { get; set; }

        public CSV_Logger<TSRecord> Logger { get; set; }

        public SimulatedAnnealing(TTP_Data problemData, ISpecimenCreator creator, int iterations, int neighborhoodSize, double startTemperature, double targetTemperature, double coolingRatio, NeighborhoodFinder neighborhoodFinder, CSV_Logger<TSRecord> logger)
        {
            ProblemData = problemData;
            Creator = creator;
            Iterations = iterations;
            NeighborhoodSize = neighborhoodSize;
            NeighborhoodFinder = neighborhoodFinder;
            Logger = logger;

            StartTemperature = startTemperature;
            TargetTemperature = targetTemperature;
            CoolingRatio = coolingRatio;
        }

        private TTP_Specimen CreateSpecimen()
        {
            TTP_Specimen specimen = new TTP_Specimen(ProblemData, Creator);
            specimen.Init();

            //Console.WriteLine("\nIteration\t\tBestScore\t\tAverageScore\t\tCurrentScore\t\tWorstScore");

            return specimen;
        }

        public TTP_Specimen RunSimulatedAnnealing()
        {
            TTP_Specimen currentSpecimen = CreateSpecimen();
            var currentScore = currentSpecimen.Score();
            var bestScore = currentScore;
            var worstScore = currentScore;

            TTP_Specimen bestSpecimen = currentSpecimen;

            var currentTemperature = StartTemperature;
            int lastTemparatureChangeIteration = 0;
            int iteration = 0;

            Random random = new Random();

            while(iteration < Iterations) // && iteration - lastTemparatureChangeIteration < 500)// && currentTemperature > TargetTemperature)
            {
                var neighborhood = NeighborhoodFinder.FindNeighborhood(currentSpecimen, NeighborhoodSize);

                foreach (var neighbor in neighborhood)
                {
                    if (currentScore < neighbor.Score())
                    {
                        currentScore = neighbor.Score();
                        currentSpecimen = neighbor;
                    }
                    else if (random.NextDouble() < Math.Exp((neighbor.Score() - currentScore) / currentTemperature))
                    {
                        currentScore = neighbor.Score();
                        currentSpecimen = neighbor;
                    }
                }
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestSpecimen = currentSpecimen;
                }
                else if (currentScore < worstScore)
                {
                    worstScore = currentScore;
                }

                //var bestNeighbor = neighborhood.MaxBy(n => n.Score());
                //if(currentScore < bestNeighbor.Score() || random.NextDouble() < Math.Exp((bestNeighbor.Score() - currentScore) / currentTemperature))
                //{
                //    currentScore = bestNeighbor.Score();
                //    currentSpecimen = bestNeighbor;

                //    if (currentScore > bestScore)
                //    {
                //        bestScore = currentScore;
                //        bestSpecimen = bestNeighbor;
                //    }
                //    else if (currentScore < worstScore)
                //    {
                //        worstScore = currentScore;
                //    }
                //}

                var record = new TSRecord(iteration, bestScore, neighborhood.Average(n => n.Score()), currentScore, worstScore);
                //if (iteration == 0 || iteration == Iterations - 1)
                //{
                //    Console.WriteLine($"{record.Generation}\t\t{record.BestSpecimenScore}\t\t{record.AverageSpecimenScore}\t\t{record.CurrentSpecimenScore}\t\t{record.WorstSpecimenScore}");
                //}
                //Console.WriteLine($"{record.Generation}\t\t{record.BestSpecimenScore}\t\t{record.AverageSpecimenScore}\t\t{record.CurrentSpecimenScore}\t\t{record.WorstSpecimenScore}");
                Logger.Log(record);

                if(currentTemperature > TargetTemperature)
                {
                    currentTemperature *= CoolingRatio;
                    lastTemparatureChangeIteration = iteration;
                }

                //currentTemperature = currentTemperature * CoolingRatio;
                iteration++;
            }
            //Console.WriteLine($"Last iteration: {iteration}");

            return bestSpecimen;
        }
    }
}
