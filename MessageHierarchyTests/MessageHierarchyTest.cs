using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MessageHierarchyTests
{
    public class MessageHierarchyTest
    {
        [Fact]
        public void TestMessageRootType()
        {
            var sut = typeof(Message);
            var ancestors = MessageHierarchy.AncestorsAndSelf(sut);
            Assert.Single(ancestors);
            Assert.Contains(typeof(Message), ancestors);

            var descendants = MessageHierarchy.DescendantsAndSelf(sut);
            Assert.Equal(4, descendants.Count());
            Assert.Contains(typeof(Message), descendants);
            Assert.Contains(typeof(MessageParent), descendants);
            Assert.Contains(typeof(MessageChild), descendants);
            Assert.Contains(typeof(MessageChild2), descendants);
        }
        [Fact]
        public void TestMessageParentType()
        {
            var sut = typeof(MessageParent);
            var ancestors = MessageHierarchy.AncestorsAndSelf(sut);
            Assert.Equal(2, ancestors.Count());
            Assert.Contains(typeof(Message), ancestors);
            Assert.Contains(typeof(MessageParent), ancestors);

            var descendants = MessageHierarchy.DescendantsAndSelf(sut);
            Assert.Equal(3, descendants.Count());
            Assert.Contains(typeof(MessageParent), descendants);
            Assert.Contains(typeof(MessageChild), descendants);
            Assert.Contains(typeof(MessageChild2), descendants);
        }
        [Fact]
        public void TestMessageChildType()
        {
            var sut = typeof(MessageChild);
            var ancestors = MessageHierarchy.AncestorsAndSelf(sut);
            Assert.Equal(3, ancestors.Count());
            Assert.Contains(typeof(Message), ancestors);
            Assert.Contains(typeof(MessageParent), ancestors);
            Assert.Contains(typeof(MessageChild), ancestors);

            var descendants = MessageHierarchy.DescendantsAndSelf(sut);
            Assert.Single(descendants);
            Assert.Contains(typeof(MessageChild), descendants);
        }

    }

}
