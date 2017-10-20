using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneExtensions.Benchmarks
{
	public class GetCloneBenchmarks
	{
		private readonly SimpleClass _simpleClass;
		private readonly List<int> _listOfInts;
		private readonly List<SimpleClass> _listOfSimpleClassSameInstance;
		private readonly List<SimpleClass> _listOfSimpleClassDifferentInstances;

		public GetCloneBenchmarks()
		{
			_simpleClass = new SimpleClass()
			{
				Int = 10,
				UInt = 1231,
				Long = 1231234561L,
				ULong = 1516524352UL,
				Double = 1235.1235762,
				Float = 1.333F,
				String = "Lorem ipsum ...",
			};

			_listOfInts = Enumerable.Range(0, 10000).ToList();

			_listOfSimpleClassSameInstance = Enumerable.Repeat(_simpleClass, 10000).ToList();
			_listOfSimpleClassDifferentInstances = Enumerable.Range(0, 10000).Select(x => new SimpleClass() { Int = x }).ToList();
		}

		[Benchmark]
		public int GetCloneSimpleClass()
		{
			var clone = _simpleClass.GetClone();
			return clone.Int;
		}

		[Benchmark]
		public int GetCloneListOfInts()
		{
			var clone = _listOfInts.GetClone();
			return clone.Count;
		}

		[Benchmark]
		public int GetCloneListOfSimpleClassSameInstance()
		{
			var clone = _listOfSimpleClassSameInstance.GetClone();
			return clone.Count;
		}

		[Benchmark]
		public int GetCloneListOfSimpleClassDifferentInstances()
		{
			var clone = _listOfSimpleClassDifferentInstances.GetClone();
			return clone.Count;
		}
	}
}
