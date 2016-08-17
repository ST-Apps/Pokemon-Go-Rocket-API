using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;

namespace PokemonGo_UWP.Tests.Utils
{
    [TestClass]
    public class CollectionExtensionsTests
    {
        [TestMethod]
        public void UpdateWith_EmptyInitialCollection_WhenUpdatesComeIn_ThenOriginalCollectionContainsAllTheUpdates()
        {
            var initialCollection = new ObservableCollection<WrappedItem>();
            var updates = new ObservableCollection<Item> { new Item(1), new Item(2), new Item(3)};

            initialCollection.UpdateWith(updates, x => new WrappedItem(x), (x,y) => x.Id == y.Id  );

            Assert.AreEqual(3, initialCollection.Count);
            Assert.IsTrue(initialCollection.Any(x => x.Id == 1));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 2));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 3));
        }

        [TestMethod]
        public void UpdateWith_InitialCollectionWithOldItems_WhenAllNewUpdatesComeIn_ThenOriginalCollectionContainsTheUpdatesOnly()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };
            var updates = new ObservableCollection<Item> { new Item(4), new Item(5), new Item(6) };

            initialCollection.UpdateWith(updates, x => new WrappedItem(x), (x, y) => x.Id == y.Id);

            Assert.AreEqual(3, initialCollection.Count);
            Assert.IsTrue(initialCollection.Any(x => x.Id == 4));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 5));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 6));
        }

        [TestMethod]
        public void UpdateWith_InitialCollectionWithItems_WhenFewerNewItemsComeIn_ThenOriginalCollectionContainsTheNewItems()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };
            var updates = new ObservableCollection<Item> { new Item(4), new Item(5) };

            initialCollection.UpdateWith(updates, x => new WrappedItem(x), (x, y) => x.Id == y.Id);

            Assert.AreEqual(2, initialCollection.Count,0, string.Join(", ", initialCollection.Select(x => x.Id)));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 4));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 5));
        }

        [TestMethod]
        public void UpdateWith_InitialCollectionWithItems_WhenMoreNewItemsComeIn_ThenOriginalCollectionContainsTheNewItems()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };
            var updates = new ObservableCollection<Item> { new Item(4), new Item(5), new Item(6), new Item(7) };

            initialCollection.UpdateWith(updates, x => new WrappedItem(x), (x, y) => x.Id == y.Id);

            Assert.AreEqual(4, initialCollection.Count);
            Assert.IsTrue(initialCollection.Any(x => x.Id == 4));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 5));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 6));
            Assert.IsTrue(initialCollection.Any(x => x.Id == 7));
        }

        [TestMethod]
        public void UpdateWith_InitialCollectionWithItems_WhenNewAndUpdateItemsComeIn_ThenOriginalCollectionContainsTheUpdatedItems()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1, "item1")),
                new WrappedItem(new Item(2, "item2")),
                new WrappedItem(new Item(3, "item3"))
            };
            var updates = new ObservableCollection<Item>
            {
                new Item(1, "item1-updated"),
                new Item(2, "item2-updated"),
                new Item(3, "item3-updated"),
                new Item(4, "item4")
            };

            initialCollection.UpdateWith(updates, x => new WrappedItem(x), (x, y) => x.Id == y.Id);

            Assert.AreEqual(4, initialCollection.Count);
            Assert.AreEqual(initialCollection.Single(x => x.Id == 1).Data, "item1-updated");
            Assert.AreEqual(initialCollection.Single(x => x.Id == 2).Data, "item2-updated");
            Assert.AreEqual(initialCollection.Single(x => x.Id == 3).Data, "item3-updated");
            Assert.AreEqual(initialCollection.Single(x => x.Id == 4).Data, "item4");
        }

        [TestMethod]
        public void UpdateByIndexWith_EmptyInitialCollection_WhenUpdatesComeIn_ThenOriginalCollectionContainsAllTheUpdates()
        {
            var initialCollection = new ObservableCollection<WrappedItem>();
            var updates = new ObservableCollection<Item> { new Item(1), new Item(2), new Item(3) };

            initialCollection.UpdateByIndexWith(updates, x => new WrappedItem(x));

            Assert.AreEqual(3, initialCollection.Count);
            Assert.AreEqual(initialCollection[0].Id, 1);
            Assert.AreEqual(initialCollection[1].Id, 2);
            Assert.AreEqual(initialCollection[2].Id, 3);
        }

        [TestMethod]
        public void UpdateByIndexWith_InitialCollectionContainsItems_WhenUpdatesComeInWithSameItemsInDifferentOrder_ThenNewUpdateOrderIsHonored()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };

            var updates = new ObservableCollection<Item> { new Item(3), new Item(1), new Item(2) };

            initialCollection.UpdateByIndexWith(updates, x => new WrappedItem(x));

            Assert.AreEqual(3, initialCollection.Count);
            Assert.AreEqual(initialCollection[0].Id, 3);
            Assert.AreEqual(initialCollection[1].Id, 1);
            Assert.AreEqual(initialCollection[2].Id, 2);
        }

        [TestMethod]
        public void UpdateByIndexWith_InitialCollectionContainsItems_WhenFewerUpdatesComeIn_ThenNewUpdateOrderIsHonored()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };

            var updates = new ObservableCollection<Item> { new Item(3), new Item(1)};

            initialCollection.UpdateByIndexWith(updates, x => new WrappedItem(x));

            Assert.AreEqual(2, initialCollection.Count);
            Assert.AreEqual(initialCollection[0].Id, 3);
            Assert.AreEqual(initialCollection[1].Id, 1);
        }

        [TestMethod]
        public void UpdateByIndexWith_InitialCollectionContainsItems_WhenMoreUpdatesComeIn_ThenNewUpdateOrderIsHonored()
        {
            var initialCollection = new ObservableCollection<WrappedItem>
            {
                new WrappedItem(new Item(1)),
                new WrappedItem(new Item(2)),
                new WrappedItem(new Item(3))
            };

            var updates = new ObservableCollection<Item> { new Item(3), new Item(1), new Item(2), new Item(4), new Item(6), new Item(5) };

            initialCollection.UpdateByIndexWith(updates, x => new WrappedItem(x));

            Assert.AreEqual(6, initialCollection.Count);
            Assert.AreEqual(initialCollection[0].Id, 3);
            Assert.AreEqual(initialCollection[1].Id, 1);
            Assert.AreEqual(initialCollection[2].Id, 2);
            Assert.AreEqual(initialCollection[3].Id, 4);
            Assert.AreEqual(initialCollection[4].Id, 6);
            Assert.AreEqual(initialCollection[5].Id, 5);
        }

        private class Item
        {
            internal int Id { get; }
            internal string Value { get; }

            public Item(int id, string value = null)
            {
                Id = id;
                Value = value;
            }
        }

        private class WrappedItem : IUpdatable<Item>
        {
            private Item _item;

            public int Id => _item.Id;
            public string Data => _item.Value;

            public WrappedItem(Item i)
            {
                _item = i;
            }

            public void Update(Item update)
            {
                _item = update;
            }
        }
    }

}
