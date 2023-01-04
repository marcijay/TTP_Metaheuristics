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
using TTP_EA.Specimen.Factories;

namespace TTP_EA.EA
{
    public class EvolutionaryAlgorithmTwoMutations<T> : IEvolutionaryAlgorithm<T> where T : ITTPSpecimen<T>
    {
        IList<T> CurrentPopulation { get; set; }
        IMutator<T> MutatorOne { get; }
        IMutator<T> MutatorTwo { get; }
        ICrossover<T> Crossover { get; }
        ISelector<T> Selector { get; }
        ISpecimenFactory<T> Factory { get; }

        readonly Random random;
        public uint PopulationSize { get; set; }
        public uint Generation { get; set; }
        public uint GenerationsToCheck { get; set; }
        public float TypeOneMutationProbability { get; set; }
        public TTP_Data ProblemData { get; set; }
        public CSV_Logger<EARecord> Logger { get; set; }

        public EvolutionaryAlgorithmTwoMutations(TTP_Data problemData, IMutator<T> mutatorOne, IMutator<T> mutatorTwo, ICrossover<T> crossover, ISelector<T> selector, ISpecimenFactory<T> factory,float mutationOneProbability, uint populationSize, uint generationsToCheck, CSV_Logger<EARecord> logger)
        {
            MutatorOne = mutatorOne;
            MutatorTwo = mutatorTwo;
            Crossover = crossover;
            Selector = selector;
            TypeOneMutationProbability = 1 - mutationOneProbability;
            PopulationSize = populationSize;
            GenerationsToCheck = generationsToCheck;
            ProblemData = problemData;
            Factory = factory;
            Logger = logger;

            random = new Random();
            CurrentPopulation = new List<T>();
        }

        public void InitializePopulation()
        {
            CurrentPopulation.Clear();

            for (int i = 0; i < PopulationSize; i++)
            {
                T specimen = Factory.ProduceSpecimen(i);
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

            foreach(var specimen in crossesSpecimens)
            {
                if(TypeOneMutationProbability <= random.NextDouble())
                {
                    MutatorOne.MutateSingleSpecimen(specimen);
                }
                else
                {
                    MutatorTwo.MutateSingleSpecimen(specimen);
                }
            }
            //IList<T> mutatedSpecimens;
            //if (TypeOneMutationProbability <= random.NextDouble())
            //{
            //    mutatedSpecimens = MutatorOne.Mutate(crossesSpecimens);
            //}
            //else
            //{
            //    mutatedSpecimens = MutatorTwo.Mutate(crossesSpecimens);
            //}

            Generation += 1;
            CurrentPopulation = crossesSpecimens;
            (double maxScore, double minScore, double averageScore) = EvaluatePopulation();
            Logger.Log(new EARecord((int)Generation, maxScore, minScore, averageScore));
            //Console.WriteLine($"{Generation}\t\t{maxScore}\t\t{minScore}\t\t{averageScore}");
        }

        private (double, double, double) EvaluatePopulation()
        {
            double scoreSum = 0;
            double minScore = double.MaxValue;
            double maxScore = double.MinValue;

            int genderTrue = 0;
            int genderFalse = 0;

            foreach (var specimen in CurrentPopulation)
            {
                if(specimen is TTP_Specimen)
                {
                    GreedyCreator<T>.FillGreedyKnapsack(specimen);
                }
                else if(specimen is TTP_Specimen_Gendered)
                {
                    if ((specimen as TTP_Specimen_Gendered).Gender == true)
                    {
                        GreedyCreator<T>.FillGreedyKnapsack(specimen);
                        genderTrue++;
                    }
                    else
                    {
                        GreedyCreator<T>.FillGreedyKnapsackEasy(specimen);
                        genderFalse++;
                    }
                }
                //GreedyCreator<T>.FillGreedyKnapsack(specimen);
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

            if (Generation == GenerationsToCheck - 1 || Generation == 0)
            {
                Console.WriteLine($"Generation: {Generation}, Genders - True: {genderTrue}, False: {genderFalse}");
            }

            return (maxScore, minScore, averageScore);
        }

        public T Execute()
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
