using LSTY.Sdtd.PatronsMod.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod.Commands
{
    public abstract class ConsoleCmdBase : ConsoleCmdAbstract
    {
        protected const string DefaultServerName = "Server";

		protected virtual void Log(string line)
        {
            SdtdConsole.Instance.Output(CustomLogger.Prefix + line);
        }

        protected virtual void Log(string line, params object[] args)
        {
            SdtdConsole.Instance.Output(CustomLogger.Prefix + string.Format(line, args));
        }
    }
}
