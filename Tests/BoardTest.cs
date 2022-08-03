using NUnit.Framework;
using IntroSE.Kanban.Backend.BusinessLayer;
using Moq;
using System;
using System.Collections.Generic;

namespace IntroSE.Kanban.Tests
{
    public class BoardTest
    {
        private IBoard board;
        private Mock<IColumn> ColumnA;
        private Mock<IColumn> ColumnB;
        private Mock<ITask> TaskA;
        private Mock<ITask> TaskB;

        [SetUp]
        public void Setup()
        {
            board = new Board("creator", "name");
            ColumnA = new Mock<IColumn>();
            ColumnB = new Mock<IColumn>();
            TaskA = new Mock<ITask>();
            TaskB = new Mock<ITask>();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AddColumn_AddInRange_CheckOrder_Success(int Ordinal)
        {
            //arrange
            ColumnA.SetupGet(m => m.Ordinal).Returns(Ordinal);
            IList<IColumn> expectedColOrder = new List<IColumn>();
            bool added = false;
            foreach (IColumn col in board.GetColumns())
            {
                if (col.Ordinal == Ordinal)
                {
                    expectedColOrder.Add(ColumnA.Object);
                    added = true;
                }
                expectedColOrder.Add(col);
            }
            if (!added)
            {
                expectedColOrder.Add(ColumnA.Object);
            }
            //act
            try
            {
                board.AddColumn(ColumnA.Object);
            }
            //assert
            catch (Exception)
            {
                Assert.Fail($"Adding Column at index {Ordinal} failed when should have succeeded");
            }
            Assert.AreEqual(expectedColOrder, board.GetColumns(), "Column order isn't right");
        }

        [TestCase(0,0)]
        [TestCase(0,4)]
        [TestCase(3,4)]
        [TestCase(3,0)]
        [TestCase(2,3)]
        public void AddColumn_AddInRange_CheckColumnCount_Success(int OrdinalA, int OrdinalB)
        {
            //arrange
            ColumnA.SetupGet(m => m.Ordinal).Returns(OrdinalA);
            ColumnB.SetupGet(m => m.Ordinal).Returns(OrdinalB);
            int OrigColCount = board.ColumnCount;
            //act
            try
            {
                board.AddColumn(ColumnA.Object);
                board.AddColumn(ColumnB.Object);
            }
            //assert
            catch (Exception)
            {
                Assert.Fail($"Adding Column at indexes {OrdinalA},{OrdinalB} failed when should have succeeded");
            }
            Assert.AreEqual(OrigColCount + 2, board.ColumnCount, "Column Count wasn't updated during addition");
        }

        [Test]
        public void AddColumn_AddNull_Fail()
        {
            //arrange
            //act
            try
            {
                board.AddColumn(null);
            }
            //assert
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail("Should have thrown exception when adding null object");
        }

        [TestCase(-1, 0)]
        [TestCase(-50, 0)]
        [TestCase(4, 0)]
        [TestCase(50, 0)]
        [TestCase(2, 5)]
        [TestCase(2, 50)]
        [TestCase(2, -1)]
        [TestCase(2, -51)]
        public void AddColumn_AddOutOfRange_Fail(int OrdinalA, int OrdinalB)
        {
            //arrange
            ColumnA.SetupGet(m => m.Ordinal).Returns(OrdinalA);
            ColumnB.SetupGet(m => m.Ordinal).Returns(OrdinalB);
            //act
            try
            {
                board.AddColumn(ColumnA.Object);
                board.AddColumn(ColumnB.Object);
            }
            //assert
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail($"Adding Columns at indexes {OrdinalA},{OrdinalB} should fail but didn't throw exception");
        }

        [TestCase(0,0)]
        [TestCase(0,1)]
        [TestCase(0,2)]
        [TestCase(1,-1)]
        [TestCase(1,0)]
        [TestCase(1,1)]
        [TestCase(2,-2)]
        [TestCase(2,-1)]
        [TestCase(2,0)]
        public void MoveColumn_InRange_Success(int Ordinal, int shift)
        {
            //arrange 
            IList<IColumn> expectedOrder = board.GetColumns();
            IColumn movedCol = expectedOrder[Ordinal];
            expectedOrder.RemoveAt(Ordinal);
            expectedOrder.Insert(Ordinal + shift, movedCol);
            //act
            try
            {
                board.MoveColumn(Ordinal, shift);
            }
            //assert
            catch (Exception)
            {
                Assert.Fail($"Moving column {Ordinal} to index {Ordinal + shift} failed when should have succeeded");
            }
            Assert.AreEqual(expectedOrder, board.GetColumns(), "New order of columns isn't right");
            for (int i = 0; i< expectedOrder.Count; i++)
            {
                Assert.AreEqual(i, expectedOrder[i].Ordinal, $"Column at index {i} has ordinal of {expectedOrder[i].Ordinal}");
            }
        }

        [TestCase(0,-1)]
        [TestCase(0,-100)]
        [TestCase(0,3)]
        [TestCase(0,43)]
        [TestCase(1,-2)]
        [TestCase(1,-52)]
        [TestCase(1,2)]
        [TestCase(1,62)]
        [TestCase(2,-3)]
        [TestCase(2,-24)]
        [TestCase(2,1)]
        [TestCase(2,2)]
        public void MoveColumn_OutOfRange_Fail(int Ordinal, int shift)
        {
            //arrange 
            //act
            try
            {
                board.MoveColumn(Ordinal, shift);
            }
            //assert
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail($"Moving Column {Ordinal} to index {Ordinal+shift} should throw exception on board with {board.ColumnCount} columns");
        }

        [Test]
        public void MoveColumn_ColumnWithTasks_Fail()
        {
            //arrange
            ColumnA.SetupGet(m => m.Ordinal).Returns(0);
            Mock<IList<ITask>> taskList = new Mock<IList<ITask>>();
            taskList.SetupGet(m => m.Count).Returns(1);
            ColumnA.Setup(m => m.GetTasks()).Returns(taskList.Object);
            board.AddColumn(ColumnA.Object);
            //act
            try
            {
                board.MoveColumn(ColumnA.Object.Ordinal, ColumnA.Object.Ordinal);
            }
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail($"Moving Column with tasks should throw exception");
        }

        [Test]
        public void GetTask_InRange_Success()
        {
            //arrange
            ColumnA.SetupGet(m => m.Ordinal).Returns(1);
            TaskA.SetupGet(m => m.ID).Returns(0);
            TaskA.SetupGet(m => m.Assignee).Returns("assignee1");
            ColumnA.Setup(m => m.GetTask(0)).Returns(TaskA.Object);
            board.AddColumn(ColumnA.Object);
            board.AddTask(TaskA.Object);
            board.AdvanceTask("assignee1", 0, 0);
            ITask returnedTask = null;
            //act
            try
            {
                returnedTask = board.GetTask(0);
            }
            //assert
            catch (Exception)
            {
                Assert.Fail("Request should have returned task but exception thrown");
            }
            Assert.AreEqual(TaskA.Object, returnedTask, "Returned different Task");
        }

        [TestCase(-100)]
        [TestCase(-1)]
        [TestCase(2)]
        [TestCase(300)]
        public void GetTask_OutOfRange_Fail(int taskID)
        {
            //arrange
            TaskA.SetupGet(m => m.ID).Returns(0);
            TaskB.SetupGet(m => m.ID).Returns(1);
            board.AddTask(TaskA.Object);
            board.AddTask(TaskB.Object);
            ITask returnedTask = null;
            //act
            try
            {
                returnedTask = board.GetTask(taskID);
            }
            //assert
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail($"When requesting task with ID {taskID} but board has tasks with IDs 0-{board.TaskCount - 1} should throw exception");
        }
    }
}
