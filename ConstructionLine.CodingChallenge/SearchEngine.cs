using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;
        private readonly ConcurrentDictionary<Guid, List<Shirt>> shirtsByColorDictionary, shirtsBySizeDictionary;

        /// <summary>
        /// Create an instance of the Search Engine
        /// </summary>
        /// <param name="shirts">Complete list of shirst</param>
        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;

            shirtsByColorDictionary =
                new ConcurrentDictionary<Guid, List<Shirt>>(_shirts.GroupBy(x => x.Color.Id)
                .ToDictionary(g => g.Key, g => g.ToList()));

            shirtsBySizeDictionary =
                new ConcurrentDictionary<Guid, List<Shirt>>(_shirts.GroupBy(x => x.Size.Id)
                .ToDictionary(g => g.Key, g => g.ToList()));

        }

        /// <summary>
        /// Search T-Shirts by Colour and Size
        /// </summary>
        /// <param name="options">T-Shirt Option Lists of Colours and Sizes</param>
        /// <returns>Lists of matched T-Shirsts, Summary of Colours and Summary of Sizes yield by the search engine</returns>
        public SearchResults Search(SearchOptions options)
        {

            var searchResults = Task.Run(async () => await SearchAsync(options));

            searchResults.Wait();

            return searchResults.Result;
        }

        /// <summary>
        /// Attempts to speed up the search by utilising TPL 
        /// </summary>
        /// <param name="options">T-Shirt Option Lists of Colours and Sizes</param>
        /// <returns>Lists of matched T-Shirsts, Summary of Colours and Summary of Sizes yield by the search engine</returns>
        private async Task<SearchResults> SearchAsync(SearchOptions options)
        {
            Task<List<Shirt>> GetShirtsByColourOptionsTask = GetShirtsByColourOptions(options);
            Task<List<Shirt>> GetShirtsBySizeOptionsTask = GetShirtsBySizeOptions(options);

            await Task.WhenAll(GetShirtsByColourOptionsTask, GetShirtsBySizeOptionsTask)
                .ConfigureAwait(false);

            if (GetShirtsByColourOptionsTask.IsFaulted || GetShirtsBySizeOptionsTask.IsFaulted)
            {
                throw new Exception($"{GetShirtsByColourOptionsTask.Exception.Message} - {GetShirtsBySizeOptionsTask.Exception.Message}");
            }

            List<Shirt> shirts = await MergeResults(GetShirtsByColourOptionsTask, GetShirtsBySizeOptionsTask);

            var ColorCounts = await ColorCountsResult(shirts, options);

            var SizeCounts = await SizeCountsResult(shirts, options);

            return new SearchResults
            {
                ColorCounts = ColorCounts,
                SizeCounts = SizeCounts,
                Shirts = shirts
            };

        }

        /// <summary>
        /// Merges the results returned by searching by colour and results returned by searching by size
        /// </summary>
        /// <param name="GetShirtsByColourOptionsTask">Results of matched T-Shirst by colour</param>
        /// <param name="GetShirtsBySizeOptionsTask">Results of matched T-Shirst by size</param>
        /// <returns></returns>
        private static async Task<List<Shirt>> MergeResults(Task<List<Shirt>> GetShirtsByColourOptionsTask, Task<List<Shirt>> GetShirtsBySizeOptionsTask)
        {
            List<Shirt> shirts = GetShirtsByColourOptionsTask
                .Result
                .Union(GetShirtsBySizeOptionsTask.Result)
                .ToList();
            
            return shirts;
        }

        /// <summary>
        /// Get T-Shirst that have the size stipulated by size
        /// </summary>
        /// <param name="GetShirtsBySizeOptionsTask">near BigO(1) dictionary look-up for TShirst grouped by size</param>
        /// <param name="options">indicates the sizes to look for</param>
        /// <returns>matched T-shirst by size</returns>
        private async Task<List<SizeCount>> SizeCountsResult(List<Shirt> GetShirtsBySizeOptionsTask, SearchOptions options)
        {
            var SizeCounts = new List<SizeCount>();
            

            await Task.Run(() =>
            {
                Parallel.ForEach(Size.All, (size) =>
                {
                    SizeCounts.Add(new SizeCount
                    {
                        Size = size,
                        Count = options.Sizes.Contains(size) ? 
                            GetShirtsBySizeOptionsTask.Count(x => x.Size == size) 
                            : 0
                    });
                });
            });

            return SizeCounts;
        }

        /// <summary>
        /// Get summary of T-Shirst that have the size stipulated by colour
        /// </summary>
        /// <param name="GetShirtsBySizeOptionsTask">near BigO(1) dictionary look-up for TShirst grouped by colour</param>
        /// <param name="options">indicates the colours to look for</param>
        /// <returns>summary of matched T-shirst by size</returns>
        private async Task<List<ColorCount>> ColorCountsResult(List<Shirt> GetShirtsByColourOptionsTask, SearchOptions options)
        {
            var ColorCounts = new List<ColorCount>();

            await Task.Run(() =>
            {
                Parallel.ForEach(Color.All, (colour) =>
                {
                    {
                        ColorCounts.Add(new ColorCount
                        {
                            Color = colour,
                            Count = options.Colors.Contains(colour) ?  
                                GetShirtsByColourOptionsTask.Count(x => x.Color == colour) 
                                : 0
                        });
                    }
                });
            });

            return ColorCounts;
        }

        /// <summary>
        /// Get summary of T-Shirst that have the size stipulated by colour
        /// </summary>
        /// <param name="GetShirtsBySizeOptionsTask">near BigO(1) dictionary look-up for TShirst grouped by colour</param>
        /// <param name="options">indicates the colours to look for</param>
        /// <returns>summary of matched T-shirst by size</returns>
        private async Task<List<Shirt>> GetShirtsByColourOptions(SearchOptions searchOptions)
        {

            List<Shirt> results = new List<Shirt>();

            foreach(var color in searchOptions.Colors) { 
                if (shirtsByColorDictionary.TryGetValue(color.Id, out var matchedByColor)) 
                { 
                    results.AddRange(matchedByColor); 
                }
            }

            return results;
        }

        /// <summary>
        /// Get T-Shirst that have the size stipulated by size
        /// </summary>
        /// <param name="GetShirtsBySizeOptionsTask">near BigO(1) dictionary look-up for TShirst grouped by size</param>
        /// <param name="options">indicates the sizes to look for</param>
        /// <returns>matched T-shirst by size</returns>
        private async Task<List<Shirt>> GetShirtsBySizeOptions(SearchOptions searchOptions)
        {
            List<Shirt> results = new List<Shirt>();

            foreach (var size in searchOptions.Sizes)
            {
                if (shirtsBySizeDictionary.TryGetValue(size.Id, out var matchedBySize))
                {
                    results.AddRange(matchedBySize);
                }
            }

            return results;
        }
    }
}