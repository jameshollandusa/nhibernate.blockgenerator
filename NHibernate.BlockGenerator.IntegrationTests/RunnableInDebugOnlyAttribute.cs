using System.Diagnostics;
using Xunit;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    public class RunnableInDebugOnlyAttribute : FactAttribute
    {
        private string skip;

        public override string Skip
        {
            get { return Debugger.IsAttached ? skip : "Only running in interactive mode."; }
            set { skip = value; }
        }
    }
}
