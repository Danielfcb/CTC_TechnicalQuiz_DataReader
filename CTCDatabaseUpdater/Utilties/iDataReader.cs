using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTCDatabaseUpdater
{
    interface iDataReader<dataModel>
    {
        List<dataModel> ReadFile(string FullDataFilePath);

    }
}
