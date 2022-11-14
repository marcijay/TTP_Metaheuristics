using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Logger;
using TTP_EA.Specimen;
using TTP_EA.TS.Neighbors;

namespace TTP_EA.TS
{
    public class TabuSearch
    {
        public TTP_Data ProblemData { get; set; }
        public ISpecimenCreator Creator { get; }
        public int Iterations { get; set; }
        public int NeighborhoodSize { get; set; }
        public int TabuSize { get; set; }

        public NeighborhoodFinder NeighborhoodFinder { get; set; }

        private List<TTP_Specimen> Tabu { get; set; }
        private HashSet<TTP_Specimen> TabuHash { get; set; }

        public CSV_Logger<TSRecord> Logger { get; set; }

        public TabuSearch(TTP_Data problemData, ISpecimenCreator creator, int iterations, int neighborhoodSize, int tabuSize, NeighborhoodFinder neighborhoodFinder, CSV_Logger<TSRecord> logger)
        {
            ProblemData = problemData;
            Creator = creator;
            Iterations = iterations;
            NeighborhoodSize = neighborhoodSize;
            TabuSize = tabuSize;
            NeighborhoodFinder = neighborhoodFinder;
            Logger = logger;

            Tabu = new List<TTP_Specimen>();
            TabuHash = new HashSet<TTP_Specimen>();
        }

        private TTP_Specimen CreateSpecimen()
        {
            TTP_Specimen specimen = new TTP_Specimen(ProblemData, Creator);
            specimen.Init();

            //Console.WriteLine("\nIteration\t\tBestScore\t\tAverageScore\t\tCurrentScore\t\tWorstScore");

            return specimen;
        }

        public TTP_Specimen RunTabuSearch()
        {
            var specimen = CreateSpecimen();
            var bestSpecimen = specimen;
            var bestScore = bestSpecimen.Score();
            var iteration = 0;
            while (iteration < Iterations)
            {
                var neighborhood = NeighborhoodFinder.FindNeighborhood(specimen, NeighborhoodSize);
                var filteredNeighborhood = neighborhood.Where(n => !TabuList().Contains(n)).ToList();
                var bestNeighbor = filteredNeighborhood.MaxBy(n => n.Score());
                var worstNeighbor = filteredNeighborhood.MinBy(n => n.Score());
                if (bestNeighbor != null && worstNeighbor != null)
                {
                    var bestNeighborScore = bestNeighbor.Score();
                    var worstNeighborScore = worstNeighbor.Score();
                    if (bestNeighborScore > bestScore)
                    {
                        bestSpecimen = bestNeighbor;
                        bestScore = bestNeighborScore;
                    }
                    specimen = bestNeighbor;
                    var record = new TSRecord(iteration, bestScore, neighborhood.Average(n => n.Score()), specimen.Score(), worstNeighborScore);
                    //if(iteration == 0 || iteration == Iterations - 1)
                    //{
                    //    Console.WriteLine($"{record.Generation}\t\t{record.BestSpecimenScore}\t\t{record.AverageSpecimenScore}\t\t{record.CurrentSpecimenScore}\t\t{record.WorstSpecimenScore}");
                    //}
                    Logger.Log(record);
                    Tabu.Add(bestNeighbor);
                    TabuHash.Add(bestNeighbor);
                }
                if (Tabu.Count > TabuSize)
                {
                    TabuHash.Remove(Tabu.First());
                    Tabu.RemoveAt(0);
                }
                iteration++;
            }
            return bestSpecimen;
        }

        public IEnumerable<TTP_Specimen> TabuList()
        {
            return TabuHash;
        }
    }
}
