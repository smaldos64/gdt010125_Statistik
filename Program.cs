

// Updated C# program to support mixed-format interval input with optional frequencies

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Statistik
{

    class Interval
    {
        public double Lower { get; set; }
        public double Upper { get; set; }
        public long Frequency { get; set; } = 0;
    }

    class Program
    {
        static void Main()
        {
            string inputPath = string.Empty;
            string groupedOutputPath = string.Empty;
            string ungroupedOutputPath = string.Empty;

            List<double> observations = new();
            List<Interval> intervals = new();
            bool leftClosed = true;

            Console.Clear();
            Console.Write("Indtast navn på datafil med observationer : ");
            inputPath = Console.ReadLine();
            Console.WriteLine("");
            Console.WriteLine("");

            foreach (var line in File.ReadAllLines(inputPath))
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("INTERVALS:", StringComparison.OrdinalIgnoreCase))
                {
                    if (trimmed.ToUpper().Contains("LEFT_OPEN"))
                    {
                        leftClosed = false;
                    }
                    else
                    {
                        if (trimmed.ToUpper().Contains("LEFT_CLOSED"))
                        {
                            leftClosed = true;
                        }
                    }
                    continue;
                }

                var parts = trimmed.Split('-');
                if (parts.Length >= 2 && double.TryParse(parts[0], out double low) && double.TryParse(parts[1], out double high))
                {
                    long freq = 0;
                    if (parts.Length == 3 && long.TryParse(parts[2], out long explicitFreq))
                    {
                        freq = explicitFreq;
                    }
                    intervals.Add(new Interval { Lower = low, Upper = high, Frequency = freq });
                }
                else if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    observations.Add(val);
                }
            }

            bool hasGrouped = intervals.Any();
            bool hasUngrouped = observations.Any();

            if (hasGrouped)
            {
                groupedOutputPath = inputPath.Split('.')[0];
                groupedOutputPath = groupedOutputPath + "_grouped.out";

                // Count observations into intervals
                foreach (var obs in observations)
                {
                    bool matched = false;
                    foreach (var interval in intervals)
                    {
                        bool inInterval = leftClosed
                            ? obs >= interval.Lower && obs < interval.Upper
                            : obs > interval.Lower && obs <= interval.Upper;

                        if (inInterval)
                        {
                            interval.Frequency++;
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        Console.WriteLine($"Warning: Observation {obs} does not fit any interval");
                    }
                }
                WriteGroupedOutput(intervals, groupedOutputPath);
            }

            if (hasUngrouped)
            {
                ungroupedOutputPath = inputPath.Split('.')[0];
                ungroupedOutputPath = ungroupedOutputPath + "_ungrouped.out";
                WriteUngroupedOutput(observations, ungroupedOutputPath);
            }

            Console.ReadLine();
        }

        static void WriteGroupedOutput(List<Interval> intervals, string path)
        {
            intervals = intervals.OrderBy(i => i.Lower).ToList();
            var output = new List<string> { "Grupperet Data Tabel:" };
            long totalAntalObservationer = intervals.Sum(i => i.Frequency);
            //long totalFreq = intervals.Sum(i => i.Frequency);
            double mean = intervals.Sum(i => ((i.Lower + i.Upper) / 2.0) * i.Frequency) / totalAntalObservationer;
            //long cumulativeFreq = 0;
            long summeretHyppighed = 0;
            double summeretFrekvens = 0;
            //double sumFx2 = 0;
            double intervalMidtpunktGangeHyppighedSamlet = 0;
            //long maxFreq = intervals.Max(i => i.Frequency);
            long maxGangetypeIntervalForekommer = intervals.Max(i => i.Frequency);

            // 
            //int summeretHyppighed = 0;
            //double summeretFrekvens = 0;
            //double ObservationHyppighedProduktSamlet = 0;
            //long totalAntalObservationer = observations.Count;

            //foreach (var kvp in freqDict.OrderBy(x => x.Key))
            //{
            //    double observation = kvp.Key;
            //    int hyppighed = kvp.Value;
            //    summeretHyppighed += hyppighed;
            //    double frekvens = hyppighed / totalAntalObservationer;
            //    summeretFrekvens += frekvens;
            //    double observationHyppighedProdukt = observation * hyppighed;
            //    ObservationHyppighedProduktSamlet += observationHyppighedProdukt;
            //    output.Add($"\t{observation}\t\t{hyppighed}\t\t{summeretHyppighed}\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{observationHyppighedProdukt}");
            //}
            //
            //output.Add("Interval\tFrequency\tCumFreq\tMidpoint\tFx\tFx^2");
            output.Add("Interval\t\tMidtpunkt\tHyppighed h(x)\tSummeret Hyppighed H(x)\tFrekvens f(x)\tSummeret Frekvns F(x)\tObservation * hyppighed");

            foreach (var i in intervals)
            {
                summeretHyppighed += i.Frequency;
                double mid = (i.Lower + i.Upper) / 2.0;
                //double fx = mid * i.Frequency;
                //double frekevns = mid * i.Frequency;
                double frekvens = (double)i.Frequency / totalAntalObservationer;
                summeretFrekvens += frekvens;
                //double fx2 = fx * mid;
                double intervalMidtpunktGangeHyppighed = frekvens * mid;
                //sumFx2 += fx2;
                intervalMidtpunktGangeHyppighedSamlet += intervalMidtpunktGangeHyppighed;
                //output.Add($"[{i.Lower}, {i.Upper})\t{i.Frequency}\t{cumulativeFreq}\t{mid}\t{fx}\t{fx2}");
                output.Add($"[{i.Lower:F2}, {i.Upper:F2})\t{mid:F2}\t\t{i.Frequency}\t\t{summeretHyppighed}\t\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{intervalMidtpunktGangeHyppighed:F2}");
            }

            //double variance = (sumFx2 / totalFreq) - (mean * mean);
            double variance = (intervalMidtpunktGangeHyppighedSamlet / totalAntalObservationer) - (mean * mean);
            double stdDev = Math.Sqrt(variance);
            double min = intervals.First(i => i.Frequency > 0).Lower;
            double max = intervals.Last(i => i.Frequency > 0).Upper;
            double range = max - min;

            //double modeLower = intervals.First(i => i.Frequency == maxFreq).Lower;
            //double modeUpper = intervals.First(i => i.Frequency == maxFreq).Upper;
            double modeLower = intervals.First(i => i.Frequency == maxGangetypeIntervalForekommer).Lower;
            double modeUpper = intervals.First(i => i.Frequency == maxGangetypeIntervalForekommer).Upper;

            //double Q1 = ComputeGroupedQuartile(intervals, totalFreq, 0.25);
            //double Q2 = ComputeGroupedQuartile(intervals, totalFreq, 0.50);
            //double Q3 = ComputeGroupedQuartile(intervals, totalFreq, 0.75);

            double Q1 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.25);
            double Q2 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.50);
            double Q3 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.75);

            output.Add("\nGrouped Statistics:");
            output.Add($"Total: {totalAntalObservationer}, Mean: {mean:F2}, StdDev: {stdDev:F2}, Variance: {variance:F2}");
            output.Add($"Min Interval: {min}, Max Interval: {max}, Range: {range}");
            output.Add($"Mode Interval: [{modeLower}, {modeUpper})");
            output.Add($"Quartiles: Q1={Q1:F2}, Q2(Median)={Q2:F2}, Q3={Q3:F2}");

            File.WriteAllLines(path, output);
            output.ForEach(Console.WriteLine);
        }

        static double ComputeGroupedQuartile(List<Interval> intervals, long totalFrequency, double fraction)
        {
            double cumulative = 0;
            foreach (var interval in intervals)
            {
                cumulative += interval.Frequency;
                if (cumulative >= fraction * totalFrequency)
                {
                    double cumulativeBefore = cumulative - interval.Frequency;
                    double h = interval.Upper - interval.Lower;
                    double result = interval.Lower + ((fraction * totalFrequency - cumulativeBefore) / interval.Frequency) * h;
                    return result;
                }
            }
            return double.NaN;
        }

        static void WriteUngroupedOutput(List<double> observations, string path)
        {
            var ordered = observations.OrderBy(x => x).ToList();
            var freqDict = ordered.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var output = new List<string> { "Ugrupperet Data Tabel:" };
            output.Add("Observation\tHyppighed h(x)\tSummeret Hyppighed H(x)\tFrekvens f(x)\tSummeret Frekvns F(x)\tObservation * hyppighed");

            long summeretHyppighed = 0;
            double summeretFrekvens = 0;
            double ObservationHyppighedProduktSamlet = 0;
            long totalAntalObservationer = observations.Count;
            
            foreach (var kvp in freqDict.OrderBy(x => x.Key))
            {
                double observation = kvp.Key;
                long hyppighed = kvp.Value;
                summeretHyppighed += hyppighed;
                double frekvens = (double)hyppighed / totalAntalObservationer;
                summeretFrekvens += frekvens;
                double observationHyppighedProdukt = observation * hyppighed;
                ObservationHyppighedProduktSamlet += observationHyppighedProdukt;
                output.Add($"\t{observation}\t\t{hyppighed}\t\t{summeretHyppighed}\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{observationHyppighedProdukt}");
            }

            double mean = ObservationHyppighedProduktSamlet / totalAntalObservationer;
            double variance = observations.Sum(x => Math.Pow(x - mean, 2)) / totalAntalObservationer;
            double stdDev = Math.Sqrt(variance);
            double min = ordered.First();
            double max = ordered.Last();
            double range = max - min;

            var typeTal = freqDict.Where(x => x.Value == freqDict.Values.Max()).Select(x => x.Key);
            double q1 = Percentile(ordered, 1);
            double q2 = Percentile(ordered, 2);
            double q3 = Percentile(ordered, 3);

            output.Add("\nUgrupperet Statistik:");
            output.Add($"Antal Observationer * Hyppighed samlet : {ObservationHyppighedProduktSamlet}");
            output.Add($"Antal Observationer                    : {totalAntalObservationer}");
            output.Add($"Middelværdi                            : {ObservationHyppighedProduktSamlet} / {totalAntalObservationer} = {mean:F2}");
            output.Add($"Varians                                : {variance:F2}");
            output.Add($"Standardafvigelse/Spredning            : {stdDev:F2}");
            output.Add("");
            output.Add($"Minimums værdi                         : {min:F2}");
            output.Add($"Maksimums værdi                        : {max:F2}");
            output.Add($"Variationsbredde                       : {range:F2}");
            output.Add($"Antal gange Typetal forekommer         : {freqDict.Values.Max()}");
            output.Add($"Typetal                                : {string.Join(", ", typeTal.Select(x => x.ToString("F2")))}");
            output.Add("");
            output.Add("Kvartilsæt");
            output.Add("----------");
            output.Add($"Nedre Kvartil                          : {q1:F2}");
            output.Add($"Median                                 : {q2:F2}");
            output.Add($"Øvre Kvartil                           : {q3:F2}");

            File.WriteAllLines(path, output);
            output.ForEach(Console.WriteLine);
        }

        //static double Percentile(List<double> sortedList, double percentile)
        //{
        //    int N = sortedList.Count;
        //    double n = (N - 1) * percentile / 100.0 + 1;
        //    if (n == 1d) return sortedList[0];
        //    else if (n == N) return sortedList[N - 1];
        //    else
        //    {
        //        int k = (int)n;
        //        double d = n - k;
        //        return sortedList[k - 1] + d * (sortedList[k] - sortedList[k - 1]);
        //    }
        //}

        static double Percentile(List<double> sortedList, int fractileValue)
        {
            double value = 0;
            
            value = sortedList.OrderBy(x => x)
                    .Skip(sortedList.Count() * fractileValue / 4 -
                            (sortedList.Count() * fractileValue % 4 == 0 ? 1 : 0))
                    .Take(sortedList.Count() * fractileValue % 4 == 0 ? 2 : 1)
                    .Average();

            return (value);
        }
    }
}