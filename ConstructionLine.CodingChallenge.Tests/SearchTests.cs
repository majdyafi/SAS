using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstructionLine.CodingChallenge.Tests
{
    [TestFixture]
    public class SearchTests
    {
        List<Shirt> _shirts;
        SearchEngine _se;

        public SearchTests()
        {
            _shirts = new List<Shirt>
            {
                new Shirt(Guid.Parse("DC1E8E26-2D52-471E-A7CA-2E2A33A5E074"), "Red - Small", Size.Small, Color.Red),
                new Shirt(Guid.Parse("5AA962ED-2D54-41D2-B0F7-316CA4AB8843"), "Red - Medium", Size.Medium, Color.Red),
                new Shirt(Guid.Parse("FE735FD2-096B-4310-9337-84F1CE835C0F"), "Black - Medium", Size.Medium, Color.Black),
                new Shirt(Guid.Parse("3D288AAD-89EE-45FB-92D4-0C504BEED567"), "Blue - Medium", Size.Medium, Color.Blue),
                new Shirt(Guid.Parse("C68C5789-8926-42F2-AD97-ABECBCCD4DA9"), "Blue - Large", Size.Large, Color.Blue),
            };

            _se = new SearchEngine(_shirts);
        }

        [Test]
        [TestCase(nameof(Color.Black), 1)]
        [TestCase(nameof(Color.Red), 2)]
        [TestCase(nameof(Color.Blue), 2)]
        public void SearchExistingByColour_Returns_ColourResults(string colour, int expected)
        {
            //Assign
            SearchOptions SO = new SearchOptions { Colors = Color.All.Where(x => x.Name == colour).ToList() };

            //Action
            var results = _se.Search(SO);

            //Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(expected, results.ColorCounts.Where(x => x.Color.Name == colour).First().Count);
            Assert.AreEqual(colour, results.ColorCounts.Where(x => x.Count > 0).First().Color.Name);
        }


        [Test]
        [TestCase(nameof(Color.Yellow))]
        [TestCase(nameof(Color.White))]
        public void SearchNonExistingByColour_Returns_ZeroColourResults(string colour)
        {
            //Assign
            SearchOptions SO = new SearchOptions { Colors = Color.All.Where(x => x.Name == colour).ToList() };

            //Action
            var results = _se.Search(SO);

            //Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.ColorCounts.Where(x => x.Color.Name == colour).First().Count);
        }


        [Test]
        [TestCase(nameof(Size.Small), 1)]
        [TestCase(nameof(Size.Medium), 3)]
        public void SearchNonExistingBySize_Returns_ZeroColourResults(string size, int expected)
        {
            //Assign
            SearchOptions SO = new SearchOptions { Sizes = Size.All.Where(x => x.Name == size).ToList() };

            //Action
            var results = _se.Search(SO);

            //Assert
            Assert.AreEqual(expected, results.SizeCounts.Where(x => x.Size.Name == size).First().Count);
        }

        [Test]
        [TestCase(nameof(Size.Medium), nameof(Color.Red))]
        public void AsserMatchedShirts_Returns_Shirts(string size, string colour)
        {
            //Assign
            SearchOptions SO = new SearchOptions
            {
                Sizes = Size.All.Where(x => x.Name == size).ToList(),
                Colors = Color.All.Where(x => x.Name == colour).ToList()
            };

            //Action
            var results = _se.Search(SO);

            //Assert
            Assert.IsTrue(results.Shirts.Where(x => x.Color.Name == colour || x.Size.Name == size).Count() > 0);
        }

        [Test]
        [TestCase("DC1E8E26-2D52-471E-A7CA-2E2A33A5E074")]
        [TestCase("5AA962ED-2D54-41D2-B0F7-316CA4AB8843")]
        [TestCase("FE735FD2-096B-4310-9337-84F1CE835C0F")]
        [TestCase("3D288AAD-89EE-45FB-92D4-0C504BEED567")]
        public void AsserMatchedShirts_Returns_MatcgedGuids(string guid)
        {
            //Assign
            SearchOptions SO = new SearchOptions
            {
                Sizes = new List<Size> { Size.Medium },
                Colors = new List<Color> { Color.Red }
            };

            //Action
            var results = _se.Search(SO);

            //Assert
            var guids = results.Shirts.Select(x => x.Id).ToList();
            Assert.IsTrue(guids.Contains(Guid.Parse(guid)));
        }

        [Test]
        [TestCase("C68C5789-8926-42F2-AD97-ABECBCCD4DA9")]
        public void AsserNonMatchedShirts_Returns_MatcgedGuids(string guid)
        {
            //Assign
            SearchOptions SO = new SearchOptions
            {
                Sizes = new List<Size> { Size.Medium },
                Colors = new List<Color> { Color.Red }
            };

            //Action
            var results = _se.Search(SO);

            //Assert
            var guids = results.Shirts.Select(x => x.Id).ToList();
            Assert.IsFalse(guids.Contains(Guid.Parse(guid)));
        }
    }
}
