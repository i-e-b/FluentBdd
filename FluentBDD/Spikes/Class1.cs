using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Reflection;
using System.Linq.Expressions;

namespace Spikes
{
    [TestFixture]
    public class Using_method_groups
    {

        [Test]
        public void can_reflect_over_method_groups()
        {
            var result = Given(a_sample_method_group)
                .When(another_method_group);

            Assert.That(result, Is.True);
        }


        public ISampleInterface a_sample_method_group()
        {
            return null;
        }

        public void another_method_group(ISampleInterface str) { }

        public Context<T> Given<T>(Func<T> mg)
        {
            Console.WriteLine(mg.GetMethodInfo().Name);
            return new Context<T>();
        }
    }

    public static class Extensions
    {
        public static Context<T> When<T>(this Context<T> src, Action<T> mutator)
        {
            Console.WriteLine(mutator.GetMethodInfo().Name);
            return src;
        }
    }

    public class Context<T> { }

    public interface ISampleInterface
    {

    }
}
