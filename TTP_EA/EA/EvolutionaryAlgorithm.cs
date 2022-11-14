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
using TTP_EA.Specimen;
using TTP_EA.Specimen.Creators;

namespace TTP_EA.EA
{
    public class EvolutionaryAlgorithm
    {
        IList<TTP_Specimen> CurrentPopulation { get; set; }
        IMutator Mutator { get; }
        ICrossover Crossover { get; }
        ISelector Selector { get; }
        ISpecimenCreator Creator { get; }
        public uint PopulationSize { get; set; }
        public uint Generation { get; set; }
        public uint GenerationsToCheck { get; set; }
        public TTP_Data ProblemData { get; set; }
        public CSV_Logger<EARecord> Logger { get; set; }
        
        public EvolutionaryAlgorithm(TTP_Data problemData, IMutator mutator, ICrossover crossover, ISelector selector, ISpecimenCreator creator, uint populationSize, uint generationsToCheck, CSV_Logger<EARecord> logger)
        {
            Mutator = mutator;
            Crossover = crossover;
            Selector = selector;
            PopulationSize = populationSize;
            GenerationsToCheck = generationsToCheck;
            ProblemData = problemData;
            Creator = creator;
            Logger = logger;

            CurrentPopulation = new List<TTP_Specimen>();
        }

        public void InitializePopulation()
        {
            CurrentPopulation.Clear();
            for (int i = 0; i < PopulationSize; i++)
            {
                TTP_Specimen specimen = new TTP_Specimen(ProblemData, Creator);
                specimen.Init();
                specimen.Score();
                CurrentPopulation.Add(specimen);
            }
            Generation = 0;

            //Console.WriteLine("\nGeneration\t\tMaxScore\t\tMinScore\t\tAverageScore");

            (double maxScore, double minScore, double averageScore) = EvaluatePopulation();

            Logger.Log(new EARecord((int)Generation, maxScore, minScore, averageScore));

            //Console.WriteLine($"{Generation}\t\t{maxScore}\t\t{minScore}\t\t{averageScore}");
        }

        public void NewPopulation()
        {
            var selectedSpecimens = Selector.Select(CurrentPopulation);
            var crossesSpecimens = Crossover.Crossover(selectedSpecimens);
            var mutatedSpecimens = Mutator.Mutate(crossesSpecimens);

            Generation += 1;
            CurrentPopulation = mutatedSpecimens;
            (double maxScore, double minScore, double averageScore) = EvaluatePopulation();
            Logger.Log(new EARecord((int)Generation, maxScore, minScore, averageScore));
            //Console.WriteLine($"{Generation}\t\t{maxScore}\t\t{minScore}\t\t{averageScore}");
        }

        private (double, double, double) EvaluatePopulation()
        {
            double scoreSum = 0;
            double minScore = double.MaxValue;
            double maxScore = double.MinValue;

            foreach(var specimen in CurrentPopulation)
            {
                GreedyCreator.FillGreedyKnapsack(specimen);
                double score = specimen.Score();
                scoreSum += score;
                if(score < minScore)
                {
                    minScore = score;
                }
                if(score > maxScore)
                {
                    maxScore = score;
                }
            }
            double averageScore = scoreSum / CurrentPopulation.Count;

            return (maxScore, minScore, averageScore);
        }

        public TTP_Specimen Execute()
        {
            InitializePopulation();

            while(Generation < GenerationsToCheck)
            {
                NewPopulation();
            }

            return CurrentPopulation.MaxBy(s => s.Score());
        }
    }
}
